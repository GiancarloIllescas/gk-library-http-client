using Yape.Library.Http.Client.Infraestructure.Adapters.Http;

namespace Yape.Library.Http.Client.Domain.Port
{
    public interface IResilienceHttpFactory
    {
        IResilientHttpClient Create(HttpClient httpClient, HttpClientOptions? httpClientOptions = null);
    }
}
