using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AgriVision.Application.DTOs.Auth;
using AgriVision.Application.Interfaces;
using AgriVision.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgriVision.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService jwtTokenService,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        private string GetFrontendBaseUrl()
        {
            var baseUrl = _configuration["Frontend:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new Exception("Frontend:BaseUrl is not configured.");
            return baseUrl.TrimEnd('/');
        }

        // POST: /api/v1/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest("Passwords do not match");

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                CreatedAt = DateTime.UtcNow,
                SubscriptionPlan = 0
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Confirm Email URL (API endpoint)
            var verifyUrl = $"{Request.Scheme}://{Request.Host}/api/v1/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            await _emailSender.SendEmailAsync(
                user.Email!,
                "Confirm your email",
                $@"
                <div style='font-family: Arial, sans-serif; background-color:#f4f7f6; padding:30px;'>
                  <div style='max-width:600px; margin:auto; background:#ffffff; border-radius:10px; box-shadow:0 4px 12px rgba(0,0,0,0.1); padding:30px;'>

                    <h2 style='color:#2e7d32; text-align:center; margin-bottom:20px;'>
                      🌱 Confirm Your Email Address
                    </h2>

                    <p style='color:#333; font-size:15px; line-height:1.6;'>
                      Hello,
                    </p>

                    <p style='color:#333; font-size:15px; line-height:1.6;'>
                      Thank you for signing up for <strong>AgriVision</strong>!  
                      To complete your registration and activate your account, please confirm your email address by clicking the button below.
                    </p>

                    <div style='text-align:center; margin:30px 0;'>
                      <a href='{verifyUrl}'
                         style='background:linear-gradient(135deg,#2e7d32,#66bb6a);
                                color:#ffffff;
                                padding:14px 28px;
                                text-decoration:none;
                                font-weight:bold;
                                border-radius:8px;
                                display:inline-block;'>
                        Confirm Email
                      </a>
                    </div>

                    <p style='color:#555; font-size:14px; line-height:1.6;'>
                      If you did not create an account, you can safely ignore this email.
                    </p>

                    <p style='color:#555; font-size:14px; line-height:1.6;'>
                      This link will expire for security reasons.
                    </p>

                    <hr style='border:none; border-top:1px solid #eee; margin:30px 0;' />

                    <p style='color:#999; font-size:12px; text-align:center;'>
                      © {DateTime.Now.Year} AgriVision. All rights reserved.
                    </p>

                  </div>
                </div>
                "
            );

            return Ok(new { message = "Registered successfully", email = user.Email });
        }

        // POST: /api/v1/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized("Invalid email or password");

            var check = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!check.Succeeded)
                return Unauthorized("Invalid email or password");

            if (!user.EmailConfirmed)
                return Unauthorized("Email not confirmed");

            user.LastLoginAtUtc = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var token = _jwtTokenService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            });
        }

        // POST: /api/v1/auth/logout  
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // With JWT, logout is usually handled client-side by removing the token.
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out" });
        }

        // GET: /api/v1/auth/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("UserId not found in token.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized("User not found.");

            return Ok(new
            {
                user.Id,
                user.Email,
                user.UserName,
                user.FullName,
                user.SubscriptionPlan,
                user.CreatedAt
            });
        }

        // GET: /api/v1/auth/confirm-email
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return BadRequest("Invalid user");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded) return BadRequest("Email confirmation failed");

            var frontendBaseUrl = GetFrontendBaseUrl();
            return Redirect($"{frontendBaseUrl}/Authentication/emailVerifiedSuccessfully.html");
        }

        // POST: /api/v1/auth/resend-email-verification
        [HttpPost("resend-email-verification")]
        public async Task<IActionResult> ResendEmailVerification([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return BadRequest("User not found");

            if (user.EmailConfirmed)
                return BadRequest("Email already confirmed");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var verifyUrl = $"{Request.Scheme}://{Request.Host}/api/v1/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            await _emailSender.SendEmailAsync(
                user.Email!,
                "Confirm your email",
                $@"
                <div style='font-family: Arial, sans-serif; background-color:#f4f7f6; padding:30px;'>
                  <div style='max-width:600px; margin:auto; background:#ffffff; border-radius:10px; box-shadow:0 4px 12px rgba(0,0,0,0.1); padding:30px;'>

                    <h2 style='color:#2e7d32; text-align:center; margin-bottom:20px;'>
                      🌱 Confirm Your Email Address
                    </h2>

                    <p style='color:#333; font-size:15px; line-height:1.6;'>
                      Hello,
                    </p>

                    <p style='color:#333; font-size:15px; line-height:1.6;'>
                      Thank you for signing up for <strong>AgriVision</strong>!  
                      To complete your registration and activate your account, please confirm your email address by clicking the button below.
                    </p>

                    <div style='text-align:center; margin:30px 0;'>
                      <a href='{verifyUrl}'
                         style='background:linear-gradient(135deg,#2e7d32,#66bb6a);
                                color:#ffffff;
                                padding:14px 28px;
                                text-decoration:none;
                                font-weight:bold;
                                border-radius:8px;
                                display:inline-block;'>
                        Confirm Email
                      </a>
                    </div>

                    <p style='color:#555; font-size:14px; line-height:1.6;'>
                      If you did not create an account, you can safely ignore this email.
                    </p>

                    <p style='color:#555; font-size:14px; line-height:1.6;'>
                      This link will expire for security reasons.
                    </p>

                    <hr style='border:none; border-top:1px solid #eee; margin:30px 0;' />

                    <p style='color:#999; font-size:12px; text-align:center;'>
                      © {DateTime.Now.Year} AgriVision. All rights reserved.
                    </p>

                  </div>
                </div>
                "
            );

            return Ok(new { message = "Verification email resent" });
        }

        // POST: /api/v1/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Ok("If email exists, reset link sent");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetUrl = $"{Request.Scheme}://{Request.Host}/api/v1/auth/reset-password-confirm?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

            await _emailSender.SendEmailAsync(
                user.Email!,
                "Reset Password",
                $"<h2>Reset your password</h2><p>Click the link below:</p><a href='{resetUrl}'>Reset Password</a>"
            );

            return Ok("If email exists, reset link sent");
        }

        // GET: /api/v1/auth/reset-password-confirm
        [HttpGet("reset-password-confirm")]
        public async Task<IActionResult> ResetPasswordConfirm(string email, string token)
        {
            var frontendBaseUrl = GetFrontendBaseUrl();
            var url =
                $"{frontendBaseUrl}/Authentication/resetPassword.html?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

            return Redirect(url);
        }

        // POST: /api/v1/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest("Passwords do not match");

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return BadRequest("Invalid request");

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded) return BadRequest(result.Errors);

            var frontendBaseUrl = GetFrontendBaseUrl();
            return Redirect($"{frontendBaseUrl}/Authentication/passwordResetSuccessfully.html");
        }

        // GET: /api/v1/auth/google-login
        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action(nameof(GoogleCallback), "Auth", null, Request.Scheme);
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl!);
            return Challenge(properties, "Google");
        }

        // GET: /api/v1/auth/google-callback
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return BadRequest("Error loading external login information");

            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false);

            ApplicationUser user;

            if (signInResult.Succeeded)
            {
                user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var fullName = info.Principal.FindFirstValue(ClaimTypes.Name);

                user = await _userManager.FindByEmailAsync(email!);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = fullName,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                        SubscriptionPlan = 0
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                        return BadRequest(createResult.Errors);
                }

                await _userManager.AddLoginAsync(user, info);
            }

            var token = _jwtTokenService.GenerateToken(user);

            var frontendBaseUrl = GetFrontendBaseUrl();
            return Redirect($"{frontendBaseUrl}/Authentication/Login.html?token={token}");

        }
    }
}
