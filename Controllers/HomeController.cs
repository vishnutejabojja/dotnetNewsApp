using Microsoft.AspNetCore.Mvc;
using AirplaneNewsScraper.Services;
using System.Threading.Tasks;

namespace AirplaneNewsScraper.Controllers
{
    public class HomeController : Controller
    {
        private readonly NewsScraperService _newsScraperService;

        public HomeController(NewsScraperService newsScraperService)
        {
            _newsScraperService = newsScraperService;
        }

        public async Task<IActionResult> Index()
        {
            var newsArticles = await _newsScraperService.GetAirplaneNewsAsync();
            return View(newsArticles);
        }
    }
}
