using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly ILogger<ProdutosController> _logger;

        public ProdutosController(IProdutoRepository produtoRepository, ILogger<ProdutosController> logger)
        {
            _produtoRepository = produtoRepository;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Produto>> Get()
        {
            var produtos = _produtoRepository.GetProdutos();
            if (produtos is null || !produtos.Any())
            {
                _logger.LogWarning("Nenhum produto encontrado.");
                return NotFound("Nenhum produto encontrado.");
            }
            return Ok(produtos);
        }

        [HttpGet("{id:int}", Name = "ObterProduto")]
        public ActionResult<Produto> GetProdutoPorId(int id)
        {
            var produto = _produtoRepository.GetProdutoId(id);
            if (produto is null)
            {
                _logger.LogWarning($"Produto com id= {id} não encontrado.");
                return NotFound($"Produto com id= {id} não encontrado.");
            }
            return Ok(produto);
        }

        [HttpGet("categoria/{id:int}", Name = "ObterProdutoPorCategoria")]
        public ActionResult<IEnumerable<Produto>> GetProdutosPorCategoria(int id)
        {
            var produtos = _produtoRepository.GetProdutosPorCategoria(id);
            if (produtos is null || !produtos.Any())
            {
                _logger.LogWarning($"Nenhum produto encontrado para a categoria com id= {id}.");
                return NotFound($"Nenhum produto encontrado para a categoria com id= {id}.");
            }
            return Ok(produtos);
        }

        [HttpPost]
        public ActionResult Post(Produto produto)
        {
            if (produto is null)
            {
                _logger.LogError("Dados inválidos para criação do produto.");
                return BadRequest("Dados inválidos para criação do produto.");
            }
            var produtoCriado = _produtoRepository.Create(produto);
            _logger.LogInformation($"Produto {produto.Nome} criado com sucesso.");
            return new CreatedAtRouteResult("ObterProduto", new { id = produtoCriado.ProdutoId }, produtoCriado);
        }

        [HttpPut("{id:int}")]
        public ActionResult Put(int id, Produto produto)
        {
            if (id != produto.ProdutoId)
            {
                _logger.LogError("ID do produto não corresponde ao ID fornecido.");
                return BadRequest("ID do produto não corresponde ao ID fornecido.");
            }
            _produtoRepository.Update(produto);
            _logger.LogInformation($"Produto com id={id} atualizado com sucesso.");
            return Ok(produto);
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            var produto = _produtoRepository.GetProdutoId(id);
            if (produto is null)
            {
                _logger.LogWarning($"Produto com id= {id} não encontrado.");
                return NotFound($"Produto com id= {id} não encontrado.");
            }
            _produtoRepository.Delete(id);
            _logger.LogInformation($"Produto com id={id} deletado com sucesso.");
            return Ok($"Produto com id={id} deletado com sucesso.");
        }
    }
}
