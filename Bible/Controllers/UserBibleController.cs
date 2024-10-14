using System.Security.Claims;
using Bible.Data;
using Bible.Dto;
using Bible.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bible.Controllers;

public class UserBibleController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly string? _origin = Environment.GetEnvironmentVariable("ORIGIN");
    public UserBibleController(ApplicationDbContext db)
    {
        _db = db;
    }
    
    [HttpGet("user-bible")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public IActionResult UserBible()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        BibleDto bibleDto = UserBibleData();
        bibleDto.Owner = username;
        bibleDto.Origin = _origin ?? string.Empty;
        return View(bibleDto);
    }
    
    [HttpGet("user-bible/chapter")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UserBibleChapter()
    {
        var bookName = HttpContext.Request.Query["bookName"];
        var chapterNumber = HttpContext.Request.Query["chapterNumber"];
        var verses = await _db.Kjv.Where(
            k => k.BookName == bookName.ToString().Trim() && k.Chapter == int.Parse(chapterNumber.ToString())).Select(k => k.Text).ToListAsync(); 
        return Ok(verses);
    }

    [HttpPost("user-bible/add")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UserBibleAdd()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value ?? HttpContext.Request.Query["username"];
        var bookName = HttpContext.Request.Query["bookName"];
        var chapterNumber = HttpContext.Request.Query["chapterNumber"];
        var verseNumber = HttpContext.Request.Query["verseNumber"];
        var verse = await _db.Kjv.FirstOrDefaultAsync(
            k => k.BookName == bookName.ToString().Trim() && k.Chapter == int.Parse(chapterNumber.ToString()) &&
                 k.Verse == int.Parse(verseNumber.ToString()));
        if (verse != null)
        {
            var verseId = verse.Id;
            User? user = await _db.User.FindAsync(username);
            if (user != null)
            {
                string collectionString = user.Collection;
                HashSet<string> collectionSet = collectionString
                    .Split(',')
                    .Select(item => item.Trim()) // Trim each item to remove any extra whitespace
                    .ToHashSet();
                collectionSet.Add(verseId.ToString());
                collectionString = string.Join(",", collectionSet);
                user.Collection = collectionString;
                await _db.SaveChangesAsync();
                return Ok();
            }
            return Problem();
        }
        return Problem(); 
    }
     
    public BibleDto UserBibleData()
    {
        BibleDto bible = new BibleDto();
        var oldTestament = _db.Kjv.Where(k => k.BookNumber <= 39).Select(k => k.BookName).Distinct().ToList();
        var newTestament = _db.Kjv.Where(k => k.BookNumber > 39).Select(k => k.BookName).Distinct().ToList();
        foreach (var bookName in oldTestament)
        {
            bible.OldTestament.Add(new BookDto
            {
                BookName = bookName,
                ChapterCount = _db.Kjv.Where(k => k.BookName == bookName).Select(k => k.Chapter).Distinct().Count()
            });
        }
        foreach (var bookName in newTestament)
        {
            bible.NewTestament.Add(new BookDto
            {
                BookName = bookName,
                ChapterCount = _db.Kjv.Where(k => k.BookName == bookName).Select(k => k.Chapter).Distinct().Count()
            });
        }
        return bible;
    }     
}