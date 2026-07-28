using Api_Labodeguita.net.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api_Labodeguita.net.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class DetalleController : ControllerBase
    {
        #region Propiedades
        private readonly DataContext contexto;
        public IConfiguration config { get; }
        public IWebHostEnvironment environment { get; }
        public const int ESTADO_RECIBIDO = 1;

        public DetalleController(DataContext context, IConfiguration config,IWebHostEnvironment environment)
        {
            this.contexto = context;
            this.config = config;
            this.environment = environment;
        }
        #endregion

        #region EndPoints
        [HttpGet("{id}")]
        [Authorize]
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
        [Authorize]
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
                    x.Producto 
                })
                .ToListAsync();
                return detalle != null ? Ok(detalle) : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }
        
        [HttpPost("GuardarDetalle")]
        [Authorize(Policy = "Cliente")]
        public async Task<IActionResult> GuardarDetalle([FromBody] Detalle detalle)

        {
            try
            {
                var pedido = await contexto.Pedido.SingleOrDefaultAsync(x => x.Id == detalle.PedidoId);
                var emailUsuario = User.Identity.Name;
                var cliente = await contexto.Usuario.SingleOrDefaultAsync(x => x.Email == emailUsuario);

                
                //Validamos que el pedido al que pertenece el detalle este en estado "Recibido" id = 1;
                //Validamos que el usuario que edita el pedido sea el mismo que creo el pedido.
                if (pedido.EstadoId == ESTADO_RECIBIDO && pedido.ClienteId == cliente.Id)
                {
                    contexto.Add(detalle);
                    await contexto.SaveChangesAsync();
                    return CreatedAtAction(nameof(GetDetalle), new { id = detalle.Id }, detalle);
                }
                else{
                    return BadRequest("No se puede editar un pedido que este en Preparacion o sea de otro Usuario");
                }
               
               
                 
            }catch(Exception ex)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }
        
        [HttpDelete("borrardetalle/{id}")]
        [Authorize(Policy = "Cliente")]
        //localhost/detalle/${id}
        public async Task<ActionResult> BorrarDetalle(int id)
        {
            try
            {
                var detalle = await contexto.Detalle.SingleOrDefaultAsync(x => x.Id == id);
                var emailUsuario = User.Identity.Name;
                var cliente = await contexto.Usuario.SingleOrDefaultAsync(x => x.Email == emailUsuario);
                if(detalle != null)
                {
                    var pedido = await contexto.Pedido.SingleOrDefaultAsync(x => x.Id == detalle.PedidoId);
                    if(pedido.EstadoId == ESTADO_RECIBIDO && pedido.ClienteId == cliente.Id)
                    {
                        contexto.Detalle.Remove(detalle);
                        await contexto.SaveChangesAsync();
                        return Ok();
                    }
                        
                        else
                    {
                        return BadRequest("No tiene permiso para eliminar el detalle");
                    }
                }
                else
                {
                    return BadRequest("No se encontro detalle para el pedido.");
                }
                
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }
        #endregion
    }
}