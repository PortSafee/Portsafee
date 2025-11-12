using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortSafe.Data;
using PortSafe.DTOs;
using PortSafe.Models;
using PortSafe.Services;
using System.Text.RegularExpressions;

namespace PortSafe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntregaController : ControllerBase
    {
        private readonly PortSafeContext _context;
        private readonly GmailService _emailService;

        public EntregaController(PortSafeContext context, GmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        /// <summary>
        /// Valida os dados do destinatário (Nome e CEP) antes de liberar o armário
        /// </summary>
        [HttpPost("ValidarDestinatario")]
        public async Task<ActionResult<ValidacaoDestinatarioResponseDTO>> ValidarDestinatario(
            [FromBody] ValidarDestinatarioRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var cepNormalizado = NormalizarCEP(request.CEP);
                var nomeNormalizado = NormalizarNome(request.NomeDestinatario);

                var unidadesCasa = await _context.UnidadesCasa
                    .Include(u => u.Morador)
                    .Where(u => u.Morador != null)
                    .ToListAsync();

                var unidadeExata = unidadesCasa.FirstOrDefault(u =>
                    u.CEP != null && NormalizarCEP(u.CEP) == cepNormalizado &&
                    NormalizarNome(u.Morador!.Nome ?? "") == nomeNormalizado
                );

                if (unidadeExata != null)
                {
                    var tokenValidacao = GerarTokenValidacao();

                    return Ok(new ValidacaoDestinatarioResponseDTO
                    {
                        Validado = true,
                        Mensagem = "Destinatário validado com sucesso! Pode prosseguir com a entrega.",
                        TipoResultado = TipoResultadoValidacao.Sucesso,
                        DadosEncontrados = MapearDadosDestinatario(unidadeExata, unidadeExata.Morador!),
                        PodeRetentar = false,
                        PodeAcionarPortaria = false,
                        TokenValidacao = tokenValidacao,
                        ValidacaoId = unidadeExata.Id
                    });
                }

                return Ok(new ValidacaoDestinatarioResponseDTO
                {
                    Validado = false,
                    Mensagem = "Destinatário não encontrado no sistema. Por favor, acione a portaria.",
                    TipoResultado = TipoResultadoValidacao.NaoEncontrado,
                    DadosEncontrados = null,
                    PodeRetentar = true,
                    PodeAcionarPortaria = true,
                    TokenValidacao = null,
                    ValidacaoId = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro ao validar destinatário",
                    erro = ex.Message
                });
            }
        }

        /// <summary>
        /// Solicita um armário disponível após validação bem-sucedida
        /// </summary>
        [HttpPost("SolicitarArmario")]
        public async Task<ActionResult<SolicitarArmarioResponseDTO>> SolicitarArmario(
            [FromBody] SolicitarArmarioRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var armarioDisponivel = await _context.Armarios
                    .Where(a => a.Status == StatusArmario.Disponivel)
                    .OrderBy(a => a.Numero)
                    .FirstOrDefaultAsync();

                if (armarioDisponivel == null)
                {
                    return Ok(new SolicitarArmarioResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = "Nenhum armário disponível no momento. Por favor, acione a portaria."
                    });
                }

                var unidade = await _context.UnidadesCasa
                    .Include(u => u.Morador)
                    .FirstOrDefaultAsync(u => u.Id == request.UnidadeId);

                if (unidade == null || unidade.Morador == null)
                {
                    return BadRequest(new SolicitarArmarioResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = "Unidade não encontrada."
                    });
                }

                var codigoEntrega = GerarCodigoEntrega();

                var entrega = new Entrega
                {
                    NomeDestinatario = unidade.Morador.Nome,
                    NumeroCasa = unidade.NumeroCasa.ToString(),
                    EnderecoGerado = $"{unidade.Rua}, Casa {unidade.NumeroCasa}",
                    ArmariumId = armarioDisponivel.Id,
                    CodigoEntrega = codigoEntrega,
                    SenhaAcesso = GerarSenhaAcesso(),
                    DataHoraRegistro = DateTime.UtcNow,
                    Status = StatusEntrega.AguardandoArmario,
                    TelefoneWhatsApp = unidade.Morador.Telefone,
                    MensagemEnviada = false
                };

                armarioDisponivel.Status = StatusArmario.Ocupado;
                armarioDisponivel.UltimaAbertura = DateTime.UtcNow;

                _context.Entregas.Add(entrega);
                await _context.SaveChangesAsync();

                return Ok(new SolicitarArmarioResponseDTO
                {
                    Sucesso = true,
                    Mensagem = $"Armário {armarioDisponivel.Numero} liberado! Deposite o pacote e feche a porta.",
                    NumeroArmario = int.Parse(armarioDisponivel.Numero ?? "0"),
                    CodigoEntrega = codigoEntrega,
                    EntregaId = entrega.Id,
                    LimiteDeposito = DateTime.UtcNow.AddMinutes(5)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new SolicitarArmarioResponseDTO
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao solicitar armário: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Confirma o fechamento do armário e finaliza a entrega
        /// </summary>
        [HttpPost("ConfirmarFechamento")]
        public async Task<ActionResult<ConfirmarFechamentoResponseDTO>> ConfirmarFechamento(
            [FromBody] ConfirmarFechamentoRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var entrega = await _context.Entregas
                    .Include(e => e.Armario)
                    .FirstOrDefaultAsync(e => e.Id == request.EntregaId);

                if (entrega == null)
                {
                    return NotFound(new ConfirmarFechamentoResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = "Entrega não encontrada."
                    });
                }

                if (entrega.Armario == null)
                {
                    return BadRequest(new ConfirmarFechamentoResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = "Armário não associado à entrega."
                    });
                }

                entrega.Status = StatusEntrega.Armazenada;
                entrega.Armario.Status = StatusArmario.Disponivel;
                entrega.Armario.UltimoFechamento = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Buscar dados do morador para enviar notificação
                bool notificacaoEnviada = false;
                
                // Verifica se o email está configurado (não são valores padrão)
                var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var emailConfigurado = config["Gmail:Email"] != "seu-email@gmail.com" && 
                                      !string.IsNullOrEmpty(config["Gmail:Email"]);
                
                if (emailConfigurado)
                {
                    try
                    {
                        // Busca a unidade casa com o morador
                        var unidadeCasa = await _context.UnidadesCasa
                            .Include(u => u.Morador)
                            .FirstOrDefaultAsync(u => u.Morador != null && u.Morador.Nome == entrega.NomeDestinatario);

                        // Se não encontrou em casa, busca em apartamento
                        if (unidadeCasa == null)
                        {
                            var unidadeApartamento = await _context.UnidadesApartamento
                                .Include(u => u.Morador)
                                .FirstOrDefaultAsync(u => u.Morador != null && u.Morador.Nome == entrega.NomeDestinatario);

                            if (unidadeApartamento?.Morador != null)
                            {
                                await _emailService.EnviarEmailEntregaArmario(
                                    unidadeApartamento.Morador.Nome ?? "N/A",
                                    unidadeApartamento.Morador.Email ?? "N/A",
                                    entrega.Armario.Numero ?? "N/A",
                                    entrega.SenhaAcesso ?? "0000",
                                    entrega.CodigoEntrega ?? "N/A"
                                );
                                notificacaoEnviada = true;
                                entrega.MensagemEnviada = true;
                                await _context.SaveChangesAsync();
                                Console.WriteLine($"Email de entrega enviado para: {unidadeApartamento.Morador.Email}");
                            }
                        }
                        else if (unidadeCasa?.Morador != null)
                        {
                            await _emailService.EnviarEmailEntregaArmario(
                                unidadeCasa.Morador.Nome ?? "N/A",
                                unidadeCasa.Morador.Email ?? "N/A",
                                entrega.Armario.Numero ?? "N/A",
                                entrega.SenhaAcesso ?? "0000",
                                entrega.CodigoEntrega ?? "N/A"
                            );
                            notificacaoEnviada = true;
                            entrega.MensagemEnviada = true;
                            await _context.SaveChangesAsync();
                            Console.WriteLine($"Email de entrega enviado para: {unidadeCasa.Morador.Email}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao enviar notificação por email: {ex.Message}");
                        // Não falha a operação se o email falhar
                    }
                }
                else
                {
                    Console.WriteLine("⚠️ Email não configurado - notificação por email desabilitada");
                }

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
            catch (Exception ex)
            {
                return StatusCode(500, new ConfirmarFechamentoResponseDTO
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao confirmar fechamento: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Aciona a portaria quando há divergência nos dados ou problema na entrega
        /// </summary>
        [HttpPost("AcionarPortaria")]
        public async Task<ActionResult<AcionarPortariaResponseDTO>> AcionarPortaria(
            [FromBody] AcionarPortariaRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var entrega = new Entrega
                {
                    NomeDestinatario = request.NomeDestinatario,
                    NumeroCasa = "N/A",
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
            catch (Exception ex)
            {
                return StatusCode(500, new AcionarPortariaResponseDTO
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao acionar portaria: {ex.Message}"
                });
            }
        }

        // ========================================
        // MÉTODOS AUXILIARES – AGORA SÃO private
        // ========================================

        private string NormalizarCEP(string CEP)
        {
            return Regex.Replace(CEP, @"[^\d]", "");
        }

        private string NormalizarNome(string nome)
        {
            return nome.Trim().ToLower();
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

        private string GerarSenhaAcesso()
        {
            var random = new Random();
            return random.Next(1000, 9999).ToString();
        }

        private string GerarCodigoEntrega()
        {
            var random = new Random();
            return new string(Enumerable.Repeat("ABCDEF", 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GerarTokenValidacao()
        {
            var random = new Random();
            return new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}