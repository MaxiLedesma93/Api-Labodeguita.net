using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_Labodeguita.net.Models
{
    public class Pedido
    {
        [Key]
        [Display(Name = "Codigo Pedido")]
        public int Id { get; set; }

        [Required, Display(Name = "Cliente")]
        public int IdCliente { get; set; }
        
        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public int IdDetalle { get; set; }

        public Detalle Detalle { get; set; }

        public Usuario Cliente { get; set; }
    }
}