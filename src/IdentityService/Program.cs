using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Service;
using IdentityService.Service.OtpRedis;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<IdentityDbContext>(options => 
  options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = false; 
    // options.Password.RequiredLength = 8;
    // options.Password.RequireDigit = true;
    // options.Password.RequireUppercase = true;
    // options.Password.RequireLowercase = true;
    // options.Password.RequireNonAlphanumeric = false;
}).AddRoles<IdentityRole>()
.AddEntityFrameworkStores<IdentityDbContext>()
.AddSignInManager();

var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("Redis connection string is not configured.");

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
 ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddSingleton<OtpHashHelper>();
builder.Services.AddScoped<IOtpService, OtpService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
