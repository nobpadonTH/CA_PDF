using CA_SVC.Services.Auth;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CA_SVC.Middlewares
{
    public class JwtHeaderPublishMiddleware<T> : IFilter<PublishContext<T>> where T : class
    {
        private readonly ILoginDetailServices _loginDetailServices;
        private readonly ILogger<JwtHeaderPublishMiddleware<T>> _logger;

        public JwtHeaderPublishMiddleware(ILoginDetailServices loginDetailServices, ILogger<JwtHeaderPublishMiddleware<T>> logger)
        {
            _loginDetailServices = loginDetailServices;
            _logger = logger;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope("publishjwtheader");
        }

        public Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
        {
            try
            {
                context.Headers.Set("Bearer", _loginDetailServices.GetClaim().Token);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[Masstransit Middleware] JWT Header could not be set");
            }

            return next.Send(context);
        }
    }
}