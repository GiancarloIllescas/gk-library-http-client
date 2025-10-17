using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;


namespace Yape.Library.Http.Client.Infraestructure.Adapters.Http;

public abstract class HttpErrorMapperBase
{
    protected readonly ILogger _logger;

    public HttpErrorMapperBase(ILogger logger)
    {
        _logger = logger;
    }

    public virtual Task<bool> HttpRequestFailed(HttpResponseMessage response, HttpRequestException ex)
    {
        return Task.FromResult(true);
    }

    public virtual bool TimeoutFailed(HttpRequestException ex)
    {
        return true;
    }

    public virtual bool BrokenCircuitFailed(BrokenCircuitException ex)
    {
        return true;
    }

    public virtual bool GeneralErrorOccurred(Exception ex)
    {
        return true;
    }

}
