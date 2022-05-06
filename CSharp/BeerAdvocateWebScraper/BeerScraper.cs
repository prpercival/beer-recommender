using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace BeerRecommender
{
    internal class BeerScraper
    {
        private static readonly HtmlWeb _web = new();
        private static readonly HttpClient client = new HttpClient();

        public BeerScraper() { }

        public async void Scrape(string url)
        {
            var topRatedBeers = ScrapeTopRated(url);

            await AddComments(topRatedBeers);

            var json = JsonConvert.SerializeObject(topRatedBeers);

            File.WriteAllText($"../../../Output/beer_data_{DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss")}.json", json);
        }

        //477e647db155098cd96b4a3f4c94cc43
        //xf_session

        private List<Beer> ScrapeTopRated(string url)
        {
            try
            {
                var htmlDoc = _web.Load(url);

                ///html/body/div[2]/div/div[2]/div[2]/div[2]/div/div/div[3]/div/div/div[2]/table/tbody/tr[2]/td[2]/a/b
                var nodes = htmlDoc.DocumentNode.SelectNodes("//a/b").Select(node => node.ParentNode).Where(node => node.Attributes[0].Value.Contains("/beer/profile")).Select(node => node.ParentNode);

                var results = new List<Beer>();

                foreach (var node in nodes)
                {
                    var nameNode = node.ChildNodes[0];

                    var name = nameNode.ChildNodes[0].InnerHtml;
                    var link = $"https://www.beeradvocate.com{nameNode.Attributes[0].Value}";

                    var brewery = node.ChildNodes[1].ChildNodes.Where(node => node.Attributes.FirstOrDefault()?.Value.Contains("/beer/profile") ?? false).FirstOrDefault()?.InnerHtml;
                    var style = node.ChildNodes[1].ChildNodes.Where(node => node.Attributes.FirstOrDefault()?.Value.Contains("/beer/top-styles") ?? false).FirstOrDefault()?.InnerHtml;
                    var alcoholString = node.ChildNodes[1].ChildNodes.Where(node => node.Name == "#text").FirstOrDefault()?.InnerHtml;
                    var alcoholDecimal = decimal.Parse(Regex.Match(alcoholString, @"\d+.+\d").Value);

                    //Console.WriteLine($"Beer: {name}, Link: {link}");
                    results.Add(new Beer(name, link, brewery, style, alcoholDecimal));
                }

                return results;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task AddComments(List<Beer> beers)
        {
            foreach (var beer in beers)
            {
                Console.WriteLine($"Beer: {beer.Name}, Link: {beer.Link}");   

                var page = 0;

                while(beer.Comments.Count < 40)
                {
                    var htmlDoc = await LoadCommentsPage($"{beer.Link}?view=beer&show=poobahs&start={page}#lists");

                    var nodes = htmlDoc.DocumentNode.SelectNodes("//div[@id='rating_fullview_content_2']");

                    if (page == 0)
                    {
                        var score = htmlDoc.DocumentNode.SelectNodes("//span[@class='ba-score Tooltip']").FirstOrDefault()?.InnerText;

                        if (score != null)
                            beer.AddScore(decimal.Parse(score));
                    }           

                    foreach (var node in nodes)
                    {
                        var rawComment = node.ChildNodes.Where(node => node.Name == "#text").Select(node => node.InnerHtml);

                        if (rawComment.Any())
                        {
                            rawComment = rawComment.Select(x => x.Replace("\n", " ").Replace("\r", " ").Replace("&quot;", " ").Replace("&nbsp;", "").Replace("rDev", ""));
                            //var comment = new string(string.Join("", rawComment).Where(c => !char.IsPunctuation(c)).ToArray());
                            var comment = string.Join("", rawComment);
                            beer.AddComment(comment);
                        }

                        if (beer.Comments.Count >= 40)
                            break;
                    }

                    page += 40;
                }
            }
        }

        private static async Task<HtmlDocument> LoadCommentsPage(string url)
        {
            Stream stream;
            var baseAddress = new Uri("https://www.beeradvocate.com");
            using (var handler = new HttpClientHandler { UseCookies = false })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                var message = new HttpRequestMessage(HttpMethod.Get, url);
                message.Headers.Add("Cookie", "xf_session=477e647db155098cd96b4a3f4c94cc43");
                var result = await client.SendAsync(message);
                result.EnsureSuccessStatusCode();
                stream = result.Content.ReadAsStream();
            }

            var document = new HtmlDocument();
            document.Load(stream);

            return document;
        }
    }
}
