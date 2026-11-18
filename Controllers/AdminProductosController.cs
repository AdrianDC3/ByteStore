using ByteStore.Models;
using ByteStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ByteStore.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminProductosController : Controller
    {
        private readonly ProductoService _productoService;

        public AdminProductosController(ProductoService productoService)
        {
            _productoService = productoService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? buscar,
            int? categoriaId,
            int? marcaId)
        {
            var productos = await _productoService.ListarAsync(
                buscar, categoriaId, marcaId);

            ViewBag.Buscar = buscar;
            ViewBag.CategoriaId = categoriaId;
            ViewBag.MarcaId = marcaId;

            ViewBag.Categorias = new SelectList(
                await _productoService.ObtenerCategoriasAsync(),
                "Id", "Nombre", categoriaId);

            ViewBag.Marcas = new SelectList(
                await _productoService.ObtenerMarcasAsync(),
                "Id", "Nombre", marcaId);

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
            await ValidarRelaciones(modelo);

            if (!ModelState.IsValid)
            {
                await CargarListas();
                return View(modelo);
            }

            var producto = new Producto
            {
                Activo = true
            };

            AsignarDatos(producto, modelo);

            await _productoService.CrearAsync(producto);

            TempData["Mensaje"] = "Producto creado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var producto = await _productoService.ObtenerAsync(id);

            if (producto == null)
                return NotFound();

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
        public async Task<IActionResult> Editar(EditarProductoViewModel modelo)
        {
            var producto = await _productoService.ObtenerAsync(modelo.Id);

            if (producto == null)
                return NotFound();

            await ValidarRelaciones(
                modelo,
                producto.CategoriaId,
                producto.MarcaId);

            if (!ModelState.IsValid)
            {
                await CargarListas(
                    producto.CategoriaId,
                    producto.MarcaId);

                return View(modelo);
            }

            AsignarDatos(producto, modelo);

            await _productoService.GuardarCambiosAsync();

            TempData["Mensaje"] = "Producto actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            bool? activo = await _productoService.CambiarEstadoAsync(id);

            if (activo == null)
                return NotFound();

            TempData["Mensaje"] = activo.Value
                ? "Producto reactivado correctamente."
                : "Producto desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado = await _productoService.EliminarAsync(id);

            switch (resultado)
            {
                case ResultadoEliminacion.NoEncontrado:
                    return NotFound();

                case ResultadoEliminacion.TieneVentas:
                    TempData["Error"] =
                        "No se puede eliminar este producto porque tiene ventas registradas. Podés desactivarlo.";
                    break;

                case ResultadoEliminacion.Error:
                    TempData["Error"] =
                        "No se pudo eliminar el producto porque tiene registros relacionados.";
                    break;

                case ResultadoEliminacion.Eliminado:
                    TempData["Mensaje"] =
                        "Producto eliminado definitivamente.";
                    break;
            }

            return RedirectToAction(nameof(Index));
        }

        // METODOS AUXILIARES

        private async Task ValidarRelaciones(
            ProductoFormularioViewModel modelo,
            int? categoriaActualId = null,
            int? marcaActualId = null)
        {
            if (!await _productoService.CategoriaValidaAsync(
                modelo.CategoriaId, categoriaActualId))
            {
                ModelState.AddModelError(
                    nameof(modelo.CategoriaId),
                    "Seleccioná una categoría válida.");
            }

            if (!await _productoService.MarcaValidaAsync(
                modelo.MarcaId, marcaActualId))
            {
                ModelState.AddModelError(
                    nameof(modelo.MarcaId),
                    "Seleccioná una marca válida.");
            }
        }

        private async Task CargarListas(
            int? categoriaActualId = null,
            int? marcaActualId = null)
        {
            ViewBag.Categorias = new SelectList(
                await _productoService.ObtenerCategoriasAsync(categoriaActualId),
                "Id", "Nombre");

            ViewBag.Marcas = new SelectList(
                await _productoService.ObtenerMarcasAsync(marcaActualId),
                "Id", "Nombre");
        }

        private static void AsignarDatos(
            Producto producto,
            ProductoFormularioViewModel modelo)
        {
            producto.Nombre = modelo.Nombre.Trim();
            producto.Descripcion = modelo.Descripcion.Trim();
            producto.Precio = modelo.Precio;
            producto.Stock = modelo.Stock;
            producto.CategoriaId = modelo.CategoriaId;
            producto.MarcaId = modelo.MarcaId;
        }
    }

    // VIEWMODEL BASE COMPARTIDO

    public class ProductoFormularioViewModel
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

    // VIEWMODEL DE CREACION

    public class CrearProductoViewModel : ProductoFormularioViewModel
    {
    }

    // VIEWMODEL DE EDICION

    public class EditarProductoViewModel : ProductoFormularioViewModel
    {
        [Range(1, int.MaxValue)]
        public int Id { get; set; }
    }
}