using Api_Labodeguita.net.Models;
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
        //localhost/detalle/${id}
        public async Task<ActionResult> GetPedido(int id)
        {
            try
            {
                var cliente = User.Identity.Name;
                var pedidos = await contexto.Pedido
                                .Include(x => x.Cliente)
                                .Include(x => x.Estado)
                                .Where(x => x.Cliente.Email == cliente)
                                .SingleOrDefaultAsync(x => x.Id == id);

                return pedidos != null ? Ok(pedidos) : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }

        [HttpGet("ListarPedidos/{idEstado}")] 
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
        #endregion
    }
}