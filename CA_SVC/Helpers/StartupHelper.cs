using CA_SVC.Configurations;
using CA_SVC.Filters;
using CA_SVC.Middlewares;
using GreenPipes;
using MassTransit;
using MassTransit.Definition;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace CA_SVC.Helpers
{
    public static class StartupHelper
    {
        /// <summary>
        /// Extesion ของ IServiceCollection สำหรับใช้ MassTransit กับ RabbitMQ
        /// </summary>
        /// <remarks>
        /// ใน Startup.cs ใน Service Pipeline ให้ทำการเพิ่ม services.AddMassTransitWithRabbitMq(serviceName)
        /// </remarks>
        /// <param name="services"></param>
        /// <param name="serviceName">ชื่อของ Project ที่ใช้งาน</param>
        public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services, string serviceName)
        {
            // เพิ่ม MassTransit ใน Service Pipeline
            services.AddMassTransit(configure =>
            {
                // กรณีมี Consumer จะทำการหาไฟล์จากในโปรเจ็คเพื่อนำมารัน Hosted Service
                configure.AddConsumers(Assembly.GetEntryAssembly());

                // เพิ่ม Config ของ RabbitMQ
                configure.UsingRabbitMq((context, configurator) =>
                {
                    // รับค่าจาก AppSetting หรือ Environment
                    var configuration = context.GetService<IConfiguration>();
                    var rabbitMQSetting = configuration.GetSection("RabbitMQSetting").Get<RabbitMQSetting>();

                    configurator.UsePublishFilter(typeof(JwtHeaderPublishMiddleware<>), context);

                    // ตั้งค่า Host บนเครื่องเดียวกัน ปกติเป็น localhost
                    if (rabbitMQSetting.Host.StartsWith("amqps:"))
                    {
                        configurator.Host(new Uri(rabbitMQSetting.Host), h =>
                        {
                            h.Username(rabbitMQSetting.Username);
                            h.Password(rabbitMQSetting.Password);
                        });
                    }
                    else if (!string.IsNullOrWhiteSpace(rabbitMQSetting.Username) && !string.IsNullOrWhiteSpace(rabbitMQSetting.Password) && rabbitMQSetting.TLS != 0)
                    {
                        configurator.Host(rabbitMQSetting.Host, rabbitMQSetting.Port, rabbitMQSetting.Vhost, h =>
                        {
                            h.Username(rabbitMQSetting.Username);
                            h.Password(rabbitMQSetting.Password);

                            if (rabbitMQSetting.TLS != 0)
                            {
                                h.UseSsl(s =>
                                {
                                    s.Protocol = (SslProtocols)rabbitMQSetting.TLS;
                                });
                            }
                        });
                    }
                    else if (!string.IsNullOrWhiteSpace(rabbitMQSetting.Username) && !string.IsNullOrWhiteSpace(rabbitMQSetting.Password))
                    {
                        configurator.Host(rabbitMQSetting.Host, h =>
                        {
                            h.Username(rabbitMQSetting.Username);
                            h.Password(rabbitMQSetting.Password);
                        });
                    }
                    else
                    {
                        configurator.Host(rabbitMQSetting.Host);
                    }

                    // ตั้ง Endpoint ของ RabbitMQ
                    configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceName, false));

                    // หากไม่สามารถรับค่าจาก RabbitMQ ได้จะทำการทำซ้ำ
                    configurator.UseMessageRetry(retryConfigurator =>
                    {
                        // ทำครั้ง 3 ครั้ง ทุก 5 นาที ก่อนเลิกทำ
                        retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                    });
                });
            });

            // เพิ่ม Hosted Service ของ MassTransit เพื่อคุยกับ RabbitMQ
            services.AddMassTransitHostedService();

            return services;
        }

        /// <summary>
        /// Add Authentication
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddAuthenticationWithOAuth(this IServiceCollection services)
        {
            var configuration = services.BuildServiceProvider().GetService<IConfiguration>();

            services.AddAuthentication(config =>
            {
                config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.Authority = configuration["OAuth:Authority"];
                    options.Audience = configuration["OAuth:Audience"];
                    options.RequireHttpsMetadata = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                });

            return services;
        }

        /// <summary>
        /// Add Swagger
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddSwaggerWithOAuth(this IServiceCollection services)
        {
            var configuration = services.BuildServiceProvider().GetService<IConfiguration>();

            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Version = configuration["Project:Version"],
                        Title = configuration["Project:Title"],
                        Description = configuration["Project:Description"]
                    });

                config.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{configuration["OAuth:Authority"]}/connect/authorize"),
                            TokenUrl = new Uri($"{configuration["OAuth:Authority"]}/connect/token"),
                            Scopes = configuration.GetSection("OAuth:Scopes").GetChildren()
                                                  .ToDictionary(_ => _.Key, _ => _.Value)
                        }
                    }
                }); ;

                config.OperationFilter<AuthorizeCheckOperationFilter>(configuration["OAuth:Audience"]);

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                config.IncludeXmlComments(xmlPath);
            });

            return services;
        }
    }
}