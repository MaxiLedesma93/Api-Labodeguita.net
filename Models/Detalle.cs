using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_Labodeguita.net.Models
{
    public class Detalle
    {
        [Key]
        [Display(Name = "Codigo Detalle")]
        public int Id { get; set; }
        
        [Required, Display(Name ="Pedido")]
        public int PedidoId { get; set; }
       
        [Required, Display(Name = "Producto")]
        public int ProductoId { get; set; }
        
        public int Cantidad { get; set; }
    }
}


