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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Api_Labodeguita.net.Servicios;
using MimeKit;
using MailKit.Net.Smtp;
using System.Security.Cryptography;



namespace Api_Labodeguita.net.Controllers

{
    //localhost:5000/usuario
    [ApiController]
    [Route("[controller]")]

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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

        [HttpGet("perfil")]
        public async Task<ActionResult<Usuario>> Get()
        {
            try
            {
                var usuario = User.Identity.Name;
                return await contexto.Usuario.SingleOrDefaultAsync(x => x.Email == usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //localhost:5000/usuario/registrar
        [HttpPost("Registrar")]
        [AllowAnonymous]
        public async Task<IActionResult> Nuevo([FromBody] Usuario usuario)
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
                    if (User.IsInRole("Recepcionista"))
                    {
                        usuario.Rol = "Recepcionista";
                    }                
                }
                else
                {
                    usuario.Rol = "Cliente";
                }
               
                    var usuarioExistente = await contexto.Usuario.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Email == usuario.Email);
                    if(usuarioExistente == null || usuarioExistente.Estado == false)
                        {
                        usuario.Clave = HashearClave(usuario.Clave);
                        usuario.Estado = true;
                        contexto.Usuario.Add(usuario);
                        await contexto.SaveChangesAsync();
                        return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
                    }else
                    {
                        return BadRequest("El email ya se encuentra registrado.");
                    }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException?.Message ?? ex.Message);

            }

        }

        //localhost:5000/usuario/editar
        [HttpPatch("Editar")]
        [Authorize]
        public async Task<IActionResult> Editar([FromBody] Usuario usuario)
        {
            Console.WriteLine("Usuario recibido: " + System.Text.Json.JsonSerializer.Serialize(usuario));
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
         
                contexto.Usuario.Update(usuario);
                await contexto.SaveChangesAsync();
                return Ok(usuario);
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
               

                string hashed = HashearClave(login.Clave);
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
        [HttpPatch("cambiarClave")]
		public async Task<IActionResult> CambiarPass([FromForm] string clVieja, [FromForm]string clNueva ){
		
			var user = User.Identity.Name;
            var usuario = await contexto.Usuario.FirstOrDefaultAsync(u=>u.Email==user);
			string hashed =  HashearClave(clVieja);
			try{
                if(usuario.Clave==hashed){
                    clNueva = HashearClave(clNueva);
                    usuario.Clave = clNueva;
                    contexto.Usuario.Update(usuario);
                    await contexto.SaveChangesAsync();
                    
                }
            
                return Ok(usuario);
            }catch(Exception ex){
                return BadRequest(ex.Message.ToString());
            }
		}


        // POST api/<controller>/email
        [HttpPost("email/{email}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByEmail(string email)
        {
            try
            {
                var entidad = await contexto.Usuario.FirstOrDefaultAsync(x => x.Email == email);
                
                // Evita NullReferenceException y previene adivinación de correos
                if (entidad == null)
                {
                    return Ok(new { message = "Si el correo existe, se enviará un enlace de recuperación." });
                }

                var key = new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(config["TokenAuthentication:SecretKey"]));
                var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, entidad.Email),
                    new Claim("FullName", $"{entidad.Nombre} {entidad.Apellido}"),
                    new Claim(ClaimTypes.Role, "Propietario"),
                    new Claim("Purpose", "PasswordReset") // Claim de seguridad adicional
                };

                // Reducimos el tiempo de vida a 15 minutos
                var token = new JwtSecurityToken(
                    issuer: config["TokenAuthentication:Issuer"],
                    audience: config["TokenAuthentication:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(15), 
                    signingCredentials: credenciales
                );

                var vToken = new JwtSecurityTokenHandler().WriteToken(token);
                var url = this.GenerarUrlCompleta("Token", "Usuario", environment);
                var dominio = $"{url}?access_token={vToken}";

                var message = new MimeMessage();
                message.To.Add(new MailboxAddress(entidad.Nombre, entidad.Email));
                message.From.Add(new MailboxAddress("La Bodeguita", config["SMTPUser"]));
                message.Subject = "Link para resetear contraseña";
                message.Body = new TextPart("html")
                {
                    Text = $@"<h1>Hola, {entidad.Nombre}</h1>
                            <p>Haz <a href='{dominio}'>click aquí</a> para resetear tu contraseña.</p>
                            <p>Este enlace expira en 15 minutos.</p>"
                };
               

                // Bloque 'using' para cerrar la conexión SMTP correctamente
                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync(config["SMTPUser"], config["SMTPPass"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return Ok(new { message = "Correo enviado con éxito." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // GET api/<controller>/token
        [HttpGet("token")]
        [Authorize] // Asegúrate de incluir el atributo explícito de autorización
        public async Task<IActionResult> Token()
        {
            try
            {
                var email = User.Identity?.Name;
                var nombre = User.Claims.FirstOrDefault(x => x.Type == "FullName")?.Value;

                if (string.IsNullOrEmpty(email))
                    return Unauthorized();

                // Generamos la clave aleatoriamente.
                string randomChars = "ABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
                char[] claveChars = new char[6];
                for (int i = 0; i < 6; i++)
                {
                    claveChars[i] = randomChars[RandomNumberGenerator.GetInt32(0, randomChars.Length)];
                }
                string nuevaClave = new string(claveChars);
                //hasheamos la nueva clave.
                var claveHasheada = HashearClave(nuevaClave);
                
                Usuario u = await contexto.Usuario.SingleOrDefaultAsync(x => x.Email == email);
                if (u == null) return NotFound("Usuario no encontrado.");

                u.Clave = claveHasheada;
                contexto.Usuario.Update(u);
                await contexto.SaveChangesAsync(); 

                var message = new MimeMessage();
                message.To.Add(new MailboxAddress(nombre ?? "Usuario", email));
                message.From.Add(new MailboxAddress("La Bodeguita", config["SMTPUser"]));
                message.Subject = "Envío de nueva contraseña";
                message.Body = new TextPart("html")
                {
                    Text = $@"<h1>Hola</h1>
                            <p>{nombre},TU NUEVA CLAVE ES: <strong>{nuevaClave}</strong></p>"
                };

                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync(config["SMTPUser"], config["SMTPPass"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                var htmlEnviado = @"<dialog open>
                                    <p>Clave Reseteada con éxito. Revisa tu correo electrónico.</p>
                                    <button onclick='window.close()'>Cerrar ventana</button>
                                    </dialog>";

                return Content(htmlEnviado, "text/html");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        private string HashearClave(string clave)
        {
            
            if(clave != "" && clave != null)
            {
                var claveHasheada = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                            password: clave,
                            salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                            prf: KeyDerivationPrf.HMACSHA1,
                            iterationCount: 1000,
                            numBytesRequested: 256 / 8));
                return (claveHasheada);
            }
            return "error en Hasheo";
            
            
        }

    }
}