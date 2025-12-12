// File: PortSafe/DTOs/ValidarDestinatarioRequestDTO.cs
using System.ComponentModel.DataAnnotations;

namespace PortSafe.DTOs
{
    public class ValidarDestinatarioRequestDTO
    {
        [Required(ErrorMessage = "O nome do destinatário é obrigatório")]
        public string NomeDestinatario { get; set; } = string.Empty;

        // Tornamos opcional: apenas obrigatório quando TipoUnidade == "Casa"
        public string? CEP { get; set; }

        // Opcional: para condominios de apartamento (ex: Torre A)
        public string? Torre { get; set; }
        
        // Opcional: número da casa ou apartamento (caso o motorista saiba)
        public string? Numero { get; set; }

        // Indica se é "Casa" ou "Apartamento" (obrigatório para guiar a validação)
        [Required(ErrorMessage = "O tipo da unidade é obrigatório (Casa ou Apartamento)")]
        public string TipoUnidade { get; set; } = string.Empty;
    }
}
