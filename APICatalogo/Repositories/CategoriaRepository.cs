using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;
using X.PagedList;

namespace APICatalogo.Repositories
{
    public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
    {
        public CategoriaRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IPagedList<Categoria>> GetCategoriasAsync(CategoriasParameters categoriasParams)
        {
            var categorias = await GetAllAsync();
            var categoriasOrdenadas = categorias.OrderBy(c => c.Nome).AsQueryable();
            var resultado = await categoriasOrdenadas.ToPagedListAsync(
                categoriasParams.PageNumber,
                categoriasParams.PageSize
            );
            return resultado;
        }
        public async Task<IPagedList<Categoria>> GetCategoriasFiltroNomeAsync(CategoriasFiltroNome categoriasFiltroNomeParams)
        {
            var categorias = await GetAllAsync();
            if (!string.IsNullOrEmpty(categoriasFiltroNomeParams.Nome))
            {
                categorias = categorias.Where(c => c.Nome.Contains(categoriasFiltroNomeParams.Nome, StringComparison.OrdinalIgnoreCase  ));
            }

            var categoriasFiltradas = await categorias.ToPagedListAsync( categoriasFiltroNomeParams.PageNumber,
                categoriasFiltroNomeParams.PageSize
            );
            return categoriasFiltradas;
        }
    }
}
