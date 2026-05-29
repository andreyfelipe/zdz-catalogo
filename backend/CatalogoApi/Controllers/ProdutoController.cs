using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CatalogoApi.Data;
using CatalogoApi.Models;
using CatalogoApi.DTOs;

namespace CatalogoApi.Controllers
{
    [ApiController]
    [Route("api/produtos")]
    public class ProdutoController : Controller
    {
        private readonly ContextoBancoDados _contexto;

        public ProdutoController(ContextoBancoDados contexto)
        {
            _contexto = contexto;
        }

        // GET /api/produtos - Retorna todos os produtos com categoria incluída
        [HttpGet]
        public async Task<ActionResult<List<Produto>>> ObterTodos()
        {
            var produtos = await _contexto.Produtos
                .Include(p => p.Categoria)
                .AsNoTracking()
                .ToListAsync();
            return Ok(produtos);
        }

        // GET /api/produtos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> ObterPorId(int id)
        {
            var produto = await _contexto.Produtos
                .Include(p => p.Categoria)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
            if (produto == null)
                return NotFound();
            return Ok(produto);
        }

        // POST /api/produtos - Criar novo produto
        [HttpPost]
        public async Task<ActionResult<Produto>> Criar(CriarProdutoDto produtoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar se categoria existe
            var categoria = await _contexto.Categorias.FindAsync(produtoDto.CategoriaId);
            if (categoria == null)
                return BadRequest(new { mensagem = "Categoria não encontrada." });

            var produto = new Produto
            {
                Nome = produtoDto.Nome,
                Descricao = produtoDto.Descricao,
                Preco = produtoDto.Preco,
                CategoriaId = produtoDto.CategoriaId
            };

            _contexto.Produtos.Add(produto);
            await _contexto.SaveChangesAsync();
            return CreatedAtAction(nameof(ObterPorId), new { id = produto.Id }, produto);
        }

        // PUT /api/produtos/{id} - Atualizar produto
        [HttpPut("{id}")]
        public async Task<ActionResult> Atualizar(int id, AtualizarProdutoDto produtoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var produto = await _contexto.Produtos.FindAsync(id);
            if (produto == null)
                return NotFound();

            // Verificar se categoria existe (se foi alterada)
            if (produto.CategoriaId != produtoDto.CategoriaId)
            {
                var categoria = await _contexto.Categorias.FindAsync(produtoDto.CategoriaId);
                if (categoria == null)
                    return BadRequest(new { mensagem = "Categoria não encontrada." });
            }

            produto.Nome = produtoDto.Nome;
            produto.Descricao = produtoDto.Descricao;
            produto.Preco = produtoDto.Preco;
            produto.CategoriaId = produtoDto.CategoriaId;

            await _contexto.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/produtos/{id} - Deletar produto
        [HttpDelete("{id}")]
        public async Task<ActionResult> Deletar(int id)
        {
            var produto = await _contexto.Produtos.FindAsync(id);
            if (produto == null)
                return NotFound();

            _contexto.Produtos.Remove(produto);
            await _contexto.SaveChangesAsync();
            return NoContent();
        }
    }
}
