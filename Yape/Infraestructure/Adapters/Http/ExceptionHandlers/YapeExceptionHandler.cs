using ErrorHandling.Yape.Domain;
using ErrorHandling.Yape.Infraestructure.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;

namespace CustomHttpClient.Yape.Infraestructure.Adapters.Http.ExceptionHandlers;

public class YapeExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken
    )
    {
        YapeException yapeException;

        if (exception is not YapeException) {
            if (exception is HttpRequestException httpException) {
                yapeException = YapeException.Builder()
                .Status(httpException.StatusCode)
                .Build();
            } else {
                yapeException = YapeException.Default();
            }
        } else {
            yapeException = (YapeException)exception;
        }

        var errorMessage = new ErrorMessage {
            Title = yapeException.Message,
            Message = yapeException.Message,
            Code = yapeException.Code
        };
        httpContext.Response.StatusCode = (int)yapeException.Status;
        
        await httpContext.Response.WriteAsJsonAsync(errorMessage, cancellationToken);

        return true;
    }
}