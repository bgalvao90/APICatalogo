using APICatalogo.Context;
using APICatalogo.Models;

namespace APICatalogo.Repositories
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly AppDbContext _context;

        public ProdutoRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "Contexto não pode ser nulo");
        }
        public Produto Create(Produto produto)
        {
            if(produto is null)
            {
                throw new ArgumentNullException(nameof(produto), "Produto não pode ser nulo");
            }
            _context.Produtos.Add(produto);
            _context.SaveChanges();
            return produto;
        }

        public Produto Delete(int id)
        {
            var produto = _context.Produtos.Find(id);
            if (produto == null)
            {
                throw new InvalidOperationException($"Produto com ID {id} não encontrado");
            }
            _context.Produtos.Remove(produto);
            _context.SaveChanges();
            return produto;
        }

        public Produto GetProdutoId(int id)
        {
            var produto = _context.Produtos.FirstOrDefault(p => p.ProdutoId == id);
            if (produto == null)
            {
                throw new InvalidOperationException($"Produto com ID {id} não encontrado");
            }
            return produto;
        }

        public IEnumerable<Produto> GetProdutos()
        {
            if (_context.Produtos == null || !_context.Produtos.Any())
            {
                throw new InvalidOperationException("Nenhum produto encontrado");
            }
            return _context.Produtos.ToList();
        }

        public IEnumerable<Produto> GetProdutosPorCategoria(int categoriaId)
        {
            var produtos = _context.Produtos.Where(p => p.CategoriaId == categoriaId).ToList();
            if (produtos == null || !produtos.Any())
            {
                throw new InvalidOperationException($"Nenhum produto encontrado para a categoria com ID {categoriaId}");
            }
            return produtos;
        }

        public Produto Update(Produto produto)
        {
            if(produto is null)
            {
                throw new ArgumentNullException(nameof(produto), "Produto não pode ser nulo");
            }
            _context.Entry(produto).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();
            return produto;
        }
    }
}
