using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PortSafe.Data;
using PortSafe.Services.AI;
using System.Security.Claims;

namespace PortSafe.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatbotController : ControllerBase
{
    private readonly PortSafeContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<ChatbotService> _logger;

    public ChatbotController(PortSafeContext context, IConfiguration config, ILogger<ChatbotService> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Processa uma pergunta do usuário sobre entregas
    /// </summary>
    [HttpPost("perguntar")]
    public async Task<IActionResult> Perguntar([FromBody] PerguntaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Mensagem))
            return BadRequest(new { erro = "Mensagem não pode ser vazia" });

        var geminiKey = _config["AI:Gemini:ApiKey"];
        if (string.IsNullOrEmpty(geminiKey))
            return StatusCode(500, new { erro = "Configuração da API Gemini não encontrada" });

        // Tentar obter ID do usuário do token JWT primeiro
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int? userId = null;
        
        // Se veio no body, usar esse (frontend pode enviar)
        if (dto.UserId.HasValue && dto.UserId.Value > 0)
        {
            userId = dto.UserId.Value;
        }
        // Senão, tentar do token JWT
        else if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedUserId))
        {
            userId = parsedUserId;
        }

        var service = new ChatbotService(
            _context,
            geminiKey,
            _config["AI:Gemini:Model"] ?? "gemini-2.5-flash",
            _logger);

        var resposta = await service.ProcessarMensagemAsync(dto.Mensagem, userId);

        return Ok(new { resposta });
    }
}

public class PerguntaDto
{
    public string Mensagem { get; set; } = string.Empty;
    public int? UserId { get; set; }
}