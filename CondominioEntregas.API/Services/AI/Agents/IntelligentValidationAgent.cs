using Microsoft.EntityFrameworkCore;
using PortSafe.Data;
using PortSafe.Services.AI.Interfaces;
using PortSafe.Services.AI.Models;
using System.Text.Json;

namespace PortSafe.Services.AI.Agents
{
    public class IntelligentValidationAgent : IIntelligentValidationAgent
    {
        private readonly IGeminiService _geminiService;
        private readonly PortSafeContext _context;
        private readonly ILogger<IntelligentValidationAgent> _logger;

        public IntelligentValidationAgent(
            IGeminiService geminiService,
            PortSafeContext context,
            ILogger<IntelligentValidationAgent> logger)
        {
            _geminiService = geminiService;
            _context = context;
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateDestinatarioAsync(string nomeInput, string cepInput)
        {
            try
            {
                // 1. Buscar todos os moradores do banco
                var unidadesCasa = await _context.UnidadesCasa
                    .Include(u => u.Morador)
                    .Where(u => u.Morador != null)
                    .Select(u => new
                    {
                        u.Id,
                        NomeMorador = u.Morador!.Nome ?? "",
                        CEP = u.CEP ?? "",
                        Endereco = $"{u.Rua}, Casa {u.NumeroCasa}",
                        Telefone = u.Morador.Telefone ?? ""
                    })
                    .ToListAsync();

                var unidadesApartamento = await _context.UnidadesApartamento
                    .Include(u => u.Morador)
                    .Where(u => u.Morador != null)
                    .Select(u => new
                    {
                        u.Id,
                        NomeMorador = u.Morador!.Nome ?? "",
                        CEP = "", // Apartamentos não têm CEP individual
                        Endereco = $" Bloco {u.Torre}, Apto {u.NumeroApartamento}",
                        Telefone = u.Morador.Telefone ?? ""
                    })
                    .ToListAsync();

                var todasUnidades = unidadesCasa.Concat(unidadesApartamento).ToList();

                if (!todasUnidades.Any())
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ConfidenceScore = 0,
                        Reason = "Nenhum morador cadastrado no sistema"
                    };
                }

                // 2. Criar prompt para o Gemini
                var prompt = $@"Você é um agente de IA especializado em validação de destinatários de entregas em condomínios.

**TAREFA:** Analisar os dados de entrada e encontrar o morador correspondente no banco de dados.

**DADOS DE ENTRADA:**
- Nome informado: {nomeInput}
- CEP informado: {cepInput}

**MORADORES CADASTRADOS:**
{JsonSerializer.Serialize(todasUnidades, new JsonSerializerOptions { WriteIndented = true })}

**INSTRUÇÕES:**
1. Analise o nome informado considerando:
   - Erros de digitação comuns
   - Nomes parciais (ex: ""João"" pode ser ""João da Silva"")
   - Apelidos comuns (ex: ""Zé"" para ""José"")
   - Variações de acentuação (ex: ""Jose"" vs ""José"")
   - Ordem invertida (ex: ""Silva João"" vs ""João Silva"")

2. Analise o CEP considerando:
   - CEP formatado (00000-000) vs não formatado (00000000)

3. Calcule um score de confiança (0-100):
   - 90-100: Match exato ou quase exato
   - 70-89: Match provável (nome similar + CEP correto)
   - 50-69: Match possível (requer confirmação)
   - 0-49: Sem match confiável

**RESPONDA APENAS COM UM JSON VÁLIDO neste formato:**
{{
  ""isValid"": boolean,
  ""confidenceScore"": number (0-100),
  ""matchedName"": ""nome exato do banco"" ou null,
  ""matchedCEP"": ""CEP exato do banco"" ou null,
  ""unidadeId"": number ou null,
  ""reason"": ""explicação da decisão"",
  ""suggestions"": [
    {{
      ""nomeMorador"": ""nome"",
      ""cep"": ""CEP"",
      ""endereco"": ""endereço completo"",
      ""unidadeId"": number,
      ""similarityScore"": number (0-100),
      ""reason"": ""por que sugeri este""
    }}
  ]
}}

**CRITÉRIOS DE VALIDAÇÃO:**
- isValid = true se confidenceScore >= 70
- Se confidenceScore < 70, retorne suggestions
- Seja rigoroso mas tolerante a erros humanos comuns";

                // 3. Chamar Gemini para análise
                var geminiResponse = await _geminiService.GenerateStructuredResponseAsync<ValidationResult>(prompt, temperature: 0.2);

                if (geminiResponse == null)
                {
                    throw new InvalidOperationException("Gemini retornou resposta nula");
                }

                _logger.LogInformation($"Validação IA: {nomeInput} / {cepInput} -> Confidence: {geminiResponse.ConfidenceScore}%, Valid: {geminiResponse.IsValid}");

                return geminiResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar destinatário com IA");
                
                // Fallback: retornar resultado indicando erro
                return new ValidationResult
                {
                    IsValid = false,
                    ConfidenceScore = 0,
                    Reason = $"Erro ao processar validação: {ex.Message}"
                };
            }
        }
    }
}
