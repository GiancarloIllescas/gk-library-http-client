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
            IHttpErrorMapper errorMapper)
        {
            var options = new HttpClientOptions()
            {
                ErrorMapper = errorMapper
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

}