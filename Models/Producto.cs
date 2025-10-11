using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Api_Labodeguita.net.Models{

    public class Producto
   {
		[Key]
        [Display(Name = "Codigo Producto")]
        public int Id { get; set; }

        [Required, Display(Name ="Producto")]
        public string Nombre { get; set; }

        [Required]
        public double Precio { get; set; }

        public bool Estado { get; set; }

   }
}