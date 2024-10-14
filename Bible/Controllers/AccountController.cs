using System.Security.Claims;
using Bible.Data;
using Bible.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bible.Controllers;

public class AccountController : Controller
{

    private readonly string? _pk = Environment.GetEnvironmentVariable("PERMISSION_KEY"); // A key needed for creating an account
    private readonly PasswordHasher<IdentityUser> _hasher;
    private readonly ApplicationDbContext _db;
    private readonly string? _origin = Environment.GetEnvironmentVariable("ORIGIN");
    public AccountController(ApplicationDbContext db)
    {
        _hasher = new PasswordHasher<IdentityUser>();
        _db = db;
    }
    
    [HttpGet("/create")]
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    [Route("/create-account")]
    public async Task<IActionResult> CreateAccount(Account account)
    {
        if (account.PermissionKey == _pk)
        {
            Account? user = await _db.Account.FirstOrDefaultAsync(u => u.Username == account.Username.Trim());
            if (user != null)
            {
                ViewBag.Message = "Username Already Exist, Please Try Another One";
                return View("Create", account);
            }
            ViewBag.Message = "";
            account.Password = _hasher.HashPassword(new IdentityUser(), account.Password.Trim());
            account.Username = account.Username.Trim();
            account.Email = account.Email.Trim();
            _db.Account.Add(account);
            User newUser = new User
            {
                Username = account.Username.Trim(),
                Collection = ""
            };
            _db.User.Add(newUser);     
            await _db.SaveChangesAsync();
            return RedirectToAction("SignIn", "Auth");
        }
        ViewBag.Message = "You Don't Have a Valid Permission Key";
        return View("Create", account);
    }
    
    
    public class AccountMetaDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
    }
    [HttpGet("/account-overview")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public IActionResult Overview(){
        var username = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        var account = _db.Account.FirstOrDefault(u => u.Username == username);
        if (account != null)
        {
            AccountMetaDto accountMetaDto = new AccountMetaDto
            {
                Username = account.Username.Trim(),
                Email = account.Email.Trim(),
                Origin = _origin ?? string.Empty
            };
            return View(accountMetaDto);
        }
        return Problem();
    }
    
    [HttpDelete("/delete-account")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task <IActionResult> DeleteAccount([FromBody] AccountMetaDto accountMetaDto){
        var usernameFromClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        var usernameFromDto = accountMetaDto.Username.Trim();
        if (usernameFromClaim.Trim() != usernameFromDto)
        {
            return Problem();
        }
        var account = await _db.Account.FirstOrDefaultAsync(u => u.Username == usernameFromClaim); // Account
        var user = await _db.User.FirstOrDefaultAsync(u => u.Username == usernameFromClaim); // User
        var note = await _db.Note.Where(n => n.Username == usernameFromClaim).ToListAsync();
        try
        {
            if (account != null && user != null)
            {
                _db.Account.Remove(account);
                _db.User.Remove(user);
                if (note.Count != 0)
                {
                    _db.Note.RemoveRange(note);
                }
                await _db.SaveChangesAsync();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // Important!
                return Ok();
            }
        }catch(Exception){
            return Problem();
        }  
        return Problem();
    }
}