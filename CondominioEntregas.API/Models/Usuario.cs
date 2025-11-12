using System.ComponentModel.DataAnnotations;

namespace PortSafe.Models
{
    public abstract class Usuario
        {
            public int Id { get; set; }

            [Required]
            public string? Nome { get; set; }
            
            [Required]
            public string? Email { get; set; }

            [Required]
            public string? SenhaHash { get; set; }

            public DateTime DataCriacao { get; set; }
            
            public string? ResetToken { get; set; }
            
            public DateTime? ResetTokenExpiracao { get; set; }
            
            public TipoUsuario Tipo { get; set; }

            public int CondominioId { get; set; }

            public Condominio? Condominio { get; set; }
        }

        public enum TipoUsuario
        {
            Morador = 0,
            Porteiro = 1
        }
}