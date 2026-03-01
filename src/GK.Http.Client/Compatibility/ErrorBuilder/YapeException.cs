// This file is deprecated. Use GK.Library.Http.Client.ErrorBuilder.Domain.GkException instead.
// This file is kept for backward compatibility only.

namespace Yape.Library.ErrorBuilder.Domain;

[Obsolete("Use GK.Library.Http.Client.ErrorBuilder.Domain.GkException instead.", false)]
public class YapeException : GK.Library.Http.Client.ErrorBuilder.Domain.GkException
{
    public YapeException()
        : base()
    {
    }

    public YapeException(string? message)
        : base(message)
    {
    }

    public YapeException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

