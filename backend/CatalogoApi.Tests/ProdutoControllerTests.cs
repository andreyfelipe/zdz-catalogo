using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CatalogoApi.Tests;

/// <summary>
/// AC 03 — Contrato de Endpoints de Produto (incluindo .Include() na categoria)
/// AC 05 — Validação de Payload (Nome)
/// </summary>
public class ProdutoControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProdutoControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<int> CriarCategoriaAsync(string nome)
    {
        var resposta = await _client.PostAsJsonAsync("/api/categorias", new { nome });
        resposta.EnsureSuccessStatusCode();
        var json = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetInt32();
    }

    private async Task<int> CriarProdutoAsync(int categoriaId, string nome = "Produto Valido")
    {
        var payload = new { nome, descricao = "Descrição do produto", preco = 99.90, categoriaId };
        var resposta = await _client.PostAsJsonAsync("/api/produtos", payload);
        resposta.EnsureSuccessStatusCode();
        var json = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetInt32();
    }

    // ── AC 03 ──────────────────────────────────────────────────────────────

    [Fact(DisplayName = "AC03 - GET /api/produtos deve retornar 200 OK")]
    public async Task GET_Produtos_DeveRetornar200()
    {
        var resposta = await _client.GetAsync("/api/produtos");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact(DisplayName = "AC03 - GET /api/produtos deve retornar array JSON")]
    public async Task GET_Produtos_DeveRetornarArrayJson()
    {
        _factory.LimparBanco();
        var catId = await CriarCategoriaAsync("Categoria Array Prod");
        await CriarProdutoAsync(catId, "Produto Array Um");
        await CriarProdutoAsync(catId, "Produto Array Dois");

        var resposta = await _client.GetAsync("/api/produtos");
        var json = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(JsonValueKind.Array, json.ValueKind);
        Assert.True(json.GetArrayLength() >= 2);
    }

    [Fact(DisplayName = "AC03 - GET /api/produtos deve incluir objeto categoria aninhado via .Include()")]
    public async Task GET_Produtos_DeveIncluirCategoriaAninhada()
    {
        _factory.LimparBanco();
        var catId = await CriarCategoriaAsync("Categoria Include");
        await CriarProdutoAsync(catId, "Produto Include Teste");

        var resposta = await _client.GetAsync("/api/produtos");
        var produtos = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        var produto = produtos.EnumerateArray().First();
        Assert.True(
            produto.TryGetProperty("categoria", out var categoria),
            "Produto deve conter a propriedade 'categoria' aninhada");
        Assert.NotEqual(JsonValueKind.Null, categoria.ValueKind);
        Assert.True(
            categoria.TryGetProperty("nome", out _),
            "Objeto categoria aninhado deve conter 'nome'");
    }

    [Fact(DisplayName = "AC03 - GET /api/produtos - categoria aninhada deve corresponder a categoria criada")]
    public async Task GET_Produtos_CategoriaAninhada_DeveCorresponderACategoriaCriada()
    {
        _factory.LimparBanco();
        var catId = await CriarCategoriaAsync("Eletrodomesticos");
        await CriarProdutoAsync(catId, "Produto Eletro Teste");

        var resposta = await _client.GetAsync("/api/produtos");
        var produtos = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        var produto = produtos.EnumerateArray().First();
        var nomeCategoria = produto.GetProperty("categoria").GetProperty("nome").GetString();
        Assert.Equal("Eletrodomesticos", nomeCategoria);
    }

    [Fact(DisplayName = "AC03 - POST /api/produtos com dados validos deve retornar 201 Created")]
    public async Task POST_Produto_ComDadosValidos_DeveRetornar201()
    {
        _factory.LimparBanco();
        var catId = await CriarCategoriaAsync("Cat Post Prod Valido");
        var payload = new { nome = "Produto Criacao", descricao = "Descricao", preco = 50.0, categoriaId = catId };
        var resposta = await _client.PostAsJsonAsync("/api/produtos", payload);
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Fact(DisplayName = "AC03 - POST /api/produtos deve retornar entidade criada com Id")]
    public async Task POST_Produto_DeveRetornarEntidadeComId()
    {
        _factory.LimparBanco();
        var catId = await CriarCategoriaAsync("Cat Post Prod Id");
        var payload = new { nome = "Produto Com Id", descricao = "Desc", preco = 150.0, categoriaId = catId };
        var resposta = await _client.PostAsJsonAsync("/api/produtos", payload);
        var json = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(json.TryGetProperty("id", out var id));
        Assert.True(id.GetInt32() > 0);
    }

    [Fact(DisplayName = "AC03 - PUT /api/produtos/{id} com dados validos deve retornar 204 NoContent")]
    public async Task PUT_Produto_ComDadosValidos_DeveRetornar204()
    {
        _factory.LimparBanco();
        var catId = await CriarCategoriaAsync("Cat Put Produto");
        var prodId = await CriarProdutoAsync(catId, "Produto Para Atualizar");
        var payload = new { nome = "Produto Atualizado", descricao = "Nova Desc", preco = 200.0, categoriaId = catId };

        var resposta = await _client.PutAsJsonAsync($"/api/produtos/{prodId}", payload);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
    }

    [Fact(DisplayName = "AC03 - DELETE /api/produtos/{id} deve retornar 204 NoContent")]
    public async Task DELETE_Produto_DeveRetornar204()
    {
        _factory.LimparBanco();
        var catId = await CriarCategoriaAsync("Cat Delete Produto");
        var prodId = await CriarProdutoAsync(catId, "Produto Para Deletar");

        var resposta = await _client.DeleteAsync($"/api/produtos/{prodId}");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
    }

    [Fact(DisplayName = "AC03 - DELETE /api/produtos/{id} deve remover produto do banco")]
    public async Task DELETE_Produto_DeveRemoverDoListagem()
    {
        _factory.LimparBanco();
        var catId = await CriarCategoriaAsync("Cat Delete Verificar");
        var prodId = await CriarProdutoAsync(catId, "Produto Removivel");

        await _client.DeleteAsync($"/api/produtos/{prodId}");

        var resposta = await _client.GetAsync($"/api/produtos/{prodId}");
        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    // ── AC 05 ──────────────────────────────────────────────────────────────

    [Fact(DisplayName = "AC05 - POST /api/produtos com Nome nulo deve retornar 400 Bad Request")]
    public async Task POST_Produto_ComNomeNulo_DeveRetornar400()
    {
        _factory.LimparBanco();
        var catId = await CriarCategoriaAsync("Cat Valida Nome Nulo");
        var payload = new { nome = (string?)null, descricao = "Desc", preco = 10.0, categoriaId = catId };
        var resposta = await _client.PostAsJsonAsync("/api/produtos", payload);
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact(DisplayName = "AC05 - POST /api/produtos com Nome de 4 caracteres deve retornar 400 Bad Request")]
    public async Task POST_Produto_ComNomeMenosDe5Chars_DeveRetornar400()
    {
        _factory.LimparBanco();
        var catId = await CriarCategoriaAsync("Cat Valida Nome Curto");
        var payload = new { nome = "Abcd", descricao = "Desc", preco = 10.0, categoriaId = catId };
        var resposta = await _client.PostAsJsonAsync("/api/produtos", payload);
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact(DisplayName = "AC05 - POST /api/produtos com Nome de exatamente 5 caracteres deve retornar 201")]
    public async Task POST_Produto_ComNomeExatamente5Chars_DeveRetornar201()
    {
        _factory.LimparBanco();
        var catId = await CriarCategoriaAsync("Cat Nome Cinco Chars");
        var payload = new { nome = "Abcde", descricao = "Desc", preco = 10.0, categoriaId = catId };
        var resposta = await _client.PostAsJsonAsync("/api/produtos", payload);
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Fact(DisplayName = "AC05 - PUT /api/produtos/{id} com Nome de 4 caracteres deve retornar 400 Bad Request")]
    public async Task PUT_Produto_ComNomeMenosDe5Chars_DeveRetornar400()
    {
        _factory.LimparBanco();
        var catId = await CriarCategoriaAsync("Cat Put Nome Curto");
        var prodId = await CriarProdutoAsync(catId, "Produto Valido Inicial");
        var payload = new { nome = "Abcd", descricao = "Desc", preco = 50.0, categoriaId = catId };

        var resposta = await _client.PutAsJsonAsync($"/api/produtos/{prodId}", payload);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact(DisplayName = "AC05 - PUT /api/produtos/{id} com Nome nulo deve retornar 400 Bad Request")]
    public async Task PUT_Produto_ComNomeNulo_DeveRetornar400()
    {
        _factory.LimparBanco();
        var catId = await CriarCategoriaAsync("Cat Put Nome Nulo");
        var prodId = await CriarProdutoAsync(catId, "Produto Para Put Nulo");
        var payload = new { nome = (string?)null, descricao = "Desc", preco = 50.0, categoriaId = catId };

        var resposta = await _client.PutAsJsonAsync($"/api/produtos/{prodId}", payload);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }
}
