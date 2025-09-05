using Microsoft.Extensions.Logging;

namespace Yape.Library.Http.Client.Infraestructure.Adapters.Http
{
    public static class HttpClientExtension
    {
        public static IResilientHttpClient CreateExtension(this HttpClient client, ILogger logger)
        {
            return new ResilientHttpClient(client, logger);
        }
    }
}
