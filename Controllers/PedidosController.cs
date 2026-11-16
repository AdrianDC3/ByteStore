
using ByteStore.Data;
using ByteStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ByteStore.Controllers
{
    [Authorize]
    public class PedidosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string ClaveCarrito = "Carrito";

        public PedidosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var datos = HttpContext.Session.GetString(ClaveCarrito);

            if (string.IsNullOrWhiteSpace(datos))
            {
                return View(new List<ItemCarrito>());
            }

            List<ItemCarrito>? carrito;

            try
            {
                carrito = JsonSerializer.Deserialize<List<ItemCarrito>>(datos);
            }
            catch (JsonException)
            {
                ViewBag.Error = "No se pudo recuperar el carrito.";
                return View(new List<ItemCarrito>());
            }

            if (carrito == null || carrito.Count == 0)
            {
                return View(new List<ItemCarrito>());
            }

            if (carrito.Any(i => i.Cantidad <= 0) ||
                carrito.Select(i => i.ProductoId).Distinct().Count()
                    != carrito.Count)
            {
                ViewBag.Error = "El carrito contiene datos inválidos.";
                return View(new List<ItemCarrito>());
            }

            var ids = carrito
                .Select(i => i.ProductoId)
                .ToList();

            var productos = await _context.Productos
                .AsNoTracking()
                .Where(p => ids.Contains(p.Id) && p.Activo)
                .ToListAsync();

            if (productos.Count != carrito.Count)
            {
                ViewBag.Error =
                    "Uno de los productos ya no está disponible. Revisá tu carrito.";

                return View(new List<ItemCarrito>());
            }

            var resumen = new List<ItemCarrito>();

            foreach (var item in carrito)
            {
                var producto = productos.First(
                    p => p.Id == item.ProductoId);

                if (producto.Stock < item.Cantidad)
                {
                    ViewBag.Error =
                        $"Stock insuficiente para {producto.Nombre}. Revisá tu carrito.";

                    return View(new List<ItemCarrito>());
                }

                resumen.Add(new ItemCarrito
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    Precio = producto.Precio,
                    Cantidad = item.Cantidad
                });
            }

            return View(resumen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(
            string? TipoTarjeta,
            string? Titular,
            string? NumeroTarjeta,
            string? Vencimiento,
            string? CodigoSeguridad)
        {
            // VALIDAR LOS DATOS DEL PAGO FICTICIO

            if (TipoTarjeta != "Credito" &&
                TipoTarjeta != "Debito")
            {
                return BadRequest(new
                {
                    error = "Seleccioná un tipo de tarjeta válido."
                });
            }

            if (string.IsNullOrWhiteSpace(Titular) ||
                Titular.Trim().Length < 3 ||
                Titular.Trim().Length > 100)
            {
                return BadRequest(new
                {
                    error = "Ingresá un nombre de titular válido."
                });
            }

            string numero = Regex.Replace(
                NumeroTarjeta ?? "",
                @"\s+",
                "");

            if (!Regex.IsMatch(numero, @"^[0-9]{16}$"))
            {
                return BadRequest(new
                {
                    error = "El número de tarjeta debe contener 16 dígitos."
                });
            }

            if (string.IsNullOrWhiteSpace(Vencimiento) ||
                !Regex.IsMatch(
                    Vencimiento,
                    @"^(0[1-9]|1[0-2])/[0-9]{2}$"))
            {
                return BadRequest(new
                {
                    error = "Ingresá un vencimiento válido en formato MM/AA."
                });
            }

            int mes = int.Parse(Vencimiento.Substring(0, 2));
            int anio = 2000 + int.Parse(Vencimiento.Substring(3, 2));

            var fechaActual = DateTime.UtcNow;

            if (anio < fechaActual.Year ||
                (anio == fechaActual.Year && mes < fechaActual.Month))
            {
                return BadRequest(new
                {
                    error = "La fecha de vencimiento ingresada ya pasó."
                });
            }

            if (!Regex.IsMatch(
                CodigoSeguridad ?? "",
                @"^[0-9]{3}$"))
            {
                return BadRequest(new
                {
                    error = "El código de seguridad debe contener 3 dígitos."
                });
            }

            // NO SE GUARDAN LOS DATOS DE LA TARJETA.
            // SOLO SE UTILIZAN PARA VALIDAR ESTA SIMULACION.

            var usuarioId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(usuarioId))
            {
                return Unauthorized(new
                {
                    error = "Debés iniciar sesión."
                });
            }

            // RECUPERAR CARRITO

            var datos = HttpContext.Session.GetString(ClaveCarrito);

            if (string.IsNullOrEmpty(datos))
            {
                return BadRequest(new
                {
                    error = "El carrito está vacío."
                });
            }

            List<ItemCarrito>? carrito;

            try
            {
                carrito = JsonSerializer.Deserialize<List<ItemCarrito>>(datos);
            }
            catch (JsonException)
            {
                return BadRequest(new
                {
                    error = "El carrito no es válido."
                });
            }

            if (carrito == null || carrito.Count == 0)
            {
                return BadRequest(new
                {
                    error = "El carrito está vacío."
                });
            }

            if (carrito.Any(i => i.Cantidad <= 0) ||
                carrito.Select(i => i.ProductoId).Distinct().Count()
                    != carrito.Count)
            {
                return BadRequest(new
                {
                    error = "El carrito contiene datos inválidos."
                });
            }

            // REGISTRAR EL PEDIDO Y DESCONTAR STOCK
            // DENTRO DE UNA TRANSACCION

            await using var transaccion = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var pedido = new Pedido
                {
                    UsuarioId = usuarioId,
                    Fecha = DateTime.UtcNow,
                    Estado = "Pendiente"
                };

                foreach (var item in carrito)
                {
                    var producto = await _context.Productos
                        .FirstOrDefaultAsync(p =>
                            p.Id == item.ProductoId && p.Activo);

                    if (producto == null)
                    {
                        await transaccion.RollbackAsync();

                        return BadRequest(new
                        {
                            error = "Uno de los productos ya no está disponible."
                        });
                    }

                    if (producto.Stock < item.Cantidad)
                    {
                        await transaccion.RollbackAsync();

                        return BadRequest(new
                        {
                            error = $"Stock insuficiente para {producto.Nombre}."
                        });
                    }

                    producto.Stock -= item.Cantidad;

                    pedido.Detalles.Add(new DetallePedido
                    {
                        ProductoId = producto.Id,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.Precio
                    });

                    pedido.Total += producto.Precio * item.Cantidad;
                }

                _context.Pedidos.Add(pedido);

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                // VACIAR EL CARRITO SOLO DESPUES
                // DE CONFIRMAR LA COMPRA

                HttpContext.Session.Remove(ClaveCarrito);

                return Ok(new
                {
                    exito = true,
                    mensaje = "Pago de prueba aprobado. Compra confirmada.",
                    pedidoId = pedido.Id
                });
            }
            catch (Exception)
            {
                await transaccion.RollbackAsync();

                return StatusCode(500, new
                {
                    error = "No se pudo procesar la compra. Intentá nuevamente."
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> MisPedidos()
        {
            var usuarioId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(usuarioId))
            {
                return Challenge();
            }

            var pedidos = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.UsuarioId == usuarioId)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            return View(pedidos);
        }
    }
}


