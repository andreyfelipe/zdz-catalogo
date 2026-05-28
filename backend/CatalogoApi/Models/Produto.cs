using System;
using CatalogoApi.Models;
namespace CatalogoApi.Models;
public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

}