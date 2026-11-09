using Microsoft.AspNetCore.RateLimiting;
using IdentityService.DTOs;
using IdentityService.Service;
using Microsoft.AspNetCore.Mvc;
using IdentityService.Service.OtpRedis.SmsOtp;

namespace IdentityService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OtpController : ControllerBase
{
    private readonly IOtpService _otpService;
    private readonly ISmsSender _smsSender;
    private readonly IWebHostEnvironment _environment;

    public OtpController(IOtpService otpService,ISmsSender smsSender, IWebHostEnvironment environment)
    {
        _otpService = otpService;
        _environment = environment;
        _smsSender = smsSender;
    }

[HttpPost("request")]
[EnableRateLimiting("otp-request-by-ip")]
public async Task<IActionResult> RequestOtp(RequestOtpRequest request, CancellationToken token)
{
    // 1. Generate OTP
    string code = await _otpService.GenerateOtpAsync(token);

    // 2. Save OTP in Redis
    OtpSaveStatus result = await _otpService.SaveOtpAsync(
        request.PhoneNumber,
        code,
        token);

    switch (result)
    {
        case OtpSaveStatus.Saved:

            // 3. Send OTP through SMS.ir
            try
            {
                await _smsSender.SendOtpAsync(
                    request.PhoneNumber,
                    code,
                    token);
            }
            catch (HttpRequestException)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new
                    {
                        message = "Unable to send SMS. Please try again later."
                    });
            }

            // 4. Development response
            if (_environment.IsDevelopment())
            {
                return Ok(new
                {
                    message = "OTP generated successfully.",
                    phoneNumber = request.PhoneNumber,
                    code = code
                });
            }

            // 5. Production response
            return Accepted(new
            {
                message = "OTP request accepted."
            });

        case OtpSaveStatus.CooldownActive:

            return StatusCode(
                StatusCodes.Status429TooManyRequests,
                new
                {
                    message = "Please wait before requesting another OTP."
                });

        case OtpSaveStatus.RequestLimitReached:

            return StatusCode(
                StatusCodes.Status429TooManyRequests,
                new
                {
                    message = "You have reached the OTP request limit."
                });

        default:

            return StatusCode(
                StatusCodes.Status500InternalServerError);
    }
}

    [HttpPost("verify")]
    [EnableRateLimiting("otp-verify-by-ip")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpRequest request, CancellationToken token)
    {
        OtpVerificationStatus result = await _otpService.VerifyOtpAsync(request.PhoneNumber, request.Code, token);

        switch (result)
        {
            case OtpVerificationStatus.Verified:

                return Ok(new
                {
                    message = "OTP verified successfully."
                });

            case OtpVerificationStatus.InvalidCode:

                return BadRequest(new
                {
                    message = "Invalid OTP code."
                });

            case OtpVerificationStatus.ExpiredOrNotFound:

                return BadRequest(new
                {
                    message = "OTP has expired or does not exist."
                });

            case OtpVerificationStatus.AttemptsExceeded:

                return StatusCode(StatusCodes.Status429TooManyRequests,
                    new
                    {
                        message = "Maximum OTP verification attempts exceeded."
                    });

            default:

                return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
