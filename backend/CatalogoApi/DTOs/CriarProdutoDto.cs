using System;
using System.ComponentModel.DataAnnotations;
namespace CatalogoApi.DTOs;

public class CriarProdutoDto
{
    [Required(ErrorMessage = "O campo Nome é obrigatório.")]
    [MinLength(5, ErrorMessage = "O Nome deve ter no mínimo 5 caracteres.")]
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    [Range(0.01, double.MaxValue, ErrorMessage = "Preço deve ser maior que zero.")]
    public decimal Preco { get; set; }
    [Required]
    public int CategoriaId { get; set; }
}