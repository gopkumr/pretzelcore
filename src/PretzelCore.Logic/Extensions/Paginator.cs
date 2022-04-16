using PretzelCore.Core.Extensibility;
using PretzelCore.Core.Templating.Context;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Text;

namespace PretzelCore.Services.Extensions
{
    public class DefaultPaginator : AbstractPlugin
    {
        [Import]
        public IFileSystem FileSystem { get; set; }

        public override void PreProcessingTransform(SiteContext context)
        {

            for (int index = 0; index < context.Posts.Count; index++)
            {
                var p = context.Posts[index];
                var previous = GetPrevious(context.Posts, index);
                var next = GetNext(context.Posts, index);

                var pageContext = PageContext.FromPage(context, p, p.OutputFile);

                pageContext.Previous = previous;
                pageContext.Next = next;

                var pageContexts = new List<PageContext> { pageContext };
                object paginateObj;
                if (p.Bag.TryGetValue("paginate", out paginateObj))
                {
                    var paginate = Convert.ToInt32(paginateObj);
                    var totalPages = (int)Math.Ceiling(context.Posts.Count / Convert.ToDouble(paginateObj));
                    var paginator = new Paginator(context, totalPages, paginate, 1);
                    pageContext.Paginator = paginator;

                    var paginateLink = "/page/:page/index.html";
                    if (p.Bag.ContainsKey("paginate_link"))
                        paginateLink = Convert.ToString(p.Bag["paginate_link"]);

                    var prevLink = p.Url;
                    for (var i = 2; i <= totalPages; i++)
                    {
                        var newPaginator = new Paginator(context, totalPages, paginate, i) { PreviousPageUrl = prevLink };
                        var link = paginateLink.Replace(":page", Convert.ToString(i));
                        paginator.NextPageUrl = link;

                        paginator = newPaginator;
                        prevLink = link;

                        var path = Path.Combine(context.OutputFolder, link.ToRelativeFile());
                        if (path.EndsWith(FileSystem.Path.DirectorySeparatorChar.ToString()))
                        {
                            path = Path.Combine(path, "index.html");
                        }
                        var pagecontext = new PageContext(pageContext) { Paginator = newPaginator, OutputPath = path };
                        pagecontext.Bag["url"] = link;
                        pageContexts.Add(pagecontext);
                    }
                }

                //var metadata = page.Bag;
            }
        }

        private static Page GetPrevious(IList<Page> pages, int index)
        {
            return index < pages.Count - 1 ? pages[index + 1] : null;
        }

        private static Page GetNext(IList<Page> pages, int index)
        {
            return index >= 1 ? pages[index - 1] : null;
        }
    }
}
