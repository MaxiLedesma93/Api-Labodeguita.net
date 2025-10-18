using Api_Labodeguita.net.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;



namespace Api_Labodeguita.net.Controllers

{
    //localhost:5000/usuario
    [Route("[controller]")]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsuarioController : ControllerBase
    {
         private readonly DataContext contexto;

        public IConfiguration config { get; }
        public IWebHostEnvironment environment { get; }

        public UsuarioController(DataContext context, IConfiguration config, IWebHostEnvironment environment)
        {
            this.contexto = context;
            this.config = config;
            this.environment = environment;
        }

        //!Actualizar perfil, probar. (hacer que obtenga el perfil de la claim Name(email)).
        //!Hacer baja logica del perfil. (evita problemas relacionales de la bd, ya que sino deberia borrarse todos los pedidos del usuario).
        //! Roles, Cliente y Recepcionista.

        //! probar todo.


        //localhost:5000/usuario/1
        //localhost:5000/usuario/${id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult> GetUsuario(int id)
        {

            try
            {
                var usuario = await contexto.Usuario.SingleOrDefaultAsync(x => x.Id == id);


                return usuario != null ? Ok(usuario) : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());

            }
        }

        //localhost:5000/usuario/nuevo
        [HttpPost("Nuevo")]
        [AllowAnonymous]
        public async Task<IActionResult> Nuevo([FromForm] Usuario usuario)
        {
            try
            {
               
                //verificamos si existe un usuario logueado
                if(User.Identity.IsAuthenticated)
                {   
                    //Validamos para que un usuario Cliente no pueda dar de alta a un Recepcionista.
                    if(User.IsInRole("Cliente"))
                    {
                        usuario.Rol = "Cliente";
                    }
                }
                else
                {
                    usuario.Rol = "Cliente";
                }
               

                if (ModelState.IsValid)
                {
                    var usuarioExistente = await contexto.Usuario.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Email == usuario.Email);
                    if(usuarioExistente == null || usuarioExistente.Estado == false)
                        {
                        usuario.Clave = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                            password: usuario.Clave,
                            salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                            prf: KeyDerivationPrf.HMACSHA1,
                            iterationCount: 1000,
                            numBytesRequested: 256 / 8));
                        usuario.Estado = true;
                        contexto.Usuario.Add(usuario);
                        await contexto.SaveChangesAsync();
                        return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
                    }else
                    {
                        return BadRequest("El email ya se encuentra registrado.");
                    }
                }
                return BadRequest();



            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);

            }

        }

        //localhost:5000/usuario/editar
        [HttpPatch("Editar")]
        [Authorize]
        public async Task<IActionResult> Editar([FromForm] Usuario usuario)
        {
            try
            {
                //obtengo el email del usuario mediante la claim Name.
                var emailUsuario = User.Identity.Name;
                //obtengo el usuario logueado
                var usuarioLogueado = await contexto.Usuario.AsNoTracking().FirstOrDefaultAsync(x => x.Email == emailUsuario);
                usuario.Id = usuarioLogueado.Id;
                usuario.Clave = usuarioLogueado.Clave;
                usuario.Email = usuarioLogueado.Email;
                usuario.Estado = usuarioLogueado.Estado;
                usuario.Rol = usuarioLogueado.Rol;


                if (ModelState.IsValid)
                {
                    contexto.Usuario.Update(usuario);
                    await contexto.SaveChangesAsync();
                    return Ok(usuario);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }


        // POST localhost:5000/usuario/login
        [HttpPost("login")]
        [AllowAnonymous]
        
        public async Task<IActionResult> Login([FromForm] Login login)
        {
            Usuario u = null;
            try
            {
               

                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: login.Clave,
                    salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8));
                 u = await contexto.Usuario.FirstOrDefaultAsync(x => x.Email== login.Email);
                if (u == null || u.Clave != hashed)
                {
                    return BadRequest("Nombre de usuario o clave incorrecta");
                }
                else if(u.Clave == hashed)
                    {
                        var key = new SymmetricSecurityKey(
                            System.Text.Encoding.ASCII.GetBytes(config["TokenAuthentication:SecretKey"]));
                        var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, u.Email),
                            new Claim("FullName", u.Nombre + " " + u.Apellido),
                            new Claim(ClaimTypes.Role, u.Rol),
                        };

                        var token = new JwtSecurityToken(
                            issuer: config["TokenAuthentication:Issuer"],
                            audience: config["TokenAuthentication:Audience"],
                            claims: claims,
                            expires: DateTime.Now.AddDays(360),
                            signingCredentials: credenciales
                        );
                        return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                    }
                    else{
                        return BadRequest("Nombre de usuario o clave incorrecta");
                    }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }



           
    }
}