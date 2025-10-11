
using Api_Labodeguita.net.Models;
using Microsoft.EntityFrameworkCore;




var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.

builder.Services.AddControllers();
builder.WebHost.UseUrls("http://localhost:5000", "http://localhost:5001", "http://*:5000", "http://*:5001");

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataContext>(
	options => options.UseMySql(
		configuration["ConnectionStrings:DefaultConnection"],
		ServerVersion.AutoDetect(configuration["ConnectionStrings:DefaultConnection"])
	)
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseCors(x => x
	.AllowAnyOrigin()
	.AllowAnyMethod()
	.AllowAnyHeader());
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
