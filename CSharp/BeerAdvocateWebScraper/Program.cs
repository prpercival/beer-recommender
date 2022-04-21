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

            scraper.Scrape(@"https://www.beeradvocate.com/beer/popular/");

            Console.ReadLine();

            Console.WriteLine("done!");
        }
    }
}

