using System.ComponentModel.DataAnnotations;

namespace PortSafe.DTOs
{
    public class ValidarDestinatarioRequestDTO
    {
        [Required(ErrorMessage = "O nome do destinatário é obrigatório")]
        public string NomeDestinatario { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CEP é obrigatório")]
        public string CEP { get; set; } = string.Empty;

        // Opcional: para condominios de apartamento
        public string? Torre { get; set; }
        
        // Opcional: número da casa ou apartamento (caso o motorista saiba)
        public string? Numero { get; set; }
    }
}
