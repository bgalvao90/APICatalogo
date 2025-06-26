using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;

namespace APICatalogo.Repositories
{
    public class ProdutoRepository : Repository<Produto>, IProdutoRepository
    {

        public ProdutoRepository(AppDbContext context) : base(context)
        {
        }

        public PagedList<Produto> GetProdutos(ProdutosParameters produtosParames)
        {
            var produtos = GetAll().OrderBy(p => p.ProdutoId).AsQueryable();
            var produtosOrdenados = PagedList<Produto>.ToPagedList(produtos, produtosParames.PageNumber, produtosParames.PageSize);
            return produtosOrdenados;
        }

        public IEnumerable<Produto> GetProdutosPorCategoria(int id)
        {
            return GetAll().Where(p => p.CategoriaId == id);
        }
    }
}
