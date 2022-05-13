using CA_SVC.Clients;
using CA_SVC.Configurations;
using CA_SVC.Data;
using CA_SVC.Filters;
using CA_SVC.Helpers;
using CA_SVC.Middlewares;
using CA_SVC.Services;
using CA_SVC.Services.CA;
using CA_SVC.Services.Logger;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CA_SVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(ErrorFilter));
                options.Filters.Add(new ValidateModelFilter());
            })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; //ReferenceLoopHandling : กันไม่ให้ dto ซ้อน dto;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; // NullValueHandling : ข้อมูลที่มีค่าเป็น null จะไม่แสดงใน response;
                });

            services.AddHttpContextAccessor();
            services.AddResponseCaching();

            //------Allow Origins------
            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
            //------End: Allow Origins------

            //------HealthChecks------
            services.AddHealthChecks().AddDbContextCheck<AppDBContext>(tags: new[] { "ready" });
            //------End: HealthChecks------

            //------AutoMapper------
            services.AddAutoMapper(typeof(Startup));
            //------End: AutoMapper------

            //------DBContext------
            services.AddDbContext<AppDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            //------End: DBContext------

            //------Options ------
            services.Configure<ServiceURL>(Configuration.GetSection("ServiceURL"));
            services.Configure<OAuthSetting>(Configuration.GetSection("OAuth"));
            //--------------------

            //------RestShape Client------
            services.AddSingleton<ShortLinkClient>();
            services.AddSingleton<SendSmsClient>();
            //------End: RestShape Client------

            //------OData------
            services.AddOData();
            //------End: OData------

            //------Swagger------
            services.AddSwaggerWithOAuth();
            //------End: Swagger------

            //------Authentication------
            services.AddAuthenticationWithOAuth();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Permission.Base, Permission.BasePermission());
                options.AddPolicy(Permission.Read, Permission.ReadPermission());
                options.AddPolicy(Permission.Write, Permission.WritePermission());
                options.AddPolicy(Permission.ReadOrWrite, Permission.ReadOrWritePermission());
                options.AddPolicy(Permission.Delete, Permission.DeletePermission());
            });
            //------End: Authentication------

            //------Service------
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //------End: Service------

            //------Host-Service------
            services.AddHostedService<LoggerRetentionServices>();
            services.AddScoped<ICAServices, CAServices>();
            //------End: Host-Service------

            AddFormatters(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //------Logging Failed Requests to Serilog Using Middleware ------
            /*app.Use(async (context, next) =>
            {
                context.Request.EnableBuffering();
                await next();
            });*/

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            //reference = https://blog.bitscry.com/2019/01/14/logging-failed-requests-using-middleware/
            //------End: Logging Failed Requests to Serilog Using Middleware ------

            //------Swagger------
            app.UseSwagger(config =>
            {
                config.PreSerializeFilters.Add((swagger, httpRequest) =>
                {
                    swagger.Servers.Clear();
                });
            });

            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", Configuration["Project:Title"]);

                config.OAuthClientId("admin-client_api_swaggerui");
                config.OAuthAppName(Configuration["Project:Title"]);
                config.OAuthUsePkce();
            });
            //------End: Swagger------

            //app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseResponseCaching();

            app.UseAuthentication();

            app.UseAuthorization();

            //------Allow Origins------
            app.UseCors("MyPolicy");
            //------End: Allow Origins------

            //------HealthChecks------
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
                {
                    ResponseWriter = HealthCheckResponseWriter.WriteResponseReadiness,
                    Predicate = (check) => check.Tags.Contains("ready")
                });

                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
                {
                    ResponseWriter = HealthCheckResponseWriter.WriteResponseLiveness,
                    Predicate = (check) => !check.Tags.Contains("ready")
                });
            });
            //------End: HealthChecks------

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        #region Method

        /// <summary>
        /// Add Formatter for OData with swagger
        /// </summary>
        /// <param name="services"></param>
        public void AddFormatters(IServiceCollection services)
        {
            services.AddMvcCore(options =>
            {
                foreach (var outputFormatter in options.OutputFormatters.OfType<OutputFormatter>().Where(x => x.SupportedMediaTypes.Count == 0))
                {
                    outputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }

                foreach (var inputFormatter in options.InputFormatters.OfType<InputFormatter>().Where(x => x.SupportedMediaTypes.Count == 0))
                {
                    inputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
                }
            }
            );
        }

        #endregion Method
    }
}