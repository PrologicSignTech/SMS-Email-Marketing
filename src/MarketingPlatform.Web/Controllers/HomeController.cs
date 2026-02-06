using Microsoft.AspNetCore.Mvc;
using MarketingPlatform.Application.Interfaces;

namespace MarketingPlatform.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IPageContentService _pageContentService;

    public HomeController(
        ILogger<HomeController> logger, 
        IConfiguration configuration,
        IPageContentService pageContentService)
    {
        _logger = logger;
        _configuration = configuration;
        _pageContentService = pageContentService;
    }

    public IActionResult Index()
    {
        ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
        return View();
    }

    public async Task<IActionResult> Privacy()
    {
        var pageContent = await _pageContentService.GetByPageKeyAsync("privacy-policy");
        return View(pageContent);
    }

    public async Task<IActionResult> Terms()
    {
        var pageContent = await _pageContentService.GetByPageKeyAsync("terms-of-service");
        return View(pageContent);
    }

    public IActionResult Error()
    {
        return View();
    }
}
