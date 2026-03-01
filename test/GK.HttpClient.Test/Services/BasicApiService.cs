using Microsoft.Extensions.Logging;
using GK.Library.Http.Client.Domain.Port;
using GK.Library.Http.Client.Test.Entities;

namespace GK.Library.Http.Client.Test.Services
{
    public class BasicApiService : IBasicApiService
    {
        private readonly IResilientHttpClient _httpClient;

        public BasicApiService(HttpClient httpClient, 
            IResilienceHttpFactory resilienceHttpFactory)
        {
            _httpClient = resilienceHttpFactory.Create(httpClient);
        }

        public async Task<MockEntity?> GetDataAsync()
        {
            return await _httpClient.GetAsync<MockEntity>("/basic-api/data");
        }
        public async Task<MockEntity?> Create(MockEntity data)
        {
            return await _httpClient.PostAsync<MockEntity, MockEntity>("/basic-api", data);
        }
        public async Task Delete(int id)
        {
            await _httpClient.DeleteAsync($"/basic-api/{id}");
        }
        public async Task<MockEntity?> Update(MockEntity data)
        {
            return await _httpClient.PutAsync<MockEntity, MockEntity>("/basic-api", data);
        }

    }

    public interface IBasicApiService
    {
        Task<MockEntity?> GetDataAsync();
        Task<MockEntity?> Create(MockEntity data);
        Task Delete(int id);
        Task<MockEntity?> Update(MockEntity data);
    }

}