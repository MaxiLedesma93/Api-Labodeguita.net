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

        [Required]
        public bool Estado { get; set; }
        
        public string? Foto { get; set; }

        [NotMapped]
		public IFormFile? Imagen { get; set;}

        [Required (ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion {get; set;}

        
        public int? IdTipo {get; set;}

        [NotMapped]
        public string? TipoProducto {get; set;}

        [NotMapped]
        public Tipo? Tipo {get; set;}
   }
}