using CatalogoApi.Models;

namespace CatalogoApi.Tests;

/// <summary>
/// AC 02 — Modelagem Relacional
/// Verifica se os modelos possuem os campos e relacionamentos exigidos.
/// </summary>
public class ModeloTests
{
    [Fact(DisplayName = "AC02 - Categoria deve ter Id, Nome e Descricao")]
    public void Categoria_DeveConterPropriedadesObrigatorias()
    {
        var tipo = typeof(Categoria);

        Assert.NotNull(tipo.GetProperty("Id"));
        Assert.NotNull(tipo.GetProperty("Nome"));
        Assert.NotNull(tipo.GetProperty("Descricao"));
    }

    [Fact(DisplayName = "AC02 - Produto deve ter Id, Nome, Descricao, Preco e CategoriaId")]
    public void Produto_DeveConterPropriedadesObrigatorias()
    {
        var tipo = typeof(Produto);

        Assert.NotNull(tipo.GetProperty("Id"));
        Assert.NotNull(tipo.GetProperty("Nome"));
        Assert.NotNull(tipo.GetProperty("Descricao"));
        Assert.NotNull(tipo.GetProperty("Preco"));
        Assert.NotNull(tipo.GetProperty("CategoriaId"));
    }

    [Fact(DisplayName = "AC02 - Produto deve ter propriedade de navegacao para Categoria (FK 1:N)")]
    public void Produto_DeveConterNavegacaoParaCategoria()
    {
        var tipo = typeof(Produto);
        var propriedade = tipo.GetProperty("Categoria");

        Assert.NotNull(propriedade);
        Assert.Equal(typeof(Categoria), propriedade!.PropertyType);
    }

    [Fact(DisplayName = "AC02 - Categoria deve ter colecao de Produtos (lado N do 1:N)")]
    public void Categoria_DeveConterColecaoDeProdutos()
    {
        var tipo = typeof(Categoria);
        var propriedade = tipo.GetProperty("Produtos");

        Assert.NotNull(propriedade);
        Assert.True(
            typeof(System.Collections.IEnumerable).IsAssignableFrom(propriedade!.PropertyType),
            "Categoria.Produtos deve ser uma coleção (IEnumerable)");
    }

    [Fact(DisplayName = "AC02 - CategoriaId em Produto deve ser do tipo int (chave estrangeira)")]
    public void Produto_CategoriaId_DeveTerTipoInt()
    {
        var prop = typeof(Produto).GetProperty("CategoriaId");
        Assert.Equal(typeof(int), prop!.PropertyType);
    }

    [Fact(DisplayName = "AC02 - Preco em Produto deve ser do tipo decimal")]
    public void Produto_Preco_DeveTerTipoDecimal()
    {
        var prop = typeof(Produto).GetProperty("Preco");
        Assert.Equal(typeof(decimal), prop!.PropertyType);
    }
}
