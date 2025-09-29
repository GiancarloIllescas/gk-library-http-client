using Microsoft.Extensions.Logging;
using Yape.Library.Http.Client.Domain.Port;
using Yape.Library.Http.Client.Infraestructure.Adapters.Http;
using Yape.Library.Http.Client.Test.Entities;

namespace Yape.Library.Http.Client.Test.Services
{
    /// <summary>
    /// Servicio que permite probar la llamda http con control custom de errores
    /// </summary>
    public class BasicApiCustomErrorService : IBasicApiCustomErrorService
    {
        private readonly IResilientHttpClient _httpClient;

        public BasicApiCustomErrorService(HttpClient httpClient, 
            IResilienceHttpFactory resilienceHttpFactory,
            ILogger<BasicApiCustomErrorService> logger)
        {
            var options = new HttpClientOptions()
            {
                ErrorMapper = new CustomErrorMapperTest(logger)
            };

            _httpClient = resilienceHttpFactory.Create(httpClient, options);
        }

        public async Task<MockEntity?> GetDataAsync()
        {
            return await _httpClient.GetAsync<MockEntity>("/basic-api/data");
        }
    }

    public interface IBasicApiCustomErrorService
    {
        Task<MockEntity?> GetDataAsync();
    }

    public class CustomErrorMapperTest : HttpErrorMapperBase
    {
        public CustomErrorMapperTest(ILogger looger) : base(looger)
        {

        }

        public override Task<bool> HttpRequestFailed(HttpResponseMessage response, HttpRequestException ex)
        {
            return Task.FromResult(false); //anulo para que no se relance el exception de http
        }
    }

}