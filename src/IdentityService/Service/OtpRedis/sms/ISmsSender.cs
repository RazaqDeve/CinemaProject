using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Service.OtpRedis.sms
{
    public interface ISmsSender
    {
         Task SendOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
    }
}
