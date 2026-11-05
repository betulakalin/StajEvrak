using StajEvrak.Core.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using StajEvrak.Infrastructure.Services;
using StajEvrak.web.Models;

namespace StajEvrak.web.Controllers;

public class AccountController : Controller
{
    private readonly AuthService _auth;

    public AccountController(AuthService auth) => _auth = auth;

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _auth.LoginAsync(model.UsernameOrEmail, model.Password, ip);

        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error!);
            return View(model);
        }

        // Rol bilgisi cookie'nin içine yazılıyor
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.User!.Id.ToString()),
            new(ClaimTypes.Name, result.User.Username),
            new(ClaimTypes.Role, result.User.Role.ToString())
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return result.User.Role == UserRole.Admin
    ? RedirectToAction("Index", "Admin")
    : RedirectToAction("Index", "Student");
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _auth.RegisterAsync(model.Username, model.Email, model.Password, model.StudentNo);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error!);
            return View(model);
        }

        TempData["Message"] = "Kayıt başarılı, giriş yapabilirsiniz.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
    [HttpGet]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = 403;
        return View();
    }
}