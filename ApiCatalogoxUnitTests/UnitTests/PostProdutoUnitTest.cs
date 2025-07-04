using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APICatalogo.Models;
using APICatalogo.Controllers;
using APICatalogo.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace ApiCatalogoxUnitTests.UnitTests
{
    public class PostProdutoUnitTest : IClassFixture<ProdutosUnitTestController>
    {
        private readonly ProdutosController _controller;

        public PostProdutoUnitTest(ProdutosUnitTestController controller)
        {
            _controller = new ProdutosController(controller.repository, controller.mapper);
        }

        //Metodos de teste para POST

        [Fact]
        public async Task PostProduto_Return_CreatedStatusCode()
        {
            //Arrange
            var produto = new ProdutoDTO //Instanciando um produto.
            {
                Nome = "Produto Teste",
                Descricao = "Descricao do Produto Teste",
                Preco = 100.00m,
                ImagemUrl = "imagem.jpg",
                CategoriaId = 1
            };
            //Act
            var data = await _controller.Post(produto);  //Chama metodo Post do controller passando o produto.

            //Assert
            var createdResult = data.Result.Should().BeOfType<CreatedAtRouteResult>(); // Verifica se o resultado é do tipo CreatedAtActionResult
            createdResult.Subject.StatusCode.Should().Be(201); // Verifica se o status code é 201 (Created)
        }

        [Fact]
        public async Task PostProduto_Return_BadRequestCode()
        {
            //Arrange
            ProdutoDTO produto = null; //Instanciando um produto nulo para simular erro de validação.
            //Act
            var data = await _controller.Post(produto); //Chama metodo Post do controller passando o produto.

            //Assert
            var createdResult = data.Result.Should().BeOfType<BadRequestObjectResult>(); // Verifica se o resultado é do tipo BadRequestObjectResult
            createdResult.Subject.StatusCode.Should().Be(400);  // Verifica se o status code é 400 (Bad Request)
        }
    }
}