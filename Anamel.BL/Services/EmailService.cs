using Anamel.Core.Interfaces.IServices;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Anamel.BL.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOtpEmailAsync(string toEmail, string userName, string otp)
        {
            var subject = "Password Reset OTP";
            var body = GenerateOtpEmailBody(userName, otp);
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]);

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(fromEmail, fromName);
                    mailMessage.To.Add(toEmail);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = true;

                    using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
                    {
                        smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                        smtpClient.EnableSsl = enableSsl;

                        await smtpClient.SendMailAsync(mailMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                throw new InvalidOperationException("Failed to send email", ex);
            }
        }

        private string GenerateOtpEmailBody(string userName, string otp)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <style>
                        body {{
                            margin: 0;
                            padding: 0;
                            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
                            background-color: #FFF5F0;
                        }}
                        .email-wrapper {{
                            width: 100%;
                            background-color: #FFF5F0;
                            padding: 40px 20px;
                        }}
                        .email-container {{
                            max-width: 600px;
                            margin: 0 auto;
                            background-color: #FFFFFF;
                            border-radius: 20px;
                            overflow: hidden;
                            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
                        }}
                        .header {{
                            background: linear-gradient(135deg, #FF9580 0%, #FF7A61 100%);
                            padding: 40px 30px;
                            text-align: center;
                        }}
                        .header h1 {{
                            color: #FFFFFF;
                            font-size: 28px;
                            font-weight: 600;
                            margin: 0;
                            letter-spacing: -0.5px;
                        }}
                        .header p {{
                            color: #FFFFFF;
                            font-size: 14px;
                            margin: 10px 0 0 0;
                            opacity: 0.9;
                        }}
                        .content {{
                            padding: 40px 30px;
                        }}
                        .greeting {{
                            font-size: 18px;
                            color: #2C2C2C;
                            margin: 0 0 20px 0;
                            font-weight: 500;
                        }}
                        .message {{
                            font-size: 15px;
                            color: #5A5A5A;
                            line-height: 1.6;
                            margin: 0 0 30px 0;
                        }}
                        .otp-container {{
                            background: linear-gradient(135deg, #FFF5F0 0%, #FFE8DD 100%);
                            border-radius: 16px;
                            padding: 30px;
                            text-align: center;
                            margin: 30px 0;
                            border: 2px solid #FFD4C6;
                        }}
                        .otp-label {{
                            font-size: 13px;
                            color: #8A8A8A;
                            text-transform: uppercase;
                            letter-spacing: 1px;
                            margin: 0 0 15px 0;
                            font-weight: 600;
                        }}
                        .otp-code {{
                            font-size: 42px;
                            font-weight: 700;
                            color: #FF7A61;
                            letter-spacing: 8px;
                            margin: 0;
                            font-family: 'Courier New', monospace;
                        }}
                        .info-box {{
                            background-color: #F8F8F8;
                            border-left: 4px solid #FF7A61;
                            padding: 20px;
                            margin: 25px 0;
                            border-radius: 8px;
                        }}
                        .info-box p {{
                            margin: 8px 0;
                            font-size: 14px;
                            color: #5A5A5A;
                        }}
                        .info-box strong {{
                            color: #2C2C2C;
                        }}
                        .warning-box {{
                            background-color: #FFF3F3;
                            border: 1px solid #FFD4D4;
                            border-radius: 12px;
                            padding: 20px;
                            margin: 25px 0;
                        }}
                        .warning-box .warning-title {{
                            font-size: 15px;
                            color: #D32F2F;
                            font-weight: 600;
                            margin: 0 0 10px 0;
                            display: flex;
                            align-items: center;
                        }}
                        .warning-box .warning-icon {{
                            font-size: 20px;
                            margin-right: 8px;
                        }}
                        .warning-box p {{
                            margin: 0;
                            font-size: 13px;
                            color: #5A5A5A;
                            line-height: 1.5;
                        }}
                        .footer {{
                            background-color: #F8F8F8;
                            padding: 30px;
                            text-align: center;
                        }}
                        .footer p {{
                            margin: 5px 0;
                            font-size: 13px;
                            color: #8A8A8A;
                        }}
                        .footer .brand {{
                            font-size: 16px;
                            color: #FF7A61;
                            font-weight: 600;
                            margin: 10px 0;
                        }}
                        @media only screen and (max-width: 600px) {{
                            .email-wrapper {{
                                padding: 20px 10px;
                            }}
                            .header {{
                                padding: 30px 20px;
                            }}
                            .header h1 {{
                                font-size: 24px;
                            }}
                            .content {{
                                padding: 30px 20px;
                            }}
                            .otp-code {{
                                font-size: 36px;
                                letter-spacing: 6px;
                            }}
                        }}
                    </style>
                </head>
                <body>
                    <div class='email-wrapper'>
                        <div class='email-container'>
                            <!-- Header -->
                            <div class='header'>
                                <h1>🔐 Password Reset Request</h1>
                                <p>Your security is our priority</p>
                            </div>
                            
                            <!-- Content -->
                            <div class='content'>
                                <p class='greeting'>Hello {userName},</p>
                                
                                <p class='message'>
                                    We received a request to reset your password. Use the verification code below to complete the process.
                                </p>
                                
                                <!-- OTP Box -->
                                <div class='otp-container'>
                                    <p class='otp-label'>Your Verification Code</p>
                                    <p class='otp-code'>{otp}</p>
                                </div>
                                
                                <!-- Info Box -->
                                <div class='info-box'>
                                    <p><strong>⏱️ Valid for:</strong> 10 minutes</p>
                                    <p><strong>🔢 Attempts allowed:</strong> 3 tries</p>
                                </div>
                                
                                <!-- Warning Box -->
                                <div class='warning-box'>
                                    <p class='warning-title'>
                                        <span class='warning-icon'>⚠️</span>
                                        Security Notice
                                    </p>
                                    <p>
                                        If you didn't request this password reset, please ignore this email. 
                                        Your account remains secure and no changes have been made.
                                    </p>
                                </div>
                            </div>
                            
                            <!-- Footer -->
                            <div class='footer'>
                                <p class='brand'>Anamel</p>
                                <p>This is an automated message, please do not reply.</p>
                                <p style='margin-top: 15px;'>© 2024 Anamel. All rights reserved.</p>
                            </div>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }

        public async Task SendVerificationEmailAsync(string toEmail, string userName, string otp)
        {
            var subject = "Email Verification Code";
            var body = GenerateVerificationEmailBody(userName, otp);
            await SendEmailAsync(toEmail, subject, body);
        }
        private string GenerateVerificationEmailBody(string userName, string otp)
        {
            return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <style>
                body {{
                    margin: 0;
                    padding: 0;
                    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
                    background-color: #FFF5F0;
                }}
                .email-wrapper {{
                    width: 100%;
                    background-color: #FFF5F0;
                    padding: 40px 20px;
                }}
                .email-container {{
                    max-width: 600px;
                    margin: 0 auto;
                    background-color: #FFFFFF;
                    border-radius: 20px;
                    overflow: hidden;
                    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
                }}
                .header {{
                    background: linear-gradient(135deg, #FF9580 0%, #FF7A61 100%);
                    padding: 40px 30px;
                    text-align: center;
                }}
                .header h1 {{
                    color: #FFFFFF;
                    font-size: 28px;
                    font-weight: 600;
                    margin: 0;
                    letter-spacing: -0.5px;
                }}
                .header p {{
                    color: #FFFFFF;
                    font-size: 14px;
                    margin: 10px 0 0 0;
                    opacity: 0.9;
                }}
                .content {{
                    padding: 40px 30px;
                }}
                .greeting {{
                    font-size: 18px;
                    color: #2C2C2C;
                    margin: 0 0 20px 0;
                    font-weight: 500;
                }}
                .message {{
                    font-size: 15px;
                    color: #5A5A5A;
                    line-height: 1.6;
                    margin: 0 0 30px 0;
                }}
                .otp-container {{
                    background: linear-gradient(135deg, #FFF5F0 0%, #FFE8DD 100%);
                    border-radius: 16px;
                    padding: 30px;
                    text-align: center;
                    margin: 30px 0;
                    border: 2px solid #FFD4C6;
                }}
                .otp-label {{
                    font-size: 13px;
                    color: #8A8A8A;
                    text-transform: uppercase;
                    letter-spacing: 1px;
                    margin: 0 0 15px 0;
                    font-weight: 600;
                }}
                .otp-code {{
                    font-size: 42px;
                    font-weight: 700;
                    color: #FF7A61;
                    letter-spacing: 8px;
                    margin: 0;
                    font-family: 'Courier New', monospace;
                }}
                .info-box {{
                    background-color: #F8F8F8;
                    border-left: 4px solid #FF7A61;
                    padding: 20px;
                    margin: 25px 0;
                    border-radius: 8px;
                }}
                .info-box p {{
                    margin: 8px 0;
                    font-size: 14px;
                    color: #5A5A5A;
                }}
                .info-box strong {{
                    color: #2C2C2C;
                }}
                .success-box {{
                    background-color: #F0FDF4;
                    border: 1px solid #BBF7D0;
                    border-radius: 12px;
                    padding: 20px;
                    margin: 25px 0;
                }}
                .success-box .success-title {{
                    font-size: 15px;
                    color: #16A34A;
                    font-weight: 600;
                    margin: 0 0 10px 0;
                    display: flex;
                    align-items: center;
                }}
                .success-box .success-icon {{
                    font-size: 20px;
                    margin-right: 8px;
                }}
                .success-box p {{
                    margin: 0;
                    font-size: 13px;
                    color: #5A5A5A;
                    line-height: 1.5;
                }}
                .footer {{
                    background-color: #F8F8F8;
                    padding: 30px;
                    text-align: center;
                }}
                .footer p {{
                    margin: 5px 0;
                    font-size: 13px;
                    color: #8A8A8A;
                }}
                .footer .brand {{
                    font-size: 16px;
                    color: #4F46E5;
                    font-weight: 600;
                    margin: 10px 0;
                }}
                @media only screen and (max-width: 600px) {{
                    .email-wrapper {{
                        padding: 20px 10px;
                    }}
                    .header {{
                        padding: 30px 20px;
                    }}
                    .header h1 {{
                        font-size: 24px;
                    }}
                    .content {{
                        padding: 30px 20px;
                    }}
                    .otp-code {{
                        font-size: 36px;
                        letter-spacing: 6px;
                    }}
                }}
            </style>
        </head>
        <body>
            <div class='email-wrapper'>
                <div class='email-container'>
                    <!-- Header -->
                    <div class='header'>
                        <h1>✉️ Email Verification</h1>
                        <p>Welcome to Anamel!</p>
                    </div>
                    
                    <!-- Content -->
                    <div class='content'>
                        <p class='greeting'>Hello {userName},</p>
                        
                        <p class='message'>
                            Thank you for registering with Anamel! To complete your account setup and verify your email address, 
                            please use the verification code below.
                        </p>
                        
                        <!-- OTP Box -->
                        <div class='otp-container'>
                            <p class='otp-label'>Your Verification Code</p>
                            <p class='otp-code'>{otp}</p>
                        </div>
                        
                        <!-- Info Box -->
                        <div class='info-box'>
                            <p><strong>⏱️ Valid for:</strong> 10 minutes</p>
                            <p><strong>🔢 Attempts allowed:</strong> 3 tries</p>
                        </div>
                        
                        <!-- Success Box -->
                        <div class='success-box'>
                            <p class='success-title'>
                                <span class='success-icon'>🎉</span>
                                Almost There!
                            </p>
                            <p>
                                Once verified, you'll have full access to all Anamel features. 
                                If you didn't create this account, please disregard this email.
                            </p>
                        </div>
                    </div>
                    
                    <!-- Footer -->
                    <div class='footer'>
                        <p class='brand'>Anamel</p>
                        <p>This is an automated message, please do not reply.</p>
                        <p style='margin-top: 15px;'>© 2024 Anamel. All rights reserved.</p>
                    </div>
                </div>
            </div>
        </body>
        </html>
    ";
        }
    }
}


