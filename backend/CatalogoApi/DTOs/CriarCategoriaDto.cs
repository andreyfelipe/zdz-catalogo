using System;
using System.ComponentModel.DataAnnotations;
namespace CatalogoApi.DTOs;
public class CriarCategoriaDto
{
    [Required(ErrorMessage = "O campo Nome é obrigatório.")]
    [MinLength(5, ErrorMessage = "O Nome deve ter no mínimo 5 caracteres.")]
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}