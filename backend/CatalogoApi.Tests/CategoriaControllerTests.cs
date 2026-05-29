using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CatalogoApi.Tests;

/// <summary>
/// AC 03 — Contrato de Endpoints de Categoria
/// AC 04 — Regra de Integridade Referencial
/// AC 05 — Validação de Payload (Nome)
/// </summary>
public class CategoriaControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CategoriaControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<int> CriarCategoriaAsync(string nome = "Categoria Teste")
    {
        var resposta = await _client.PostAsJsonAsync("/api/categorias", new { nome });
        resposta.EnsureSuccessStatusCode();
        var json = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetInt32();
    }

    private async Task<int> CriarProdutoVinculadoAsync(int categoriaId)
    {
        var payload = new { nome = "Produto Vinculado", descricao = "Desc", preco = 10.0, categoriaId };
        var resposta = await _client.PostAsJsonAsync("/api/produtos", payload);
        resposta.EnsureSuccessStatusCode();
        var json = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetInt32();
    }

    // ── AC 03 ──────────────────────────────────────────────────────────────

    [Fact(DisplayName = "AC03 - GET /api/categorias deve retornar 200 OK")]
    public async Task GET_Categorias_DeveRetornar200()
    {
        var resposta = await _client.GetAsync("/api/categorias");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact(DisplayName = "AC03 - GET /api/categorias deve retornar array JSON")]
    public async Task GET_Categorias_DeveRetornarArrayJson()
    {
        _factory.LimparBanco();
        await CriarCategoriaAsync("Listagem Um");
        await CriarCategoriaAsync("Listagem Dois");

        var resposta = await _client.GetAsync("/api/categorias");
        var json = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(JsonValueKind.Array, json.ValueKind);
        Assert.True(json.GetArrayLength() >= 2);
    }

    [Fact(DisplayName = "AC03 - POST /api/categorias com dados validos deve retornar 201 Created")]
    public async Task POST_Categoria_ComDadosValidos_DeveRetornar201()
    {
        _factory.LimparBanco();
        var resposta = await _client.PostAsJsonAsync("/api/categorias", new { nome = "Eletrônicos" });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Fact(DisplayName = "AC03 - POST /api/categorias deve retornar entidade criada com Id")]
    public async Task POST_Categoria_DeveRetornarEntidadeCriadaComId()
    {
        _factory.LimparBanco();
        var resposta = await _client.PostAsJsonAsync("/api/categorias", new { nome = "Móveis" });
        var json = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(json.TryGetProperty("id", out var id));
        Assert.True(id.GetInt32() > 0);
    }

    [Fact(DisplayName = "AC03 - PUT /api/categorias/{id} com dados validos deve retornar 204 NoContent")]
    public async Task PUT_Categoria_ComDadosValidos_DeveRetornar204()
    {
        _factory.LimparBanco();
        var id = await CriarCategoriaAsync("Nome Original");
        var resposta = await _client.PutAsJsonAsync($"/api/categorias/{id}", new { nome = "Nome Atualizado" });
        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
    }

    [Fact(DisplayName = "AC03 - DELETE /api/categorias/{id} sem produtos deve retornar 204 NoContent")]
    public async Task DELETE_Categoria_SemProdutos_DeveRetornar204()
    {
        _factory.LimparBanco();
        var id = await CriarCategoriaAsync("Para Deletar");
        var resposta = await _client.DeleteAsync($"/api/categorias/{id}");
        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
    }

    // ── AC 04 ──────────────────────────────────────────────────────────────

    [Fact(DisplayName = "AC04 - DELETE /api/categorias/{id} com produtos vinculados deve retornar 409 Conflict")]
    public async Task DELETE_Categoria_ComProdutosVinculados_DeveRetornar409()
    {
        _factory.LimparBanco();
        var categoriaId = await CriarCategoriaAsync("Categoria Com Produtos");
        await CriarProdutoVinculadoAsync(categoriaId);

        var resposta = await _client.DeleteAsync($"/api/categorias/{categoriaId}");

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact(DisplayName = "AC04 - Mensagem de erro deve ser exatamente a especificada")]
    public async Task DELETE_Categoria_ComProdutosVinculados_DeveRetornarMensagemExata()
    {
        _factory.LimparBanco();
        var categoriaId = await CriarCategoriaAsync("Categoria Mensagem");
        await CriarProdutoVinculadoAsync(categoriaId);

        var resposta = await _client.DeleteAsync($"/api/categorias/{categoriaId}");
        var json = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(json.TryGetProperty("mensagem", out var mensagem));
        Assert.Equal(
            "Não é possível excluir uma categoria que possua produtos vinculados.",
            mensagem.GetString());
    }

    [Fact(DisplayName = "AC04 - Categoria com produtos vinculados deve permanecer no banco apos tentativa de exclusao")]
    public async Task DELETE_Categoria_ComProdutos_NaoDeveRemoverCategoria()
    {
        _factory.LimparBanco();
        var categoriaId = await CriarCategoriaAsync("Categoria Persistente");
        await CriarProdutoVinculadoAsync(categoriaId);

        await _client.DeleteAsync($"/api/categorias/{categoriaId}");

        var resposta = await _client.GetAsync($"/api/categorias/{categoriaId}");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    // ── AC 05 ──────────────────────────────────────────────────────────────

    [Fact(DisplayName = "AC05 - POST /api/categorias com Nome nulo deve retornar 400 Bad Request")]
    public async Task POST_Categoria_ComNomeNulo_DeveRetornar400()
    {
        var resposta = await _client.PostAsJsonAsync("/api/categorias", new { nome = (string?)null });
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact(DisplayName = "AC05 - POST /api/categorias com Nome de 4 caracteres deve retornar 400 Bad Request")]
    public async Task POST_Categoria_ComNomeMenosDe5Chars_DeveRetornar400()
    {
        var resposta = await _client.PostAsJsonAsync("/api/categorias", new { nome = "Abcd" });
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact(DisplayName = "AC05 - POST /api/categorias com Nome de exatamente 5 caracteres deve retornar 201")]
    public async Task POST_Categoria_ComNomeExatamente5Chars_DeveRetornar201()
    {
        _factory.LimparBanco();
        var resposta = await _client.PostAsJsonAsync("/api/categorias", new { nome = "Abcde" });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Fact(DisplayName = "AC05 - PUT /api/categorias/{id} com Nome de 4 caracteres deve retornar 400 Bad Request")]
    public async Task PUT_Categoria_ComNomeMenosDe5Chars_DeveRetornar400()
    {
        _factory.LimparBanco();
        var id = await CriarCategoriaAsync("Nome Valido Inicial");
        var resposta = await _client.PutAsJsonAsync($"/api/categorias/{id}", new { nome = "Ab" });
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact(DisplayName = "AC05 - PUT /api/categorias/{id} com Nome nulo deve retornar 400 Bad Request")]
    public async Task PUT_Categoria_ComNomeNulo_DeveRetornar400()
    {
        _factory.LimparBanco();
        var id = await CriarCategoriaAsync("Nome Para Atualizar");
        var resposta = await _client.PutAsJsonAsync($"/api/categorias/{id}", new { nome = (string?)null });
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }
}
