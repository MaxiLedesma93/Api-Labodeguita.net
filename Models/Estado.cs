using System.ComponentModel.DataAnnotations;

namespace Api_Labodeguita.net.Models
{
    public class Estado
    {
        [Key]
        [Display(Name = "Estado Pedido")]
        public int Id { get; set; }

        [Required]
        public string Descripcion { get; set; }
    }
}
