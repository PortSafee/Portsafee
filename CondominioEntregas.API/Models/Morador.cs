using System.ComponentModel.DataAnnotations;


namespace PortSafe.Models
{
    public class Morador : Usuario
    {
        [Required]
        public string? Telefone { get; set; }
        
        [Required]
        public string? CPF { get; set; }
        
        public string? Photo { get; set; }

        public int? UnidadeId { get; set; } // Relação para saber qual unidade o morador pertence

        public Unidade? Unidade { get; set; }
    }
}


