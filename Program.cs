using System.ComponentModel.Design;
using Microsoft.AspNetCore.Cors.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors((options) =>
{
    options.AddPolicy("DevCors", (corsbuilder) =>
    {
            corsbuilder.WithOrigins("http://localhost4200","http://localhost3000","http://localhost8000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
     options.AddPolicy("ProdCors", (corsbuilder) =>
    {
            corsbuilder.WithOrigins("https://myProductionSite.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });


});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseCors("ProdCors");
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.MapControllers();
// app.MapGet("/weatherforecast", () =>
// {

// })
// .WithName("GetWeatherForecast")
// .WithOpenApi();
app.Run();

