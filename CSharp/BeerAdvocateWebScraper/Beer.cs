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
        public List<string> Comments { get; private set; }

        public Beer(string name, string link, List<string>? comments = null)
        {
            Name = name;
            Link = link;

            if (comments != null)
                Comments = comments;
            else
                Comments = new List<string>();
        }

        public void AddComment(string comment)
        {
            Comments.Add(comment);
        }
    }
}
