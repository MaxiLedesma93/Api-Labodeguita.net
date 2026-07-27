using Api_Labodeguita.net.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllers();
builder.WebHost.UseUrls("http://localhost:5000", "http://localhost:5001", "http://*:5000", "http://*:5001");

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Base de Datos MySQL
builder.Services.AddDbContext<DataContext>(
    options => options.UseMySql(
        configuration["ConnectionStrings:DefaultConnection"],
        ServerVersion.AutoDetect(configuration["ConnectionStrings:DefaultConnection"])
    )
);

// Autenticación JWT
// SE AGREGA: JwtBearerDefaults.AuthenticationScheme para marcar JWT como esquema por defecto
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["TokenAuthentication:Issuer"],
        ValidAudience = configuration["TokenAuthentication:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(
            configuration["TokenAuthentication:SecretKey"]))
    };

    // Lectura del token desde la URL (?access_token=...)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// Autorización
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Cliente", policy =>
        policy.RequireRole("Cliente"));
    options.AddPolicy("Recepcionista", policy =>
        policy.RequireRole("Recepcionista"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseStaticFiles();

// Pipeline de solicitudes
app.UseRouting();          // Mapea la ruta primero
app.UseAuthentication();   // 1. Identifica al usuario
app.UseAuthorization();    // 2. Comprueba los permisos/roles

app.MapControllers();

app.Run();