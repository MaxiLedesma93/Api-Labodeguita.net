using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Api_Labodeguita.net.Models{

    public class Usuario
   {
		[Key]
        [Display(Name = "Codigo Usuario")]
        public int Id { get; set; }

        [Required, Display(Name = "Cliente")]
        public string Nombre { get; set; }
        [Required]
        public string Apellido { get; set; }
        [Required]
        public string Email { get; set; }

        [Required]
        public string Direccion { get; set; }

        [Required]
        public string Telefono { get; set; }

        public bool Estado { get; set; }
   }
}