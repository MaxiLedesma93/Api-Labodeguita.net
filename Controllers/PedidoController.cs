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
                                .Include(x => x.Detalle)
                                .Where(x => x.Cliente.Email == cliente)
                                .SingleOrDefaultAsync(x => x.Id == id);

                return pedidos != null ? Ok(pedidos) : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());

            }
        }

        
        #endregion
    }
}