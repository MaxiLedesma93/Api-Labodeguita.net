using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ApiInmobiliaria.Models{

    public class Usuario
   {
		[Key]
        [Display(Name = "Codigo Usuario")]
        public int Id { get; set; }

        [Required, Display(Name ="Cliente")]
        public string Nombre { get; set; }

        [Required]
        public string Direccion { get; set; }

        [Required]
        public string Telefono { get; set; }
   }
}