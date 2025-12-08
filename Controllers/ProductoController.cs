using Api_Labodeguita.net.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Text.Json.Serialization.Metadata;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

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

        [HttpGet ("listar")]
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
                  // 3. ¡SI ALGO FALLA, LA EXCEPCIÓN SE CAPTURA AQUÍ!
                    // En lugar de crashear, el servidor ahora hará dos cosas:

                    // a. Registrar el error en la consola del servidor para que tú puedas verlo.
                    Console.WriteLine($"Error al listar productos: {ex.Message}");
                    // (En un proyecto real, usarías un sistema de logging como ILogger)

                    // b. Devolver una respuesta de error 500 limpia a la app de Android.
                    // Esto NO causará 'unexpected end of stream'. Retrofit lo interpretará
                    // como un error HTTP y podrás manejarlo en el callback onFailure.
                    return StatusCode(500, $"Error interno del servidor: {ex.Message}");
                //return BadRequest(ex.Message.ToString());
            }
        }

        [HttpPost("GuardarProducto")]
        //Alta producto
        public async Task<IActionResult> GuardarProducto([FromForm] Producto producto)
        {
            try
            {
                producto.Foto = "Sin foto";
                if (ModelState.IsValid)
                {
                    contexto.Add(producto);
                    await contexto.SaveChangesAsync();
                    if(producto.Imagen!=null){
                        var imagePath = await guardarImagen(producto);
                        producto.Foto = imagePath;
                        await contexto.SaveChangesAsync();
                        //seteo null la imagen para evitar un error de que llega un objeto cuando espera un array el retrofit.
                        producto.Imagen = null;
                    }
                    

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

        [HttpPatch("EditarProducto")]
        //Editar producto
        public async Task<IActionResult> Patch([FromBody] Producto p)
        {
            try
            {

                Producto productoBD = await contexto.Producto.AsNoTracking().FirstOrDefaultAsync(x => x.Id == p.Id);
                // productoBD.Nombre = p.Nombre;
                // productoBD.Precio = p.Precio;
                // productoBD.Estado = p.Estado;
                if (p.Imagen != null)
                {
                    var imagePath = await guardarImagen(p);
                    p.Foto = imagePath;
                    //seteo null la imagen para evitar un error de que llega un objeto cuando espera un array el retrofit.
                    p.Imagen = null;
                }
                else
                {
                    p.Foto = productoBD.Foto;
                }
                if (ModelState.IsValid){ 
					contexto.Producto.Update(p);
					await contexto.SaveChangesAsync();
					return Ok(p);
				}
                return BadRequest();
                
                
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }
        #endregion


        //funcion asincrona para guardar la imagen y modificarle tamaño.
        public async Task<string> guardarImagen(Producto entidad)
        {
            try
            {
                string wwwPath = environment.WebRootPath;
                string path = Path.Combine(wwwPath, "uploads/productos");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string fileName = "producto_" + entidad.Id + Path.GetExtension(entidad.Imagen.FileName);
                string pathCompleto = Path.Combine(path, fileName);

                // Esta operación guarda la foto en memoria en la ruta que necesitamos
                using (FileStream stream = new FileStream(pathCompleto, FileMode.Create))
                {
                    await entidad.Imagen.CopyToAsync(stream);
                    stream.Dispose();
                }
                using (var image = Image.Load(pathCompleto))
                {
                    image.Mutate(x => x.Resize(500, 500));
                    var resizedImagePath = Path.Combine(environment.WebRootPath, "uploads/productos", Path.GetFileName(fileName));
                    image.Save(resizedImagePath);
                    return Path.Combine("uploads/productos", Path.GetFileName(pathCompleto)).Replace("\\", "/");
                }

            }
            catch (Exception ex)
            {
                return (ex.Message);
            }
        }
    }
}