using System.ComponentModel.DataAnnotations;
using PortSafe.Models;

namespace PortSafe.Models
{
    public class Morador
    {
        public int Id { get; set; }

        [Required] // Torna o campo obrigatório
        public string? Nome { get; set; }

        [Required]
        public Condominio? Condominio { get; set; } // Propriedade para o condomínio (pode ser CondApartamento ou CondCasa)

        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Senha { get; set; }  

        public int CondominioId { get; set; } // Chave estrangeira

        public string? Telefone { get; set; } = string.Empty; // Telefone do morador (opcional)

        public string? Photo { get; set; } = string.Empty; // URL ou caminho para a foto do morador (opcional) 

    }
}


