using System;
using System.Linq;
using DotLiquid;
using PretzelCore.Services.Templating.Context;
using PretzelCore.Services.Templating.Jekyll.Extensions;

namespace PretzelCore.Services.Templating.Jekyll
{
    public class SiteContextDrop : Drop
    {
        private readonly SiteContext context;

        public DateTime Time
        {
            get
            {
                return context.Time;
            }
        }

        public string Title
        {
            get { return context.Title; }
        }

        public SiteContextDrop(SiteContext context)
        {
            this.context = context;
        }

        public Hash ToHash()
        {
            var x = Hash.FromDictionary(context.Config.ToDictionary());
            x["posts"] = context.Posts.Select(p => p.ToHash()).ToList();
            x["pages"] = context.Pages.Select(p => p.ToHash()).ToList();
            x["html_pages"] = context.Html_Pages.Select(p => p.ToHash()).ToList();
            x["title"] = context.Title;
            x["tags"] = context.Tags;
            x["categories"] = context.Categories;
            x["time"] = Time;
            x["data"] = context.Data;

            return x;
        }
    }
}
