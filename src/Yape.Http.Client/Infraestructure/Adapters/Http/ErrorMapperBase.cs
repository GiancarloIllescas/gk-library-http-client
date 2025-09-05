using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;

namespace Yape.Library.Http.Client.Infraestructure.Adapters.Http
{
    public abstract class ErrorMapperBase
    {
        protected readonly ILogger _logger;

        public ErrorMapperBase(ILogger logger)
        {
            _logger = logger;
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
