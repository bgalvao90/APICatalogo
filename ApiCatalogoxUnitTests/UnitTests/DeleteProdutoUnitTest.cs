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
    public class DeleteProdutoUnitTest : IClassFixture<ProdutosUnitTestController>
    {
        private readonly ProdutosController _controller;
        public DeleteProdutoUnitTest(ProdutosUnitTestController controller)
        {
            _controller = new ProdutosController(controller.repository, controller.mapper);
        }
        // Metodos de teste para DELETE
        // Implementar os testes para o método Delete do controller
        [Fact]
        public async Task DeleteProduto_Return_OkStatusCode()
        {
            // Arrange
            var id = 2; // ID do produto que será deletado, deve existir no banco de dados.
            // Act
            var result = await _controller.Delete(id) as ActionResult<ProdutoDTO>; // Chama metodo Delete do controller passando o id do produto.
            // Assert
            result.Should().NotBeNull(); // Verifica se o resultado não é nulo  
            result.Result.Should().BeOfType<OkObjectResult>(); // Verifica se o resultado é do tipo OkResult
        }
        [Fact]
        public async Task DeleteProduto_Return_NotFoundStatusCode()
        {
            // Arrange
            var id = 999; // ID do produto que não existe no banco de dados.
            // Act
            var result = await _controller.Delete(id) as ActionResult<ProdutoDTO>; // Chama metodo Delete do controller passando o id do produto.
            // Assert
            result.Should().NotBeNull(); // Verifica se o resultado não é nulo  
            result.Result.Should().BeOfType<NotFoundObjectResult>(); // Verifica se o resultado é do tipo NotFoundResult
        }
    }
}
