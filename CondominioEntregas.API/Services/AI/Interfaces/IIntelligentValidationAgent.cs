using PortSafe.Services.AI.Models;

namespace PortSafe.Services.AI.Interfaces
{
    public interface IIntelligentValidationAgent
    {
        Task<ValidationResult> ValidateDestinatarioAsync(string nomeInput, string cepInput);
    }
}
