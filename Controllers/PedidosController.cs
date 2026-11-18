using ByteStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ByteStore.Controllers
{
    [Authorize]
    public class PedidosController : Controller
    {
        private readonly PedidoService _pedidoService;
        private readonly CarritoService _carritoService;

        public PedidosController(
            PedidoService pedidoService,
            CarritoService carritoService)
        {
            _pedidoService = pedidoService;
            _carritoService = carritoService;
        }

        // CHECKOUT

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            if (!_carritoService.TryObtener(
                out var carrito,
                out var error))
            {
                if (error != "El carrito está vacío.")
                    ViewBag.Error = error;

                return View(new List<ByteStore.Models.ItemCarrito>());
            }

            var resultado = await _pedidoService
                .PrepararCheckoutAsync(carrito);

            if (resultado.Error != null)
                ViewBag.Error = resultado.Error;

            return View(resultado.Resumen);
        }

        // CONFIRMAR COMPRA

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
                return BadRequest(new { error = errorPago });

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

            var resultado = await _pedidoService
                .RegistrarAsync(usuarioId, carrito);

            if (resultado.Error != null)
            {
                return StatusCode(
                    resultado.CodigoEstado,
                    new { error = resultado.Error });
            }

            _carritoService.Vaciar();

            return Ok(new
            {
                exito = true,
                mensaje = "Pago de prueba aprobado. Compra confirmada.",
                pedidoId = resultado.PedidoId
            });
        }

        // HISTORIAL DE PEDIDOS

        [HttpGet]
        public async Task<IActionResult> MisPedidos()
        {
            var usuarioId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(usuarioId))
                return Challenge();

            var pedidos = await _pedidoService
                .ObtenerPedidosAsync(usuarioId);

            return View(pedidos);
        }

        // VALIDAR DATOS DEL PAGO FICTICIO

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

            // Los datos de la tarjeta no se almacenan.
            // El pago es una simulación académica.

            return null;
        }
    }
}