namespace Yape.Library.Http.Client.Infraestructure.Adapters.Http
{
    public class HttpClientOptions
    {
        public bool IncludeHeadersRequired { get; set; } = true;
        public IHttpErrorMapper? ErrorMapper { get; set; } = null;
    }
}
