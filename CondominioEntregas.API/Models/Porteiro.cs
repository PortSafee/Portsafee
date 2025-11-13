using System.ComponentModel.DataAnnotations;

namespace PortSafe.Models
{
    public class Porteiro : Usuario
    {
        [Required]
        public string? Telefone { get; set; }
        
    }
}
