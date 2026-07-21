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

    public class EstadoController : ControllerBase
    {
        #region Propiedades
        private readonly DataContext contexto;
        public IConfiguration config { get; }
        public IWebHostEnvironment environment { get; }

        public EstadoController(DataContext context, IConfiguration config,IWebHostEnvironment environment)
        {
            this.contexto = context;
            this.config = config;
            this.environment = environment;
        }
        #endregion

        #region EndPoints
        [HttpGet("{id}")]
        //localhost/Estado/${id}
        public async Task<ActionResult> GetEstado(int id)
        {
            try
            {
                var estado = await contexto.Estado.SingleOrDefaultAsync(x => x.Id == id);
                return estado != null ? Ok(estado) : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }
        
        [HttpGet("listarEstados")]
        //localhost/listarEstados
        public async Task<ActionResult> listarEstados()
        {
            try
            {
                var estados = await contexto.Estado.ToListAsync();
                return estados != null ? Ok(estados) : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }


        #endregion
    }
}