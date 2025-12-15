using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortSafe.Data;
using PortSafe.Models;
using PortSafe.DTOs;

namespace PortSafe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CondominioController : ControllerBase
    {
        private readonly PortSafeContext _context;

        public CondominioController(PortSafeContext context) => _context = context;

        // Lista todos os condomínios com total de moradores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCondominios()
        {
            try
            {
                var condominios = await _context.Condominios
                    .Select(c => new
                    {
                        c.Id,
                        c.NomeDoCondominio,
                        c.Tipo,
                        TotalMoradores = c.Moradores.Count
                    })
                    .ToListAsync();
                
                return Ok(condominios);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar condomínios: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Erro ao buscar condomínios", error = ex.Message });
            }
        }

        // Detalhes do condomínio por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetCondominio(int id)
        {
            var condominio = await _context.Condominios
                .Include(c => c.Moradores)
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.NomeDoCondominio,
                    c.Tipo,
                    Moradores = c.Moradores.Select(m => new
                    {
                        m.Id,
                        m.Nome,
                        m.Email,
                        m.Telefone
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return condominio == null
                ? NotFound(new { message = "Condomínio não encontrado." })
                : Ok(condominio);
        }

        // Criação unificada para Casa ou Apartamento
        [HttpPost("{tipo}")]
        public async Task<ActionResult<object>> CreateCondominio(string tipo, [FromBody] CreateCondominioRequest request)
        {
            if (tipo is not ("Casa" or "Apartamento"))
                return BadRequest(new { message = "Tipo deve ser 'Casa' ou 'Apartamento'." });

            if (string.IsNullOrWhiteSpace(request.NomeDoCondominio))
                return BadRequest(new { message = "Nome do condomínio é obrigatório." });

            var condominio = new Condominio
            {
                NomeDoCondominio = request.NomeDoCondominio,
                Tipo = tipo
            };

            _context.Condominios.Add(condominio);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCondominio),
                new { id = condominio.Id },
                new { condominio.Id, condominio.NomeDoCondominio, condominio.Tipo }
            );
        }

        // Atualiza condomínio
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCondominio(int id, [FromBody] UpdateCondominioRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NomeDoCondominio))
                return BadRequest(new { message = "Nome do condomínio é obrigatório." });

            var condominio = await _context.Condominios.FindAsync(id);
            if (condominio == null)
                return NotFound(new { message = "Condomínio não encontrado." });

            condominio.NomeDoCondominio = request.NomeDoCondominio;

            if (!string.IsNullOrWhiteSpace(request.Tipo))
            {
                if (request.Tipo is not ("Casa" or "Apartamento"))
                    return BadRequest(new { message = "Tipo deve ser 'Casa' ou 'Apartamento'." });

                condominio.Tipo = request.Tipo;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Condominios.Any(e => e.Id == id))
                    return NotFound(new { message = "Condomínio não encontrado." });
                throw;
            }

            return Ok(new
            {
                message = "Condomínio atualizado com sucesso.",
                condominio = new
                {
                    condominio.Id,
                    condominio.NomeDoCondominio,
                    condominio.Tipo
                }
            });
        }

        // Método auxiliar (opcional, pode remover se não for usado em outros lugares)
        private bool CondominioExists(int id) => _context.Condominios.Any(e => e.Id == id);
    }

    // DTOs unificados
    public class CreateCondominioRequest
    {
        public string NomeDoCondominio { get; set; } = string.Empty;
    }

    // Mantém o DTO de atualização (caso queira permitir mudar o tipo opcionalmente)
    public class UpdateCondominioRequest
    {
        public string NomeDoCondominio { get; set; } = string.Empty;
        public string? Tipo { get; set; }
    }

    public class CondominioImplementation : Condominio { }
}