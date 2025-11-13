namespace PortSafe.Services.AI.Models
{
    public class AIConfiguration
    {
        public GeminiSettings Gemini { get; set; } = new();
    }

    public class GeminiSettings
    {
        public string? ApiKey { get; set; }
        public string Model { get; set; } = "gemini-1.5-pro";
        public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
        public double Temperature { get; set; } = 0.3;
        public int MaxTokens { get; set; } = 1024;
    }
}
