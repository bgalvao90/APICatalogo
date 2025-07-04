using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APICatalogo.Controllers;
using APICatalogo.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace ApiCatalogoxUnitTests.UnitTests
{
    public class PutProdutoUnitTest : IClassFixture<ProdutosUnitTestController>
    {
        private readonly ProdutosController _controller;

        public PutProdutoUnitTest(ProdutosUnitTestController controller)
        {
            _controller = new ProdutosController(controller.repository, controller.mapper);
        }

        // Metodos de teste para PUT
        [Fact]
        public async Task PutProduto_Return_OkStatusCode()
        {
            // Arrange
            var id = 1; // ID do produto que será atualizado, deve existir no banco de dados.
            var updateProduto = new ProdutoDTO // Instanciando um produto.
            {
                ProdutoId = id,
                Nome = "Produto Atualizado",
                Descricao = "Descricao do Produto Atualizado",
                ImagemUrl = "imagem_atualizada.jpg",
                CategoriaId = 1
            };
            // Act
            var result = await _controller.Put(id, updateProduto) as ActionResult<ProdutoDTO>; // Chama metodo Put do controller passando o produto.
            // Assert
            result.Should().NotBeNull(); // Verifica se o resultado não é nulo  
            result.Result.Should().BeOfType<OkObjectResult>(); // Verifica se o resultado é do tipo OkObjectResult
        }

        [Fact]
        public async Task PutProduto_Return_BadRequestCode()
        {
            // Arrange
            var updateProduto = new ProdutoDTO // Instanciando um produto.
            {
                ProdutoId = 3,
                Nome = "Produto Atualizado",
                Descricao = "Descricao do Produto Atualizado",
                ImagemUrl = "imagem_atualizada.jpg",
                CategoriaId = 1
            };// Instanciando um produto nulo para simular erro de validação.
            // Act
            var data = await _controller.Put(0, updateProduto); // Chama metodo Put do controller passando o produto.
            // Assert
            data.Should().NotBeNull(); // Verifica se o resultado não é nulo
            data.Result.Should().BeOfType<BadRequestObjectResult>(); // Verifica se o resultado é do tipo BadRequestObjectResult
        }
    }
}
