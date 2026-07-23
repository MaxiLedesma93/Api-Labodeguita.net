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
    //[ApiController]
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
                if(idEstado != -1)
                {
                    var listaP = await contexto.Pedido
                        .Include(x => x.Cliente)
                        .Include(x => x.Estado)
                        .Where(x => x.EstadoId == idEstado && x.Fecha == DateTime.Today).ToListAsync();
                    foreach (Pedido p in listaP)
                        {
                            var detalles = await contexto.Detalle.Where(x => x.PedidoId == p.Id).ToListAsync();
                            p.Detalles = detalles;
                            foreach (Detalle d in detalles)
                            {
                                var producto = await contexto.Producto.SingleOrDefaultAsync(x => x.Id == d.ProductoId);
                                d.Producto = producto;
                            }
                        }
                    if (listaP != null)
                    {
                        return Ok(listaP);
                    }
                    else
                    {
                        
                        return NotFound();
                    }
                }
                else
                {
                    var listaP = await contexto.Pedido
                        .Include(x => x.Cliente)
                        .Include(x => x.Estado)
                        .Where(x => x.Fecha == DateTime.Today && (x.EstadoId == 1 || x.EstadoId == 2)).ToListAsync();

                    foreach (Pedido p in listaP)
                            {
                                var detalles = await contexto.Detalle.Where(x => x.PedidoId == p.Id).ToListAsync();
                                p.Detalles = detalles;
                                foreach (Detalle d in detalles)
                                {
                                    var producto = await contexto.Producto.SingleOrDefaultAsync(x => x.Id == d.ProductoId);
                                    d.Producto = producto;
                                }
                            }
                    if (listaP != null)
                    {
                         
                        return Ok(listaP);
                    }
                    else
                    {
                        
                        return NotFound();
                    }
                }
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }

        //lista los pedidos al usuario logueado
        [HttpGet("ListarPedidosDeUsuario")]
        [Authorize]
        public async Task<ActionResult<List<Pedido>>> ListaPedidosPorUsuario()
        {
            try
            {
                var cliente = User.Identity.Name;

                var listaP = await contexto.Pedido
                    .AsNoTracking()
                    .Include(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Where(x => x.Cliente.Email == cliente)
                    .OrderByDescending(x => x.Id)
                    //traemos los ultimos 10 pedidos del cliente.
                    .Take<Pedido>(10)
                    .ToListAsync();

                foreach (Pedido p in listaP)
                {
                    var detalles = await contexto.Detalle.Where(x => x.PedidoId == p.Id).ToListAsync();
                    p.Detalles = detalles;
                    foreach (Detalle d in detalles)
                    {
                        var producto = await contexto.Producto.SingleOrDefaultAsync(x => x.Id == d.ProductoId);
                        d.Producto = producto;
                    }
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

//authorize con rol 
        [HttpPatch("CambiarEstadoPedido")]
        [Authorize]
        public async Task<ActionResult> CambiarEstadoPedido([FromForm] int idPedido, [FromForm] int idEstado)
        {
            try
            {
                Console.WriteLine("idPedido "+ idPedido);
                Console.WriteLine("idEstado "+ idEstado);
                var pedido = await contexto.Pedido
                                    .Include(x => x.Cliente)
                                    .SingleOrDefaultAsync(x => x.Id == idPedido);
               // var detalles = await contexto.Detalle.Where(x => x.PedidoId == idPedido).ToListAsync();
                //pedido.Detalles = detalles;
                
                

                if (pedido != null)
                {
                    var detalles = await contexto.Detalle.Where(x => x.PedidoId == pedido.Id).ToListAsync();
                    foreach (Detalle d in detalles)
                    {
                        var producto = await contexto.Producto.SingleOrDefaultAsync(x => x.Id == d.ProductoId);
                        d.Producto = producto;
                    }
                    pedido.Detalles = detalles;
                    var estado = await contexto.Estado.SingleOrDefaultAsync(x => x.Id == idEstado);
                    pedido.Estado = estado;
                    pedido.EstadoId = idEstado;
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
       
        [HttpPost("GuardarPedido")]
        public async Task<IActionResult> GuardarPedido([FromBody] Pedido pedido)

        {
            try
            {

                var emailUsuario = User.Identity.Name;
                var cliente = await contexto.Usuario.SingleOrDefaultAsync(x => x.Email == emailUsuario);
                //asignamos el Id del cliente con el id del usuario logueado/autenticado.
                pedido.ClienteId = cliente.Id;
                pedido.EstadoId = 1;
                    contexto.Add(pedido);
                    await contexto.SaveChangesAsync();
                    return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id }, pedido);
            
                 
            }catch(Exception ex)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
           
            
        }
    


        //filtrar solo para cliente. fijarse q el estado del pedido no sea distinto a recibido.
        [HttpPatch("EditarPedido")]
        //Editar producto
        public async Task<IActionResult> EditarPedido([FromBody] Pedido p)
        {
            try
            {
                var emailUsuario = User.Identity.Name;
                var cliente = await contexto.Usuario.SingleOrDefaultAsync(x => x.Email == emailUsuario);
                //
                
                Pedido pedidoBD = await contexto.Pedido.AsNoTracking().FirstOrDefaultAsync(x => x.Id == p.Id);
               /* foreach (var kvp in ModelState)
                {
                    foreach (var error in kvp.Value.Errors)
                    {
                        Console.WriteLine($"ModelState Error - Campo: {kvp.Key} | Error: {error.ErrorMessage}");
                    }
                }  */
            
                //validar q el usuario sea el mismo que edita.
                if (pedidoBD.ClienteId == cliente.Id)
                {
                    pedidoBD.Delivery = p.Delivery;
                    pedidoBD.DireccionEntrega = p.DireccionEntrega;
                    pedidoBD.Fecha = p.Fecha;
                    pedidoBD.ImporteTotal = p.ImporteTotal;
                    contexto.Pedido.Update(pedidoBD);
                    await contexto.SaveChangesAsync();
                    return Ok(p);
                }else{
                    return BadRequest("No tiene permiso para editar este pedido.");
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