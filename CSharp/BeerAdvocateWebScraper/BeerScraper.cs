using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var nodes = htmlDoc.DocumentNode.SelectNodes("//a/b").Select(node => node.ParentNode).Where(node => node.Attributes[0].Value.Contains("/beer/profile"));

            var results = new List<Beer>();

            foreach (var node in nodes)
            {
                var name = node.ChildNodes[0].InnerHtml;
                var link = $"https://www.beeradvocate.com{node.Attributes[0].Value}?show=poobahs#lists";

                //Console.WriteLine($"Beer: {name}, Link: {link}");
                results.Add(new Beer(name, link));
            }

            return results;
        }

        private void AddComments(List<Beer> beers)
        {
            foreach (var beer in beers)
            {
                Console.WriteLine($"Beer: {beer.Name}, Link: {beer.Link}");

                var htmlDoc = _web.Load(beer.Link);

                ///html/body/div[2]/div/div[2]/div[2]/div[2]/div/div/div[3]/div/div/div[2]/div[9]/div/div[1]/div[2]
                var nodes = htmlDoc.DocumentNode.SelectNodes("//div[@id='rating_fullview_content_2']");

                var comments = new List<string>();

                foreach (var node in nodes)
                {
                    var comment = node.ChildNodes.Where(node => node.Name == "#text").Select(node => node.InnerHtml);

                    if (comment.Any())
                        beer.AddComment(string.Join("", comment));
                }
            }
        }
    }
}
