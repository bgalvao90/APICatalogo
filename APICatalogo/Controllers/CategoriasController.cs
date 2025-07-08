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
using X.PagedList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;

namespace APICatalogo.Controllers
{
    [EnableCors("OrigensComAcessoPermitido")]
    [Route("[controller]")]
    [ApiController]
    [EnableRateLimiting("fixed-window")]
    [Produces("application/json")]
    //[ApiExplorerSettings(IgnoreApi = true)]
    public class CategoriasController : ControllerBase
    {
        private readonly IUnitOfWork _uof;
        private readonly ILogger<CategoriasController> _logger;
        private readonly IMemoryCache _cache;
        private const string CacheCategoriasKey = "CacheCategorias";


        public CategoriasController(IUnitOfWork uof, ILogger<CategoriasController> logger, IMemoryCache cache)
        {
            _uof = uof;
            _logger = logger;
            _cache = cache;
        }


        /// <summary>
        /// Obtem uma lista de objetos por Categoria
        /// </summary>
        /// <returns>Uma lista de Objetos Categoria</returns>
        [HttpGet]
        //[Authorize]
        [DisableRateLimiting]
        [ServiceFilter(typeof(ApiLoggingFilter))]
        public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get()
        {
            if (!_cache.TryGetValue(CacheCategoriasKey, out IEnumerable<CategoriaDTO>? categoriasDto))
            {
                var categorias = await _uof.CategoriaRepository.GetAllAsync();

                if (categorias == null || !categorias.Any())
                {
                    _logger.LogWarning("Nenhuma categoria encontrada.");
                    return NotFound("Nenhuma categoria encontrada.");
                }

                categoriasDto = categorias.ToCategoriaDTOList();
                SetCache(CacheCategoriasKey, categoriasDto);
                _logger.LogInformation("Categorias obtidas do banco de dados e armazenadas no cache.");
            }
            else
            {
                _logger.LogInformation("Categorias obtidas do cache.");
            }

            return Ok(categoriasDto);
        }
        [HttpGet("pagination")]
        public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetAsync([FromQuery] CategoriasParameters categoriasParameters)
        {
            var categorias = await _uof.CategoriaRepository.GetCategoriasAsync(categoriasParameters);
            return ObterCategorias(categorias);

        }
        [HttpGet("filter/nome/pagination")]
        public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetCategoriasFiltroNomeAsync([FromQuery] CategoriasFiltroNome cateriasFiltroNomeParams)
        {
            var categorias = await _uof.CategoriaRepository.GetCategoriasFiltroNomeAsync(cateriasFiltroNomeParams);
            return ObterCategorias(categorias);
        }
        private ActionResult<IEnumerable<CategoriaDTO>> ObterCategorias(IPagedList<Categoria> categorias)
        {
            var metadata = new
            {
                categorias.Count,
                categorias.PageSize,
                categorias.PageCount,
                categorias.TotalItemCount,
                categorias.HasNextPage,
                categorias.HasPreviousPage
            };
            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            var categoriasDto = categorias.ToCategoriaDTOList();
            return Ok(categoriasDto);
        }



        /// <summary>
        /// Obtem uma lista de objetos por Categoria por Id
        /// </summary>
        /// param name="id"></param>
        /// <returns>Objetos Categoria</returns>
        [DisableCors]
        [HttpGet("{id:int}", Name = "ObterCategoria")]
        public async Task<ActionResult<CategoriaDTO>> GetAsync(int id)
        {
            var cacheKey = GetCategoriaCacheKey(id);
            if (!_cache.TryGetValue(cacheKey, out CategoriaDTO? categoriaDto))
            {
                var categoria = await _uof.CategoriaRepository.GetAsync(c => c.CategoriaId == id);
                if (categoria is null)
                {
                    _logger.LogWarning($"Categoria com id= {id} não encontrada...");
                    return NotFound($"Categoria com id= {id} não encontrada...");
                }
                categoriaDto = categoria.ToCategoriaDTO();
                SetCache(cacheKey, categoriaDto);
                _logger.LogInformation($"Categoria com id= {id} obtida do banco de dados e armazenada no cache.");
            }
            else
            {
                _logger.LogInformation($"Categoria com id= {id} obtida do cache.");
            }
            return Ok(categoriaDto);
        }
        /// <summary>
        /// Cria uma nova Categoria
        /// </summary>
        /// <remarks>
        /// Exemplo de Request:
        /// 
        ///     POST api/Categorias
        ///     {
        ///         "CategoriaId": 1,
        ///         "nome": "Categoria Exemplo",
        ///         "imagemUrl": "https://example.com/imagem.jpg"
        /// </remarks>
        /// <param name="categoriaDto">Objeto Categoria</param>
        /// <returns>O objeto Categoria incluida</returns>
        /// <remarks>Retorna um objeto incluído</remarks>
        [HttpPost]
        public async Task<ActionResult<CategoriaDTO>> Post(CategoriaDTO categoriaDto)
        {
            if (categoriaDto is null)
            {
                _logger.LogError("Dados inválidos...");
                return BadRequest("Dados inválidos...");
            }

            var categoria = categoriaDto.ToCategoria();

            var categoriaCriada = _uof.CategoriaRepository.Create(categoria);
            await _uof.CommitAsync();
           InvalidateCacheAfterChange(categoriaCriada.CategoriaId, categoriaCriada); // Invalida o cache após a criação


            var novaCategoriaDTO = categoriaCriada.ToCategoriaDTO();

            _logger.LogInformation($"Categoria {novaCategoriaDTO.Nome} criada com sucesso.");
            return new CreatedAtRouteResult("ObterCategoria", new { id = novaCategoriaDTO.CategoriaId }, novaCategoriaDTO);
        }
        [HttpPut("{id:int}")]
        public async Task<ActionResult<CategoriaDTO>> Put(int id, CategoriaDTO categoriaDto)
        {
            if (id != categoriaDto.CategoriaId)
            {
                _logger.LogError($"Erro ao atualizar a categoria com id={id}.");
                return BadRequest($"Erro ao atualizar a categoria com id={id}.");
            }
            var categoria = categoriaDto.ToCategoria();
            var categoriaAtualizada = _uof.CategoriaRepository.Update(categoria);
            await _uof.CommitAsync();

            InvalidateCacheAfterChange(id, categoriaAtualizada); // Invalida o cache geral de categorias




            var categoriaAtualizadaDto = categoriaAtualizada.ToCategoriaDTO();

            _logger.LogInformation($"Categoria {categoriaAtualizadaDto.Nome} atualizada com sucesso.");
            return Ok(categoriaAtualizadaDto);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<CategoriaDTO>> Delete(int id)
        {
            var categoria = await _uof.CategoriaRepository.GetAsync(c => c.CategoriaId == id);
            if (categoria is null)
            {
                _logger.LogWarning($"Categoria com id= {id} não encontrada para exclusão.");
                return NotFound($"Categoria com id= {id} não encontrada para exclusão.");
            }
            var categoriaDeletar = _uof.CategoriaRepository.Delete(categoria);
            await _uof.CommitAsync();

            InvalidateCacheAfterChange(id); // Invalida o cache geral de categorias

            var categoriaDeletarDto = categoriaDeletar.ToCategoriaDTO();
            _logger.LogInformation($"Categoria {categoriaDeletarDto.Nome} excluída com sucesso.");
            return Ok(categoriaDeletarDto);
        }
        private string GetCategoriaCacheKey(int id) => $"CacheCategoria_{id}";



        private void SetCache<T>(string key, T data)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
                SlidingExpiration = TimeSpan.FromSeconds(15),
                Priority = CacheItemPriority.High
            };
            _cache.Set(key, data, cacheOptions);
            _logger.LogInformation($"Dados armazenados no cache com a chave: {key}");
        }

        private void InvalidateCacheAfterChange(int id, Categoria? categoria = null)
        {
            _cache.Remove(CacheCategoriasKey);
            _cache.Remove(GetCategoriaCacheKey(id));

            if (categoria != null)
            {
                SetCache(GetCategoriaCacheKey(id), categoria.ToCategoriaDTO());
                _logger.LogInformation($"Cache atualizado para a categoria com id={id}.");
            }

            }
    }
}
