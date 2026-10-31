

namespace IdentityService.Service;

public interface IOtpService
{
    Task<string> GenerateOtpAsync(CancellationToken cancellationToken = default);

    Task SaveOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);

    Task<bool> VerifyOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
}

  //Task GenerateOtpAsync(string phoneNumber, CancellationToken cancellationToken = default);
  