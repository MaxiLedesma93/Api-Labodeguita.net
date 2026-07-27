using Api_Labodeguita.net.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Api_Labodeguita.net.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

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
        [Authorize]
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
        [Authorize]
        public async Task<IActionResult> RegistrarPago([FromBody] Pago pago)

        {
            try
            {
                contexto.Add(pago);
                await contexto.SaveChangesAsync();
                return CreatedAtAction(nameof(GetPago), new { id = pago.Id }, pago);
                 
            }catch(Exception ex)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            } 
        }
        [HttpGet("TotalFacturadoFecha/{fecha}")]
        [Authorize(Policy = "Recepcionista")]
        //localhost/pago/${id}
        public async Task<ActionResult> TotalFacturadoFecha(String Fecha)
        {
            try
            {
                double totalMp = 0;
                double totalEfectivo = 0;
                
                string formato = "dd-MM-yyyy";
                DateTime fechaConvertida = DateTime.ParseExact(Fecha, formato, CultureInfo.InvariantCulture);
                Console.WriteLine(fechaConvertida);
                var listaPedidos = await contexto.Pedido
                .Where(x => x.Fecha == fechaConvertida && x.EstadoId == 1).ToListAsync();
                var listaEnviar = new List<Pago>();
                foreach (Pedido pedido in listaPedidos) {
                    var listaPagosxPedido = await contexto.Pago.Where(x => x.PedidoId == pedido.Id).ToListAsync();
                    foreach(Pago pago in listaPagosxPedido)
                    {
                         if(pago != null)
                        {
                            pago.Direccion = pedido.DireccionEntrega;
                            listaEnviar.Add(pago);
                        }
                    }
                   
                }              
                return Ok(listaEnviar);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }
        [HttpGet("ObtenerPagosPorPedido/{idPedido}")]
        [Authorize(Policy = "Recepcionista")]
        public async Task<ActionResult> ObtenerPagosPorPedido(int idPedido)
        {
            try
            {
                var pagos = await contexto.Pago.Where(x => x.PedidoId == idPedido).ToListAsync();
                return pagos != null ? Ok(pagos) : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }

        #endregion
    }
}