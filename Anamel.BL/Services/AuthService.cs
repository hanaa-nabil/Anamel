using Anamel.Core.DTOs.Auth;
using Anamel.Core.Entities;
using Anamel.Core.Interfaces.IServices;
using Anamel.Core.IRepositories.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Anamel.BL.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailService = emailService;
        }


        public async Task<string> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                // If user exists but not verified, resend OTP
                if (!existingUser.EmailConfirmed)
                {
                    await SendVerificationOtp(existingUser);
                    return "A verification code has been resent to your email";
                }
                throw new InvalidOperationException("Email already registered");
            }

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                EmailConfirmed = false // Set to false initially
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

            // Assign default Customer role
            await _userManager.AddToRoleAsync(user, "Customer");

            // Send verification OTP
            await SendVerificationOtp(user);

            return "Registration successful. Please check your email for verification code.";
        }

        public async Task<bool> VerifyEmailAsync(string email, string otp)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new InvalidOperationException("User not found");

            if (user.EmailConfirmed)
                throw new InvalidOperationException("Email already verified");

            if (string.IsNullOrEmpty(user.Otp))
                throw new InvalidOperationException("No verification code found. Please request a new one.");

            // Check if OTP is expired
            if (user.OtpExpiryTime == null || DateTime.UtcNow > user.OtpExpiryTime)
            {
                await ClearOtpData(user);
                throw new InvalidOperationException("Verification code has expired");
            }

            // Check attempts limit (max 3 attempts)
            if (user.OtpAttempts >= 3)
            {
                await ClearOtpData(user);
                throw new InvalidOperationException("Maximum verification attempts exceeded. Please request a new code.");
            }

            // Verify OTP
            if (user.Otp != otp)
            {
                user.OtpAttempts++;
                await _userManager.UpdateAsync(user);
                throw new InvalidOperationException($"Invalid verification code. {3 - user.OtpAttempts} attempts remaining.");
            }

            // Mark email as confirmed
            user.EmailConfirmed = true;
            user.IsOtpVerified = true;
            await _userManager.UpdateAsync(user);

            // Clear OTP data
            await ClearOtpData(user);

            return true;
        }

        public async Task<bool> ResendVerificationCodeAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new InvalidOperationException("User not found");

            if (user.EmailConfirmed)
                throw new InvalidOperationException("Email already verified");

            await SendVerificationOtp(user);
            return true;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials");

            // Check if email is verified
            if (!user.EmailConfirmed)
                throw new UnauthorizedAccessException("Please verify your email before logging in");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                throw new UnauthorizedAccessException("Invalid credentials");

            return await GenerateAuthResponse(user);
        }

        public async Task<bool> LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return true;
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var userId = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid token");

            return await GenerateAuthResponse(user);
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                // Don't reveal that the user doesn't exist for security reasons
                return true;

            // Generate 6-digit OTP
            var otp = GenerateOtp();
            var otpExpiry = DateTime.UtcNow.AddMinutes(10);

            // Store OTP in database
            user.Otp = otp;
            user.OtpExpiryTime = otpExpiry;
            user.OtpAttempts = 0;
            user.IsOtpVerified = false;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException("Failed to generate verification code");

            // Send OTP via email
            await _emailService.SendOtpEmailAsync(user.Email, user.FirstName, otp);

            return true;
        }

        public async Task<bool> VerifyOtpAsync(string email, string otp)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new InvalidOperationException("User not found");

            if (string.IsNullOrEmpty(user.Otp))
                throw new InvalidOperationException("No verification code found for this email");

            // Check if OTP is expired
            if (user.OtpExpiryTime == null || DateTime.UtcNow > user.OtpExpiryTime)
            {
                await ClearOtpData(user);
                throw new InvalidOperationException("Verification code has expired");
            }

            // Check attempts limit (max 3 attempts)
            if (user.OtpAttempts >= 3)
            {
                await ClearOtpData(user);
                throw new InvalidOperationException("Maximum verification attempts exceeded");
            }

            // Verify OTP
            if (user.Otp != otp)
            {
                user.OtpAttempts++;
                await _userManager.UpdateAsync(user);
                throw new InvalidOperationException($"Invalid verification code. {3 - user.OtpAttempts} attempts remaining.");
            }

            // Mark as verified
            user.IsOtpVerified = true;
            await _userManager.UpdateAsync(user);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string otp, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new InvalidOperationException("User not found");

            // Verify OTP is present and verified
            if (string.IsNullOrEmpty(user.Otp) || !user.IsOtpVerified || user.Otp != otp)
                throw new InvalidOperationException("Verification code must be verified first");

            // Check if OTP is expired
            if (user.OtpExpiryTime == null || DateTime.UtcNow > user.OtpExpiryTime)
            {
                await ClearOtpData(user);
                throw new InvalidOperationException("Verification code has expired");
            }

            // Remove old password and set new one
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

            // Clear OTP data after successful password reset
            await ClearOtpData(user);

            return true;
        }

        private async Task SendVerificationOtp(ApplicationUser user)
        {
            // Generate 6-digit OTP
            var otp = GenerateOtp();
            var otpExpiry = DateTime.UtcNow.AddMinutes(10);

            // Store OTP in database
            user.Otp = otp;
            user.OtpExpiryTime = otpExpiry;
            user.OtpAttempts = 0;
            user.IsOtpVerified = false;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException("Failed to generate verification code");

            // Send OTP via email
            await _emailService.SendVerificationEmailAsync(user.Email, user.FirstName, otp);
        }

        private async Task ClearOtpData(ApplicationUser user)
        {
            user.Otp = null;
            user.OtpExpiryTime = null;
            user.OtpAttempts = 0;
            user.IsOtpVerified = false;
            await _userManager.UpdateAsync(user);
        }

        private async Task<AuthResponseDto> GenerateAuthResponse(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var token = await GenerateJwtToken(user, roles);

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = token,
                Roles = roles.ToList()
            };
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateOtp()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                var randomNumber = BitConverter.ToUInt32(bytes, 0);
                return (randomNumber % 10000).ToString("D6");
            }
        }

    }
}