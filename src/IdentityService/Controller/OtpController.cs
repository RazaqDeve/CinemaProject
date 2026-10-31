using IdentityService.DTOs;
using IdentityService.Service;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService
{
    [Route("api/[controller]")]
    [ApiController]
    public class OtpController : ControllerBase
    {
        private readonly IOtpService _otpService;

        public OtpController(IOtpService otpService) => _otpService = otpService;

        [HttpPost("request")]
        public async Task<IActionResult> RequestOtp(RequestOtpRequest request, CancellationToken token)
        {
            string code = await _otpService.GenerateOtpAsync(token);
            await _otpService.SaveOtpAsync(request.PhoneNumber, code, token);

            return Ok(new
            {
                message = "OTP generated successfully.",
                phoneNumber = request.PhoneNumber,
                code = code
            });
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpRequest request, CancellationToken token)
        {
            bool isValid = await _otpService.VerifyOtpAsync(request.PhoneNumber, request.Code, token);

            if (!isValid)
            {
                return BadRequest(new
                {
                    message = "Invalid or expired OTP."
                });
            }

            return Ok(new
            {
                message = "OTP verified successfully."
            });
        }
    }
}
