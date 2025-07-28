using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Yape.Http.Client.Infraestructure.Adapters.Http;
using Yape.Http.Client.Settings;

namespace Yape.Http.Client.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IHttpClientBuilder AddResilientHttpClient<TClient, TImplementation>(
        this IServiceCollection services,
        string clientName,
        IConfiguration configuration)
        where TClient : class
        where TImplementation : class, TClient
    {

        // 1. Configurar las opciones específicas de este cliente (BaseAddress, DefaultHeaders, etc.)
        services.Configure<ClientSettings>(configuration.GetSection($"HttpClients:{clientName}"));

        // 2. Obtener el nombre de la política de resiliencia para este cliente
        var clientSettings = configuration.GetSection($"HttpClients:{clientName}").Get<ClientSettings>();
        var resiliencePolicyConfigName = clientSettings?.ResiliencePolicyName;

        // 3. Registrar el cliente tipado
        var httpClientBuilder = services.AddHttpClient<TClient, TImplementation>(httpClient =>
        {
            // Configurar HttpClient (BaseAddress, DefaultTimeout, DefaultHeaders de ClientSettings)
            if (clientSettings != null)
            {
                if (!string.IsNullOrEmpty(clientSettings.BaseAddress))
                {
                    httpClient.BaseAddress = new Uri(clientSettings.BaseAddress);
                }
                if (clientSettings.DefaultTimeout != TimeSpan.Zero)
                {
                    httpClient.Timeout = clientSettings.DefaultTimeout;
                }
                if (clientSettings.DefaultHeaders != null)
                {
                    foreach (var header in clientSettings.DefaultHeaders)
                    {
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
            }
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5)); // Recomienda reciclar el handler


        // 4. Aplicar la política de resiliencia si se especifica un nombre de política
        if (!string.IsNullOrEmpty(resiliencePolicyConfigName))
        {
            httpClientBuilder.AddStandardResilienceHandler(options =>
            {
                // Obtener la sección de configuración para esta política específica
                var resilienceSection = configuration.GetSection($"ResiliencePolicies:{resiliencePolicyConfigName}");

                var configOptions = resilienceSection.Get<ResilienceConfigurationOptions>();

                if (configOptions != null)
                {
                    // Configurar Retry
                    //
                    if (configOptions.Retry == null)
                    {
                        // --- DESHABILITAR LA POLÍTICA DE REINTENTOS ---
                        // Configura ShouldHandle para que nunca reintente, sin importar el resultado o la excepción.
                        options.Retry.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                            .HandleResult(res => false) // Nunca reintentar por el resultado HTTP
                            .Handle<Exception>(ex => false); // Nunca reintentar por ninguna excepción
                    }
                    else
                    {
                        options.Retry.MaxRetryAttempts = configOptions.Retry.MaxRetries;
                        options.Retry.Delay = configOptions.Retry.Delay;
                        // Conversión de string a enum para BackoffType
                        if (Enum.TryParse<DelayBackoffType>(configOptions.Retry.BackoffType, true, out var backoffType))
                        {
                            options.Retry.BackoffType = backoffType;
                        }
                        // Opciones adicionales para tipos de backoff específicos si es necesario
                        // options.Retry.MaxDelay = TimeSpan.FromSeconds(configOptions.Retry.MaxDelaySeconds);
                        // options.Retry.Factor = configOptions.Retry.Factor;
                    }

                    // Configurar CircuitBreaker
                    //
                    if (configOptions.CircuitBreaker == null)
                    {
                        // --- DESHABILITAR LA POLÍTICA DE Circuit Breaker ---
                        options.CircuitBreaker.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                            .HandleResult(res => false) // Nunca reintentar por el resultado HTTP
                            .Handle<Exception>(ex => false); // Nunca reintentar por ninguna excepción
                    }
                    else
                    {
                        options.CircuitBreaker.FailureRatio = configOptions.CircuitBreaker.FailureRatio;
                        options.CircuitBreaker.MinimumThroughput = configOptions.CircuitBreaker.MinimumThroughput;
                        options.CircuitBreaker.SamplingDuration = configOptions.CircuitBreaker.SamplingDuration;
                        options.CircuitBreaker.BreakDuration = configOptions.CircuitBreaker.BreakDuration;
                    }

                    // Configurar Timeout
                    //
                    if (configOptions.Timeout != null)
                    {
                        options.AttemptTimeout.Timeout = configOptions.Timeout.Timeout;
                        if (configOptions.Timeout.TotalRequestTimeout != null)
                        {
                            options.TotalRequestTimeout.Timeout = configOptions.Timeout.TotalRequestTimeout.Value;
                        }
                    }
                }
            });
        }
        else
        {
            // Opcional: Si no se especifica una política nombrada, puedes aplicar una estándar con valores por defecto
            // httpClientBuilder.AddStandardResilienceHandler();
        }

        return httpClientBuilder;
    }

    // Sobrecarga para registrar solo el IResilientHttpClient sin Typed Client específico
    public static IHttpClientBuilder AddResilientHttpClient(
        this IServiceCollection services,
        string clientName,
        IConfiguration configuration)
    {
        return services.AddResilientHttpClient<IResilientHttpClient, ResilientHttpClient>(clientName, configuration);
    }
}



