using Microsoft.AspNetCore.Http;

namespace Yape.Library.Http.Client.Infraestructure.Adapters.Http
{
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

            if (headers == null)
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
