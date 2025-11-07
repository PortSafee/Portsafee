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
        private readonly AuthService? _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }



        // Endpoint para cadastro de morador

        [HttpPost("Cadastro")]
        public async Task<IActionResult> Cadastro([FromBody] CadastroMoradorRequestDTO request)
        {
            try
            {
                if (_authService == null)
                {
                    return StatusCode(500, "AuthService is not available."); // Verifica se o serviço está disponível
                }

                var morador = await _authService.Cadastro(request); // Chama o método de cadastro do serviço

                if (morador == null)
                {
                    return BadRequest(new { Message = "Erro ao cadastrar morador." });
                }

                // Retorna apenas dados seguros (sem senha)

                var response = new UsuarioResponseDTO
                {
                    Id = morador.Id,
                    Nome = morador.Nome ?? string.Empty,
                    Email = morador.Email ?? string.Empty,
                    Tipo = "Morador",
                    DataCriacao = morador.DataCriacao
                };

                return Ok(new { Message = "Morador cadastrado com sucesso", Usuario = response });
            }
            catch (Exception ex)
            {

                return BadRequest(new { Message = ex.Message });
            }
        }


        // Endpoint para cadastro de porteiro

        [HttpPost("CadastroPorteiro")]
        public async Task<IActionResult> CadastroPorteiro([FromBody] CadastroPorteiroRequestDTO request)
        {
            try
            {
                if (_authService == null)
                {
                    return StatusCode(500, "AuthService is not available.");
                }

                var porteiro = await _authService.CadastroPorteiro(request);

                if (porteiro == null)
                {
                    return BadRequest(new { Message = "Erro ao cadastrar porteiro." });
                }

                // Retorna apenas dados seguros (sem senha)
                var response = new UsuarioResponseDTO
                {
                    Id = porteiro.Id,
                    Nome = porteiro.Nome ?? string.Empty,
                    Email = porteiro.Email ?? string.Empty,
                    Tipo = "Porteiro",
                    DataCriacao = porteiro.DataCriacao
                };

                return Ok(new { Message = "Porteiro cadastrado com sucesso", Usuario = response });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


        // Endpoint para login

        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginDTO dto) // Recebe o DTO de login
        {
            _logger.LogInformation($"Tentativa de login para o email: {dto.UsernameOrEmail}");

            if (string.IsNullOrEmpty(dto.UsernameOrEmail)) // Verifica se o email foi fornecido
            {
                _logger.LogWarning("Login falhou: email não fornecido.");
                return BadRequest(new { message = "Email não pode ser nulo ou vazio." });
            }

            if (string.IsNullOrEmpty(dto.Password)) // Verifica se a senha foi fornecida
            {
                _logger.LogWarning($"Login falhou: senha não fornecida para o email: {dto.UsernameOrEmail}");
                return BadRequest(new { message = "Senha não pode ser nula ou vazia." });
            }

            if (_authService == null) // Verifica se o serviço está disponível
            {
                _logger.LogError("AuthService não está disponível.");
                return StatusCode(500, "AuthService não está disponível.");
            }

            var usuario = _authService.Login(dto.UsernameOrEmail, dto.Password); // Chama o método de login do serviço

            if (usuario == null)
            {
                _logger.LogWarning($"Login falhou: credenciais inválidas para o email: {dto.UsernameOrEmail}");
                return Unauthorized(new { message = "Credenciais inválidas." });
            }

            var token = _authService.GenerateJwtToken(usuario, TipoUsuario.Morador); // Gera o token JWT

            _logger.LogInformation($"Login realizado com sucesso para o email: {dto.UsernameOrEmail} - Sessão iniciada.{token}");

            return Ok(new
            {
                usuario,
                token
                
            });
        }


        // Endpoint para solicitar reset de senha

        [HttpPost("SolicitarResetSenha")]
        public async Task<IActionResult> SolicitarResetSenha([FromBody] SolicitarResetSenhaRequestDTO request)
        {
            try
            {
                if (_authService == null)
                {
                    return StatusCode(500, "AuthService não está disponível.");
                }

                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { Message = "Email é obrigatório." });
                }

                var token = await _authService.SolicitarResetSenha(request.Email);

                // Por segurança, sempre retornamos sucesso mesmo se o email não existir
                // Isso evita que hackers descubram quais emails estão cadastrados
                
                _logger.LogInformation($"Solicitação de reset de senha para: {request.Email}");
                
                // O código é enviado APENAS por email, não na resposta da API
                return Ok(new 
                { 
                    Message = "Se o email estiver cadastrado, você receberá um código no seu email para redefinir sua senha. Verifique também a caixa de spam."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao solicitar reset de senha: {ex.Message}");
                return BadRequest(new { Message = "Erro ao processar solicitação." });
            }
        }


        // Endpoint para redefinir senha

        [HttpPost("RedefinirSenha")]
        public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaRequestDTO request)
        {
            try
            {
                if (_authService == null)
                {
                    return StatusCode(500, "AuthService não está disponível.");
                }

                if (string.IsNullOrWhiteSpace(request.Email) || 
                    string.IsNullOrWhiteSpace(request.Token) ||
                    string.IsNullOrWhiteSpace(request.NovaSenha))
                {
                    return BadRequest(new { Message = "Email, token e nova senha são obrigatórios." });
                }

                if (request.NovaSenha != request.ConfirmarSenha)
                {
                    return BadRequest(new { Message = "As senhas não conferem." });
                }

                var sucesso = await _authService.RedefinirSenha(
                    request.Email, 
                    request.Token, 
                    request.NovaSenha
                );

                if (!sucesso)
                {
                    return BadRequest(new { Message = "Token inválido ou expirado." });
                }

                _logger.LogInformation($"Senha redefinida com sucesso para: {request.Email}");

                return Ok(new { Message = "Senha redefinida com sucesso! Você já pode fazer login com a nova senha." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao redefinir senha: {ex.Message}");
                return BadRequest(new { Message = "Erro ao redefinir senha." });
            }
        }
    }
    
}
