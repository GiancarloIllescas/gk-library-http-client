namespace Yape.Library.Http.Client.Infraestructure.Adapters.Http
{
    public interface IResilientHttpClient
    {
        // Métodos HTTP para obtener y enviar datos, serializando/deserializando automáticamente 
        Task<TResponse?> GetAsync<TResponse>(string requestUri, IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
        Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUri, TRequest data, IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
        Task PostAsync<TRequest>(string requestUri, TRequest data, IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
        Task<TResponse?> PutAsync<TRequest, TResponse>(string requestUri, TRequest data, IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
        Task PutAsync<TRequest>(string requestUri, TRequest data, IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
        Task DeleteAsync(string requestUri, IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    }
}
