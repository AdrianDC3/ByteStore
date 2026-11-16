using ByteStore.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ByteStore.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? buscar,
            int? categoriaId,
            int? marcaId)
        {
            var productos = _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .Where(p => p.Activo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                productos = productos.Where(
                    p => p.Nombre.Contains(buscar));
            }

            if (categoriaId.HasValue)
            {
                productos = productos.Where(
                    p => p.CategoriaId == categoriaId.Value);
            }

            if (marcaId.HasValue)
            {
                productos = productos.Where(
                    p => p.MarcaId == marcaId.Value);
            }

            ViewBag.Categorias = new SelectList(
                await _context.Categorias
                    .Where(c => c.Activa)
                    .OrderBy(c => c.Nombre)
                    .ToListAsync(),
                "Id",
                "Nombre",
                categoriaId);

            ViewBag.Marcas = new SelectList(
                await _context.Marcas
                    .Where(m => m.Activa)
                    .OrderBy(m => m.Nombre)
                    .ToListAsync(),
                "Id",
                "Nombre",
                marcaId);

            ViewBag.Buscar = buscar;

            return View(await productos
                .OrderBy(p => p.Nombre)
                .ToListAsync());
        }
        public async Task<IActionResult> Detalle(int id)
        {
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .FirstOrDefaultAsync(p => p.Id == id && p.Activo);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

    }
}

