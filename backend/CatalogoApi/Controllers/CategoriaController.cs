using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CatalogoApi.Data;
using CatalogoApi.Models;
using CatalogoApi.DTOs;

namespace CatalogoApi.Controllers
{
    [ApiController]
    [Route("api/categorias")]
    public class CategoriaController : Controller
    {


        // Le o contexto do entity framework para acessar o banco de dados
        private readonly ContextoBancoDados _contexto;

        //Construtor para injetar o contexto do entity framework
        public CategoriaController(ContextoBancoDados contexto)
        {
            _contexto = contexto;
        }

        //HttpGet para obter todas as categorias
        [HttpGet]
        public async Task<ActionResult<List<Categoria>>> ObterTodos()
        {
            var categorias = await _contexto.Categorias.ToListAsync();
            return Ok(categorias);
        }


        //HttpGet para obter uma categoria por id
        [HttpGet("{id}")]
        public async Task<ActionResult<Categoria>> ObterPorId(int id)
        {
            var categoria = await _contexto.Categorias.FindAsync(id);
            if (categoria == null)
                return NotFound();
            return Ok(categoria);
        }

        //HttpPost para criar uma nova categoria
        [HttpPost]
        public async Task<ActionResult<Categoria>> Criar(CriarCategoriaDto categoriaDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var categoria = new Categoria
            {
                Nome = categoriaDto.Nome,
                Descricao = categoriaDto.Descricao
            };
            _contexto.Categorias.Add(categoria);
            await _contexto.SaveChangesAsync();
            return CreatedAtAction(nameof(ObterPorId), new { id = categoria.Id }, categoria);


        }

        //HttpPut para atualizar uma categoria existente
        [HttpPut("{id}")]
        public async Task<ActionResult> Atualizar(int id, AtualizarCategoriaDto categoriaDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var categoria = await _contexto.Categorias.FindAsync(id);
            if (categoria == null)
                return NotFound();
            categoria.Nome = categoriaDto.Nome;
            categoria.Descricao = categoriaDto.Descricao;
            await _contexto.SaveChangesAsync();
            return NoContent();

        }

        //HttpDelete para deletar uma categoria por id
        [HttpDelete("{id}")]
        public async Task<ActionResult> Deletar(int id)
        {
            var categoria = await _contexto.Categorias.Include(c => c.Produtos).FirstOrDefaultAsync(c => c.Id == id);
            if (categoria == null)
                return NotFound();

            // Verificar se a categoria possui produtos vinculados
            if (categoria.Produtos.Any())
                return Conflict(new { mensagem = "Não é possível excluir uma categoria que possua produtos vinculados." });

            _contexto.Categorias.Remove(categoria);
            await _contexto.SaveChangesAsync();
            return NoContent();
        }




    }
}