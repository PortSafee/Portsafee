using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortSafe.Data;
using PortSafe.DTOs;
using PortSafe.Models;
using System.Text.RegularExpressions;

namespace PortSafe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntregaController : ControllerBase
    {
        private readonly PortSafeContext _context;

        public EntregaController(PortSafeContext context)
        {
            _context = context;
        }


        /// Valida os dados do destinatário (Nome e CEP) antes de liberar o armário

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
                // Normalizar CEP (remover caracteres especiais)

                var cepNormalizado = NormalizarCEP(request.CEP);
                var nomeNormalizado = NormalizarNome(request.NomeDestinatario);

                // Buscar moradores em unidades tipo Casa (que têm CEP)

                var unidadesCasa = await _context.UnidadesCasa
                    .Include(u => u.Morador)
                    .Where(u => u.Morador != null)
                    .ToListAsync();

                // Procurar correspondência exata

                var unidadeExata = unidadesCasa.FirstOrDefault(u =>
                    u.CEP != null && NormalizarCEP(u.CEP) == cepNormalizado &&
                    NormalizarNome(u.Morador!.Nome ?? "") == nomeNormalizado
                );

                // Correspondência exata encontrada

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


                // Nenhuma correspondência encontrada

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
        /// que o armário foi liberado e deverá clicar em "Confirmar" após depositar.
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
                // Buscar armário disponível

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

                // Buscar dados da unidade para pegar info do morador

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

                // Gerar código único de entrega

                var codigoEntrega = GerarCodigoEntrega();

                // Criar registro de entrega

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

                // Atualizar status do armário

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
                    LimiteDeposito = DateTime.UtcNow.AddMinutes(5) // 5 minutos para colocar a entrega no armário
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
        /// NOTA: Este endpoint é chamado quando o MOTORISTA clica no botão
        /// "CONFIRMAR FECHAMENTO" na tela/app após depositar o pacote.
        /// Não depende de sensor - é confirmação manual/visual.
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
                // Buscar entrega

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

                // Atualizar status da entrega

                entrega.Status = StatusEntrega.Armazenada;
                
                // Atualizar armário
                entrega.Armario.Status = StatusArmario.Disponivel;
                entrega.Armario.UltimoFechamento = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // TODO: Enviar notificação WhatsApp/SMS para o morador
                // await EnviarNotificacaoMorador(entrega);

                return Ok(new ConfirmarFechamentoResponseDTO
                {
                    Sucesso = true,
                    Mensagem = "Entrega confirmada com sucesso! Morador será notificado.",
                    CodigoEntrega = entrega.CodigoEntrega,
                    SenhaAcesso = entrega.SenhaAcesso,
                    DataHoraEntrega = entrega.DataHoraRegistro,
                    NotificacaoEnviada = false // Mudar para true quando implementar notificação
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
                // Criar registro de entrega redirecionada

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

                // TODO: Enviar notificação para portaria
                // await NotificarPortaria(entrega);

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

        public string NormalizarCEP(string CEP)
        {
            return Regex.Replace(CEP, @"[^\d]", ""); 
        }

        public string NormalizarNome(string nome)
        {
            return nome.Trim().ToLower();
        }

        public DadosDestinatarioDTO MapearDadosDestinatario(UnidadeCasa unidade, Morador morador)
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

        public string GerarSenhaAcesso()
        {
            var random = new Random();
            return random.Next(1000, 9999).ToString(); // Senha de 4 dígitos
        }

        public string GerarCodigoEntrega()
        {
            var random = new Random();
            return new string(Enumerable.Repeat("ABCDEF", 6) // gera código de 6 chars
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public string GerarTokenValidacao()
        {
            var random = new Random();
            return new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 16) // gera token de 16 chars
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        
    }
}