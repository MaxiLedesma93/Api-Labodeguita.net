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

    public class ProductoController : ControllerBase
    {
         #region Propiedades
        private readonly DataContext contexto;
        public IConfiguration config { get; }
        public IWebHostEnvironment environment { get; }

        public ProductoController(DataContext context, IConfiguration config, IWebHostEnvironment environment)
        {
            this.contexto = context;
            this.config = config;
            this.environment = environment;
        }
        #endregion

        #region EndPoints
        [HttpGet("{id}")]
        //localhost/producto/${id}
        //obtiene un producto por id
        public async Task<ActionResult> GetProducto(int id)
        {
            try
            {
                var producto = await contexto.Producto.SingleOrDefaultAsync(x => x.Id == id);
                return producto != null ? Ok(producto) : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());

            }
        }

        [HttpGet]
        //Obtiene una lista de productos
        public async Task<ActionResult<List<Producto>>> ListaProductos()
        {
            try
            {
                var lista = await contexto.Producto.Where(x => x.Estado == true).ToListAsync();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }

        [HttpPost("GuardarProducto")]
        //Alta producto
        public async Task<IActionResult> GuardarProducto([FromForm] Producto producto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    contexto.Add(producto);
                    await contexto.SaveChangesAsync();

                    return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
                }
                else
                {
                    return BadRequest("Model State no es valido.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpPatch("{id}")]
        //Editar producto
        public async Task<IActionResult> EditarProducto(Producto producto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (producto != null)
                    {
                        contexto.Producto.Update(producto);
                        await contexto.SaveChangesAsync();
                        return Ok(producto);
                    }
                    else return BadRequest();
                }
                else
                {
                    return BadRequest("Model State no es valido.");
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