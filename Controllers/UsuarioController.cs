using ApiInmobiliaria.Models;
using Microsoft.AspNetCore.Mvc;


namespace Api_Labodeguita.net.Controllers

{
    //localhost:5000/usuarios
    [Route("[controller]")]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsuariosController : ControllerBase
    {
        /* private readonly DataContext contexto;

         public ContratosController(DataContext context)
         {
             contexto = context;
         }
         */
         //localhost/usuarios/1
         //localhost/usuarios/${id}
        [HttpGet("{id}")]
        public async Task<ActionResult> GetUsuario(int id)
        {
            
             try
            {


                var usuario = new Usuario
                {
                    Id = 1,
                    Nombre = "Carlos Alberto",
                    Telefono = "2664935541"
                };


                return usuario != null ? Ok(usuario) : NotFound();
            }
          

            
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());

            }
        }

           
    }
}