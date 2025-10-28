using System.ComponentModel.DataAnnotations;

namespace PortSafe.DTOs
{
    public class AcionarPortariaRequestDTO
    {
        [Required(ErrorMessage = "Nome do destinatário é obrigatório")]
        public string NomeDestinatario { get; set; } = string.Empty;

        [Required(ErrorMessage = "CEP é obrigatório")]
        public string CEP { get; set; } = string.Empty;

        public string? MotivoAcionamento { get; set; }
    }
}
