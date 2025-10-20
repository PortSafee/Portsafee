using System.ComponentModel.DataAnnotations;

namespace PortSafe.Models
{
    // Representa um apartamento dentro de um condomínio de apartamentos
    public class UnidadeApartamento : Unidade
    {
        [Required]
        public string? Torre { get; set; }

        [Required]
        public string? NumeroApartamento { get; set; }
    }
}
