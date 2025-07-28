namespace Yape.Http.Client.Settings
{
    // Clase que contendrá la configuración para una política de resiliencia nombrada
    // ej. "ResiliencePolicies:MyApiPolicy"
    public class ResilienceConfigurationOptions
    {
        public RetryOptions? Retry { get; set; }  //= new RetryOptions();
        public CircuitBreakerOptions? CircuitBreaker { get; set; } //= new CircuitBreakerOptions();
        public TimeoutOptions? Timeout { get; set; } //= new TimeoutOptions();
        // Puedes añadir aquí otros patrones si los usas
        // public FallbackOptions Fallback { get; set; } = new FallbackOptions();
        // public RateLimiterOptions RateLimiter { get; set; } = new RateLimiterOptions();
    }

    // Clases para las opciones del patrón de Reintento
    public class RetryOptions
    {
        public int MaxRetries { get; set; } = 3;
        public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(1);
        public string BackoffType { get; set; } = "Exponential"; // Mapea a Polly.Contrib.WaitAndRetry.BackoffType
        public int MaxDelaySeconds { get; set; } = 60; // Para Exponential, un límite
        public double Factor { get; set; } = 2; // Para Exponential
    }

    // Clases para las opciones del patrón de Circuit Breaker
    public class CircuitBreakerOptions
    {
        public double FailureRatio { get; set; } = 0.1; // 10% de fallas
        public int MinimumThroughput { get; set; } = 10; // Mínimo de 10 solicitudes para calcular el ratio
        public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan BreakDuration { get; set; } = TimeSpan.FromSeconds(5);
    }

    // Clases para las opciones del patrón de Timeout
    public class TimeoutOptions
    {
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan? TotalRequestTimeout { get; set; } = null;
    }

    // Puedes añadir clases similares para FallbackOptions, RateLimiterOptions, etc.,
    // si tu appsettings.json los incluye y necesitas configurarlos.

}
