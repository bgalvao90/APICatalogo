using APICatalogo.Context;
using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace APICatalogo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly IUnitOfWork _uof;
        private readonly ILogger<ProdutosController> _logger;
        private readonly IMapper _mapper;

        public ProdutosController(IUnitOfWork uof, ILogger<ProdutosController> logger, IMapper mapper)
        {
            _uof = uof;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("pagination")]
        public ActionResult<IEnumerable<ProdutoDTO>> Get([FromQuery] ProdutosParameters produtosParameters)
        {
            var produtos = _uof.ProdutoRepository.GetProdutos(produtosParameters);
            var metadata = new    
            {
                produtos.TotalCount,
                produtos.PageSize,
                produtos.CurrentPage,
                produtos.TotalPages,
                produtos.HasNext,
                produtos.HasPrevious
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

            return Ok(produtosDto);
        }

        [HttpGet]
        public ActionResult<IEnumerable<Produto>> Get()
        {
            var produtos = _uof.ProdutoRepository.GetAll();
            if (produtos is null || !produtos.Any())
            {
                _logger.LogWarning("Nenhum produto encontrado.");
                return NotFound("Nenhum produto encontrado.");
            }
            var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);
            return Ok(produtosDto);
        }


        [HttpGet("{id:int}", Name = "ObterProduto")]
        public ActionResult<ProdutoDTO> GetProdutoPorId(int id)
        {
            var produtos = _uof.ProdutoRepository.Get(p => p.ProdutoId == id);
            if (produtos is null)
            {
                _logger.LogWarning($"Produto com id= {id} não encontrado.");
                return NotFound($"Produto com id= {id} não encontrado.");
            }
            var produtosDto = _mapper.Map<ProdutoDTO>(produtos);
            return Ok(produtosDto);
        }

        [HttpGet("categoria/{id:int}", Name = "ObterProdutoPorCategoria")]
        public ActionResult<IEnumerable<ProdutoDTO>> GetProdutosPorCategoria(int id)
        {
            var produtos = _uof.ProdutoRepository.GetProdutosPorCategoria(id);
            if (produtos is null || !produtos.Any())
            {
                _logger.LogWarning($"Nenhum produto encontrado para a categoria com id= {id}.");
                return NotFound($"Nenhum produto encontrado para a categoria com id= {id}.");
            }
            var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);
            return Ok(produtosDto);
        }

        [HttpPost]
        public ActionResult<ProdutoDTO> Post(ProdutoDTO produtoDto)
        {
            if (produtoDto is null)
            {
                _logger.LogError("Dados inválidos para criação do produto.");
                return BadRequest("Dados inválidos para criação do produto.");
            }
            var produto = _mapper.Map<Produto>(produtoDto);
            var produtoCriado = _uof.ProdutoRepository.Create(produto);
            _uof.Commit();
            var produtoCriadoDto = _mapper.Map<ProdutoDTO>(produtoCriado);
            _logger.LogInformation($"Produto {produtoCriadoDto.Nome} criado com sucesso.");
            return new CreatedAtRouteResult("ObterProduto", new { id = produtoCriadoDto.ProdutoId }, produtoCriadoDto);
        }


        [HttpPatch("{id}/UpdatePartial")]
        public ActionResult<ProdutoDTOUpdateResponse> Patch(int id, JsonPatchDocument<ProdutoDTOUpdateRequest> patchProdutoDTO)
        {
            if(patchProdutoDTO is null || id <= 0)
            {
                _logger.LogError("Dados inválidos para atualização parcial do produto.");
                return BadRequest("Dados inválidos para atualização parcial do produto.");
            }
            var produto = _uof.ProdutoRepository.Get(p => p.ProdutoId == id);
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
            _uof.Commit();

            return Ok(_mapper.Map<ProdutoDTOUpdateRequest>(produto));
        }

        [HttpPut("{id:int}")]
        public ActionResult<ProdutoDTO> Put(int id, ProdutoDTO produtoDto)
        {
            if (id != produtoDto.ProdutoId)
            {
                _logger.LogError("ID do produto não corresponde ao ID fornecido.");
                return BadRequest("ID do produto não corresponde ao ID fornecido.");
            }
            var produto = _mapper.Map<Produto>(produtoDto);

            var produtoAtualizado = _uof.ProdutoRepository.Update(produto);
            _uof.Commit();

            var produtoAtualizadoDto = _mapper.Map<ProdutoDTO>(produtoAtualizado);

            _logger.LogInformation($"Produto com id={id} atualizado com sucesso.");
            return Ok($"Produto {produtoAtualizadoDto} atualizado com sucesso.");
        }

        [HttpDelete("{id:int}")]
        public ActionResult<ProdutoDTO> Delete(int id)
        {
            var produto = _uof.ProdutoRepository.Get(p => p.ProdutoId == id);
            if (produto is null)
            {
                _logger.LogWarning($"Produto com id= {id} não encontrado.");
                return NotFound($"Produto com id= {id} não encontrado.");
            }
            var produtoDeletado = _uof.ProdutoRepository.Delete(produto);
            _uof.Commit();

            var produtoDeletadoDto = _mapper.Map<ProdutoDTO>(produtoDeletado);
            _logger.LogInformation($"Produto com id={id} deletado com sucesso.");
            return Ok($"Produto {produtoDeletadoDto} deletado com sucesso.");
        }
    }
}
