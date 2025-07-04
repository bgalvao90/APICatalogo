using APICatalogo.Controllers;
using APICatalogo.DTOs;
using APICatalogo.Pagination;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ApiCatalogoxUnitTests.UnitTests
{
    public class GetProdutoUnitTest : IClassFixture<ProdutosUnitTestController>
    {
        private readonly ProdutosController _controller;

        public GetProdutoUnitTest(ProdutosUnitTestController controller)
        {
            _controller = new ProdutosController(controller.repository, controller.mapper);
        }

        [Fact]
        public async Task GetProdutoById_Return_OkResult()
        {
            //Arrange
            var produtoId = 1; 

            //Act
            var data = await _controller.Get(produtoId);

            //Assert
            //var okResult = Assert.IsType<OkObjectResult>(data.Result);
            //Assert.Equal(200, okResult.StatusCode);

            //Assert (fluentsasertions)
            data.Result.Should().BeOfType<OkObjectResult>() //verifica se o resultado é do tipo OkObjectResult
                         .Which.StatusCode.Should().Be(200); //verifica se o status code é 200

        }

        [Fact]
        public async Task GetProdutoById_Return_NotFound()
        {

            //Arrange
            var produtoId = 999; 

            //Act
            var data = await _controller.Get(produtoId);

            //Assert (fluentsasertions)
            data.Result.Should().BeOfType<NotFoundObjectResult>() //verifica se o resultado é do tipo NotFoundObjectResult
                         .Which.StatusCode.Should().Be(404); //verifica se o status code é 404
        }

        [Fact]
        public async Task GetProdutoById_Return_BadRequest()
        {
            //Arrange
            var produtoId = 0; 

            //Act
            var data = await _controller.Get(produtoId);


            //Assert (fluentsasertions)
            data.Result.Should().BeOfType<BadRequestObjectResult>() //verifica se o resultado é do tipo BadRequestObjectResult
                         .Which.StatusCode.Should().Be(400); //verifica se o status code é 400
        }

        [Fact]
        public async Task GetProdutos_Return_ListOFProdutoDTO()
        {
            // Arrange

            // Act
            var data = await _controller.Get();
            // Assert
            data.Result.Should().BeOfType<OkObjectResult>() //verifica se o resultado é do tipo OkObjectResult
                         .Which.Value.Should().BeAssignableTo<IEnumerable<ProdutoDTO>>()
                         .And.NotBeNull();//verifica se o valor do OkObjectResult é atribuível a uma coleção de ProdutoDTO e não é nulo

        }

        [Fact]
        public async Task GetProdutos_Return_BadRequestResult()
        {
            // Arrange

            // Act
            var data = await _controller.Get();
            // Assert
            data.Result.Should().BeOfType<BadRequestResult>(); //verifica se o resultado é do tipo BadRequestResult
        }
    }
}
