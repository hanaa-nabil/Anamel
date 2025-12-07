using Anamel.Core.DTOs.Auth;
using Anamel.Core.IRepositories.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Anamel.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user - sends verification code to email
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var message = await _authService.RegisterAsync(registerDto);
                return Ok(new { message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Registration failed for {Email}", registerDto.Email);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration");
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }

        /// <summary>
        /// Verify email with OTP after registration
        /// </summary>
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyOtpDto verifyDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Email verification requested for {Email}", verifyDto.Email);
                var result = await _authService.VerifyEmailAsync(verifyDto.Email, verifyDto.Otp);
                return Ok(new { message = "Email verified successfully. You can now login.", isVerified = result });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Email verification failed for {Email}", verifyDto.Email);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during email verification for {Email}", verifyDto.Email);
                return StatusCode(500, new { message = "An error occurred during email verification" });
            }
        }

        /// <summary>
        /// Resend verification code
        /// </summary>
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ForgotPasswordDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _authService.ResendVerificationCodeAsync(request.Email);
                return Ok(new { message = "Verification code has been resent to your email" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Resend verification failed for {Email}", request.Email);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during resend verification");
                return StatusCode(500, new { message = "An error occurred while resending verification code" });
            }
        }

        /// <summary>
        /// Login user - requires verified email
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.LoginAsync(loginDto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Login failed for {Email}", loginDto.Email);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login");
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        /// <summary>
        /// Logout user
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _authService.LogoutAsync();
                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { message = "An error occurred during logout" });
            }
        }

        /// <summary>
        /// Refresh JWT token
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] string token)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(token);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Token refresh failed");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token refresh");
                return StatusCode(500, new { message = "An error occurred during token refresh" });
            }
        }

        /// <summary>
        /// Request password reset - sends OTP to email
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Password reset requested for {Email}", forgotPasswordDto.Email);
                await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);
                return Ok(new { message = "If the email exists, a verification code has been sent to your email address" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to process forgot password for {Email}", forgotPasswordDto.Email);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during forgot password for {Email}", forgotPasswordDto.Email);
                return StatusCode(500, new { message = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Verify OTP for password reset
        /// </summary>
        //[HttpPost("verify-reset-otp")]
        //public async Task<IActionResult> VerifyResetOtp([FromBody] VerifyOtpDto verifyOtpDto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    try
        //    {
        //        _logger.LogInformation("Password reset OTP verification requested for {Email}", verifyOtpDto.Email);
        //        var result = await _authService.VerifyOtpAsync(verifyOtpDto.Email, verifyOtpDto.Otp);
        //        return Ok(new { message = "Verification code verified successfully", isVerified = result });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        _logger.LogWarning(ex, "OTP verification failed for {Email}", verifyOtpDto.Email);
        //        return BadRequest(new { message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Unexpected error during OTP verification for {Email}", verifyOtpDto.Email);
        //        return StatusCode(500, new { message = "An error occurred during verification" });
        //    }
        //}

        /// <summary>
        /// Reset password with verified OTP
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Password reset requested for {Email}", resetPasswordDto.Email);
                var result = await _authService.ResetPasswordAsync(
                    resetPasswordDto.Email,
                    resetPasswordDto.Otp,
                    resetPasswordDto.NewPassword);

                return Ok(new { message = "Password reset successfully", success = result });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Password reset failed for {Email}", resetPasswordDto.Email);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during password reset for {Email}", resetPasswordDto.Email);
                return StatusCode(500, new { message = "An error occurred during password reset" });
            }
        }
    }
}