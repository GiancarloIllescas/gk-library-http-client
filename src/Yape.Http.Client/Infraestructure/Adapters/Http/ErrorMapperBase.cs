using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;

namespace Yape.Library.Http.Client.Infraestructure.Adapters.Http
{
    public abstract class ErrorMapperBase
    {
        protected readonly ILoggerFactory _loggerFactory;

        public ErrorMapperBase(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public virtual void HttpRequestFailed(HttpResponseMessage response, HttpRequestException ex)
        {

        }

        public virtual void TimeoutFailed(HttpRequestException ex)
        {

        }

        public virtual void BrokenCircuitFailed(BrokenCircuitException ex)
        {

        }

        public virtual void GeneralErrorOccurred(Exception ex)
        {

        }

    }
}
