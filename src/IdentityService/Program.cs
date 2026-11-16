using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Service;
using IdentityService.Service.MeliPayamak;
using IdentityService.Service.OtpRedis;
using IdentityService.Service.OtpRedis.SmsOtp;
using IdentityService.Service.SmsOtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;



using StackExchange.Redis;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------
// Database Configuration
// ----------------------------------

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ----------------------------------
// Identity Configuration
// ----------------------------------

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = false;

})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<IdentityDbContext>()
.AddSignInManager();

// ----------------------------------
// Redis Configuration
// ----------------------------------

var redisConnectionString =
    builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException(
        "Redis connection string is not configured.");

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(redisConnectionString));

// ----------------------------------
// https://www.melipayamak.com/ Options
// ----------------------------------

builder.Services.Configure<MeliPayamakOptions>(
    builder.Configuration.GetSection("MeliPayamak"));

builder.Services.AddHttpClient<ISmsSender, MeliPayamakSender>(
    client =>
    {
        client.BaseAddress = new Uri(
            "https://rest.payamak-panel.com/");

        client.Timeout = TimeSpan.FromSeconds(15);
    });


// ----------------------------------
// OTP Services
// ----------------------------------

builder.Services.AddSingleton<OtpHashHelper>();

builder.Services.AddScoped<IOtpService, OtpService>();

// ----------------------------------
// Controllers & Swagger
// ----------------------------------

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

// ----------------------------------
// Rate Limiting
// ----------------------------------

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

// ----------------------------------
// HTTP Pipeline
// ----------------------------------

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