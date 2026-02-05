using Api_Labodeguita.net.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api_Labodeguita.net.Controllers
{
    [Route("[controller]")]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class TipoController : ControllerBase
    {
        #region Propiedades
        private readonly DataContext contexto;
        public IConfiguration config { get; }
        public IWebHostEnvironment environment { get; }

        public TipoController(DataContext context, IConfiguration config,IWebHostEnvironment environment)
        {
            this.contexto = context;
            this.config = config;
            this.environment = environment;
        }
        #endregion

        #region EndPoints
        [HttpGet("{id}")]
        //localhost/tipo/${id}
        public async Task<ActionResult> GetTipo(int id)
        {
            try
            {
                var tipo = await contexto.Tipo.SingleOrDefaultAsync(x => x.Id == id);
                return tipo != null ? Ok(tipo) : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }
        [HttpPost("GuardarTipo")]
        public async Task<IActionResult> GuardarTipo([FromForm] Tipo tipo)

        {
            try
            {
                if (ModelState.IsValid)
                {
                    contexto.Add(tipo);
                    await contexto.SaveChangesAsync();
                    return CreatedAtAction(nameof(GetTipo), new { id = tipo.Id }, tipo);
                }
                else
                {
                    return BadRequest("Model state no es valido");
                }
                 
            }catch(Exception ex)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
           
            
        }


        #endregion
    }
}