using APICatalogo.Context;
using APICatalogo.Filters;
using APICatalogo.Repositories;
using APICatalogo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaRepository _repository;
        private readonly ILogger<CategoriasController> _logger;

        public CategoriasController(ICategoriaRepository repository, ILogger<CategoriasController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        [ServiceFilter(typeof(ApiLoggingFilter))]
        public ActionResult<IEnumerable<Categoria>> Get()
        {
            var categorias = _repository.GetCategorias();
            return Ok(categorias);

        }

        [HttpGet("{id:int}", Name = "ObterCategoria")]
        public ActionResult<Categoria> Get(int id)
        {
            var categoria = _repository.GetCategoria(id);
            if (categoria is null)
            {
                _logger.LogWarning($"Categoria com id= {id} não encontrada...");
                return NotFound($"Categoria com id= {id} não encontrada...");
            }
            return Ok(categoria);
        }

        [HttpPost]
        public ActionResult<Categoria> Post(Categoria categoria)
        {
            if (categoria is null)
            {
                _logger.LogError("Dados inválidos...");
                return BadRequest("Dados inválidos...");
            }
            var categoriaCriada = _repository.Create(categoria);
            _logger.LogInformation($"Categoria {categoria.Nome} criada com sucesso.");
            return new CreatedAtRouteResult("ObterCategoria", new { id = categoriaCriada.CategoriaId }, categoriaCriada);
        }
        [HttpPut("{id:int}")]
        public ActionResult Put(int id, Categoria categoria)
        {
            if (id != categoria.CategoriaId)
            {
                _logger.LogError($"Erro ao atualizar a categoria com id={id}.");
                return BadRequest($"Erro ao atualizar a categoria com id={id}.");
            }
            _repository.Update(categoria);
            _logger.LogInformation($"Categoria {categoria.Nome} atualizada com sucesso.");
            return Ok(categoria);
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            var categoria = _repository.GetCategoria(id);
            if (categoria is null)
            {
                _logger.LogWarning($"Categoria com id= {id} não encontrada para exclusão.");
                return NotFound($"Categoria com id= {id} não encontrada para exclusão.");
            }
            var categoriaDeletar = _repository.Delete(id);
            _logger.LogInformation($"Categoria {categoriaDeletar.Nome} excluída com sucesso.");
            return Ok(categoriaDeletar);
        }
    }
}
