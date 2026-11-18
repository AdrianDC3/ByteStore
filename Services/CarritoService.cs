using ByteStore.Models;
using System.Text.Json;

namespace ByteStore.Services
{
    public class CarritoService
    {
        private const string ClaveCarrito = "Carrito";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CarritoService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Sesion =>
            _httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException(
                "No hay una sesión HTTP disponible.");

        public List<ItemCarrito> Obtener()
        {
            var datos = Sesion.GetString(ClaveCarrito);

            if (string.IsNullOrWhiteSpace(datos))
                return new List<ItemCarrito>();

            try
            {
                return JsonSerializer.Deserialize<List<ItemCarrito>>(datos)
                    ?? new List<ItemCarrito>();
            }
            catch (JsonException)
            {
                return new List<ItemCarrito>();
            }
        }

        public bool TryObtener(
            out List<ItemCarrito> carrito,
            out string? error)
        {
            carrito = new List<ItemCarrito>();
            error = null;

            var datos = Sesion.GetString(ClaveCarrito);

            if (string.IsNullOrWhiteSpace(datos))
            {
                error = "El carrito está vacío.";
                return false;
            }

            try
            {
                carrito = JsonSerializer.Deserialize<List<ItemCarrito>>(datos)
                    ?? new List<ItemCarrito>();
            }
            catch (JsonException)
            {
                error = "El carrito no es válido.";
                return false;
            }

            if (carrito.Count == 0)
            {
                error = "El carrito está vacío.";
                return false;
            }

            if (carrito.Any(i => i.Cantidad <= 0) ||
                carrito.Select(i => i.ProductoId).Distinct().Count()
                    != carrito.Count)
            {
                error = "El carrito contiene datos inválidos.";
                return false;
            }

            return true;
        }

        public void Guardar(List<ItemCarrito> carrito)
        {
            var datos = JsonSerializer.Serialize(carrito);
            Sesion.SetString(ClaveCarrito, datos);
        }

        public int ObtenerCantidad()
        {
            return Obtener().Sum(item => item.Cantidad);
        }

        public void Vaciar()
        {
            Sesion.Remove(ClaveCarrito);
        }
    }
}