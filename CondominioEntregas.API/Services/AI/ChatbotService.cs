using Mscc.GenerativeAI;
using Microsoft.EntityFrameworkCore;
using PortSafe.Data;
using PortSafe.Models;

namespace PortSafe.Services.AI
{
    public class ChatbotService
    {
        private readonly PortSafeContext _context;
        private readonly GoogleAI _googleAI;
        private readonly GenerativeModel _model;
        private readonly ILogger<ChatbotService> _logger;

        public ChatbotService(PortSafeContext context, string geminiApiKey, ILogger<ChatbotService> logger)
        {
            _context = context;
            _logger = logger;
            _googleAI = new GoogleAI(apiKey: geminiApiKey);
            _model = _googleAI.GenerativeModel(model: "gemini-pro");
        }

        public async Task<string> ProcessarMensagemAsync(string mensagemUsuario, string? telefoneWhatsApp = null)
        {
            try
            {
                // 1. Verificar se a mensagem é sobre entrega
                var prompt = $@"
Você é um assistente de entregas para um condomínio.
O usuário está perguntando sobre sua entrega.
Responda de forma amigável e objetiva.

Regras:
- Se for sobre entrega, armário ou senha, indique que você vai buscar as informações
- Se não for sobre entrega, peça educadamente que faça perguntas sobre entregas
- Seja breve e direto
- Use tom profissional mas amigável

Mensagem do usuário: '{mensagemUsuario}'

Responda em uma única frase curta.";

                var response = await _model.GenerateContent(prompt);
                var textoGemini = response?.Text?.Trim();

                // 2. Verificar se é uma pergunta sobre entrega
                var mensagemLower = mensagemUsuario.ToLower();
                bool isPerguntaEntrega = mensagemLower.Contains("entrega") ||
                                        mensagemLower.Contains("armario") ||
                                        mensagemLower.Contains("armário") ||
                                        mensagemLower.Contains("senha") ||
                                        mensagemLower.Contains("pacote") ||
                                        mensagemLower.Contains("encomenda");

                if (isPerguntaEntrega)
                {
                    // Buscar entregas pendentes
                    var query = _context.Entregas
                        .Include(e => e.Armario)
                        .Where(e => e.Status == StatusEntrega.Armazenada);

                    // Se tiver telefone, filtrar por ele
                    if (!string.IsNullOrEmpty(telefoneWhatsApp))
                    {
                        query = query.Where(e => e.TelefoneWhatsApp == telefoneWhatsApp);
                    }

                    var entregas = await query.OrderByDescending(e => e.DataHoraRegistro).ToListAsync();

                    if (entregas.Any())
                    {
                        if (entregas.Count == 1)
                        {
                            var entrega = entregas.First();
                            return $"📦 Sua entrega para {entrega.NomeDestinatario} está no **armário {entrega.Armario?.Numero}** com a senha **{entrega.SenhaAcesso}**. " +
                                   $"Registrada em {entrega.DataHoraRegistro:dd/MM/yyyy HH:mm}.";
                        }
                        else
                        {
                            var lista = string.Join("\n", entregas.Select((e, i) => 
                                $"{i + 1}. Armário {e.Armario?.Numero} - Senha {e.SenhaAcesso} - {e.NomeDestinatario}"));
                            return $"📦 Você tem {entregas.Count} entregas armazenadas:\n{lista}";
                        }
                    }
                    else
                    {
                        return "📭 Não encontrei nenhuma entrega armazenada no momento. Quando sua entrega chegar, você receberá uma notificação com os detalhes!";
                    }
                }

                // Resposta genérica do Gemini
                return textoGemini ?? "Olá! Como posso ajudar com informações sobre suas entregas?";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem do chatbot");
                return "Desculpe, ocorreu um erro ao processar sua mensagem. Tente novamente em instantes.";
            }
        }
    }
}
