using System.Security.Claims;
using Bible.Data;
using Bible.Dto;
using Bible.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bible.Controllers;

public class UserCollectionController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly string? _origin = Environment.GetEnvironmentVariable("ORIGIN");
    public UserCollectionController(ApplicationDbContext db)
    {
        _db = db;
    }
    
    [HttpGet("/user-collection")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UserCollection(){
        var username = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        CollectionDto collectionDto = new CollectionDto
        {
            Owner = username,
            Origin = _origin ?? string.Empty
        };
        User? user = await _db.User.FindAsync(username);
        if (user != null)
        {
            string collectionString = user.Collection;
            string[] collectionArray = collectionString.Split(",");
            collectionArray = collectionArray.Skip(1).ToArray();
            foreach (string str in collectionArray)
            {
                int verseId = int.Parse(str);
                Kjv? verse = await _db.Kjv.FindAsync(verseId);
                if (verse != null)
                {
                    CardAndNoteDto cardAndNoteDto = new CardAndNoteDto
                    {
                        Id = verseId,
                        Verse = $"{verse.BookName} {verse.Chapter}:{verse.Verse}",
                        Text = verse.Text
                    };          
                    Note? note = await _db.Note.FirstOrDefaultAsync(
                        n => n.Username == username && n.ForVerseId == verseId);
                    cardAndNoteDto.Note = note != null ? note.Content : string.Empty;  
                    collectionDto.CardAndNote.Add(cardAndNoteDto);
                }
            }
        }
        return View(collectionDto);
    }
    
    public class DataToSave
    {
        public string Username { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int ForVerseId { get; set; }
    }
    [HttpPost("/user-collection/save-note")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IActionResult> SaveNote([FromBody] DataToSave data)
    {
        try
        {
            Note? note = await _db.Note.FirstOrDefaultAsync(
                u => u.Username == data.Username && u.ForVerseId == data.ForVerseId);
            if (note != null)
            {
                note.Content = data.Content;
            }
            else
            {
                Note newNote = new Note
                {
                    Username = data.Username,
                    Content = data.Content,
                    ForVerseId = data.ForVerseId
                };
                _db.Note.Add(newNote);
            }
            await _db.SaveChangesAsync();
            return Ok();
        }catch(Exception){
            return Problem();
        }  
    }
    
    public class DataToDelete
    {
        public string Username { get; set; }= string.Empty;
        public int ForVerseId { get; set; }
    }
    [HttpDelete("/user-collection/delete-verse")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IActionResult> DeleteNote([FromBody] DataToDelete data)
    {
        try
        {
            Note? note = await _db.Note.FirstOrDefaultAsync(
                u => u.Username == data.Username && u.ForVerseId == data.ForVerseId);
            if (note != null)
            {
                _db.Note.Remove(note);
            }
            User? user = await _db.User.FindAsync(data.Username);
            if (user != null)
            {
                string collectionString = user.Collection;
                HashSet<string> collectionSet = collectionString
                    .Split(',')
                    .Select(item => item.Trim()) // Trim each item to remove any extra whitespace
                    .ToHashSet();
                collectionSet.Remove(data.ForVerseId.ToString());
                collectionString = string.Join(",", collectionSet);
                user.Collection = collectionString;
            }         
            await _db.SaveChangesAsync();
            return Ok();
        }catch(Exception){
            return Problem();
        }  
    }
}