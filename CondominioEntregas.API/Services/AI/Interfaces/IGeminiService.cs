namespace PortSafe.Services.AI.Interfaces
{
    public interface IGeminiService
    {
        Task<string> GenerateContentAsync(string prompt, double? temperature = null);
        Task<T?> GenerateStructuredResponseAsync<T>(string prompt, double? temperature = null) where T : class;
    }
}
