using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_Labodeguita.net.Models
{
    public class Pago
    {
        [Key]
        [Display(Name = "Codigo Detalle")]
        public int Id { get; set; }
        
        [Required]
        public int PedidoId { get; set; }

        [Required]
        public String MetodoDePago  { get; set; }
        
        [Required]
        public double Importe {get; set;}
        [NotMapped]
        public String? Direccion{get; set;}
    }
}