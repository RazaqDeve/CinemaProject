namespace IdentityService.Service.OtpRedis
{
    public static class OtpRedisScripts
    {
        // Save OTP and check request limits
        public const string SaveOtp = """
        if redis.call('EXISTS', KEYS[3]) == 1 then
            return 1
        end

        local requests = tonumber(
            redis.call('GET', KEYS[2]) or '0'
        )

        if requests >= tonumber(ARGV[2]) then
            return 2
        end

        local newCount = redis.call('INCR', KEYS[2])

        if newCount == 1 then
            redis.call('EXPIRE', KEYS[2], tonumber(ARGV[3]))
        end

        redis.call(
            'SET',
            KEYS[1],
            ARGV[1],
            'EX',
            tonumber(ARGV[4])
        )

        redis.call(
            'SET',
            KEYS[4],
            0,
            'EX',
            tonumber(ARGV[4])
        )

        redis.call(
            'SET',
            KEYS[3],
            1,
            'EX',
            tonumber(ARGV[5])
        )

        return 0
        """;


        // Consume OTP if valid
        public const string ConsumeOtp = """
        local current = redis.call('GET', KEYS[1])

        if not current or current ~= ARGV[1] then
            return 0
        end

        redis.call('DEL', KEYS[1], KEYS[2])

        return 1
        """;


        // Increment failed attempts
        public const string FailedAttempt = """
        local current = redis.call('GET', KEYS[1])

        if not current or current ~= ARGV[1] then
            return 0
        end

        local attempts = redis.call('INCR', KEYS[2])

        if attempts == 1 then
            redis.call('EXPIRE', KEYS[2], tonumber(ARGV[3]))
        end

        if attempts >= tonumber(ARGV[2]) then
            redis.call('DEL', KEYS[1], KEYS[2])
            return 2
        end

        return 1
        """;
    }

}
