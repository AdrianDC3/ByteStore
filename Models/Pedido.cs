using System.ComponentModel.DataAnnotations;

namespace ByteStore.Models
{
    public class Pedido
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        public decimal Total { get; set; }

        [Required]
        public string Estado { get; set; } = "Pendiente";

        public List<DetallePedido> Detalles { get; set; }
            = new List<DetallePedido>();
    }
}

