using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Service;
using IdentityService.Service.OtpRedis;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;


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


builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    // OTP Request: 10 requests per IP per minute
    options.AddPolicy("otp-request-by-ip", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",

            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    // OTP Verify: 20 requests per IP per minute
    options.AddPolicy("otp-verify-by-ip", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",

            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
