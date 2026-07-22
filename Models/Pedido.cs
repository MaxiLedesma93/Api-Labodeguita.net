using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_Labodeguita.net.Models
{
    public class Pedido
    {
        [Key]
        [Display(Name = "Codigo Pedido")]
        public int Id { get; set; }

        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }
        
        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime Fecha { get; set; }

        public Usuario? Cliente { get; set; }

        [Required(ErrorMessage = "El estado del pedido es obligatorio.")]
        public int EstadoId { get; set; }

        public Estado? Estado { get; set; }

        public Pago? Pago {get; set;}


        public List<Detalle>? Detalles { get; set; }
        public bool Delivery {get; set;}
        [Required(ErrorMessage = "La direccion es obligatoria.")]
        public string DireccionEntrega {get; set;}
        [Required(ErrorMessage = "Importe es obligatorio.")]
        public double ImporteTotal { get; set; }


    }
}