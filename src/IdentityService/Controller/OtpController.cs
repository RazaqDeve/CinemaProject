using Microsoft.AspNetCore.RateLimiting;
using IdentityService.DTOs;
using IdentityService.Service;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OtpController : ControllerBase
{
    private readonly IOtpService _otpService;
    private readonly IWebHostEnvironment _environment;

    public OtpController(IOtpService otpService, IWebHostEnvironment environment)
    {
        _otpService = otpService;
        _environment = environment;
    }

    [HttpPost("request")]
    [EnableRateLimiting("otp-request-by-ip")]
    public async Task<IActionResult> RequestOtp(RequestOtpRequest request, CancellationToken token)
    {
        string code = await _otpService.GenerateOtpAsync(token);

        OtpSaveStatus result = await _otpService.SaveOtpAsync(
            request.PhoneNumber,
            code,
            token);

        switch (result)
        {
            case OtpSaveStatus.Saved:

                // نمایش کد فقط برای آزمایش در Development
                if (_environment.IsDevelopment())
                {
                    return Ok(new
                    {
                        message = "OTP generated successfully.",
                        phoneNumber = request.PhoneNumber,
                        code = code
                    });
                }

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
