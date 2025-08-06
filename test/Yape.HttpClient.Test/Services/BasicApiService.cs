using Microsoft.Extensions.Logging;
using Yape.Library.Http.Client.Infraestructure.Adapters.Http;
using Yape.Library.Http.Client.Test.Entities;

namespace Yape.Library.Http.Client.Test.Services
{
    public class BasicApiService : IBasicApiService
    {
        private readonly IResilientHttpClient _httpClient;

        public ErrorMapperBase? ErrorMapper { get; set; }

        public BasicApiService(HttpClient httpClient, ILoggerFactory loggerFactory)
        {
            _httpClient = httpClient.CreateExtension(loggerFactory);
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
            _httpClient.ErrorMapper = this.ErrorMapper;

            return await _httpClient.PutAsync<MockEntity, MockEntity>("/basic-api", data);
        }

    }

    public interface IBasicApiService
    {
        public ErrorMapperBase ErrorMapper { get; set; }
        Task<MockEntity?> GetDataAsync();
        Task<MockEntity?> Create(MockEntity data);
        Task Delete(int id);
        Task<MockEntity?> Update(MockEntity data);
    }
}