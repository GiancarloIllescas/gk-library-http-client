using Microsoft.Extensions.Logging;
using GK.Library.Http.Client.Domain.Port;
using GK.Library.Http.Client.Test.Entities;

namespace GK.Library.Http.Client.Test.Services
{
    public class ResilientRetryApiService : IResilientRetryApiService
    {
        private readonly IResilientHttpClient _httpClient;

        public ResilientRetryApiService(HttpClient httpClient, IResilienceHttpFactory resilienceHttpFactory)
        {
            _httpClient = resilienceHttpFactory.Create(httpClient);
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
