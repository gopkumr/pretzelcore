using DotLiquid;
using PretzelCore.Services.Templating.Context;

namespace PretzelCore.Services.Templating.Jekyll.Extensions
{
    public static class PageExtensions
    {
        public static Hash ToHash(this Page page)
        {
            var p = Hash.FromDictionary(page.Bag);
            p.Remove("Date");
            p.Remove("Title");
            p.Remove("Url");
            p.Add("Title", page.Title);
            p.Add("Url", page.Url);
            p.Add("Date", page.Date);
            p.Add("Content", page.Content);
            return p;
        }
    }
}
