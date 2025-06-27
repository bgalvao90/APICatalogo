using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;

namespace APICatalogo.Repositories
{
    public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
    {
        public CategoriaRepository(AppDbContext context) : base(context)
        {
        }

        public PagedList<Categoria> GetCategorias(CategoriasParameters categoriasParames)
        {
           var categorias = GetAll().OrderBy(c => c.CategoriaId).AsQueryable();
            var categoriasOrdenadas = PagedList<Categoria>.ToPagedList(categorias, categoriasParames.PageNumber, categoriasParames.PageSize);
            return categoriasOrdenadas;
        }
        public PagedList<Categoria> GetCategoriasFiltroNome(CategoriasFiltroNome categoriasFiltroNomeParams)
        {
            var categorias = GetAll().AsQueryable();
            if (!string.IsNullOrEmpty(categoriasFiltroNomeParams.Nome))
            {
                categorias = categorias.Where(c => c.Nome.Contains(categoriasFiltroNomeParams.Nome, StringComparison.OrdinalIgnoreCase));
            }
            var categoriasFiltradas = PagedList<Categoria>.ToPagedList(categorias, categoriasFiltroNomeParams.PageNumber, categoriasFiltroNomeParams.PageSize);
            return categoriasFiltradas;
        }
    }
}
