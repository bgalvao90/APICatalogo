using APICatalogo.Context;
using APICatalogo.Models;

namespace APICatalogo.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AppDbContext _context;
        public CategoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Categoria> GetCategorias()
        {
            var categoria = _context.Categorias.ToList();
            if (categoria == null || !categoria.Any())
            {
                throw new InvalidOperationException("Nenhuma categoria encontrada");
            }
            return categoria;
        }
        public Categoria GetCategoria(int id)
        {
            var categoria = _context.Categorias.FirstOrDefault(c => c.CategoriaId == id);
            if (categoria == null)
            {
                throw new InvalidOperationException($"Categoria com ID {id} não encontrada");
            }
            return categoria;
        }

        public Categoria Create(Categoria categoria)
        {
            if (categoria != null)
            {
                _context.Categorias.Add(categoria);
                _context.SaveChanges();
                return categoria;
            }
            throw new ArgumentNullException(nameof(categoria), "Categoria não pode ser nula");
        }

        public Categoria Update(Categoria categoria)
        {
            if (categoria != null)
            {
                _context.Entry(categoria).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _context.SaveChanges();
                return categoria;
            }
            throw new ArgumentNullException(nameof(categoria), "Categoria não pode ser nula");
        }

        public Categoria Delete(int id)
        {
            var categoria = _context.Categorias.Find(id);
            if (categoria != null)
            {
                _context.Categorias.Remove(categoria);
                _context.SaveChanges();
                return categoria;
            }
            throw new ArgumentNullException($"Categoria com ID {id} não encontrada");
        }
    }
}
