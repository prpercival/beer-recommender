using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerRecommender
{
    public class Beer
    {
        public string Name { get; private set; }
        public string Link { get; private set; }
        public string Brewery { get; private set; }
        public string Style { get; private set; }
        public decimal Alcohol { get; private set; }
        public decimal Score { get; private set; }
        public List<string> Comments { get; private set; }

        public Beer(string name, string link, string brewery, string style, decimal alcohol, decimal? score = null, List<string>? comments = null)
        {
            Name = name;
            Link = link;
            Brewery = brewery;
            Style = style;
            Alcohol = alcohol;

            if (comments != null)
                Comments = comments;
            else
                Comments = new List<string>();
        }

        public void AddComment(string comment)
        {
            Comments.Add(comment);
        }

        public void AddScore(decimal score)
        {
            this.Score = score;
        }
    }
}
