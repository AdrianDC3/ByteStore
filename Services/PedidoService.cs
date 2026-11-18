using ByteStore.Data;
using ByteStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ByteStore.Services
{
    public class PedidoService
    {
        private readonly ApplicationDbContext _context;

        public PedidoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // PREPARAR RESUMEN DEL CHECKOUT

        public async Task<(List<ItemCarrito> Resumen, string? Error)>
            PrepararCheckoutAsync(List<ItemCarrito> carrito)
        {
            var ids = carrito.Select(i => i.ProductoId).ToList();

            var productos = await _context.Productos
                .AsNoTracking()
                .Where(p => ids.Contains(p.Id) && p.Activo)
                .ToDictionaryAsync(p => p.Id);

            if (productos.Count != carrito.Count)
            {
                return (
                    new List<ItemCarrito>(),
                    "Uno de los productos ya no está disponible. Revisá tu carrito."
                );
            }

            var resumen = new List<ItemCarrito>();

            foreach (var item in carrito)
            {
                var producto = productos[item.ProductoId];

                if (producto.Stock < item.Cantidad)
                {
                    return (
                        new List<ItemCarrito>(),
                        $"Stock insuficiente para {producto.Nombre}. Revisá tu carrito."
                    );
                }

                resumen.Add(new ItemCarrito
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    Precio = producto.Precio,
                    Cantidad = item.Cantidad
                });
            }

            return (resumen, null);
        }

        // REGISTRAR PEDIDO

        public async Task<ResultadoPedido> RegistrarAsync(
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

                        return new ResultadoPedido(
                            null,
                            "Uno de los productos ya no está disponible.",
                            400);
                    }

                    if (producto.Stock < item.Cantidad)
                    {
                        await transaccion.RollbackAsync();

                        return new ResultadoPedido(
                            null,
                            $"Stock insuficiente para {producto.Nombre}.",
                            400);
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

                return new ResultadoPedido(pedido.Id, null, 200);
            }
            catch (Exception)
            {
                await transaccion.RollbackAsync();

                return new ResultadoPedido(
                    null,
                    "No se pudo procesar la compra. Intentá nuevamente.",
                    500);
            }
        }

        // OBTENER HISTORIAL DEL CLIENTE

        public async Task<List<Pedido>> ObtenerPedidosAsync(
            string usuarioId)
        {
            return await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.UsuarioId == usuarioId)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();
        }
    }

    public record ResultadoPedido(
        int? PedidoId,
        string? Error,
        int CodigoEstado);
}