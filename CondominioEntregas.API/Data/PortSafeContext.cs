using Microsoft.EntityFrameworkCore;
using PortSafe.Models;

    
namespace PortSafe.Data
{
    public class PortSafeContext : DbContext
    {
        public PortSafeContext(DbContextOptions<PortSafeContext> options) 
            : base(options) {} // Construtor que recebe opções de configuração do DbContext

        public DbSet<Morador> Moradores { get; set; }

        public DbSet<Condominio> Condominios { get; set; }
        
        public DbSet<CondApartamento> CondApartamento { get; set; }

        public DbSet<CondCasa> CondCasa { get; set; }

        public DbSet<Entrega> Entregas { get; set; }

        public DbSet<Armario> Armarios { get; set; }

    }
}

