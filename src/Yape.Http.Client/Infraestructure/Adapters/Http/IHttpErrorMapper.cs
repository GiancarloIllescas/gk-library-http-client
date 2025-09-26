using Polly.CircuitBreaker;

namespace Yape.Library.Http.Client.Infraestructure.Adapters.Http
{
    public interface IHttpErrorMapper
    {
        bool BrokenCircuitFailed(BrokenCircuitException ex);
        bool GeneralErrorOccurred(Exception ex);
        Task<bool> HttpRequestFailed(HttpResponseMessage response, HttpRequestException ex);
        bool TimeoutFailed(HttpRequestException ex);
    }
}