

namespace IdentityService.Service;

public interface IOtpService
{
    Task<string> GenerateOtpAsync(CancellationToken cancellationToken = default);

    Task<OtpSaveStatus> SaveOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);

    Task<OtpVerificationStatus> VerifyOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
}

  //Task GenerateOtpAsync(string phoneNumber, CancellationToken cancellationToken = default);
  