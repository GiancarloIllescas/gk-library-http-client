using Microsoft.Extensions.DependencyInjection;
using WireMock.Server;
using Yape.Library.Http.Client.Infraestructure.Adapters.Http;
using Yape.Library.Http.Client.Test.Services;

namespace Yape.Library.Http.Client.Test;

// IClassFixture asegura que una única instancia de CustomWebApplicationFactory
// se comparta entre todas las pruebas en la misma clase de prueba.
public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory<Program> _factory;
    protected readonly WireMockServer _mockServer;

    protected readonly IResilientHttpClient? _client;
    protected readonly IBasicApiService? _basicApiService;
    protected readonly IBasicApiCustomErrorService? _basicApiCustomErrorService;
    protected readonly IResilientBasicApiService? _resilientBasicApiService;
    protected readonly IResilientRetryApiService? _resilientRetryApiService;
    protected readonly IResilientCircuitBreakerApiService? _resilientCircuitBreakerApiService;

    public IntegrationTestBase(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _mockServer = _factory.WireMockServer;

        var services = factory.Services;
        _client = services.GetService<IResilientHttpClient>();
        _basicApiService = services.GetService<IBasicApiService>();
        _basicApiCustomErrorService = services.GetService<IBasicApiCustomErrorService>();
        _resilientBasicApiService = services.GetService<IResilientBasicApiService>();
        _resilientRetryApiService = services.GetService<IResilientRetryApiService>();
        _resilientCircuitBreakerApiService = services.GetService<IResilientCircuitBreakerApiService>();
        
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public Task InitializeAsync()
    {
        _mockServer.Reset(); // Limpia el historial y los mappings antes de cada prueba
        return Task.CompletedTask;
    }

    // Aquí puedes añadir métodos de ayuda comunes para tus pruebas
}

