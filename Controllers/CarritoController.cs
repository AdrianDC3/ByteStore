using ByteStore.Data;
using ByteStore.Models;
using ByteStore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ByteStore.Controllers
{
    public class CarritoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CarritoService _carritoService;

        public CarritoController(
            ApplicationDbContext context,
            CarritoService carritoService)
        {
            _context = context;
            _carritoService = carritoService;
        }

        [HttpGet]
        public IActionResult Contenido()
        {
            return PartialView(
                "_ContenidoCarrito",
                _carritoService.Obtener());
        }

        [HttpGet]
        public IActionResult Cantidad()
        {
            return Json(new
            {
                cantidad = _carritoService.ObtenerCantidad()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(
            int productoId,
            int cantidad = 1)
        {
            if (cantidad <= 0)
                return BadRequest(new { error = "Cantidad inválida." });

            var producto = await _context.Productos
                .FirstOrDefaultAsync(p =>
                    p.Id == productoId && p.Activo);

            if (producto == null)
                return NotFound(new { error = "Producto no encontrado." });

            var carrito = _carritoService.Obtener();

            var item = carrito.FirstOrDefault(
                i => i.ProductoId == productoId);

            int cantidadActual = item?.Cantidad ?? 0;

            if (cantidad > producto.Stock - cantidadActual)
            {
                return BadRequest(new
                {
                    error = "No hay stock suficiente."
                });
            }

            if (item == null)
            {
                carrito.Add(new ItemCarrito
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    Precio = producto.Precio,
                    Cantidad = cantidad
                });
            }
            else
            {
                item.Cantidad += cantidad;
            }

            _carritoService.Guardar(carrito);

            return Ok(new
            {
                mensaje = "Producto agregado correctamente."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Actualizar(
            int productoId,
            int cantidad)
        {
            if (cantidad <= 0)
                return BadRequest(new { error = "Cantidad inválida." });

            var carrito = _carritoService.Obtener();

            var item = carrito.FirstOrDefault(
                i => i.ProductoId == productoId);

            if (item == null)
            {
                return NotFound(new
                {
                    error = "Producto no encontrado en el carrito."
                });
            }

            var producto = await _context.Productos
                .FirstOrDefaultAsync(p =>
                    p.Id == productoId && p.Activo);

            if (producto == null)
                return NotFound(new { error = "Producto no disponible." });

            if (cantidad > producto.Stock)
                return BadRequest(new { error = "No hay stock suficiente." });

            item.Cantidad = cantidad;

            _carritoService.Guardar(carrito);

            return Ok(new { mensaje = "Cantidad actualizada." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int productoId)
        {
            var carrito = _carritoService.Obtener();

            carrito.RemoveAll(i => i.ProductoId == productoId);

            _carritoService.Guardar(carrito);

            return Ok(new { mensaje = "Producto eliminado." });
        }
    }
}