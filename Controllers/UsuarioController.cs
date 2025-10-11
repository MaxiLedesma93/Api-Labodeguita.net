using Api_Labodeguita.net.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        public IConfiguration config { get; }
        public IWebHostEnvironment environment { get; }

        public UsuariosController(DataContext context, IConfiguration config,IWebHostEnvironment environment)
         {
            this.contexto = context;
            this.config = config;
            this.environment = environment;
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

        [HttpPost("Nuevo")]
         public async Task<IActionResult> Nuevo([FromForm] Usuario usuario){
            try{
                
                if(ModelState.IsValid){


                    usuario.Clave = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                           password: usuario.Clave,
                           salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                           prf: KeyDerivationPrf.HMACSHA1,
                           iterationCount: 1000,
                           numBytesRequested: 256 / 8));
                    usuario.Estado = true;
                    contexto.Usuarios.Add(usuario);
                    await contexto.SaveChangesAsync();
                    return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
                }
                return BadRequest();
               
                 

            }catch(Exception ex){
                 return BadRequest(ex.InnerException?.Message ?? ex.Message);
                
            }

        }


           
    }
}