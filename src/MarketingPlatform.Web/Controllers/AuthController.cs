using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MarketingPlatform.Application.DTOs.Auth;
using MarketingPlatform.Web.Services;

namespace MarketingPlatform.Web.Controllers;

/// <summary>
/// Controller for authentication (Login, Register, etc.)
/// </summary>
public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;
    private readonly Services.IAuthenticationService _authenticationService;

    public AuthController(
        ILogger<AuthController> logger,
        IConfiguration configuration,
        Services.IAuthenticationService authenticationService)
    {
        _logger = logger;
        _configuration = configuration;
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Login page
    /// </summary>
    [Route("login")]
    public IActionResult Login()
    {
        ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
        ViewBag.RecaptchaSiteKey = _configuration["ReCaptcha:SiteKey"];
        return View();
    }

    /// <summary>
    /// Registration page
    /// </summary>
    [Route("register")]
    public IActionResult Register()
    {
        ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
        ViewBag.RecaptchaSiteKey = _configuration["ReCaptcha:SiteKey"];
        return View();
    }

    /// <summary>
    /// Server-side login action (POST)
    /// </summary>
    [HttpPost]
    [Route("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginPost([FromForm] LoginRequestDto model, string? returnUrl = null)
    {
        try
        {
            _logger.LogInformation("Login POST received for email: {Email}", model.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state invalid for login: {Email}", model.Email);
                ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
                ViewBag.RecaptchaSiteKey = _configuration["ReCaptcha:SiteKey"];
                ViewBag.ErrorMessage = "Please fill in all required fields";
                return View("Login", model);
            }

            _logger.LogInformation("Calling AuthenticationService for email: {Email}", model.Email);
            var result = await _authenticationService.LoginAsync(model);
            _logger.LogInformation("AuthenticationService returned Success={Success}", result.Success);

            if (result.Success && result.Data != null)
            {
                _logger.LogInformation("User {Email} logged in successfully via server-side", model.Email);

                // Redirect to return URL or dashboard
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "UserDashboard");
            }
            else
            {
                ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
                ViewBag.RecaptchaSiteKey = _configuration["ReCaptcha:SiteKey"];
                ViewBag.ErrorMessage = result.Message ?? "Invalid email or password";
                return View("Login", model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", model.Email);
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            ViewBag.RecaptchaSiteKey = _configuration["ReCaptcha:SiteKey"];
            ViewBag.ErrorMessage = "An error occurred during login. Please try again.";
            return View("Login", model);
        }
    }

    /// <summary>
    /// Server-side registration action (POST)
    /// </summary>
    [HttpPost]
    [Route("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterPost([FromForm] RegisterRequestDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
                ViewBag.RecaptchaSiteKey = _configuration["ReCaptcha:SiteKey"];
                ViewBag.ErrorMessage = "Please fill in all required fields";
                return View("Register", model);
            }

            var result = await _authenticationService.RegisterAsync(model);

            if (result.Success && result.Data != null)
            {
                _logger.LogInformation("User {Email} registered successfully via server-side", model.Email);
                return RedirectToAction("Index", "UserDashboard");
            }
            else
            {
                ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
                ViewBag.RecaptchaSiteKey = _configuration["ReCaptcha:SiteKey"];
                ViewBag.ErrorMessage = result.Message ?? "Registration failed";
                return View("Register", model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", model.Email);
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            ViewBag.RecaptchaSiteKey = _configuration["ReCaptcha:SiteKey"];
            ViewBag.ErrorMessage = "An error occurred during registration. Please try again.";
            return View("Register", model);
        }
    }

    /// <summary>
    /// Logout action
    /// </summary>
    [Authorize]
    [Route("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await _authenticationService.LogoutAsync(userId);
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation("User {UserId} logged out successfully", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }

        return RedirectToAction("Login");
    }

    /// <summary>
    /// Access denied page
    /// </summary>
    [Route("access-denied")]
    public IActionResult AccessDenied()
    {
        return View();
    }

    /// <summary>
    /// Forgot password page
    /// </summary>
    [Route("forgot-password")]
    public IActionResult ForgotPassword()
    {
        ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
        ViewBag.RecaptchaSiteKey = _configuration["ReCaptcha:SiteKey"];
        return View();
    }

    /// <summary>
    /// Get ReCaptcha configuration for dynamic loading
    /// </summary>
    /// <returns>ReCaptcha configuration JSON</returns>
    [HttpGet("recaptcha-config")]
    public IActionResult GetRecaptchaConfig()
    {
        return Json(new
        {
            siteKey = _configuration["ReCaptcha:SiteKey"],
            version = _configuration["ReCaptcha:Version"]
        });
    }
}
