using ByteStore.Data;
using ByteStore.Models;
using Microsoft.EntityFrameworkCore;

namespace ByteStore.Services
{
    public enum ResultadoEliminacion
    {
        Eliminado,
        NoEncontrado,
        TieneVentas,
        Error
    }

    public class ProductoService
    {
        private readonly ApplicationDbContext _context;

        public ProductoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Producto>> ListarAsync(
            string? buscar,
            int? categoriaId,
            int? marcaId)
        {
            var consulta = _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
                consulta = consulta.Where(p => p.Nombre.Contains(buscar));

            if (categoriaId.HasValue)
                consulta = consulta.Where(p => p.CategoriaId == categoriaId.Value);

            if (marcaId.HasValue)
                consulta = consulta.Where(p => p.MarcaId == marcaId.Value);

            return await consulta
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public Task<Producto?> ObtenerAsync(int id)
        {
            return _context.Productos.FirstOrDefaultAsync(p => p.Id == id);
        }

        public Task<List<Categoria>> ObtenerCategoriasAsync(
            int? categoriaActualId = null)
        {
            return _context.Categorias
                .AsNoTracking()
                .Where(c => c.Activa || c.Id == categoriaActualId)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public Task<List<Marca>> ObtenerMarcasAsync(
            int? marcaActualId = null)
        {
            return _context.Marcas
                .AsNoTracking()
                .Where(m => m.Activa || m.Id == marcaActualId)
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }

        public Task<bool> CategoriaValidaAsync(
            int id,
            int? categoriaActualId = null)
        {
            return _context.Categorias.AnyAsync(c =>
                c.Id == id && (c.Activa || c.Id == categoriaActualId));
        }

        public Task<bool> MarcaValidaAsync(
            int id,
            int? marcaActualId = null)
        {
            return _context.Marcas.AnyAsync(m =>
                m.Id == id && (m.Activa || m.Id == marcaActualId));
        }

        public async Task CrearAsync(Producto producto)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
        }

        public Task GuardarCambiosAsync()
        {
            return _context.SaveChangesAsync();
        }

        public async Task<bool?> CambiarEstadoAsync(int id)
        {
            var producto = await ObtenerAsync(id);

            if (producto == null)
                return null;

            producto.Activo = !producto.Activo;

            await _context.SaveChangesAsync();

            return producto.Activo;
        }

        public async Task<ResultadoEliminacion> EliminarAsync(int id)
        {
            var producto = await ObtenerAsync(id);

            if (producto == null)
                return ResultadoEliminacion.NoEncontrado;

            bool tieneVentas = await _context.DetallesPedido
                .AnyAsync(d => d.ProductoId == id);

            if (tieneVentas)
                return ResultadoEliminacion.TieneVentas;

            _context.Productos.Remove(producto);

            try
            {
                await _context.SaveChangesAsync();
                return ResultadoEliminacion.Eliminado;
            }
            catch (DbUpdateException)
            {
                return ResultadoEliminacion.Error;
            }
        }
    }
}