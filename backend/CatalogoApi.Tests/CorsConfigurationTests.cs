using System.Net;

namespace CatalogoApi.Tests;

/// <summary>
/// AC 06 — Segurança de Comunicação (CORS)
/// Verifica que a política de CORS permite apenas origens específicas
/// e proíbe o uso de AllowAnyOrigin (*).
/// </summary>
public class CorsConfigurationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CorsConfigurationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private HttpRequestMessage CriarPreflightRequest(string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/categorias");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "Content-Type");
        return request;
    }

    [Theory(DisplayName = "AC06 - Origens permitidas devem receber cabecalho Access-Control-Allow-Origin correto")]
    [InlineData("http://localhost:3000")]
    [InlineData("http://localhost:3001")]
    public async Task CORS_OrigemPermitida_DeveRetornarCabecalhoAccessControl(string origin)
    {
        var request = CriarPreflightRequest(origin);
        var resposta = await _client.SendAsync(request);

        Assert.True(
            resposta.Headers.Contains("Access-Control-Allow-Origin"),
            $"Resposta deve conter 'Access-Control-Allow-Origin' para origem permitida: {origin}");

        var valorHeader = resposta.Headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault();
        Assert.Equal(origin, valorHeader);
    }

    [Fact(DisplayName = "AC06 - CORS nao deve usar AllowAnyOrigin (*) — proibido pela especificacao")]
    public async Task CORS_NaoDeveUsarAllowAnyOrigin()
    {
        var request = CriarPreflightRequest("http://origem-nao-autorizada.com");
        var resposta = await _client.SendAsync(request);

        if (resposta.Headers.TryGetValues("Access-Control-Allow-Origin", out var valores))
        {
            Assert.DoesNotContain("*", valores);
            Assert.DoesNotContain("http://origem-nao-autorizada.com", valores);
        }
        // Se o header não estiver presente, a origem foi bloqueada — comportamento correto
    }

    [Fact(DisplayName = "AC06 - Origem nao listada nao deve receber acesso liberado")]
    public async Task CORS_OrigemNaoListada_NaoDeveReceberAcesso()
    {
        var request = CriarPreflightRequest("http://atacante.com");
        var resposta = await _client.SendAsync(request);

        var temAcessoTotal = resposta.Headers.TryGetValues("Access-Control-Allow-Origin", out var valores)
            && valores.Any(v => v == "*" || v == "http://atacante.com");

        Assert.False(temAcessoTotal,
            "Origem não cadastrada não deve receber Access-Control-Allow-Origin com acesso liberado");
    }

    [Fact(DisplayName = "AC06 - Requisicao normal de origem permitida deve retornar 200")]
    public async Task GET_OrigemPermitida_DeveRetornar200()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/categorias");
        request.Headers.Add("Origin", "http://localhost:3001");

        var resposta = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }
}
