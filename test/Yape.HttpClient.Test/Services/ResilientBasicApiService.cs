using Microsoft.Extensions.Logging;
using Yape.Http.Client.Infraestructure.Adapters.Http;
using Yape.Http.Client.Test.Entities;

namespace Yape.Http.Client.Test.Services
{
    public class ResilientBasicApiService : IResilientBasicApiService
    {
        private readonly IResilientHttpClient _httpClient;

        public ResilientBasicApiService(HttpClient httpClient, ILoggerFactory loggerFactory)
        {
            _httpClient = httpClient.CreateExtension(loggerFactory);
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
