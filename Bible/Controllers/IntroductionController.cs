using Microsoft.AspNetCore.Mvc;

namespace Bible.Controllers;

public class IntroductionController : Controller
{
    [HttpGet]
    [Route("")]
    [Route("home")]
    public IActionResult Home()
    {
        return View();
    }
    
    [HttpGet]
    [Route("overview")]
    public IActionResult Overview()
    {
        return View();
    }
    
    [HttpGet]
    [Route("about")]
    public IActionResult About()
    {
        return View();
    }
}