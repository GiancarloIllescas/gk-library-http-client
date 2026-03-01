using GK.Library.Http.Client.Domain.Port;

namespace GK.Library.Http.Client.Infraestructure.Adapters.Http;

public class HttpClientOptions
{
    public bool IncludeHeadersRequired { get; set; } = true;
    public HttpErrorMapperBase? ErrorMapper { get; set; } = null;
}
