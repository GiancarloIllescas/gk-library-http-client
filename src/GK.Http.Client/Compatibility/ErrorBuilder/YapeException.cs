using System.Net;

namespace Yape.Library.ErrorBuilder.Domain;

public class YapeException : Exception
{
    public HttpStatusCode Status { get; set; }
    public string? ErrorCode { get; set; }
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public IDictionary<string, object?>? Extensions { get; set; }

    public YapeException()
    {
    }

    public YapeException(string? message) : base(message)
    {
    }

    public YapeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
