using GK.Library.Http.Client.Infraestructure.Adapters.Http;

namespace GK.Library.Http.Client.Domain.Port
{
    public interface IResilienceHttpFactory
    {
        IResilientHttpClient Create(HttpClient httpClient, HttpClientOptions? httpClientOptions = null);
    }
}
