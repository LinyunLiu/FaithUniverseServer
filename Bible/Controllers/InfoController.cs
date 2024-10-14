using Microsoft.AspNetCore.Mvc;

namespace Bible.Controllers;

public class InfoController : Controller
{
    [HttpGet("/privacy")]
    public IActionResult Privacy(){
        return View();
    }
}