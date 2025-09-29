using Yape.Library.Http.Client.Domain.Port;

namespace Yape.Library.Http.Client.Infraestructure.Adapters.Http
{
    public class HttpClientOptions
    {
        public bool IncludeHeadersRequired { get; set; } = true;
        public HttpErrorMapperBase? ErrorMapper { get; set; } = null;
    }
}
