using System.ComponentModel.DataAnnotations;

namespace PortSafe.Models
{
    
    public abstract class Unidade
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CondominioId { get; set; }

        public Condominio? Condominio { get; set; } 

        public int? MoradorId { get; set; } // Relacionamento com Morador (um morador por unidade)

        public Morador? Morador { get; set; } 

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
