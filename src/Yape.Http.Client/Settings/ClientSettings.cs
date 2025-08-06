namespace Yape.Library.Http.Client.Settings
{
    public class ClientSettings
    {
        public string BaseAddress { get; set; } = string.Empty;
        public Dictionary<string, string>? DefaultHeaders { get; set; }
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public string? ResiliencePolicyName { get; set; } // Nombre de la política de resiliencia a usar para este cliente 
    }

}
