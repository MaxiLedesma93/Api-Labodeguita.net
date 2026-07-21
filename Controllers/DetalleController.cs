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

    public class DetalleController : ControllerBase
    {
        #region Propiedades
        private readonly DataContext contexto;
        public IConfiguration config { get; }
        public IWebHostEnvironment environment { get; }

        public DetalleController(DataContext context, IConfiguration config,IWebHostEnvironment environment)
        {
            this.contexto = context;
            this.config = config;
            this.environment = environment;
        }
        #endregion

        #region EndPoints
        [HttpGet("{id}")]
        //localhost/detalle/${id}
        public async Task<ActionResult> GetDetalle(int id)
        {
            try
            {
                var detalle = await contexto.Detalle.SingleOrDefaultAsync(x => x.Id == id);
                return detalle != null ? Ok(detalle) : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }
        [HttpGet("obtenerdetalleporpedido/{pedidoId}")]
        //localhost/detalle/${id}
        public async Task<ActionResult> GetDetallePorPedido(int pedidoId)
        {
            try
            {
                var detalle = await contexto.Detalle
                .Where(x => x.PedidoId == pedidoId)
                .Select(x => new 
                {
                    x.Id,
                    x.Cantidad,
                    x.PedidoId,
                    x.ProductoId,
                    x.Producto //tratamos de enviar el producto.
                })
                .ToListAsync();
                return detalle != null ? Ok(detalle) : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }
        //fijarse q el estado del pedido no sea distinto a recibido.
        [HttpPost("GuardarDetalle")]
        public async Task<IActionResult> GuardarDetalle([FromForm] Detalle detalle)

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
                    contexto.Add(detalle);
                    await contexto.SaveChangesAsync();
                    return CreatedAtAction(nameof(GetDetalle), new { id = detalle.Id }, detalle);
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
        
         [HttpDelete("borrardetalle/{id}")]
        //localhost/detalle/${id}
        public async Task<ActionResult> BorrarDetalle(int id)
        {
            try
            {
                var detalle = await contexto.Detalle.SingleOrDefaultAsync(x => x.Id == id);
                if(detalle != null)
                {
                     contexto.Detalle.Remove(detalle);
                }
                contexto.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }
        //fijarse q el estado del pedido no sea distinto a recibido.
         [HttpPatch("EditarDetalle")]
        public async Task<IActionResult> EditarDetalle([FromForm] Detalle detalle)

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
                    contexto.Update(detalle);
                    await contexto.SaveChangesAsync();
                    return Ok();
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