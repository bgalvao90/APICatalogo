using APICatalogo.Context;
using APICatalogo.Filters;
using APICatalogo.Repositories;
using APICatalogo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APICatalogo.DTOs;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Pagination;
using Newtonsoft.Json;

namespace APICatalogo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly IUnitOfWork _uof;
        private readonly ILogger<CategoriasController> _logger;


        public CategoriasController(IUnitOfWork uof, ILogger<CategoriasController> logger)
        {
            _uof = uof;
            _logger = logger;
        }

        [HttpGet]
        [ServiceFilter(typeof(ApiLoggingFilter))]
        public ActionResult<IEnumerable<CategoriaDTO>> Get()
        {
            var categorias = _uof.CategoriaRepository.GetAll();
            if (categorias is null || !categorias.Any())
            {
                _logger.LogWarning("Nenhuma categoria encontrada.");
                return NotFound("Nenhuma categoria encontrada.");
            }
            var categoriasDto = categorias.ToCategoriaDTOList();
            return Ok(categoriasDto);

        }
        [HttpGet("pagination")]
        public ActionResult<IEnumerable<CategoriaDTO>> Get([FromQuery] CategoriasParameters categoriasParameters)
        {
            var categorias = _uof.CategoriaRepository.GetCategorias(categoriasParameters);
            return ObterCategorias(categorias);

        }
        [HttpGet("filter/nome/pagination")]
        public ActionResult<IEnumerable<CategoriaDTO>> GetCategoriasFiltroNome([FromQuery] CategoriasFiltroNome cateriasFiltroNomeParams)
        {
            var categorias = _uof.CategoriaRepository.GetCategoriasFiltroNome(cateriasFiltroNomeParams);
            return ObterCategorias(categorias);
        }
        private ActionResult<IEnumerable<CategoriaDTO>> ObterCategorias(PagedList<Categoria> categorias)
        {
            var metadata = new
            {
                categorias.TotalCount,
                categorias.PageSize,
                categorias.CurrentPage,
                categorias.TotalPages,
                categorias.HasNext,
                categorias.HasPrevious
            };
            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            var categoriasDto = categorias.ToCategoriaDTOList();
            return Ok(categoriasDto);
        }

        [HttpGet("{id:int}", Name = "ObterCategoria")]
        public ActionResult<CategoriaDTO> Get(int id)
        {
            var categoria = _uof.CategoriaRepository.Get(c => c.CategoriaId ==id);
            if (categoria is null)
            {
                _logger.LogWarning($"Categoria com id= {id} não encontrada...");
                return NotFound($"Categoria com id= {id} não encontrada...");
            }

            var categoriaDto = categoria.ToCategoriaDTO();
            return Ok(categoriaDto);
        }

        [HttpPost]
        public ActionResult<CategoriaDTO> Post(CategoriaDTO categoriaDto)
        {
            if (categoriaDto is null)
            {
                _logger.LogError("Dados inválidos...");
                return BadRequest("Dados inválidos...");
            }

           var categoria = categoriaDto.ToCategoria();

            var categoriaCriada = _uof.CategoriaRepository.Create(categoria);
            _uof.Commit();

            var novaCategoriaDTO = categoriaCriada.ToCategoriaDTO();

            _logger.LogInformation($"Categoria {novaCategoriaDTO.Nome} criada com sucesso.");
            return new CreatedAtRouteResult("ObterCategoria", new { id = novaCategoriaDTO.CategoriaId }, novaCategoriaDTO);
        }
        [HttpPut("{id:int}")]
        public ActionResult<CategoriaDTO> Put(int id, CategoriaDTO categoriaDto)
        {
            if (id != categoriaDto.CategoriaId)
            {
                _logger.LogError($"Erro ao atualizar a categoria com id={id}.");
                return BadRequest($"Erro ao atualizar a categoria com id={id}.");
            }
            var categoria = categoriaDto.ToCategoria();
            var categoriaAtualizada = _uof.CategoriaRepository.Update(categoria);
            _uof.Commit();
            
            var categoriaAtualizadaDto = categoriaAtualizada.ToCategoriaDTO();

            _logger.LogInformation($"Categoria {categoriaAtualizadaDto.Nome} atualizada com sucesso.");
            return Ok(categoriaAtualizadaDto);
        }

        [HttpDelete("{id:int}")]
        public ActionResult<CategoriaDTO> Delete(int id)
        {
            var categoria = _uof.CategoriaRepository.Get(c => c.CategoriaId == id);
            if (categoria is null)
            {
                _logger.LogWarning($"Categoria com id= {id} não encontrada para exclusão.");
                return NotFound($"Categoria com id= {id} não encontrada para exclusão.");
            }
            var categoriaDeletar = _uof.CategoriaRepository.Delete(categoria);
            _uof.Commit();

            var categoriaDeletarDto = categoriaDeletar.ToCategoriaDTO();
            _logger.LogInformation($"Categoria {categoriaDeletarDto.Nome} excluída com sucesso.");
            return Ok(categoriaDeletarDto);
        }
    }
}
