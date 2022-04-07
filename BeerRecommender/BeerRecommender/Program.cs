using System;
using System.Xml;
using System.Linq;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace BeerRecommender
{
    public class Program
    {
        static void Main()
        {
            var scraper = new BeerScraper();

            var beers = scraper.Scrape(@"https://www.beeradvocate.com/beer/popular/");

            var json = JsonConvert.SerializeObject(beers);

            File.WriteAllText($"../../../Output/beer_data_{DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss")}.json", json);

            Console.WriteLine("done!");
        }
    }
}

