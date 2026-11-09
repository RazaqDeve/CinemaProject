using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Service;
using IdentityService.Service.OtpRedis;
using IdentityService.Service.OtpRedis.SmsOtp;
using IdentityService.Service.Sms;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
// SMS.ir Options
// ----------------------------------

builder.Services.AddOptions<SmsIrOptions>()
    .Bind(builder.Configuration.GetSection("SmsIr"))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ApiKey),
        "SMS.ir API Key is required.")
    .Validate(
        options => options.TemplateId > 0,
        "SMS.ir TemplateId must be greater than zero.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ParameterName),
        "SMS.ir ParameterName is required.")
    .ValidateOnStart();

// ----------------------------------
// SMS.ir HttpClient
// ----------------------------------

builder.Services.AddHttpClient<ISmsSender, SmsIrSender>(
    (serviceProvider, client) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<SmsIrOptions>>()
            .Value;

        client.BaseAddress = new Uri("https://api.sms.ir/");

        client.Timeout = TimeSpan.FromSeconds(15);

        client.DefaultRequestHeaders.TryAddWithoutValidation(
            "X-API-KEY",
            options.ApiKey);

        client.DefaultRequestHeaders.Accept.ParseAdd(
            "application/json");
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