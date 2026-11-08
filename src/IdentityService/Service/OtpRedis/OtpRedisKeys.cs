using StackExchange.Redis;

namespace IdentityService.Service.OtpRedis;

public sealed record OtpRedisKeys(
    string PhoneHash,
    RedisKey Otp,
    RedisKey Attempts,
    RedisKey Requests,
    RedisKey Cooldown)
{
    public static OtpRedisKeys CreateFromPhoneHash(string phoneHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneHash);

        string prefix = $"IdentityService:{{{phoneHash}}}";

        return new OtpRedisKeys(
            phoneHash,
            $"{prefix}:otp",
            $"{prefix}:attempts",
            $"{prefix}:requests",
            $"{prefix}:cooldown");
    }
}
