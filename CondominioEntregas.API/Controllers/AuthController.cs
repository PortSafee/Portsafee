using Microsoft.AspNetCore.Mvc;
using PortSafe.DTOs;
using PortSafe.Services;
using PortSafe.Models;

namespace PortSafe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger;
        }

        private IActionResult ServiceNotAvailable() => 
            StatusCode(500, new { Message = "Serviço de autenticação indisponível." });

        private IActionResult BadRequestResponse(string message) => 
            BadRequest(new { Message = message });

        private IActionResult OkResponse(string message, object? data = null) => 
            Ok(data is null ? new { Message = message } : new { Message = message, data });

        // POST: api/auth/cadastro
        [HttpPost("Cadastro")]
        public async Task<IActionResult> Cadastro([FromBody] CadastroMoradorRequestDTO request)
        {
            if (_authService == null) return ServiceNotAvailable();

            try
            {
                var morador = await _authService.Cadastro(request);
                if (morador == null)
                    return BadRequestResponse("Erro ao cadastrar morador.");

                var response = new UsuarioResponseDTO
                {
                    Id = morador.Id,
                    Nome = morador.Nome ?? string.Empty,
                    Email = morador.Email ?? string.Empty,
                    Tipo = "Morador",
                    DataCriacao = morador.DataCriacao
                };

                return OkResponse("Morador cadastrado com sucesso", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no cadastro de morador");
                return BadRequestResponse(ex.Message);
            }
        }

        // POST: api/auth/cadastroporteiro
        [HttpPost("CadastroPorteiro")]
        public async Task<IActionResult> CadastroPorteiro([FromBody] CadastroPorteiroRequestDTO request)
        {
            if (_authService == null) return ServiceNotAvailable();

            try
            {
                var porteiro = await _authService.CadastroPorteiro(request);
                if (porteiro == null)
                    return BadRequestResponse("Erro ao cadastrar porteiro.");

                var response = new UsuarioResponseDTO
                {
                    Id = porteiro.Id,
                    Nome = porteiro.Nome ?? string.Empty,
                    Email = porteiro.Email ?? string.Empty,
                    Tipo = "Porteiro",
                    DataCriacao = porteiro.DataCriacao
                };

                return OkResponse("Porteiro cadastrado com sucesso", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no cadastro de porteiro");
                return BadRequestResponse(ex.Message);
            }
        }

        // POST: api/auth/login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            _logger.LogInformation("Tentativa de login para: {Email}", dto.UsernameOrEmail);

            if (string.IsNullOrWhiteSpace(dto.UsernameOrEmail))
                return BadRequestResponse("Email é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequestResponse("Senha é obrigatória.");

            if (_authService == null) return ServiceNotAvailable();

            var usuario = await _authService.LoginAsync(dto.UsernameOrEmail, dto.Password);
            if (usuario == null)
            {
                _logger.LogWarning("Login falhou: credenciais inválidas para {Email}", dto.UsernameOrEmail);
                return Unauthorized(new { Message = "Credenciais inválidas." });
            }

            var token = _authService.GenerateJwtToken(usuario, usuario.Tipo);
            _logger.LogInformation("Login bem-sucedido para {Email}", dto.UsernameOrEmail);

            return Ok(new { usuario, token });
        }

        // POST: api/auth/solicitarresetsenha
        [HttpPost("SolicitarResetSenha")]
        public async Task<IActionResult> SolicitarResetSenha([FromBody] SolicitarResetSenhaRequestDTO request)
        {
            if (_authService == null) return ServiceNotAvailable();
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequestResponse("Email é obrigatório.");

            try
            {
                await _authService.SolicitarResetSenha(request.Email);
                _logger.LogInformation("Solicitação de reset de senha para: {Email}", request.Email);

                return Ok(new
                {
                    Message = "Se o email estiver cadastrado, você receberá um código no seu email para redefinir sua senha. Verifique também a caixa de spam."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao solicitar reset de senha para {Email}", request.Email);
                return BadRequestResponse("Erro ao processar solicitação.");
            }
        }

        // POST: api/auth/redefinirsenha
        [HttpPost("RedefinirSenha")]
        public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaRequestDTO request)
        {
            if (_authService == null) return ServiceNotAvailable();

            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Token) ||
                string.IsNullOrWhiteSpace(request.NovaSenha) ||
                string.IsNullOrWhiteSpace(request.ConfirmarSenha))
                return BadRequestResponse("Todos os campos são obrigatórios.");

            if (request.NovaSenha != request.ConfirmarSenha)
                return BadRequestResponse("As senhas não conferem.");

            try
            {
                var sucesso = await _authService.RedefinirSenha(request.Email, request.Token, request.NovaSenha);
                if (!sucesso)
                    return BadRequestResponse("Token inválido ou expirado.");

                _logger.LogInformation("Senha redefinida com sucesso para: {Email}", request.Email);
                return OkResponse("Senha redefinida com sucesso! Você já pode fazer login com a nova senha.");
            }
            catch (ArgumentException ex)
            {
                return BadRequestResponse(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao redefinir senha para {Email}", request.Email);
                return BadRequestResponse("Erro ao redefinir senha.");
            }
        }
    }
}