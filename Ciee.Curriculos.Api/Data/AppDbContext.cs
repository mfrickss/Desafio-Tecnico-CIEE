using Ciee.Curriculos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ciee.Curriculos.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Candidato> Candidatos => Set<Candidato>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Candidato>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.NomeCompleto).IsRequired().HasMaxLength(150);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(150);
            entity.Property(c => c.Telefone).HasMaxLength(30);
            entity.Property(c => c.CargoInteresse).HasMaxLength(100);
            entity.Property(c => c.ResumoProfissional).HasMaxLength(2000);
            entity.Property(c => c.DataCadastro).IsRequired();
            entity.Property(c => c.TeveOrigemPdf).IsRequired();

            entity.HasIndex(c => c.Email);
        });
    }
}
