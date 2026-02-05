using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Api_Labodeguita.net.Models
{
    public class Tipo
    {
            [Key]
            
            public int Id { get; set; }

            [Required]
           
            public string Descripcion { get; set; }

    }
}