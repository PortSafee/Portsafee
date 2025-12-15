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

        public ChatbotService(PortSafeContext context, string geminiApiKey, string modelName, ILogger<ChatbotService> logger)
        {
            _context = context;
            _logger = logger;
            _googleAI = new GoogleAI(apiKey: geminiApiKey);
            _model = _googleAI.GenerativeModel(model: modelName);
        }

        public async Task<string> ProcessarMensagemAsync(string mensagemUsuario, int? userId = null)
        {
            try
            {
                _logger.LogInformation("Processando mensagem do chatbot: {Mensagem} para usuário {UserId}", mensagemUsuario, userId ?? 0);
                
                // Buscar informações do usuário (Morador) se autenticado
                Morador? morador = null;
                if (userId.HasValue)
                {
                    morador = await _context.Moradores
                        .Include(m => m.Unidade)
                        .FirstOrDefaultAsync(m => m.Id == userId.Value);
                }
                
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

                _logger.LogInformation("Chamando API do Gemini...");
                var response = await _model.GenerateContent(prompt);
                var textoGemini = response?.Text?.Trim();
                _logger.LogInformation("Resposta do Gemini recebida: {Resposta}", textoGemini);

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
                    // Se não estiver autenticado, pedir para fazer login
                    if (morador == null)
                    {
                        return "Para consultar suas entregas, por favor faça login no sistema. 🔐";
                    }
                    
                    // Buscar entregas pendentes APENAS DO MORADOR LOGADO
                    var entregas = await _context.Entregas
                        .Include(e => e.Armario)
                        .Where(e => e.Status == StatusEntrega.Armazenada && 
                                   (e.NomeDestinatario.ToLower().Contains(morador.Nome.ToLower()) ||
                                    e.TelefoneWhatsApp == morador.Telefone))
                        .OrderByDescending(e => e.DataHoraRegistro)
                        .ToListAsync();

                    if (entregas.Any())
                    {
                        if (entregas.Count == 1)
                        {
                            var entrega = entregas.First();
                            return $"📦 Olá {morador.Nome}! Sua entrega está no **armário {entrega.Armario?.Numero}** com a senha **{entrega.SenhaAcesso}**. " +
                                   $"Registrada em {entrega.DataHoraRegistro:dd/MM/yyyy HH:mm}.";
                        }
                        else
                        {
                            var lista = string.Join("\n", entregas.Select((e, i) => 
                                $"{i + 1}. Armário {e.Armario?.Numero} - Senha {e.SenhaAcesso}"));
                            return $"📦 Olá {morador.Nome}! Você tem {entregas.Count} entregas armazenadas:\n{lista}";
                        }
                    }
                    else
                    {
                        return $"📭 Olá {morador.Nome}! Não encontrei nenhuma entrega sua armazenada no momento. Quando sua entrega chegar, você receberá uma notificação!";
                    }
                }

                // Resposta genérica do Gemini
                return textoGemini ?? "Olá! Como posso ajudar com informações sobre suas entregas?";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem do chatbot. Mensagem: {Message}, StackTrace: {StackTrace}", 
                    ex.Message, ex.StackTrace);
                return $"Desculpe, ocorreu um erro ao processar sua mensagem: {ex.Message}";
            }
        }
    }
}
