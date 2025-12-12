using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortSafe.Data;
using PortSafe.Models;
using PortSafe.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;

namespace PortSafe.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CondominioController : ControllerBase
    {
        private readonly PortSafeContext _context;

        public CondominioController(PortSafeContext context)
        {
            _context = context;
        }

        // Lista todos os condominios
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCondominios()
        {
            try
            {
                var condominios = await _context.Condominios
                    .Include(c => c.Moradores)
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
                return StatusCode(500, new { message = "Erro ao buscar condomínios.", error = ex.Message });
            }
        }

        // Detalhes do cond por id

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

            if (condominio == null)
            {
                return NotFound(new { message = "Condomínio não encontrado." });
            }

            return Ok(condominio);
        }


        // Cria condominio casa

        [HttpPost("Casa")]
        public async Task<ActionResult<object>> CreateCondCasa([FromBody] CreateCondCasaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NomeDoCondominio))
            {
                return BadRequest(new { message = "Nome do condomínio é obrigatório." });
            }


            var condominio = new CondominioImplementation
            {
                NomeDoCondominio = request.NomeDoCondominio,
                Tipo = "Casa"
            };

            _context.Condominios.Add(condominio);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCondominio), new { id = condominio.Id }, new
            {
                condominio.Id,
                condominio.NomeDoCondominio,
                condominio.Tipo
            });
        }

        // Cria condominio apartamento

        [HttpPost("Apartamento")]
        public async Task<ActionResult<object>> CreateCondApartamento([FromBody] CreateCondApartamentoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NomeDoCondominio))
            {
                return BadRequest(new { message = "Nome do condomínio é obrigatório." });
            }


            var condominio = new CondominioImplementation
            {
                NomeDoCondominio = request.NomeDoCondominio,
                Tipo = "Apartamento"
            };

            _context.Condominios.Add(condominio);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCondominio), new { id = condominio.Id }, new
            {
                condominio.Id,
                condominio.NomeDoCondominio,
                condominio.Tipo
            });
        }

        // 'Atualiza as informações de um condomínio existente'

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCondominio(int id, [FromBody] UpdateCondominioRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NomeDoCondominio))
            {
                return BadRequest(new { message = "Nome do condomínio é obrigatório." });
            }

            var condominio = await _context.Condominios.FindAsync(id);

            if (condominio == null)
            {
                return NotFound(new { message = "Condomínio não encontrado." });
            }

            condominio.NomeDoCondominio = request.NomeDoCondominio;

            if (!string.IsNullOrWhiteSpace(request.Tipo))
            {
                if (request.Tipo != "Casa" && request.Tipo != "Apartamento")
                {
                    return BadRequest(new { message = "Tipo deve ser 'Casa' ou 'Apartamento'." });
                }
                condominio.Tipo = request.Tipo;
            }

            _context.Entry(condominio).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CondominioExists(id))
                {
                    return NotFound(new { message = "Condomínio não encontrado." });
                }
                else
                {
                    throw;
                }
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
        
        private bool CondominioExists(int id)
        {
            return _context.Condominios.Any(e => e.Id == id);
        }
    }

    
    public class CondominioImplementation : Condominio
    {
    }
}
