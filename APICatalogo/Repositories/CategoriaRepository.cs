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

        public async Task<PagedList<Categoria>> GetCategoriasAsync(CategoriasParameters categoriasParames)
        {
            var categorias = await GetAllAsync();
            if(categorias == null || !categorias.Any())
            {
                return new PagedList<Categoria>(new List<Categoria>(), 0, categoriasParames.PageNumber, categoriasParames.PageSize);
            }
            var categoriasOrdenadas = categorias.OrderBy(c => c.Nome).AsQueryable();
            var resutltado = PagedList<Categoria>.ToPagedList(categoriasOrdenadas, categoriasParames.PageNumber, categoriasParames.PageSize);
            return resutltado;
        }
        public async Task<PagedList<Categoria>> GetCategoriasFiltroNomeAsync(CategoriasFiltroNome categoriasFiltroNomeParams)
        {
            var categorias = await GetAllAsync();
            if (!string.IsNullOrEmpty(categoriasFiltroNomeParams.Nome))
            {
                categorias = categorias.Where(c => c.Nome.Contains(categoriasFiltroNomeParams.Nome, StringComparison.OrdinalIgnoreCase  ));
            }
            var categoriasFiltradas = PagedList<Categoria>.ToPagedList(categorias.AsQueryable(), categoriasFiltroNomeParams.PageNumber, categoriasFiltroNomeParams.PageSize);
            return categoriasFiltradas;
        }
    }
}
