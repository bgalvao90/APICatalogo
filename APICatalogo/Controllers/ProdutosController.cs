using APICatalogo.Context;
using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using X.PagedList;

namespace APICatalogo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    //[ApiExplorerSettings(IgnoreApi = true)]
    public class ProdutosController : ControllerBase
    {
        private readonly IUnitOfWork _uof;
        private readonly ILogger<ProdutosController> _logger;
        private readonly IMapper _mapper;

        public ProdutosController(IUnitOfWork uof, IMapper mapper) //ILogger<ProdutosController> logger)
        {
            _uof = uof;
            //_logger = logger;
            _mapper = mapper;
        }

        //public ProdutosController(IUnitOfWork uof, IMapper mapper)
        //{
        //    _uof = uof;
        //    _mapper = mapper;
        //}

        [HttpGet("pagination")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> Get([FromQuery] ProdutosParameters produtosParameters)
        {
            var produtos = await _uof.ProdutoRepository.GetProdutosAsync(produtosParameters);
            return ObterProdutos(produtos);
        }
        private ActionResult<IEnumerable<ProdutoDTO>> ObterProdutos(IPagedList<Produto> produtos)
        {
            var metadata = new
            {
                produtos.Count,
                produtos.PageSize,
                produtos.PageCount,
                produtos.TotalItemCount,
                produtos.HasNextPage,
                produtos.HasPreviousPage
            };
            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));
            var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);
            return Ok(produtosDto);
        }
        [HttpGet("filter/preco/pagination")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosFiltroPrecoAsync([FromQuery] ProdutosFiltroPreco produtoFiltroPrecoParameters)
        {
            var prodoutos = await _uof.ProdutoRepository.GetProdutosFiltroPrecoAsync(produtoFiltroPrecoParameters);
            return ObterProdutos(prodoutos);
        }

        /// <summary>
        /// Exibe uma relação de produtos.
        /// </summary>
        /// <returns>Retorna uma lista de objetos Produto</returns>
        [Authorize(Policy = "UserOnly")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> Get()
        {
            try {
            var produtos = await _uof.ProdutoRepository.GetAllAsync();
            if (produtos is null || !produtos.Any())
            {
                return NotFound("Nenhum produto encontrado.");          
                _logger.LogWarning("Nenhum produto encontrado.");

            }
            var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);
            return Ok(produtosDto);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Obtem o produto pelo seu identificador único (ID).
        /// </summary>
        /// <param name="id">Codigo do produto</param>
        /// <returns>Um objeto produto</returns>
        [HttpGet("{id:int}", Name = "ObterProduto")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ProdutoDTO>> Get(int id)
        {
            if (id <= 0)
            {
//                _logger.LogError("ID do produto inválido.");
                return BadRequest("ID do produto inválido.");
            }
            var produtos = await _uof.ProdutoRepository.GetAsync(p => p.ProdutoId == id);
            if (produtos is null)
            {
               // _logger.LogWarning($"Produto com id= {id} não encontrado.");
                return NotFound($"Produto com id= {id} não encontrado.");
            }
            var produtosDto = _mapper.Map<ProdutoDTO>(produtos);
            return Ok(produtosDto);
        }

        [HttpGet("categoria/{id:int}", Name = "ObterProdutoPorCategoria")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosPorCategoriaAsync(int id)
        {
            var produtos = await _uof.ProdutoRepository.GetProdutosPorCategoriaAsync(id);
            if (produtos is null || !produtos.Any())
            {
                _logger.LogWarning($"Nenhum produto encontrado para a categoria com id= {id}.");
                return NotFound($"Nenhum produto encontrado para a categoria com id= {id}.");
            }
            var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);
            return Ok(produtosDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProdutoDTO>> Post(ProdutoDTO produtoDto)
        {
            if (produtoDto is null)
            {
               // _logger.LogError("Dados inválidos para criação do produto.");
                return BadRequest("Dados inválidos para criação do produto.");
            }

            var produto = _mapper.Map<Produto>(produtoDto);

            var produtoCriado = _uof.ProdutoRepository.Create(produto);
            await _uof.CommitAsync();

            var produtoCriadoDto = _mapper.Map<ProdutoDTO>(produtoCriado);
            //_logger.LogInformation($"Produto {produtoCriadoDto.Nome} criado com sucesso.");
            return new CreatedAtRouteResult("ObterProduto", new { id = produtoCriadoDto.ProdutoId }, produtoCriadoDto);
        }


        [HttpPatch("{id}/UpdatePartial")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ProdutoDTOUpdateResponse>> Patch(int id, JsonPatchDocument<ProdutoDTOUpdateRequest> patchProdutoDTO)
        {
            if(patchProdutoDTO is null || id <= 0)
            {
                _logger.LogError("Dados inválidos para atualização parcial do produto.");
                return BadRequest("Dados inválidos para atualização parcial do produto.");
            }
            var produto = await _uof.ProdutoRepository.GetAsync(p => p.ProdutoId == id);
            if (produto is null)
            {
                _logger.LogWarning($"Produto com id= {id} não encontrado.");
                return NotFound($"Produto com id= {id} não encontrado.");
            }
            var produtoUpdateRequet = _mapper.Map<ProdutoDTOUpdateRequest>(produto);
            patchProdutoDTO.ApplyTo(produtoUpdateRequet, ModelState);
            if (!ModelState.IsValid || !TryValidateModel(produtoUpdateRequet))
            {
                _logger.LogError("Dados inválidos para atualização parcial do produto.");
                return BadRequest(ModelState);
            }
            _mapper.Map(produtoUpdateRequet, produto);
            _uof.ProdutoRepository.Update(produto);
            await _uof.CommitAsync();

            return Ok(_mapper.Map<ProdutoDTOUpdateRequest>(produto));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ProdutoDTO>> Put(int id, ProdutoDTO produtoDto)
        {
            if (id != produtoDto.ProdutoId)
            {
                //_logger.LogError("ID do produto não corresponde ao ID fornecido.");
                return BadRequest("ID do produto não corresponde ao ID fornecido.");
            }
            var produto = _mapper.Map<Produto>(produtoDto);

            var produtoAtualizado = _uof.ProdutoRepository.Update(produto);
            await _uof.CommitAsync();

            var produtoAtualizadoDto = _mapper.Map<ProdutoDTO>(produtoAtualizado);

            //_logger.LogInformation($"Produto com id={id} atualizado com sucesso.");
            return Ok($"Produto {produtoAtualizadoDto} atualizado com sucesso.");
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ProdutoDTO>> Delete(int id)
        {
            var produto = await _uof.ProdutoRepository.GetAsync(p => p.ProdutoId == id);
            if (produto is null)
            {
               // _logger.LogWarning($"Produto com id= {id} não encontrado.");
                return NotFound($"Produto com id= {id} não encontrado.");
            }
            var produtoDeletado =  _uof.ProdutoRepository.Delete(produto);
            await _uof.CommitAsync();

            var produtoDeletadoDto = _mapper.Map<ProdutoDTO>(produtoDeletado);
           // _logger.LogInformation($"Produto com id={id} deletado com sucesso.");
            return Ok($"Produto {produtoDeletadoDto} deletado com sucesso.");
        }
    }
}
