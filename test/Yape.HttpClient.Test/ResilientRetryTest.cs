using System.Net;
using System.Text.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Yape.Library.Http.Client.Test.Entities;

namespace Yape.Library.Http.Client.Test;

public class ResilientRetryTest : IntegrationTestBase
{
    public ResilientRetryTest(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }


    [Fact]
    public async Task Get_ShouldFail_WhenBasicApiReturnsTimeoutException()
    {

        _mockServer
         .Given(Request.Create().WithPath("/resilient-api/data").UsingGet())
         .RespondWith(Response.Create()
                .WithDelay(TimeSpan.FromSeconds(30)));

        // Act
        var exception = await Record.ExceptionAsync(async () => await _resilientBasicApiService.GetDataAsync());

        // Assertions
        Assert.NotNull(exception);
        Assert.IsType<TimeoutException>(exception);
    }

    [Fact]
    public async Task Get_ShouldRetryTwo_WhenFinalReturnData()
    {
        var response = new MockEntity() { Id = 1, Name = "Name Mock", Description = "Mocked Data" };

        string scenarioName = "RetryScenario-TwoRetry";

        // 1. Configurar WireMock con Escenarios para simular fallos transitorios y eventual éxito

        // MAPPING 1: Intento inicial (estado STARTED)
        // Responde con 503 y cambia el estado a "FirstRetry"
        _mockServer
            .Given(Request.Create().WithPath("/resilient-api/data").UsingGet())
                .InScenario(scenarioName)
                .WillSetStateTo("FirstRetry") // Cambia el estado del escenario
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.ServiceUnavailable)); // Primer intento falla

        // MAPPING 2: Primer reintento (cuando el estado es "FirstRetry")
        // Responde con 500 y cambia el estado a "SecondRetry"
        _mockServer
            .Given(Request.Create().WithPath("/resilient-api/data").UsingGet())
                .InScenario(scenarioName)
                .WhenStateIs("FirstRetry") // Solo coincide si el estado es "FirstRetry"
                .WillSetStateTo("SecondRetry") // Cambia el estado
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.InternalServerError)); // Segundo intento (primer reintento) falla

        // MAPPING 3: Segundo reintento (cuando el estado es "SecondRetry")
        // Responde con 200 y se mantiene en "SecondRetry" (o podrías no cambiar el estado si es el final)
        _mockServer
            .Given(Request.Create().WithPath("/resilient-api/data").UsingGet())
                .InScenario(scenarioName)
                .WhenStateIs("SecondRetry") // Solo coincide si el estado es "SecondRetry"
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(response))); // Tercer intento (segundo reintento) exitoso


        // Act
        var mockentity = await _resilientRetryApiService.GetDataAsync();

        // Assertions
        Assert.NotNull(mockentity);
        Assert.Equal(1, mockentity.Id);

        // Verificar que tu aplicación realmente llamó al mock server
        var findEntries = _mockServer.FindLogEntries(
            Request.Create().WithPath("/resilient-api/data").UsingGet()).ToList();
        //Assert.Equal(3, findEntries.Count);
        Assert.Equal((int)HttpStatusCode.ServiceUnavailable, findEntries[0].ResponseMessage.StatusCode);
        Assert.Equal((int)HttpStatusCode.InternalServerError, findEntries[1].ResponseMessage.StatusCode);
    }

    [Fact]
    public async Task Get_ShouldRetryTwo_WhenAllInvokeFail()
    {
        string scenarioName = "RetryScenario-AllInvokeFail";

        // 1. Configurar WireMock con Escenarios para simular fallos

        // MAPPING 1: Intento inicial (estado STARTED)
        // Responde con 503 y cambia el estado a "FirstRetry"
        _mockServer
            .Given(Request.Create().WithPath("/resilient-api/data").UsingGet())
                .InScenario(scenarioName)
                .WillSetStateTo("FirstRetry")
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.ServiceUnavailable)); // Primer intento falla

        // MAPPING 2: Primer reintento (cuando el estado es "FirstRetry")
        // Responde con 500 y cambia el estado a "SecondRetry"
        _mockServer
            .Given(Request.Create().WithPath("/resilient-api/data").UsingGet())
                .InScenario(scenarioName)
                .WhenStateIs("FirstRetry")
                .WillSetStateTo("SecondRetry") 
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.InternalServerError)); // Segundo intento (primer reintento) falla

        // MAPPING 3: Segundo reintento
        _mockServer
            .Given(Request.Create().WithPath("/resilient-api/data").UsingGet())
                .InScenario(scenarioName)
                .WhenStateIs("SecondRetry")
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.ServiceUnavailable)); // Tercer intento (segundo reintento) fallido

        // Act
        var exception = await Record.ExceptionAsync(async () => await _resilientRetryApiService.GetDataAsync());

        // Assertions
        Assert.NotNull(exception);
        Assert.IsType<HttpRequestException>(exception);

        // Verificar que tu aplicación realmente llamó al mock server
        var findEntries = _mockServer.FindLogEntries(
            Request.Create().WithPath("/resilient-api/data").UsingGet()
        ).ToList();
        //Assert.Equal(3, findEntries.Count);
        Assert.Equal((int)HttpStatusCode.ServiceUnavailable, findEntries[0].ResponseMessage.StatusCode);
        Assert.Equal((int)HttpStatusCode.InternalServerError, findEntries[1].ResponseMessage.StatusCode);
        Assert.Equal((int)HttpStatusCode.ServiceUnavailable, findEntries[2].ResponseMessage.StatusCode);
    }
}

