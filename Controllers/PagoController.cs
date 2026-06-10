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

    public class PagoController : ControllerBase
    {
        #region Propiedades
        private readonly DataContext contexto;
        public IConfiguration config { get; }
        public IWebHostEnvironment environment { get; }

        public PagoController(DataContext context, IConfiguration config,IWebHostEnvironment environment)
        {
            this.contexto = context;
            this.config = config;
            this.environment = environment;
        }
        #endregion

        #region EndPoints
        [HttpGet("{id}")]
        //localhost/pago/${id}
        public async Task<ActionResult> GetPago(int id)
        {
            try
            {
                var pago = await contexto.Pago.SingleOrDefaultAsync(x => x.Id == id);
                return pago != null ? Ok(pago) : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }
        [HttpPost("RegistrarPago")]
        public async Task<IActionResult> RegistrarPago([FromForm] Pago pago)

        {
            try
            {
                 Console.WriteLine("MODEL STATE: " + ModelState.IsValid);
                
                foreach (var kvp in ModelState)
                {
                    foreach (var error in kvp.Value.Errors)
                    {
                        Console.WriteLine($"ModelState Error - Campo: {kvp.Key} | Error: {error.ErrorMessage}");
                    }
                } 
                if (ModelState.IsValid)
                {
                    contexto.Add(pago);
                    await contexto.SaveChangesAsync();
                    return CreatedAtAction(nameof(GetPago), new { id = pago.Id }, pago);
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