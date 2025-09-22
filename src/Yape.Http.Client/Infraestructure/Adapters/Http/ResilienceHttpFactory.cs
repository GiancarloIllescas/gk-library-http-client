using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Yape.Library.Http.Client.Infraestructure.Adapters.Http
{

    public interface IResilienceHttpFactory
    {
        IResilientHttpClient Create(HttpClient httpClient, bool includeHeadersRequired = true);
    }

    public class ResilienceHttpFactory : IResilienceHttpFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;

        public ResilienceHttpFactory(ILogger<ResilientHttpClient> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public IResilientHttpClient Create(HttpClient httpClient, bool includeHeadersRequired = true)
        {
            
            var resilientHttpClient = new ResilientHttpClient(httpClient, _logger);

            if (includeHeadersRequired)
            {
                resilientHttpClient.HeadersRequired = new HttpHeaders(_httpContextAccessor);
            }

            return resilientHttpClient;
        }

    }

    public class HttpHeaders
    {
        private readonly IHeaderDictionary? _requestHeaders;
        private readonly List<string> _headerList;

        public HttpHeaders(IHttpContextAccessor _httpContextAccessor) 
        {
            _requestHeaders = _httpContextAccessor.HttpContext?.Request.Headers;
            _headerList = new List<string>()
            {
                "X-Correlation-Id",
                "Request-Date",
                "Channel"
            };
        }

        public void AddHeaders(ref IDictionary<string, string> headers)
        {

            if(headers == null)
            {
                headers = new Dictionary<string, string>();
            }

            foreach (var item in _headerList)
            {
                if (_requestHeaders != null && _requestHeaders.ContainsKey(item))
                {
                    headers.TryAdd(item, _requestHeaders[item].FirstOrDefault());
                }
            }

        }

    }

}
