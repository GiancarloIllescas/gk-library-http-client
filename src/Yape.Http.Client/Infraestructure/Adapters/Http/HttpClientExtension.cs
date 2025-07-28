using Microsoft.Extensions.Logging;

namespace Yape.Http.Client.Infraestructure.Adapters.Http
{
    public static class HttpClientExtension
    {
        public static IResilientHttpClient CreateExtension(this HttpClient client, ILoggerFactory loggerFactory)
        {
            return new ResilientHttpClient(client, loggerFactory);
        }
    }
}
