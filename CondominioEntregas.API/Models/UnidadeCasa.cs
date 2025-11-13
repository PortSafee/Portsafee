using System.ComponentModel.DataAnnotations;

namespace PortSafe.Models
{
    // Representa uma casa dentro de um condomínio de casas
    public class UnidadeCasa : Unidade
    {
        [Required]
        public string? Rua { get; set; }

        [Required]
        public int NumeroCasa { get; set; }

        [Required]
        public string? CEP { get; set; }
    }
}
