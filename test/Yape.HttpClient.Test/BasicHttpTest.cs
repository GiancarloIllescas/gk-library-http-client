using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Yape.Library.Http.Client.Infraestructure.Adapters.Http;
using Yape.Library.Http.Client.Test.Entities;

namespace Yape.Library.Http.Client.Test;

public class BasicHttpTest : IntegrationTestBase
{
    public BasicHttpTest(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Get_ShouldReturnData_WhenBasicApiReturnsSuccess()
    {
        // Arrange
        // Configura WireMock para emular la respuesta de la API externa
        _mockServer
            .Given(Request.Create().WithPath("/basic-api/data").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new { id = 1, description = "Mocked Data" })));

        // Act
        var account = await _basicApiService.GetDataAsync();

        // Assert
        Assert.NotNull(account);
        Assert.Equal("Mocked Data", account.Description);

        // Verificar que tu aplicación realmente llamó al mock server
        var findEntries = _mockServer.FindLogEntries(
            Request.Create().WithPath("/basic-api/data").UsingGet()
        );
        Assert.Single(findEntries);
    }

    [Fact]
    public async Task Post_ShouldCreateData_WhenBasicApiReturnsSuccess()
    {
        // Arrange
        var request = new MockEntity() { Name = "Name Mock", Description = "Mocked Data" };
        var response = new MockEntity() { Id = 1, Name = "Name Mock", Description = "Mocked Data" };
        
        _mockServer
            .Given(Request.Create().WithPath("/basic-api").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.Created)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(response)));

        // Act
        var account = await _basicApiService.Create(request);

        // Assert
        Assert.NotNull(account);
        Assert.Equal(1, account.Id);

        // Verificar que tu aplicación realmente llamó al mock server
        var findEntries = _mockServer.FindLogEntries(
            Request.Create().WithPath("/basic-api").UsingPost()
        );
        Assert.Single(findEntries);
    }

    [Fact]
    public async Task Delete_ShouldDeleteData_WhenBasicApiReturnsSuccess()
    {
        // Arrange
        int itemIdToDelete = 10;
        string uri = $"/basic-api/{itemIdToDelete}";

        _mockServer
            .Given(Request.Create().WithPath(uri).UsingDelete())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.NoContent)
                .WithHeader("Content-Type", "application/json"));

        // Act
        await _basicApiService.Delete(itemIdToDelete);

        // Verificar que tu aplicación realmente llamó al mock server
        var findEntries = _mockServer.FindLogEntries(
            Request.Create().WithPath(uri).UsingDelete()
        );
        Assert.Single(findEntries);
    }

    [Fact]
    public async Task Get_ShouldFail_WhenBasicApiReturnsHttpRequestException()
    {
        var request = new MockEntity() { Id = 1, Name = "Name Mock", Description = "Mocked Data" };

        // Arrange
        _mockServer
            .Given(Request.Create().WithPath("/basic-api/data").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.InternalServerError)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(request)));

        // Act
        var exception = await Record.ExceptionAsync(async () => await _basicApiService.GetDataAsync());

        // Assertions
        Assert.NotNull(exception);
        Assert.IsType<HttpRequestException>(exception);
    }

    [Fact]
    public async Task Put_ShouldFail_HandlerExceptionWithErrorMapper()
    {
        var request = new MockEntity() { Id = 1, Name = "Name Mock", Description = "Mocked Data" };

        // Arrange
        _mockServer
            .Given(Request.Create().WithPath("/basic-api").UsingPut())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.InternalServerError));

        var logger = new Mock<ILogger>();
        var errorMapper = new Mock<ErrorMapperBase>(logger.Object);

        errorMapper.Setup(x => x.HttpRequestFailed(It.IsAny<HttpResponseMessage>(), It.IsAny<HttpRequestException>()));

        _basicApiService.ErrorMapper = errorMapper.Object;

        // Act
        var response = await _basicApiService.Update(request);

        // Assertions
        Assert.Null(response);
        errorMapper.Verify(x => x.HttpRequestFailed(It.IsAny<HttpResponseMessage>(), It.IsAny<HttpRequestException>()), Times.Once());
    }


    [Fact]
    public async Task Get_ShouldFail_WhenBasicApiReturnsTimeoutException()
    {

        _mockServer
         .Given(Request.Create().WithPath("/basic-api/data").UsingGet())
         .RespondWith(Response.Create()
                .WithDelay(TimeSpan.FromSeconds(30)));

        // Act
        var exception = await Record.ExceptionAsync(async () =>  await _basicApiService.GetDataAsync());

        // Assertions
        Assert.NotNull(exception);
        Assert.IsType<TimeoutException>(exception);
    }

    /// <summary>
    /// Desde WebHostFactory se agregan en el HttpContext los headers que se inyectan 
    /// en la llamada HttpClient
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Get_OK_WhenBasicApiSendHeadersRequired()
    {
        // Arrange
        _mockServer
            .Given(Request.Create().WithPath("/basic-api/data").UsingGet()
                .WithHeader("Channel", "006")
                .WithHeader("Request-Date", "2025-06-01T17:15:20.509-0400")
                .WithHeader("X-Correlation-Id", "c22abab4-d709-4d85-9e98-45657a0eec44"))
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new { id = 1, description = "Mocked Data" })));

        // Act
        var account = await _basicApiService.GetDataAsync();

        // Assert
        Assert.NotNull(account);
        Assert.Equal("Mocked Data", account.Description);

        // Verificar que tu aplicación realmente llamó al mock server
        var findEntries = _mockServer.FindLogEntries(
            Request.Create().WithPath("/basic-api/data").UsingGet()
        );
        Assert.Single(findEntries);
    }

}