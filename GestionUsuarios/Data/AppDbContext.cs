using GestionUsuarios.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionUsuarios.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Vecino> Vecinos { get; set; }
        public DbSet<FuncionarioMunicipal> FuncionariosMunicipal { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de Vecino y su relación con Usuario
            modelBuilder.Entity<Vecino>()
                .HasOne(v => v.Usuario)
                .WithOne()
                .HasForeignKey<Vecino>(v => v.id); // FK = PK
            modelBuilder.Entity<FuncionarioMunicipal>()
                .HasOne(v => v.Usuario)
                .WithOne()
                .HasForeignKey<FuncionarioMunicipal>(v => v.id);
        }
    }
}
