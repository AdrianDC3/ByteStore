using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace ByteStore.Models
{
    public class Producto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        [Range(0.01, 999999999)]
        [Precision(18, 2)]
        public decimal Precio { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public bool Activo { get; set; } = true;

        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = null!;

        public int MarcaId { get; set; }
        public Marca Marca { get; set; } = null!;
    }
}
