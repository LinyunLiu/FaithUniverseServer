using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Bible.Data;
using Bible.Dto;
using Bible.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bible.Controllers;

public class UserHomeController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly ApplicationDbContext _db;   
    private readonly string? _chatGptToken = Environment.GetEnvironmentVariable("CHATGPT_API_KEY");
    private readonly string? _origin = Environment.GetEnvironmentVariable("ORIGIN"); // SERVER ENDPOINT
    public UserHomeController(HttpClient httpClient, ApplicationDbContext db)
    {
        _httpClient = httpClient;
        _db = db;
    }
    
    [HttpGet("/user-home")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UserHome()
    {
        // var username = HttpContext.User.Identity.Name; // Alternative
        var username = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        HomeDto homeDto = new HomeDto
        {
            Owner = username,
            Search = string.Empty,
            Origin = _origin ?? string.Empty
        };       
        // Initialize 6 random Bible verses
        // Range [1, 31102], total 31102 verses in the current KJV in the databases
        Random random = new Random();
        for (var i = 0; i < 6; i++)
        {
            Kjv verse = await _db.Kjv.FindAsync(random.Next(1, 31103)) ?? new Kjv();
            homeDto.Cards.Add(new CardDto
            {
                Id = verse.Id,
                Verse = $"{verse.BookName} {verse.Chapter}:{verse.Verse}",
                Text = verse.Text
            });
        }
        return View(homeDto);
    }
    
    [HttpPost("/user-home")][ActionName(nameof(UserHome))]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UserHomePost(HomeDto homeDto)
    {   
        var username = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        homeDto.Owner = username;
        homeDto.Origin = _origin ?? string.Empty;
        var response = await Chat(homeDto.Search);
        if (response == "err")
        {
            TempData["UserHomeMessage"] = "Error while trying to connect to ChatGPT";
            return View("UserHome", homeDto);
        }
        try
        {
            int count = 0;
            homeDto.Cards.Clear();
            var items = response.Trim().Split(";");
            foreach (var item in items)
            {
                try
                {
                    var tmp = item.Trim().Split(",");
                    string bookName = tmp[0];
                    int chapter = int.Parse(tmp[1]);
                    int verseNumber = int.Parse(tmp[2]);
                    Kjv? verse = await _db.Kjv.FirstOrDefaultAsync(v =>
                        v.BookName == bookName && v.Chapter == chapter && v.Verse == verseNumber);
                    if (verse != null)
                    {
                        homeDto.Cards.Add(new CardDto
                        {
                            Id = verse.Id,
                            Text = verse.Text,
                            Verse = $"{verse.BookName} {verse.Chapter}:{verse.Verse}"
                        });
                        count++;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Error Parsing Data");
                }
                if (count == 6)
                {
                    break;
                }
            }
            TempData["UserHomeMessage"] = homeDto.Cards.Count == 0 ? "Oops! There are no results, so sorry :(" : "AI Responded!";
            return View("UserHome", homeDto);
        }catch(Exception)
        {
            TempData["UserHomeMessage"] = "Something Went Wrong, Please Try Again";
            return View("UserHome", homeDto);
        }
    }   
    public async Task<string> Chat(string message)
    {
        string apiUrl = "https://api.openai.com/v1/chat/completions";
        string rules =
            "Give me 10 and 10 only Bible Verses from King James Version that can potentially help with the following description (Please only give the answer and strictly in this format separated by ';' -> 'BookName,ChapterNumber,VerseNumber' -> 'Genesis,2,1;Genesis,3,2;Genesis,5,5'): ";
        var requestBody = new
        {
            model = "gpt-4",
            messages = new[]
            {
                new { role = "user", content = rules + message }
            }
        };
        var jsonContent = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_chatGptToken}");
        try
        {
            var response = await _httpClient.PostAsync(apiUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();   
            var responseJson = JsonDocument.Parse(responseString);
            var target = responseJson.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "err";
            return target;
        }
        catch (Exception)
        {
            return "err";
        }
    }
    
    [HttpPost("/user-home/add")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UserHomeAddCard()
    {
        var username = HttpContext.Request.Query["username"];
        var verseId = HttpContext.Request.Query["verseId"];
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
}