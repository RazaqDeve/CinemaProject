
namespace IdentityService.Service
{
    public enum OtpVerificationStatus
    {
        Verified,
        InvalidCode,
        ExpiredOrNotFound,
        AttemptsExceeded
    }
}