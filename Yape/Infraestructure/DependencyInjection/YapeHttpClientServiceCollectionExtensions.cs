using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace CustomHttpClient.Yape.Infraestructure.DependencyInjection;

public static class YapeHttpClientServiceCollectionExtensions
{
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int errorThreshold, int breakTimeout)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(errorThreshold, TimeSpan.FromSeconds(breakTimeout));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int errorRetries)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => {
                Console.WriteLine(msg);
                return msg.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable;
            })
            .WaitAndRetryAsync(errorRetries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    public static IServiceCollection AddRetryableHttpClient<TClient, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
        this IServiceCollection services, 
        IConfiguration configuration,
        string clientName
    ) 
        where TClient : class
        where TImplementation : class, TClient
    {
        var errorThreshold = Int32.Parse(configuration.GetSection(string.Format("Http:Client:{0}:CircuitBreaker:ErrorThreshold", clientName)).Value);
        var breakTimeout = Int32.Parse(configuration.GetSection(string.Format("Http:Client:{0}:CircuitBreaker:BreakTimeout", clientName)).Value);
        var errorRetries = Int32.Parse(configuration.GetSection(string.Format("Http:Client:{0}:CircuitBreaker:ErrorRetries", clientName)).Value);
        var url = configuration.GetSection(string.Format("Http:Client:{0}:Url", clientName)).Value;

        var circuitBreakerPolicy = GetCircuitBreakerPolicy(errorThreshold, breakTimeout);
        var retryPolicy = GetRetryPolicy(errorRetries);

        services.AddHttpClient<TClient, TImplementation>(client => {
            client.BaseAddress = new Uri(url);
        }).AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);

        return services;
    }
}
