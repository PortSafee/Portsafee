using System.ComponentModel.DataAnnotations;

namespace PortSafe.DTOs
{
    public class SolicitarArmarioRequestDTO
    {
        [Required(ErrorMessage = "Token de validação é obrigatório")]
        public string TokenValidacao { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID da unidade é obrigatório")]
        public int UnidadeId { get; set; }

        public string? ObservacaoEntrega { get; set; }
    }
}
