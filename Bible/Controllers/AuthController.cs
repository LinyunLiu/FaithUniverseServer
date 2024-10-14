using System.Security.Claims;
using Bible.Data;
using Bible.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bible.Controllers;

public class AuthController: Controller
{
    private readonly PasswordHasher<IdentityUser> _hasher;
    private readonly ApplicationDbContext _db;
    public AuthController(ApplicationDbContext db)
    {
        _hasher = new PasswordHasher<IdentityUser>();
        _db = db;
    }
    
    [HttpGet("/signin")]
    public IActionResult SignIn()
    {
        return View();
    }
    
    [HttpGet("/signout")]
    public new async Task<IActionResult> SignOut()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Home", "Introduction");
    }
    
    [HttpPost("/authenticate")]
    public async Task<IActionResult> Authenticate(Account account)
    {
        Account? user = await _db.Account.FirstOrDefaultAsync(u => u.Username == account.Username.Trim());
        if (user != null)
        {
            var result = _hasher.VerifyHashedPassword(new IdentityUser(), user.Password, account.Password.Trim());
            if (result == PasswordVerificationResult.Success)
            {
                ViewBag.Message = "";
                var claims = new List<Claim>
                {
                    new (ClaimTypes.Name, user.Username)
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false,  // Non-persistent session (cookie-based)
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                };
                // Sign in the user and create the cookie
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
                
                return RedirectToAction("UserHome", "UserHome");
            }
            ViewBag.Message = "Incorrect Password!";
            return View("SignIn", account);
        }
        ViewBag.Message = "User Not Found!";
        return View("SignIn", account);
    }
}