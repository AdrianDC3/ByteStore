using ByteStore.Data;
using ByteStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ByteStore.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminProductosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
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
            {
                consulta = consulta.Where(p =>
                    p.Nombre.Contains(buscar));
            }

            if (categoriaId.HasValue)
            {
                consulta = consulta.Where(p =>
                    p.CategoriaId == categoriaId.Value);
            }

            if (marcaId.HasValue)
            {
                consulta = consulta.Where(p =>
                    p.MarcaId == marcaId.Value);
            }

            var productos = await consulta
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ViewBag.Buscar = buscar;
            ViewBag.CategoriaId = categoriaId;
            ViewBag.MarcaId = marcaId;

            ViewBag.Categorias = new SelectList(
                await _context.Categorias
                    .AsNoTracking()
                    .OrderBy(c => c.Nombre)
                    .ToListAsync(),
                "Id",
                "Nombre",
                categoriaId);

            ViewBag.Marcas = new SelectList(
                await _context.Marcas
                    .AsNoTracking()
                    .OrderBy(m => m.Nombre)
                    .ToListAsync(),
                "Id",
                "Nombre",
                marcaId);

            return View(productos);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarListas();

            return View(new CrearProductoViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearProductoViewModel modelo)
        {
            if (!await _context.Categorias.AnyAsync(
                c => c.Id == modelo.CategoriaId && c.Activa))
            {
                ModelState.AddModelError(
                    nameof(modelo.CategoriaId),
                    "Seleccioná una categoría válida.");
            }

            if (!await _context.Marcas.AnyAsync(
                m => m.Id == modelo.MarcaId && m.Activa))
            {
                ModelState.AddModelError(
                    nameof(modelo.MarcaId),
                    "Seleccioná una marca válida.");
            }

            if (!ModelState.IsValid)
            {
                await CargarListas();
                return View(modelo);
            }

            var producto = new Producto
            {
                Nombre = modelo.Nombre.Trim(),
                Descripcion = modelo.Descripcion.Trim(),
                Precio = modelo.Precio,
                Stock = modelo.Stock,
                CategoriaId = modelo.CategoriaId,
                MarcaId = modelo.MarcaId,
                Activo = true
            };

            _context.Productos.Add(producto);

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Producto creado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var producto = await _context.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
            {
                return NotFound();
            }

            var modelo = new EditarProductoViewModel
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Stock = producto.Stock,
                CategoriaId = producto.CategoriaId,
                MarcaId = producto.MarcaId
            };

            await CargarListas(
                producto.CategoriaId,
                producto.MarcaId);

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            EditarProductoViewModel modelo)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == modelo.Id);

            if (producto == null)
            {
                return NotFound();
            }

            if (!await _context.Categorias.AnyAsync(
                c => c.Id == modelo.CategoriaId &&
                     (c.Activa || c.Id == producto.CategoriaId)))
            {
                ModelState.AddModelError(
                    nameof(modelo.CategoriaId),
                    "Seleccioná una categoría válida.");
            }

            if (!await _context.Marcas.AnyAsync(
                m => m.Id == modelo.MarcaId &&
                     (m.Activa || m.Id == producto.MarcaId)))
            {
                ModelState.AddModelError(
                    nameof(modelo.MarcaId),
                    "Seleccioná una marca válida.");
            }

            if (!ModelState.IsValid)
            {
                await CargarListas(
                    producto.CategoriaId,
                    producto.MarcaId);

                return View(modelo);
            }

            producto.Nombre = modelo.Nombre.Trim();
            producto.Descripcion = modelo.Descripcion.Trim();
            producto.Precio = modelo.Precio;
            producto.Stock = modelo.Stock;
            producto.CategoriaId = modelo.CategoriaId;
            producto.MarcaId = modelo.MarcaId;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Producto actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
            {
                return NotFound();
            }

            producto.Activo = !producto.Activo;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = producto.Activo
                ? "Producto reactivado correctamente."
                : "Producto desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
            {
                return NotFound();
            }

            bool tieneVentas = await _context.DetallesPedido
                .AnyAsync(d => d.ProductoId == id);

            if (tieneVentas)
            {
                TempData["Error"] =
                    "No se puede eliminar este producto porque tiene ventas registradas. Podés desactivarlo.";

                return RedirectToAction(nameof(Index));
            }

            _context.Productos.Remove(producto);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "No se pudo eliminar el producto porque tiene registros relacionados.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] = "Producto eliminado definitivamente.";

            return RedirectToAction(nameof(Index));
        }
        private async Task CargarListas(
            int? categoriaActualId = null,
            int? marcaActualId = null)
        {
            var categorias = await _context.Categorias
                .AsNoTracking()
                .Where(c => c.Activa || c.Id == categoriaActualId)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            var marcas = await _context.Marcas
                .AsNoTracking()
                .Where(m => m.Activa || m.Id == marcaActualId)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            ViewBag.Categorias = new SelectList(
                categorias, "Id", "Nombre");

            ViewBag.Marcas = new SelectList(
                marcas, "Id", "Nombre");
        }
    }

    public class CrearProductoViewModel
    {
        [Required(ErrorMessage = "Ingresá el nombre del producto.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá la descripción.")]
        public string Descripcion { get; set; } = string.Empty;

        [Range(0.01, 999999999,
            ErrorMessage = "El precio debe ser mayor que cero.")]
        public decimal Precio { get; set; }

        [Range(0, int.MaxValue,
            ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccioná una categoría.")]
        public int CategoriaId { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccioná una marca.")]
        public int MarcaId { get; set; }
    }

    public class EditarProductoViewModel
    {
        [Range(1, int.MaxValue)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ingresá el nombre del producto.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá la descripción.")]
        public string Descripcion { get; set; } = string.Empty;

        [Range(0.01, 999999999,
            ErrorMessage = "El precio debe ser mayor que cero.")]
        public decimal Precio { get; set; }

        [Range(0, int.MaxValue,
            ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccioná una categoría.")]
        public int CategoriaId { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Seleccioná una marca.")]
        public int MarcaId { get; set; }
    }
}




