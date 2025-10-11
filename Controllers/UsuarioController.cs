using Api_Labodeguita.net.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;



namespace Api_Labodeguita.net.Controllers

{
    //localhost:5000/usuarios
    [Route("[controller]")]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsuariosController : ControllerBase
    {
         private readonly DataContext contexto;

         public UsuariosController(DataContext context)
         {
             contexto = context;
         }
         
         //localhost/usuarios/1
         //localhost/usuarios/${id}
        [HttpGet("{id}")]
        public async Task<ActionResult> GetUsuario(int id)
        {
            
             try
            {


                var usuario = await contexto.Usuarios.SingleOrDefaultAsync(x => x.Id == id);


                return usuario != null ? Ok(usuario) : NotFound();
            }
          

            
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());

            }
        }

           
    }
}