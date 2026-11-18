using ByteStore.Data;
using ByteStore.Models;
using ByteStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ByteStore.Controllers
{
    [Authorize]
    public class PedidosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CarritoService _carritoService;

        public PedidosController(
            ApplicationDbContext context,
            CarritoService carritoService)
        {
            _context = context;
            _carritoService = carritoService;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            if (!_carritoService.TryObtener(
                out var carrito,
                out var error))
            {
                if (error != "El carrito está vacío.")
                    ViewBag.Error = error;

                return View(new List<ItemCarrito>());
            }

            var ids = carrito.Select(i => i.ProductoId).ToList();

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
            var errorPago = ValidarPago(
                TipoTarjeta,
                Titular,
                NumeroTarjeta,
                Vencimiento,
                CodigoSeguridad);

            if (errorPago != null)
            {
                return BadRequest(new { error = errorPago });
            }

            var usuarioId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(usuarioId))
            {
                return Unauthorized(new
                {
                    error = "Debés iniciar sesión."
                });
            }

            if (!_carritoService.TryObtener(
                out var carrito,
                out var errorCarrito))
            {
                return BadRequest(new
                {
                    error = errorCarrito
                });
            }

            return await RegistrarPedido(usuarioId, carrito);
        }

        [HttpGet]
        public async Task<IActionResult> MisPedidos()
        {
            var usuarioId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(usuarioId))
                return Challenge();

            var pedidos = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.UsuarioId == usuarioId)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            return View(pedidos);
        }

        // VALIDACION DEL PAGO FICTICIO

        private static string? ValidarPago(
            string? tipoTarjeta,
            string? titular,
            string? numeroTarjeta,
            string? vencimiento,
            string? codigoSeguridad)
        {
            if (tipoTarjeta != "Credito" &&
                tipoTarjeta != "Debito")
            {
                return "Seleccioná un tipo de tarjeta válido.";
            }

            if (string.IsNullOrWhiteSpace(titular) ||
                titular.Trim().Length < 3 ||
                titular.Trim().Length > 100)
            {
                return "Ingresá un nombre de titular válido.";
            }

            string numero = Regex.Replace(
                numeroTarjeta ?? "",
                @"\s+",
                "");

            if (!Regex.IsMatch(numero, @"^[0-9]{16}$"))
            {
                return "El número de tarjeta debe contener 16 dígitos.";
            }

            if (string.IsNullOrWhiteSpace(vencimiento) ||
                !Regex.IsMatch(
                    vencimiento,
                    @"^(0[1-9]|1[0-2])/[0-9]{2}$"))
            {
                return "Ingresá un vencimiento válido en formato MM/AA.";
            }

            int mes = int.Parse(vencimiento.Substring(0, 2));
            int anio = 2000 + int.Parse(vencimiento.Substring(3, 2));

            var fechaActual = DateTime.UtcNow;

            if (anio < fechaActual.Year ||
                (anio == fechaActual.Year && mes < fechaActual.Month))
            {
                return "La fecha de vencimiento ingresada ya pasó.";
            }

            if (!Regex.IsMatch(
                codigoSeguridad ?? "",
                @"^[0-9]{3}$"))
            {
                return "El código de seguridad debe contener 3 dígitos.";
            }

            // No se almacenan datos de tarjetas.
            // El pago es únicamente una simulación.
            return null;
        }

        // REGISTRO DEL PEDIDO Y ACTUALIZACION DEL STOCK

        private async Task<IActionResult> RegistrarPedido(
            string usuarioId,
            List<ItemCarrito> carrito)
        {
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

                _carritoService.Vaciar();

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
    }
}