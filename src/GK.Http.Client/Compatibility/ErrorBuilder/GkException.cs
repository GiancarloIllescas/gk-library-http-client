using System.Net;

namespace GK.Library.Http.Client.ErrorBuilder.Domain;

public class GkException : Exception
{
    public HttpStatusCode Status { get; set; }
    public string? ErrorCode { get; set; }
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public IDictionary<string, object?>? Extensions { get; set; }

    public GkException()
    {
    }

    public GkException(string? message) : base(message)
    {
    }

    public GkException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
