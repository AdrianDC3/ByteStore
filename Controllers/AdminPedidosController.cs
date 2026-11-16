using ByteStore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ByteStore.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminPedidosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminPedidosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var pedidos = await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            var usuarios = await _context.Users
                .AsNoTracking()
                .Select(u => new
                {
                    u.Id,
                    u.Email
                })
                .ToDictionaryAsync(
                    u => u.Id,
                    u => u.Email ?? "Sin correo");

            ViewBag.Usuarios = usuarios;

            return View(pedidos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(
            int id,
            string nuevoEstado)
        {
            var estadosPermitidos = new[]
            {
                "Pendiente",
                "En preparación",
                "Enviado",
                "Entregado"
            };

            if (!estadosPermitidos.Contains(nuevoEstado))
            {
                TempData["Error"] = "El estado seleccionado no es válido.";

                return RedirectToAction(nameof(Index));
            }

            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                return NotFound();
            }

            pedido.Estado = nuevoEstado;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                $"El pedido #{pedido.Id} ahora está en estado: {nuevoEstado}.";

            return RedirectToAction(nameof(Index));
        }
    }
}

