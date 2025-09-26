namespace Yape.Library.Http.Client.Infraestructure.Adapters.Http
{
    public interface IResilienceHttpFactory
    {
        IResilientHttpClient Create(HttpClient httpClient, HttpClientOptions? httpClientOptions = null);
    }
}
