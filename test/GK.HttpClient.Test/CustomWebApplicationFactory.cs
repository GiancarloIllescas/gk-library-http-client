using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using WireMock.Server;
using WireMock.Settings;
using GK.Library.Http.Client.Domain.Port;
using GK.Library.Http.Client.Extensions;
using GK.Library.Http.Client.Test.Services;

namespace GK.Library.Http.Client.Test;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    public WireMockServer WireMockServer { get; private set; }

    public CustomWebApplicationFactory()
    {
        // 1. Iniciar WireMock.NET Server
        // Puedes especificar un puerto fijo o dejar que WireMock elija uno libre (recomendado para CI/CD)
        WireMockServer = WireMockServer.Start(new WireMockServerSettings
        {
            Port = 0, // Usar el puerto 0 para que WireMock asigne un puerto libre
            UseSSL = true
        });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, conf) =>
        {
            // Limpiar cualquier configuración existente para asegurar un entorno limpio
            conf.Sources.Clear();

            var testBasePath = Directory.GetCurrentDirectory();
            conf.AddJsonFile(Path.Combine(testBasePath, $"appsettings.{context.HostingEnvironment.EnvironmentName}.json"), optional: true);

            // Sobrescribo el valor leido en el appsetting reemplazando las url dinamicas de wiremock
            //
            var inMemoryOverrides = new Dictionary<string, string?>
                {
                    {"HttpClients:BasicApiClient:BaseAddress", WireMockServer.Url},
                    {"HttpClients:ResilientBasicApiClient:BaseAddress", WireMockServer.Url},
                    {"HttpClients:ResilientRetryApiClient:BaseAddress", WireMockServer.Url},
                    {"HttpClients:ResilientCircuitBrekerApiClient:BaseAddress", WireMockServer.Url},
                };
            conf.AddInMemoryCollection(inMemoryOverrides);

        });

        builder.ConfigureServices(services =>
        {
            var serviceProvider = services.BuildServiceProvider();

            var configuration = serviceProvider.GetService<IConfiguration>();

            // Add HttpContext Headers
            //
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Headers["Channel"] = "006";
            context.Request.Headers["Request-Date"] = "2025-06-01T17:15:20.509-0400";
            context.Request.Headers["X-Correlation-Id"] = "c22abab4-d709-4d85-9e98-45657a0eec44";

            mockHttpContextAccessor
                .Setup(x => x.HttpContext)
                .Returns(context);

            services.AddSingleton<IHttpContextAccessor>(x=> mockHttpContextAccessor.Object);


            var basicApiClientBuilder = services.AddResilientHttpClient<IBasicApiService, BasicApiService>("BasicApiClient", configuration);
            
            var basicApiCustomErrorService = services.AddResilientHttpClient<IBasicApiCustomErrorService, BasicApiCustomErrorService>("BasicApiClient", configuration);

            var resilientBasicApiClientBuilder = services.AddResilientHttpClient<IResilientBasicApiService, ResilientBasicApiService>("ResilientBasicApiClient", configuration);
            
            var resilientRetryApiClientBuilder = services.AddResilientHttpClient<IResilientRetryApiService, ResilientRetryApiService>("ResilientRetryApiClient", configuration);
            
            var resilientCircuitBreakerApiClientBuilder = services.AddResilientHttpClient<IResilientCircuitBreakerApiService, ResilientCircuitBreakerApiService>("ResilientCircuitBrekerApiClient", configuration);


            // Asigna el handler para aceptar el certificado que requiere una llamada https
            //
            foreach (var item in new IHttpClientBuilder[] { basicApiClientBuilder,
                                                            basicApiCustomErrorService,
                                                            resilientBasicApiClientBuilder, 
                                                            resilientRetryApiClientBuilder,
                                                            resilientCircuitBreakerApiClientBuilder})
            {
                item.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { 
                    ServerCertificateCustomValidationCallback = (m, cert, chain, sslPolicyErrors) => true 
                });
            }

        });

        

        // Puedes ejecutar código después de configurar los servicios pero antes de que se construya el host
        builder.UseEnvironment("Testing"); // "Develop" o "Testing" si tienes un ambiente específico
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Crea el host primero
        var host = base.CreateHost(builder);
        return host;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            WireMockServer.Stop(); // Detener el servidor WireMock.NET al finalizar las pruebas
            WireMockServer.Dispose();
        }
        base.Dispose(disposing);
    }
}
