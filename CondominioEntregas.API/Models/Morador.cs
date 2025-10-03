using System;
using System.ComponentModel.DataAnnotations;

namespace Moradores.API.Models
{
    public class Morador
    {
        public int Id { get; set; }
        
        [Required]
        public string? Nome { get; set; }
        
        [Required]
        public string? NumeroCasa { get; set; }
        
        [Phone]
        public string? TelefoneWhatsApp { get; set; }
        
        public string? Email { get; set; }
    }
}