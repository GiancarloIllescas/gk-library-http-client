using Microsoft.Extensions.Logging;
using Yape.Library.Http.Client.Infraestructure.Adapters.Http;
using Yape.Library.Http.Client.Test.Entities;

namespace Yape.Library.Http.Client.Test.Services
{
    public class ResilientBasicApiService : IResilientBasicApiService
    {
        private readonly IResilientHttpClient _httpClient;

        public ResilientBasicApiService(HttpClient httpClient, IResilienceHttpFactory resilienceHttpFactory)
        {
            _httpClient = resilienceHttpFactory.Create(httpClient);
        }

        public async Task<MockEntity?> GetDataAsync()
        {
            return await _httpClient.GetAsync<MockEntity>("/resilient-api/data");
        }

    }

    public interface IResilientBasicApiService
    {
        Task<MockEntity?> GetDataAsync();
    }
}
