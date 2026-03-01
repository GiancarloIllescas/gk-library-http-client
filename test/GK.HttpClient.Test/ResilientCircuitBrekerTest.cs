using Polly.CircuitBreaker;
using System.Net;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace GK.Library.Http.Client.Test;

public class ResilientCircuitBrekerTest : IntegrationTestBase
{
    public ResilientCircuitBrekerTest(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetUserById_CircuitBreakerShouldOpenAfterThresholdAndRejectCalls()
    {
        // Configura WireMock para que responda con un error 500 para todas las llamadas
        _mockServer
            .Given(Request.Create().WithPath("/resilient-api/data").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.InternalServerError));

        // Act - Parte 1: Provocar fallos para abrir el circuito
        // Necesitamos al menos 'MinimumThroughput' llamadas en 'SamplingDuration'
        // y que el 50% (FailureRatio) de ellas fallen para que el circuito se abra.
        // Si FailureThreshold = 5, necesitamos 5 fallos consecutivos.
        // Si FailureRatio = 0.5 y MinimumThroughput = 10, necesitamos 5 fallos en 10 llamadas.
        // Para simplificar, haremos más fallos que el umbral de fallos consecutivos.
        int failuresToTriggerBreaker = 10; // Más que el FailureThreshold de 5

        for (int i = 0; i < failuresToTriggerBreaker; i++)
        {
            await Assert.ThrowsAsync<HttpRequestException>(() => _resilientCircuitBreakerApiService.GetDataAsync());
        }

        // Assert - Parte 1: Verificar que las llamadas iniciales fallaron y se registraron en WireMock
        int minimumThroughput = 10;
        var findEntriesPart1 = _mockServer.FindLogEntries(
                Request.Create().WithPath("/resilient-api/data").UsingGet()).ToList();
        Assert.Equal(minimumThroughput, findEntriesPart1.Count);

        // Act - Parte 2: Intentar una llamada cuando el circuito debería estar abierto
        // Esta llamada NO debería llegar a WireMock, sino ser rechazada inmediatamente por el Circuit Breaker.
        var exception = await Record.ExceptionAsync(async () => await _resilientCircuitBreakerApiService.GetDataAsync());

        // Assert - Parte 2: Verificar que se lanzó una CircuitBreakerRejectedException
        Assert.NotNull(exception);
        Assert.IsType<BrokenCircuitException>(exception);

        // Assert - Parte 3: Verificar que la última llamada NO llegó a WireMock
        // El número total de llamadas a WireMock debe ser el mismo que las llamadas que provocaron el fallo.
        // Si el circuito se abrió, la última llamada no debería haber pasado.
        var findEntriesPart3 = _mockServer.FindLogEntries(
                Request.Create().WithPath("/resilient-api/data").UsingGet()).ToList();
        Assert.Equal(minimumThroughput, findEntriesPart3.Count);
    }

}
