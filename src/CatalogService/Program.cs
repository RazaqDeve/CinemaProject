using CatalogService.Context;
using CatalogService.Services.CinemaService;
using CatalogService.Services.HallService;
using CatalogService.Services.MovieService;
using CatalogService.Services.RowSerivce;
using CatalogService.Services.SeatService;
using CatalogService.Services.ShowtimeService;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CatalogDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<ICinemaService, CinemaService>();
builder.Services.AddScoped<IHallService, HallService>();
builder.Services.AddScoped<IRowService, RowService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<IShowtimeService, ShowtimeService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
