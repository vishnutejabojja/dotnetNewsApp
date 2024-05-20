using HtmlAgilityPack;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Web;
using System.Runtime.InteropServices;

namespace AirplaneNewsScraper.Services
{
    public class NewsScraperService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NewsScraperService> _logger;

        public NewsScraperService(HttpClient httpClient, ILogger<NewsScraperService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<NewsArticle>> GetAirplaneNewsAsync()
        {
            var newsArticles = new List<NewsArticle>();
            var url = "https://www.ainonline.com/channel/aircraft"; // Replace with actual news site URL
            string response;

            try
            {
                response = await _httpClient.GetStringAsync(url);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching news from {Url}", url);
                return newsArticles;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(response);

            var articles = doc.DocumentNode.SelectNodes("//div[@class='List_row-container__KxB2C ']"); // Adjust the XPath as necessary

            if (articles == null)
            {
                _logger.LogWarning("No articles found at {Url}", url);
                return newsArticles;
            }

            foreach (var article in articles)
            {
                var titleNode = article.SelectSingleNode(".//div/div[@class='Row_heading-container__N22Y6']/a[@class='Row_link__0_lcz']");
                var summaryNode = article.SelectSingleNode(".//div/div[@class='Row_heading-container__N22Y6']/div[@class='Row_subheading__Lr7aG']");
                var linkNode = article.SelectSingleNode(".//div/div[@class='Row_heading-container__N22Y6']/a");
                var imgNode = article.SelectSingleNode(".//div/div[@class='Row_media__29Sjt']/img");
                var subHeadingNode = article.SelectSingleNode(".//div/div[@class='Row_heading-container__N22Y6']/div[@class='Row_label__OW2g4 ']");

                if (titleNode == null || summaryNode == null || linkNode == null || imgNode == null)
                {
                    _logger.LogWarning("Incomplete article information found at {Url}", url);
                    continue;
                }

                var existingHref = linkNode.GetAttributeValue("href", string.Empty);
                var newHref = "https://www.ainonline.com" + existingHref; // Modify this as needed
                var imageUrl = imgNode.GetAttributeValue("srcset", string.Empty);

                linkNode.SetAttributeValue("href", newHref);

                var newsArticle = new NewsArticle
                {
                    Title = HttpUtility.HtmlDecode(titleNode.InnerText.Trim()),
                    Summary = HttpUtility.HtmlDecode(summaryNode.InnerText.Trim()),
                    Link = HttpUtility.HtmlDecode(linkNode.GetAttributeValue("href", string.Empty)),
                    Img = imageUrl,
                    SubHeading = HttpUtility.HtmlDecode(subHeadingNode.InnerHtml.Trim())
                };

                newsArticles.Add(newsArticle);
            }

            return newsArticles;
        }
    }

    public class NewsArticle
    {
        public string Title { get; set; }
        public string SubHeading { get; set; }
        public string Summary { get; set; }
        public string Link { get; set; }
        public string Img { get; set; }
    }
}
