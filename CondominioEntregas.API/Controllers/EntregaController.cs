// File: PortSafe/Controllers/EntregaController.cs
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
[HttpGet("Armazenadas")]
public async Task<ActionResult<IEnumerable<Entrega>>> GetArmazenadas()
{
    var entregas = await _context.Entregas
        .Where(e => e.Status == StatusEntrega.Armazenada)
        .OrderByDescending(e => e.DataHoraRegistro)
        .ToListAsync();

    return Ok(entregas);
}

        [HttpPost("ValidarDestinatario")]
        public async Task<ActionResult<ValidacaoDestinatarioResponseDTO>> ValidarDestinatario([FromBody] ValidarDestinatarioRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var nomeNormalizado = NormalizarNome(request.NomeDestinatario);

            // Validação para CASA
            if (string.Equals(request.TipoUnidade, "Casa", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(request.CEP))
                {
                    return BadRequest(new { Message = "CEP é obrigatório para tipo 'Casa'." });
                }

                var cep = NormalizarCEP(request.CEP);

                if (cep.Length != 8)
                {
                    return BadRequest(new { Message = "CEP inválido." });
                }

                var unidadeCasa = _context.UnidadesCasa
                    .Include(u => u.Morador)
                    .AsEnumerable() // passar para LINQ to Objects para usar NormalizarNome no comparador
                    .FirstOrDefault(u => u.Morador != null &&
                        NormalizarCEP(u.CEP) == cep &&
                        NormalizarNome(u.Morador.Nome) == nomeNormalizado);

                return unidadeCasa != null
                    ? CriarRespostaSucesso(unidadeCasa, unidadeCasa.Morador!, isIA: false)
                    : CriarRespostaNaoEncontrado();
            }

            // Validação para APARTAMENTO
            if (string.Equals(request.TipoUnidade, "Apartamento", StringComparison.OrdinalIgnoreCase))
            {
                // Para apartamento, espera-se Torre + Numero (ou ao menos Numero)
                if (string.IsNullOrWhiteSpace(request.Numero) && string.IsNullOrWhiteSpace(request.Torre))
                {
                    // podemos permitir busca apenas por nome, mas preferível que informe torre/numero
                    // Retornamos não encontrado sugerindo informar torre/numero
                    return Ok(new ValidacaoDestinatarioResponseDTO
                    {
                        Validado = false,
                        Mensagem = "Por favor informe Torre e Número do apartamento para validação mais precisa.",
                        TipoResultado = TipoResultadoValidacao.NaoEncontrado,
                        DadosEncontrados = null,
                        PodeRetentar = true,
                        PodeAcionarPortaria = true,
                        TokenValidacao = null,
                        ValidacaoId = null
                    });
                }

                // Normalizamos entrada
                var torreReq = (request.Torre ?? "").Trim();
                var numeroReq = (request.Numero ?? "").Trim();

                var unidadeApto = _context.UnidadesApartamento
                    .Include(u => u.Morador)
                    .AsEnumerable()
                    .FirstOrDefault(u =>
                        u.Morador != null &&
                        NormalizarNome(u.Morador.Nome) == nomeNormalizado &&
                        (string.IsNullOrEmpty(torreReq) || string.Equals(u.Torre?.Trim(), torreReq, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrEmpty(numeroReq) || string.Equals(u.NumeroApartamento?.Trim(), numeroReq, StringComparison.OrdinalIgnoreCase))
                    );

                if (unidadeApto != null)
                {
                    return Ok(new ValidacaoDestinatarioResponseDTO
                    {
                        Validado = true,
                        Mensagem = "Destinatário validado com sucesso!",
                        TipoResultado = TipoResultadoValidacao.Sucesso,
                        DadosEncontrados = new DadosDestinatarioDTO
                        {
                            NomeMorador = unidadeApto.Morador!.Nome,
                            TelefoneWhatsApp = unidadeApto.Morador.Telefone,
                            TipoUnidade = "Apartamento",
                            Endereco = $"Torre {unidadeApto.Torre}, Apto {unidadeApto.NumeroApartamento}",
                            CEP = null,
                            UnidadeId = unidadeApto.Id,
                            MoradorId = unidadeApto.Morador.Id
                        },
                        PodeRetentar = false,
                        PodeAcionarPortaria = false,
                        TokenValidacao = GerarTokenValidacao(),
                        ValidacaoId = unidadeApto.Id
                    });
                }

                return CriarRespostaNaoEncontrado();
            }

            return BadRequest(new { Message = "TipoUnidade inválido. Use 'Casa' ou 'Apartamento'." });
        }

        [HttpPost("ValidarDestinatarioComIA")]
        public async Task<ActionResult<ValidarDestinatarioComIAResponseDTO>> ValidarDestinatarioComIA([FromBody] ValidarDestinatarioRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Se for casa, passe CEP para a IA; se for apto, passe torre/numero concatenados
            string referencia = request.TipoUnidade?.Equals("Casa", StringComparison.OrdinalIgnoreCase) == true
                ? request.CEP ?? ""
                : $"{request.Torre} {request.Numero}";

            var resultadoIA = await _validationAgent.ValidateDestinatarioAsync(request.NomeDestinatario, referencia);

            if (resultadoIA.IsValid && resultadoIA.UnidadeId.HasValue)
            {
                // Tentamos buscar a unidade (pode ser casa ou apto)
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

                // Se não achou em casas, tenta em apartamentos
                var unidadeApto = await _context.UnidadesApartamento
                    .Include(u => u.Morador)
                    .FirstOrDefaultAsync(u => u.Id == resultadoIA.UnidadeId.Value);

                if (unidadeApto?.Morador != null)
                {
                    return Ok(new ValidarDestinatarioComIAResponseDTO
                    {
                        Validado = true,
                        ConfiancaIA = resultadoIA.ConfidenceScore,
                        Mensagem = $"Destinatário validado pela IA! (Confiança: {resultadoIA.ConfidenceScore:F0}%) - {resultadoIA.Reason}",
                        TipoResultado = TipoResultadoValidacao.Sucesso,
                        DadosEncontrados = new DadosDestinatarioDTO
                        {
                            NomeMorador = unidadeApto.Morador!.Nome,
                            TelefoneWhatsApp = unidadeApto.Morador.Telefone,
                            TipoUnidade = "Apartamento",
                            Endereco = $"Torre {unidadeApto.Torre}, Apto {unidadeApto.NumeroApartamento}",
                            CEP = null,
                            UnidadeId = unidadeApto.Id,
                            MoradorId = unidadeApto.Morador.Id
                        },
                        Sugestoes = null,
                        PodeRetentar = false,
                        PodeAcionarPortaria = false,
                        TokenValidacao = GerarTokenValidacao(),
                        ValidacaoId = unidadeApto.Id
                    });
                }
            }

            return Ok(new ValidarDestinatarioComIAResponseDTO
            {
                Validado = false,
                ConfiancaIA = 0,
                Mensagem = "Destinatário não encontrado pela IA.",
                TipoResultado = TipoResultadoValidacao.NaoEncontrado,
                DadosEncontrados = null,
                Sugestoes = null,
                PodeRetentar = true,
                PodeAcionarPortaria = true,
                TokenValidacao = null,
                ValidacaoId = null
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

    // Atualiza status da entrega: pacote armazenado (aguardando retirada pelo morador)
    entrega.Status = StatusEntrega.Armazenada;
    entrega.DataHoraRegistro = entrega.DataHoraRegistro; // mantém a data de registro original

    // O armário deve permanecer OCUPADO enquanto a encomenda estiver dentro dele.
    entrega.Armario.Status = StatusArmario.Ocupado;
    entrega.Armario.UltimoFechamento = DateTime.UtcNow;

    // Envia notificação se configurado (email)
    bool notificacaoEnviada = await TentarEnviarNotificacaoAsync(
        entrega.NomeDestinatario,
        entrega.Armario.Numero!,
        entrega.SenhaAcesso!,
        entrega.CodigoEntrega!);

    if (notificacaoEnviada)
        entrega.MensagemEnviada = true;

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



      [HttpPost("SolicitarArmario")]
public async Task<ActionResult<SolicitarArmarioResponseDTO>> SolicitarArmario([FromBody] SolicitarArmarioRequestDTO request)
{
    if (!ModelState.IsValid) return BadRequest(ModelState);

    UnidadeCasa? unidadeCasa = await _context.UnidadesCasa
        .Include(u => u.Morador)
        .FirstOrDefaultAsync(u => u.Id == request.UnidadeId);

    UnidadeApartamento? unidadeApto = null;

    if (unidadeCasa == null)
    {
        unidadeApto = await _context.UnidadesApartamento
            .Include(u => u.Morador)
            .FirstOrDefaultAsync(u => u.Id == request.UnidadeId);

        if (unidadeApto == null || unidadeApto.Morador == null)
            return NotFound("Unidade não encontrada.");
    }

    // Buscar armários Disponíveis e escolher o primeiro que NÃO tenha uma entrega não-retirada associada.
    var armariosDisponiveis = await _context.Armarios
        .Where(a => a.Status == StatusArmario.Disponivel)
        .OrderBy(a => a.Id)
        .ToListAsync();

    Armario? armarioEscolhido = null;
    foreach (var a in armariosDisponiveis)
    {
        var existeEntregaPendente = await _context.Entregas.AnyAsync(e =>
    e.ArmariumId == a.Id &&
    (
        e.Status == StatusEntrega.Armazenada ||
        e.Status == StatusEntrega.AguardandoArmario ||
        e.Status == StatusEntrega.AguardandoValidacao
    )
);


        if (!existeEntregaPendente)
{
    armarioEscolhido = a;
    break;
}

    }

    if (armarioEscolhido == null)
        return BadRequest(new { mensagem = "Nenhum armário disponível no momento." });

    // Reservar o armário: marca como ocupado enquanto o porteiro deposita
    armarioEscolhido.Status = StatusArmario.Ocupado;
    armarioEscolhido.UltimaAbertura = DateTime.UtcNow;

    // Criar nova entrega e vincular ao armário
    var entrega = new Entrega
    {
        NomeDestinatario = unidadeCasa?.Morador?.Nome ?? unidadeApto!.Morador!.Nome,
        EnderecoGerado = unidadeCasa != null 
            ? $"{unidadeCasa.Rua}, Casa {unidadeCasa.NumeroCasa}"
            : $"Torre {unidadeApto!.Torre}, Apto {unidadeApto.NumeroApartamento}",
        TelefoneWhatsApp = unidadeCasa?.Morador?.Telefone ?? unidadeApto!.Morador!.Telefone,
        ArmariumId = armarioEscolhido.Id,
        CodigoEntrega = GerarCodigoEntrega(),
        SenhaAcesso = GerarSenhaAcesso(),
        DataHoraRegistro = DateTime.UtcNow,
        Status = StatusEntrega.AguardandoArmario,
        MensagemEnviada = false
    };

    _context.Entregas.Add(entrega);
    await _context.SaveChangesAsync();

    return Ok(new SolicitarArmarioResponseDTO
    {
        Sucesso = true,
        Mensagem = $"Armário {armarioEscolhido.Numero} liberado! Deposite o pacote e feche a porta.",
        NumeroArmario = int.Parse(armarioEscolhido.Numero ?? "0"),
        CodigoEntrega = entrega.CodigoEntrega,
        EntregaId = entrega.Id,
        LimiteDeposito = DateTime.UtcNow.AddMinutes(5)
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

        [HttpGet("PorMorador")]
public async Task<ActionResult<IEnumerable<Entrega>>> GetByMorador([FromQuery] string nome)
{
    if (string.IsNullOrWhiteSpace(nome))
        return BadRequest("Nome do morador é obrigatório.");

    var nomeNormalizado = nome.Trim().ToLower();

    var entregas = await _context.Entregas
        .AsQueryable()
        .Where(e => e.NomeDestinatario.ToLower() == nomeNormalizado)
        .OrderByDescending(e => e.DataHoraRegistro)
        .ToListAsync();

    return Ok(entregas);
}

[HttpGet("PorMoradorId")]
public async Task<ActionResult<IEnumerable<Entrega>>> GetByMoradorId([FromQuery] int id)
{
    if (id <= 0)
        return BadRequest("ID do morador inválido.");

    // Busca entregas onde o NomeDestinatario pertence ao Morador do ID informado
    var morador = await _context.Moradores.FirstOrDefaultAsync(m => m.Id == id);

    if (morador == null)
        return NotFound("Morador não encontrado.");

    var nomeNormalizado = morador.Nome.Trim().ToLower();

    var entregas = await _context.Entregas
        .Where(e => e.NomeDestinatario.ToLower() == nomeNormalizado)
        .OrderByDescending(e => e.DataHoraRegistro)
        .ToListAsync();

    return Ok(entregas);
}

[HttpPut("ConfirmarRetirada")]
public async Task<IActionResult> ConfirmarRetirada(int entregaId)
{
    var entrega = await _context.Entregas
        .Include(e => e.Armario)
        .FirstOrDefaultAsync(e => e.Id == entregaId);

    if (entrega == null)
        return NotFound("Entrega não encontrada.");

    if (entrega.Armario == null)
        return BadRequest("Nenhum armário associado a esta entrega.");

    // Atualiza entrega
    entrega.Status = StatusEntrega.Retirada;
    entrega.DataHoraRetirada = DateTime.UtcNow;

    // 🔥 Libera o armário
    entrega.Armario.Status = StatusArmario.Disponivel;
    entrega.Armario.UltimoFechamento = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    return Ok(new { message = "Retirada confirmada. Armário liberado." });
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

            // Busca em Apartamentos (por nome; poderia ser por torre+numero se disponível)
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