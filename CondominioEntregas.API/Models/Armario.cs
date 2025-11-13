using System;
using System.ComponentModel.DataAnnotations;

namespace PortSafe.Models
{
    public class Armario
    {
        public int Id { get; set; }

        [Required]
        public string? Numero { get; set; }

        public StatusArmario Status { get; set; }

        public DateTime? UltimaAbertura { get; set; }

        public DateTime? UltimoFechamento { get; set; }

        public virtual ICollection<Entrega>? Entregas { get; set; } 
    }


    public enum StatusArmario
    {
        Disponivel,
        Ocupado,
        EmManutencao,
        Indisponivel

    }
}