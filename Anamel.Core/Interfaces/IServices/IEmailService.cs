using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anamel.Core.Interfaces.IServices
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string userName, string otp);
        Task SendEmailAsync(string toEmail, string subject, string body);

        Task SendVerificationEmailAsync(string toEmail, string userName, string otp);
    }
}
