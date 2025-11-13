using Microsoft.EntityFrameworkCore;
using PortSafe.Models;

    
namespace PortSafe.Data
{
    public class PortSafeContext : DbContext
    {
        public PortSafeContext(DbContextOptions<PortSafeContext> options)
            : base(options) { } // Construtor que recebe opções de configuração do DbContext

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Morador> Moradores { get; set; }

        public DbSet<Porteiro> Porteiros { get; set; }

        public DbSet<Condominio> Condominios { get; set; }

        public DbSet<Entrega> Entregas { get; set; }

        public DbSet<Armario> Armarios { get; set; }

        public DbSet<UnidadeCasa> UnidadesCasa { get; set; }

        public DbSet<UnidadeApartamento> UnidadesApartamento { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurar tabela para cada tipo de Usuario 

            modelBuilder.Entity<Usuario>()
                .HasDiscriminator<TipoUsuario>("Tipo")
                .HasValue<Morador>(TipoUsuario.Morador)
                .HasValue<Porteiro>(TipoUsuario.Porteiro);
                



            // Configurar relacionamento Morador -> Condominio

            modelBuilder.Entity<Morador>()
                .HasOne(m => m.Condominio)
                .WithMany(c => c.Moradores)
                .HasForeignKey(m => m.CondominioId)
                .OnDelete(DeleteBehavior.Restrict);



            // Configurar relacionamento Porteiro -> Condominio

            modelBuilder.Entity<Porteiro>()
                .HasOne(p => p.Condominio)
                .WithMany(c => c.Porteiros)
                .HasForeignKey(p => p.CondominioId)
                .OnDelete(DeleteBehavior.Restrict);




            // Configurar relacionamento Morador -> Unidade (1:1)

            modelBuilder.Entity<Morador>()
                .HasOne(m => m.Unidade)
                .WithOne(u => u.Morador)
                .HasForeignKey<Morador>(m => m.UnidadeId)
                .OnDelete(DeleteBehavior.SetNull);



            // Configurar relacionamento Unidade -> Condominio
            
            modelBuilder.Entity<Unidade>()
                .HasOne(u => u.Condominio)
                .WithMany()
                .HasForeignKey(u => u.CondominioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurar Tabela para cada tipo de Unidade (TPH)
            modelBuilder.Entity<UnidadeCasa>().ToTable("UnidadesCasa");
            modelBuilder.Entity<UnidadeApartamento>().ToTable("UnidadesApartamento");

            // Configurar relacionamento Entrega -> Armario
            modelBuilder.Entity<Entrega>()
                .HasOne(e => e.Armario)
                .WithMany(a => a.Entregas)
                .HasForeignKey(e => e.ArmariumId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

