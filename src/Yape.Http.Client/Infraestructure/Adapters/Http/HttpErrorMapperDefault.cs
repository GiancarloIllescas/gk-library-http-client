using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Yape.Library.ErrorBuilder.Domain.Builder;
using Yape.Library.Http.Client.Domain.Port;


namespace Yape.Library.Http.Client.Infraestructure.Adapters.Http
{
    public class HttpErrorMapperDefault : HttpErrorMapperBase
    {

        public HttpErrorMapperDefault(ILogger logger) : base(logger)
        {
        }

        public async override Task<bool> HttpRequestFailed(HttpResponseMessage response, HttpRequestException ex)
        {
            bool result = false;

            if (response.Content.Headers.ContentLength == 0) // Manejar respuestas vacías 
            {
                return true;
            }

            ProblemDetails? problemDetails = null;
            try
            {
                problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

                });
            }
            catch (JsonException)
            {
                result = true;
            }

            if (problemDetails != null)
            {
                throw new YapeGeneralExceptionBuilder()
                {
                    Status = (HttpStatusCode)problemDetails.Status.GetValueOrDefault(),
                    ErrorCode = problemDetails.Type,
                    Title = problemDetails.Title,
                    Detail = problemDetails.Detail
                };
            }

            return result;
        }


    }
}
