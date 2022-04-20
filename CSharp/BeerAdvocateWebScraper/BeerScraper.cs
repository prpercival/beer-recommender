using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BeerRecommender
{
    internal class BeerScraper
    {
        private static readonly HtmlWeb _web = new();

        public BeerScraper() { }

        public List<Beer> Scrape(string url)
        {
            var topRatedBeers = ScrapeTopRated(url);

            AddComments(topRatedBeers);

            return topRatedBeers;
        }

        private List<Beer> ScrapeTopRated(string url)
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

        private void AddComments(List<Beer> beers)
        {
            foreach (var beer in beers)
            {
                Console.WriteLine($"Beer: {beer.Name}, Link: {beer.Link}");

                var htmlDoc = _web.Load($"{beer.Link}?show=poobahs#lists");

                ///html/body/div[2]/div/div[2]/div[2]/div[2]/div/div/div[3]/div/div/div[2]/div[9]/div/div[1]/div[2]
                var nodes = htmlDoc.DocumentNode.SelectNodes("//div[@id='rating_fullview_content_2']");

                var comments = new List<string>();

                foreach (var node in nodes)
                {
                    var rawComment = node.ChildNodes.Where(node => node.Name == "#text").Select(node => node.InnerHtml);             

                    var score = htmlDoc.DocumentNode.SelectNodes("//span[@class='ba-score Tooltip']").FirstOrDefault()?.InnerText;

                    if (score != null)
                        beer.AddScore(decimal.Parse(score));

                    if (rawComment.Any())
                    {
                        rawComment = rawComment.Select(x => x.Replace("\n", " ").Replace("\r", " ").Replace("&quot;", " ").Replace("&nbsp;", " "));
                        //var comment = new string(string.Join("", rawComment).Where(c => !char.IsPunctuation(c)).ToArray());
                        var comment = string.Join("", rawComment);
                        beer.AddComment(comment);
                    }  
                }
            }
        }
    }
}
