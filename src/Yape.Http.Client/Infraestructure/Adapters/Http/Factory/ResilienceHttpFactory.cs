using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Yape.Library.Http.Client.Infraestructure.Adapters.Http
{
    public class ResilienceHttpFactory : IResilienceHttpFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;

        public ResilienceHttpFactory(ILogger<ResilientHttpClient> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public IResilientHttpClient Create(HttpClient httpClient, HttpClientOptions? httpClientOptions = null)
        {
            if(httpClientOptions == null)
            {
                httpClientOptions = new HttpClientOptions();
            }

            HttpHeaders? headersRequired = null;
            if (httpClientOptions.IncludeHeadersRequired)
            {
                headersRequired = new HttpHeaders(_httpContextAccessor);
            }

            IHttpErrorMapper errorMapper = new HttpErrorMapperDefault(_logger);
            if (httpClientOptions.ErrorMapper != null)
            {
                errorMapper = httpClientOptions.ErrorMapper;
            }

            var resilientHttpClient = new ResilientHttpClient(httpClient, _logger, errorMapper, headersRequired);


            return resilientHttpClient;
        }

    }

}
