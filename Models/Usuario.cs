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
        [Required (ErrorMessage = "El apellido es obligatorio.")]
        public string Apellido { get; set; }
        [Required (ErrorMessage = "El mail es obligatorio.")]
        public string Email { get; set; }

        [Required (ErrorMessage = "La dirección es obligatoria.")]
        public string Direccion { get; set; }

        [Required (ErrorMessage = "El teléfono es obligatorio.")]
        public string Telefono { get; set; }

        [Required]
        public bool Estado { get; set; }
        
        public string? Clave { get; set; }
        
        public string? Rol { get; set; }
   }
}