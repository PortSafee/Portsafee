using Microsoft.AspNetCore.Mvc;
using PortSafe.DTOs;
using PortSafe.Services;

namespace PortSafe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService? _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }



        [HttpPost("Cadastro")]
        public async Task<IActionResult> Cadastro([FromBody] MoradorResponse moradorResponse)
        {
            try
            {
                if (_authService == null)
                {
                    return StatusCode(500, "AuthService is not available."); // Verifica se o serviço está disponível
                }

                var morador = await _authService.Cadastro(moradorResponse); // Chama o método de cadastro do serviço

                return Ok(new { Message = "Morador cadastrado com sucesso", Morador = morador }); // Retorna uma resposta de sucesso com os detalhes do morador cadastrado
            }
            catch (Exception ex)
            {

                return BadRequest(new { Message = ex.Message });
            }
        }
    }
    
}
