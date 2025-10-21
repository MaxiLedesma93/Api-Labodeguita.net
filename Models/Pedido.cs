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
        public int ClienteId { get; set; }
        
        [Required]
        public DateTime Fecha { get; set; }

        public Usuario Cliente { get; set; }

        public int EstadoId { get; set; }

        public Estado Estado { get; set; }

        public bool Pagado { get; set; }
    }
}