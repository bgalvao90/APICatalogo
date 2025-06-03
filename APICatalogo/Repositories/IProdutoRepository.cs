using APICatalogo.Models;

namespace APICatalogo.Repositories
{
    public interface IProdutoRepository
    {
        IEnumerable<Produto> GetProdutos();
        Produto GetProdutoId(int id);
        IEnumerable<Produto> GetProdutosPorCategoria(int categoriaId);
        Produto Create(Produto produto);
        Produto Update(Produto produto);
        Produto Delete(int id);
    }
}
