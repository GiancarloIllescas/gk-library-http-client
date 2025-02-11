using System;
using CustomHttpClient.Yape.Infraestructure.Adapters.Http.ExceptionHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace CustomHttpClient.Yape.Infraestructure.DependencyInjection;

public static class YapeExceptionHandlingServiceCollectionExtensions
{
    public static IServiceCollection AddYapeExceptionHandling(
        this IServiceCollection services
    )
    {
        services.AddExceptionHandler<YapeExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
