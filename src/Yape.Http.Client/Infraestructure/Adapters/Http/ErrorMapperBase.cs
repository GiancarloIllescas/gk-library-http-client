using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;

namespace Yape.Http.Client.Infraestructure.Adapters.Http
{
    public abstract class ErrorMapperBase
    {
        protected readonly ILoggerFactory _loggerFactory;

        public ErrorMapperBase(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public virtual void HttpRequestFailed(HttpRequestException ex)
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
