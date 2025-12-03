using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortSafe.Data;
using PortSafe.DTOs;
using PortSafe.Models;
using PortSafe.Services;
using PortSafe.Services.AI.Interfaces;
using System.Text.RegularExpressions;

namespace PortSafe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntregaController : ControllerBase
    {
        private readonly PortSafeContext _context;
        private readonly GmailService _emailService;
        private readonly IIntelligentValidationAgent _validationAgent;
        private readonly IConfiguration _configuration;

        public EntregaController(
            PortSafeContext context,
            GmailService emailService,
            IIntelligentValidationAgent validationAgent,
            IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _validationAgent = validationAgent;
            _configuration = configuration;
        }

        private static string NormalizarCEP(string cep) => Regex.Replace(cep ?? "", @"[^\d]", "");
        private static string NormalizarNome(string nome) => (nome ?? "").Trim().ToLower();

        private static string GerarCodigo(int min, int max, string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            var random = Random.Shared;
            return new string(Enumerable.Repeat(chars, max - min == 9999 ? 4 : 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static string GerarSenhaAcesso() => GerarCodigo(1000, 9999, "0123456789");
        private static string GerarCodigoEntrega() => GerarCodigo(0, 1, "ABCDEF");
        private static string GerarTokenValidacao() => GerarCodigo(0, 1); // 16 chars

        // ========================================
        // ENDPOINTS
        // ========================================

        [HttpPost("ValidarDestinatario")]
        public async Task<ActionResult<ValidacaoDestinatarioResponseDTO>> ValidarDestinatario([FromBody] ValidarDestinatarioRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var cep = NormalizarCEP(request.CEP);
            var nome = NormalizarNome(request.NomeDestinatario);

            var unidade = _context.UnidadesCasa
                .Include(u => u.Morador)
                .AsEnumerable()
                .FirstOrDefault(u => u.Morador != null &&
                    NormalizarCEP(u.CEP) == cep &&
                    NormalizarNome(u.Morador.Nome) == nome);

            return unidade != null
                ? CriarRespostaSucesso(unidade, unidade.Morador!, isIA: false)
                : CriarRespostaNaoEncontrado();
        }

        [HttpPost("ValidarDestinatarioComIA")]
        public async Task<ActionResult<ValidarDestinatarioComIAResponseDTO>> ValidarDestinatarioComIA([FromBody] ValidarDestinatarioRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var resultadoIA = await _validationAgent.ValidateDestinatarioAsync(request.NomeDestinatario, request.CEP);

            if (resultadoIA.IsValid && resultadoIA.UnidadeId.HasValue)
            {
                var unidade = await _context.UnidadesCasa
                    .Include(u => u.Morador)
                    .FirstOrDefaultAsync(u => u.Id == resultadoIA.UnidadeId.Value);

                if (unidade?.Morador != null)
                {
                    return Ok(new ValidarDestinatarioComIAResponseDTO
                    {
                        Validado = true,
                        ConfiancaIA = resultadoIA.ConfidenceScore,
                        Mensagem = $"Destinatário validado pela IA! (Confiança: {resultadoIA.ConfidenceScore:F0}%) - {resultadoIA.Reason}",
                        TipoResultado = TipoResultadoValidacao.Sucesso,
                        DadosEncontrados = MapearDadosDestinatario(unidade, unidade.Morador),
                        Sugestoes = null,
                        PodeRetentar = false,
                        PodeAcionarPortaria = false,
                        TokenValidacao = GerarTokenValidacao(),
                        ValidacaoId = unidade.Id
                    });
                }
            }

            return Ok(new ValidarDestinatarioComIAResponseDTO
            {
                Validado = false,
                ConfiancaIA = resultadoIA.ConfidenceScore,
                Mensagem = resultadoIA.Suggestions.Any()
                    ? $"Destinatário não encontrado com certeza. Veja as sugestões abaixo: {resultadoIA.Reason}"
                    : $"Destinatário não encontrado no sistema. {resultadoIA.Reason}",
                TipoResultado = resultadoIA.Suggestions.Any() ? TipoResultadoValidacao.MultiplasCombinacoes : TipoResultadoValidacao.NaoEncontrado,
                DadosEncontrados = null,
                Sugestoes = resultadoIA.Suggestions,
                PodeRetentar = true,
                PodeAcionarPortaria = true,
                TokenValidacao = null,
                ValidacaoId = null
            });
        }

        [HttpPost("SolicitarArmario")]
        public async Task<ActionResult<SolicitarArmarioResponseDTO>> SolicitarArmario([FromBody] SolicitarArmarioRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var armario = await _context.Armarios
                .FirstOrDefaultAsync(a => a.Status == StatusArmario.Disponivel);

            if (armario == null)
                return Ok(new SolicitarArmarioResponseDTO { Sucesso = false, Mensagem = "Nenhum armário disponível no momento. Por favor, acione a portaria." });

            var unidade = await _context.UnidadesCasa
                .Include(u => u.Morador)
                .FirstOrDefaultAsync(u => u.Id == request.UnidadeId && u.Morador != null);

            if (unidade == null)
                return BadRequest(new SolicitarArmarioResponseDTO { Sucesso = false, Mensagem = "Unidade não encontrada." });

            var entrega = new Entrega
            {
                NomeDestinatario = unidade.Morador.Nome,
                NumeroCasa = unidade.NumeroCasa.ToString(),
                EnderecoGerado = $"{unidade.Rua}, Casa {unidade.NumeroCasa}",
                ArmariumId = armario.Id,
                CodigoEntrega = GerarCodigoEntrega(),
                SenhaAcesso = GerarSenhaAcesso(),
                DataHoraRegistro = DateTime.UtcNow,
                Status = StatusEntrega.AguardandoArmario,
                TelefoneWhatsApp = unidade.Morador.Telefone,
                MensagemEnviada = false
            };

            armario.Status = StatusArmario.Ocupado;
            armario.UltimaAbertura = DateTime.UtcNow;

            _context.Entregas.Add(entrega);
            await _context.SaveChangesAsync();

            return Ok(new SolicitarArmarioResponseDTO
            {
                Sucesso = true,
                Mensagem = $"Armário {armario.Numero} liberado! Deposite o pacote e feche a porta.",
                NumeroArmario = int.Parse(armario.Numero ?? "0"),
                CodigoEntrega = entrega.CodigoEntrega,
                EntregaId = entrega.Id,
                LimiteDeposito = DateTime.UtcNow.AddMinutes(5)
            });
        }

        [HttpPost("ConfirmarFechamento")]
        public async Task<ActionResult<ConfirmarFechamentoResponseDTO>> ConfirmarFechamento([FromBody] ConfirmarFechamentoRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entrega = await _context.Entregas
                .Include(e => e.Armario)
                .FirstOrDefaultAsync(e => e.Id == request.EntregaId);

            if (entrega?.Armario == null)
                return NotFound(new ConfirmarFechamentoResponseDTO { Sucesso = false, Mensagem = "Entrega ou armário não encontrado." });

            entrega.Status = StatusEntrega.Armazenada;
            entrega.Armario.Status = StatusArmario.Disponivel;
            entrega.Armario.UltimoFechamento = DateTime.UtcNow;

            bool notificacaoEnviada = await TentarEnviarNotificacaoAsync(entrega.NomeDestinatario, entrega.Armario.Numero!, entrega.SenhaAcesso!, entrega.CodigoEntrega!);

            if (notificacaoEnviada)
            {
                entrega.MensagemEnviada = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new ConfirmarFechamentoResponseDTO
            {
                Sucesso = true,
                Mensagem = "Entrega confirmada com sucesso! Morador será notificado.",
                CodigoEntrega = entrega.CodigoEntrega,
                SenhaAcesso = entrega.SenhaAcesso,
                DataHoraEntrega = entrega.DataHoraRegistro,
                NotificacaoEnviada = notificacaoEnviada
            });
        }

        [HttpPost("AcionarPortaria")]
        public async Task<ActionResult<AcionarPortariaResponseDTO>> AcionarPortaria([FromBody] AcionarPortariaRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entrega = new Entrega
            {
                NomeDestinatario = request.NomeDestinatario,
                EnderecoGerado = $"CEP: {request.CEP}",
                DataHoraRegistro = DateTime.UtcNow,
                Status = StatusEntrega.RedirecionadoPortaria,
                MensagemEnviada = false
            };

            _context.Entregas.Add(entrega);
            await _context.SaveChangesAsync();

            return Ok(new AcionarPortariaResponseDTO
            {
                Sucesso = true,
                Mensagem = "Portaria acionada com sucesso! Aguarde o contato.",
                ProtocoloAtendimento = entrega.Id,
                DataHoraAcionamento = entrega.DataHoraRegistro
            });
        }

        // ========================================
        // MÉTODOS PRIVADOS REUTILIZÁVEIS
        // ========================================

        private ValidacaoDestinatarioResponseDTO CriarRespostaSucesso(UnidadeCasa unidade, Morador morador, bool isIA = false)
        {
            return new ValidacaoDestinatarioResponseDTO
            {
                Validado = true,
                Mensagem = "Destinatário validado com sucesso! Pode prosseguir com a entrega.",
                TipoResultado = TipoResultadoValidacao.Sucesso,
                DadosEncontrados = MapearDadosDestinatario(unidade, morador),
                PodeRetentar = false,
                PodeAcionarPortaria = false,
                TokenValidacao = GerarTokenValidacao(),
                ValidacaoId = unidade.Id
            };
        }

        private ValidacaoDestinatarioResponseDTO CriarRespostaNaoEncontrado()
        {
            return new ValidacaoDestinatarioResponseDTO
            {
                Validado = false,
                Mensagem = "Destinatário não encontrado no sistema. Por favor, acione a portaria.",
                TipoResultado = TipoResultadoValidacao.NaoEncontrado,
                DadosEncontrados = null,
                PodeRetentar = true,
                PodeAcionarPortaria = true,
                TokenValidacao = null,
                ValidacaoId = null
            };
        }

        private DadosDestinatarioDTO MapearDadosDestinatario(UnidadeCasa unidade, Morador morador)
        {
            return new DadosDestinatarioDTO
            {
                NomeMorador = morador.Nome,
                TelefoneWhatsApp = morador.Telefone,
                TipoUnidade = "Casa",
                Endereco = $"{unidade.Rua}, Casa {unidade.NumeroCasa}",
                CEP = unidade.CEP,
                UnidadeId = unidade.Id,
                MoradorId = morador.Id
            };
        }

        private async Task<bool> TentarEnviarNotificacaoAsync(string nomeDestinatario, string numeroArmario, string senha, string codigo)
        {
            if (_configuration["Gmail:Email"] == "seu-email@gmail.com" || string.IsNullOrEmpty(_configuration["Gmail:Email"]))
            {
                Console.WriteLine("Email não configurado - notificação desabilitada");
                return false;
            }

            // Busca em Casas
            var moradorCasa = await _context.UnidadesCasa
                .Include(u => u.Morador)
                .FirstOrDefaultAsync(u => u.Morador != null && u.Morador.Nome == nomeDestinatario);

            if (moradorCasa?.Morador != null && !string.IsNullOrEmpty(moradorCasa.Morador.Email))
            {
                await _emailService.EnviarEmailEntregaArmario(moradorCasa.Morador.Nome!, moradorCasa.Morador.Email, numeroArmario, senha, codigo);
                return true;
            }

            // Busca em Apartamentos
            var moradorApto = await _context.UnidadesApartamento
                .Include(u => u.Morador)
                .FirstOrDefaultAsync(u => u.Morador != null && u.Morador.Nome == nomeDestinatario);

            if (moradorApto?.Morador != null && !string.IsNullOrEmpty(moradorApto.Morador.Email))
            {
                await _emailService.EnviarEmailEntregaArmario(moradorApto.Morador.Nome!, moradorApto.Morador.Email, numeroArmario, senha, codigo);
                return true;
            }

            return false;
        }
    }
}