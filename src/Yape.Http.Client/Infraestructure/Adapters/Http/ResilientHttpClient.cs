using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout; // Para TimeoutRejectedException
using System.Net; // Para HttpStatusCode.RequestTimeout 
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Yape.Library.Http.Client.Infraestructure.Adapters.Http
{
    public class ResilientHttpClient : IResilientHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ErrorMapperBase? ErrorMapper {  get; set; }

        public ResilientHttpClient(
            HttpClient httpClient,
            ILogger logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger;

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // --- Implementación de Métodos HTTP --- 

        public async Task<TResponse?> GetAsync<TResponse>(string requestUri, IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            return await SendRequestAsync<TResponse>(HttpMethod.Get, requestUri, null, headers, cancellationToken);
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUri, TRequest data, IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            return await SendRequestAsync<TResponse>(HttpMethod.Post, requestUri, data, headers, cancellationToken);
        }

        public async Task PostAsync<TRequest>(string requestUri, TRequest data, IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            await SendRequestAsync<object>(HttpMethod.Post, requestUri, data, headers, cancellationToken);
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string requestUri, TRequest data, IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            return await SendRequestAsync<TResponse>(HttpMethod.Put, requestUri, data, headers, cancellationToken);
        }

        public async Task PutAsync<TRequest>(string requestUri, TRequest data, IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            await SendRequestAsync<object>(HttpMethod.Put, requestUri, data, headers, cancellationToken);
        }

        public async Task DeleteAsync(string requestUri, IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            await SendRequestAsync<object>(HttpMethod.Delete, requestUri, null, headers, cancellationToken);
        }

        /// <summary> 
        /// Método genérico para enviar cualquier tipo de solicitud HTTP con manejo de errores, eventos y serialización. 
        /// </summary> 
        private async Task<TResponse?> SendRequestAsync<TResponse>(
            HttpMethod method,
            string requestUri,
            object? data,
            IDictionary<string, string>? headers,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage? responseError = null;
            try
            {
                var request = new HttpRequestMessage(method, requestUri);

                // Añadir contenido para POST y PUT 
                if (data != null && (method == HttpMethod.Post || method == HttpMethod.Put))
                {
                    // Esto asigna el encabezado Content-Type automáticamente con "application/json"
                    request.Content = JsonContent.Create(data, options: _jsonSerializerOptions);
                }

                // Aplicar headers específicos de la solicitud 
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                _logger.LogInformation("Sending {Method} request to {Uri}.", method, requestUri);

                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                // Disparar evento si el StatusCode no es de éxito antes de EnsureSuccessStatusCode 
                if (!response.IsSuccessStatusCode)
                {
                    responseError = response;
                    _logger.LogWarning("HTTP request failed with status {StatusCode} for {Method} {Uri}.", response.StatusCode, method, requestUri);               
                }

                response.EnsureSuccessStatusCode(); // Lanza HttpRequestException para códigos de estado 4xx/5xx 

                // Serializar la respuesta a TResponse si se espera una 
                if (typeof(TResponse) != typeof(object)) // object es el tipo placeholder para métodos sin retorno específico 
                {
                    if (response.Content.Headers.ContentLength == 0) // Manejar respuestas vacías 
                    {
                        return default;
                    }
                    return await response.Content.ReadFromJsonAsync<TResponse>(_jsonSerializerOptions, cancellationToken);
                }

            }
            catch (TaskCanceledException ex)
            {
                // --- CAPTURA ESPECÍFICA DE TIMEOUT DE HTTPCLIENT ---
                _logger.LogError(ex, "HTTP request timed out for {Method} {Uri}.", method, requestUri);

                ErrorMapper?.TimeoutFailed(new HttpRequestException($"Request timed out.", ex, HttpStatusCode.RequestTimeout));
                
                if (ErrorMapper == null)
                    throw new TimeoutException($"The HTTP request to {requestUri} timed out.", ex); // Re-lanzar un TimeoutException 
            }
            catch (HttpRequestException ex)
            {
                ErrorMapper?.HttpRequestFailed(responseError, ex);

                if (ErrorMapper == null)
                    throw; // Re-lanzar para que el llamador pueda manejarlo si quiere 
            }
            catch (TimeoutRejectedException ex) // Lanzada por la política de Timeout de Polly 
            {
                _logger.LogError(ex, "HTTP request timed out for {Method} {Uri}.", method, requestUri);

                ErrorMapper?.TimeoutFailed(new HttpRequestException($"Request timed out.", ex, HttpStatusCode.RequestTimeout));

                if (ErrorMapper == null)
                    throw new TimeoutException($"The HTTP request to {requestUri} timed out.", ex); // Re-lanzar un TimeoutException 
            }
            catch(BrokenCircuitException ex)
            {
                _logger.LogError(ex, "HTTP request timed out for {Method} {Uri}.", method, requestUri);
                ErrorMapper?.BrokenCircuitFailed(ex);

                if (ErrorMapper == null)
                    throw;
            }
            catch (Exception ex) // Captura cualquier otra excepción (deserialización, red, etc.) 
            {
                _logger.LogError(ex, "An unexpected error occurred during {Method} request to {Uri}.", method, requestUri);
                ErrorMapper?.GeneralErrorOccurred(ex);

                if (ErrorMapper == null)
                    throw;
            }

            return default; // Para métodos que no esperan una respuesta TResponse 

        }
    }

}