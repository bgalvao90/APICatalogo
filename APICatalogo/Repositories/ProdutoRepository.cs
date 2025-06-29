using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using X.PagedList;

namespace APICatalogo.Repositories
{
    public class ProdutoRepository : Repository<Produto>, IProdutoRepository
    {

        public ProdutoRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IPagedList<Produto>> GetProdutosAsync(ProdutosParameters produtosParames)
        {
            var produtos = await GetAllAsync();
            var produtosOrdenados = produtos.OrderBy(p => p.ProdutoId).AsQueryable();
            //var resultado = PagedList<Produto>.ToPagedList(produtosOrdenados, produtosParames.PageNumber, produtosParames.PageSize);
            var resultado = await produtosOrdenados.ToPagedListAsync(
                produtosParames.PageNumber,
                produtosParames.PageSize
            );
            return resultado;
        }

        public async Task<IPagedList<Produto>> GetProdutosFiltroPrecoAsync(ProdutosFiltroPreco produtosFiltroPrecoParams)
        {
            var produtos = await GetAllAsync();
            if (produtosFiltroPrecoParams.Preco.HasValue && !string.IsNullOrEmpty(produtosFiltroPrecoParams.PrecoCriterio))
            {
                if (produtosFiltroPrecoParams.PrecoCriterio.Equals("maior", StringComparison.OrdinalIgnoreCase))
                {
                    produtos = produtos.Where(p => p.Preco > produtosFiltroPrecoParams.Preco.Value);
                }
                else if (produtosFiltroPrecoParams.PrecoCriterio.Equals("menor", StringComparison.OrdinalIgnoreCase))
                {
                    produtos = produtos.Where(p => p.Preco < produtosFiltroPrecoParams.Preco.Value);

                }
                else if (produtosFiltroPrecoParams.PrecoCriterio.Equals("igual", StringComparison.OrdinalIgnoreCase))
                {
                    produtos = produtos.Where(p => p.Preco == produtosFiltroPrecoParams.Preco.Value);
                }
            }
            var produtosFiltrados = await produtos.ToPagedListAsync(
                produtosFiltroPrecoParams.PageNumber,
                produtosFiltroPrecoParams.PageSize
            );

            return produtosFiltrados;
        }
        public async Task<IEnumerable<Produto>> GetProdutosPorCategoriaAsync(int id)
        {
            var produtos = await GetAllAsync();
            var produtosCategoria = produtos.Where(p => p.CategoriaId == id);
            return produtosCategoria;
        }
    }
}
