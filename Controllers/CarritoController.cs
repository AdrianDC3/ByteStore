using ByteStore.Data;
using ByteStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ByteStore.Controllers
{
    public class CarritoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string ClaveCarrito = "Carrito";

        public CarritoController(ApplicationDbContext context)
        {
            _context = context;
        }

        private List<ItemCarrito> ObtenerCarrito()
        {
            var datos = HttpContext.Session.GetString(ClaveCarrito);

            if (string.IsNullOrEmpty(datos))
                return new List<ItemCarrito>();

            return JsonSerializer.Deserialize<List<ItemCarrito>>(datos)
                   ?? new List<ItemCarrito>();
        }

        private void GuardarCarrito(List<ItemCarrito> carrito)
        {
            var datos = JsonSerializer.Serialize(carrito);
            HttpContext.Session.SetString(ClaveCarrito, datos);
        }

        [HttpGet]
        public IActionResult Contenido()
        {
            return PartialView("_ContenidoCarrito", ObtenerCarrito());
        }

        [HttpGet]
        public IActionResult Cantidad()
        {
            var carrito = ObtenerCarrito();

            int cantidad = carrito.Sum(item => item.Cantidad);

            return Json(new { cantidad });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(int productoId, int cantidad = 1)
        {
            if (cantidad <= 0)
                return BadRequest(new { error = "Cantidad inválida." });

            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == productoId && p.Activo);

            if (producto == null)
                return NotFound(new { error = "Producto no encontrado." });

            var carrito = ObtenerCarrito();

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

            GuardarCarrito(carrito);

            return Ok(new { mensaje = "Producto agregado correctamente." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Actualizar(int productoId, int cantidad)
        {
            if (cantidad <= 0)
                return BadRequest(new { error = "Cantidad inválida." });

            var carrito = ObtenerCarrito();

            var item = carrito.FirstOrDefault(
                i => i.ProductoId == productoId);

            if (item == null)
                return NotFound(new { error = "Producto no encontrado en el carrito." });

            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == productoId && p.Activo);

            if (producto == null)
                return NotFound(new { error = "Producto no disponible." });

            if (cantidad > producto.Stock)
                return BadRequest(new { error = "No hay stock suficiente." });

            item.Cantidad = cantidad;

            GuardarCarrito(carrito);

            return Ok(new { mensaje = "Cantidad actualizada." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int productoId)
        {
            var carrito = ObtenerCarrito();

            carrito.RemoveAll(i => i.ProductoId == productoId);

            GuardarCarrito(carrito);

            return Ok(new { mensaje = "Producto eliminado." });
        }
    }
}



