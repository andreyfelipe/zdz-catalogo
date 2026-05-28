using System;
using Microsoft.EntityFrameworkCore;
using CatalogoApi.Models;

namespace CatalogoApi.Data
{
    public class ContextoBancoDados : DbContext
{
    public ContextoBancoDados(DbContextOptions<ContextoBancoDados> opcoes)
        : base(opcoes) { }

    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Produto> Produtos { get; set; }

    protected override void OnModelCreating(ModelBuilder construtor)
    {
        construtor.Entity<Produto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Produtos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        construtor.Entity<Categoria>()
            .Property(c => c.Nome).IsRequired().HasMaxLength(200);

        construtor.Entity<Produto>()
            .Property(p => p.Nome).IsRequired().HasMaxLength(200);

        construtor.Entity<Produto>()
            .Property(p => p.Preco).HasColumnType("decimal(18,2)");
    }
}
}