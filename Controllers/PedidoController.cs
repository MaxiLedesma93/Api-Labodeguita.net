using Api_Labodeguita.net.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api_Labodeguita.net.Controllers
{
    [Route("[controller]")]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class PedidoController : ControllerBase
    {
        #region Propiedades
        private readonly DataContext contexto;
        public IConfiguration config { get; }
        public IWebHostEnvironment environment { get; }

        public PedidoController(DataContext context, IConfiguration config,IWebHostEnvironment environment)
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
        public async Task<ActionResult> GetPedido(int id)
        {
            try
            {
                var cliente = User.Identity.Name;
                var detalles = await contexto.Detalle.Where(x => x.PedidoId == id).ToListAsync();
                var pedidos = await contexto.Pedido
                                .Include(x => x.Cliente)
                                .Include(x => x.Estado)
                                .Where(x => x.Cliente.Email == cliente)
                                .SingleOrDefaultAsync(x => x.Id == id);

                pedidos.Detalles = detalles;
                return pedidos != null ? Ok(pedidos) : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }

        [HttpGet("ListarPedidos/{idEstado}")]
        [Authorize]
        //Lo usa la recepcionista, devuelve una lista de todos los pedidos
        //que esten en estado = "Recibido", "En Preparación", "Terminado"
        public async Task<ActionResult<List<Pedido>>> ListaPedidos(int idEstado)
        {
            try
            {
                var listaP = await contexto.Pedido
                .Include(x => x.Cliente)
                .Include(x => x.Estado)
                .Where(x => x.EstadoId == idEstado).ToListAsync();
                if (listaP != null)
                {
                    return Ok(listaP);
                }
                else
                {
                    //!ver como devolver un mensaje de no hay registros
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }

        [HttpGet("ListarPedidosPorUsuario")]
        [Authorize]
        public async Task<ActionResult<List<Pedido>>> ListaPedidosPorUsuario()
        {
            try
            {
                var cliente = User.Identity.Name;

                var listaP = await contexto.Pedido
                .Include(x => x.Cliente)
                .Include(x => x.Estado)
                .Where(x => x.Cliente.Email == cliente).ToListAsync();

                foreach (Pedido p in listaP)
                {
                    var detalles = await contexto.Detalle.Where(x => x.PedidoId == p.Id).ToListAsync();
                    p.Detalles = detalles;
                }
                if (listaP != null)
                {
                    return Ok(listaP);
                }
                else
                {
                    //!ver como devolver un mensaje de no hay registros
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }

        [HttpPatch("CambiarEstadoPedido")]
        [Authorize]
        public async Task<ActionResult> CambiarEstadoPedido([FromForm] int IdEstado, int IdPedido)
        {
            try
            {
                var pedido = await contexto.Pedido
                                    .Include(x => x.Cliente)
                                    .Include(x => x.Estado)
                                    .SingleOrDefaultAsync(x => x.Id == IdPedido);
                var detalles = await contexto.Detalle.Where(x => x.PedidoId == IdPedido).ToListAsync();
                pedido.Detalles = detalles;

                if (pedido != null)
                {
                    pedido.EstadoId = IdEstado;
                    contexto.Pedido.Update(pedido);
                    await contexto.SaveChangesAsync();
                    return Ok(pedido);
                }
                else { return BadRequest("No se encontró el pedido"); }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }

        [HttpPatch("RegistrarPago")]
        [Authorize]
        public async Task<ActionResult> RegistrarPago(int idPedido)
        {
            try
            {
                var pedido = await contexto.Pedido
                                    .Include(x => x.Cliente)
                                    .Include(x => x.Estado)
                                    .SingleOrDefaultAsync(x => x.Id == idPedido);
                var detalles = await contexto.Detalle.Where(x => x.PedidoId == idPedido).ToListAsync();
                pedido.Detalles = detalles;

                if (pedido != null)
                {
                    pedido.Pagado = true;
                    contexto.Pedido.Update(pedido);
                    await contexto.SaveChangesAsync();
                    return Ok(pedido);
                }
                else { return BadRequest("No se encontró el pedido"); }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }
        [HttpGet("ListaPedidosPorFecha/{Fecha}")]
        [Authorize]
        public async Task<ActionResult<List<Pedido>>> ListaPedidosPorFecha(DateTime Fecha)
        {
            try
            {
                var listaP = await contexto.Pedido
                .Include(x => x.Cliente)
                .Include(x => x.Estado)
                .Where(x => x.Fecha == Fecha && x.EstadoId == 5).ToListAsync();

                foreach (Pedido p in listaP)
                {
                    var detalles = await contexto.Detalle
                    .Include(x => x.Producto)
                    .Where(x => x.PedidoId == p.Id).ToListAsync();
                    p.Detalles = detalles;
                }
                if (listaP != null)
                {
                    return Ok(listaP);
                }
                else
                {
                    //!ver como devolver un mensaje de no hay registros
                    return NotFound();
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