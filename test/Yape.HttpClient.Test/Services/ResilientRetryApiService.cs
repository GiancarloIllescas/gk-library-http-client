using Microsoft.Extensions.Logging;
using Yape.Http.Client.Infraestructure.Adapters.Http;
using Yape.Http.Client.Test.Entities;

namespace Yape.Http.Client.Test.Services
{
    public class ResilientRetryApiService : IResilientRetryApiService
    {
        private readonly IResilientHttpClient _httpClient;

        public ResilientRetryApiService(HttpClient httpClient, ILoggerFactory loggerFactory)
        {
            _httpClient = httpClient.CreateExtension(loggerFactory);
        }

        public async Task<MockEntity?> GetDataAsync()
        {
            return await _httpClient.GetAsync<MockEntity>("/resilient-api/data");
        }

    }

    public interface IResilientRetryApiService
    {
        Task<MockEntity?> GetDataAsync();
    }
}
