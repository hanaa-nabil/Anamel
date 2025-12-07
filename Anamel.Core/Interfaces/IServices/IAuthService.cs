using Anamel.Core.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anamel.Core.IRepositories.Services
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<bool> LogoutAsync();
        Task<AuthResponseDto> RefreshTokenAsync(string token);

        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> VerifyOtpAsync(string email, string otp);
        Task<bool> ResetPasswordAsync(string email, string otp, string newPassword);


        // Email verification
        Task<bool> VerifyEmailAsync(string email, string otp);
        Task<bool> ResendVerificationCodeAsync(string email);



    }
}
