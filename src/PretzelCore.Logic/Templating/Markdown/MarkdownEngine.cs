using PretzelCore.Core.Exceptions;
using PretzelCore.Core.Extensibility;
using PretzelCore.Core.Extensions;
using PretzelCore.Core.Telemetry;
using PretzelCore.Core.Templating;
using PretzelCore.Core.Templating.Context;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PretzelCore.Services.Templating.Markdown
{
    [Shared]
    [SiteEngineInfo(Engine = "markdown")]
    public class MarkdownEngine : ISiteEngine
    {
        [Import(AllowDefault = true)]
        public ILightweightMarkupEngine LightweightMarkupEngine { get; set; }

        [Import]
        public IFileSystem FileSystem { get; set; }

        SiteContext _context;
        static readonly Regex paragraphRegex = new Regex(@"(<(?:p|h\d{1})>.*?</(?:p|h\d{1})>)", RegexOptions.Compiled | RegexOptions.Singleline);

        public void Process(SiteContext context, bool skipFileOnError = false)
        {
            _context = context;

            for (int index = 0; index < context.Posts.Count; index++)
            {
                var p = context.Posts[index];
                var previous = GetPrevious(context.Posts, index);
                var next = GetNext(context.Posts, index);
                ProcessFile(context.OutputFolder, p, previous, next, skipFileOnError, p.Filepath);
            }

            for (int index = 0; index < context.Pages.Count; index++)
            {
                var p = context.Pages[index];
                var previous = GetPrevious(context.Pages, index);
                var next = GetNext(context.Pages, index);
                ProcessFile(context.OutputFolder, p, previous, next, skipFileOnError);
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

        private void ProcessFile(string outputDirectory, Page page, Page previous, Page next, bool skipFileOnError, string relativePath = "")
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                relativePath = MapToOutputPath(page.File);

            page.OutputFile = Path.Combine(outputDirectory, relativePath);
            var extension = Path.GetExtension(page.File);

            if (extension.IsImageFormat() || page is NonProcessedPage)
            {
                return;
            }
            if (extension.IsMarkdownFile() || extension.IsRazorFile())
            {
                page.OutputFile = page.OutputFile.Replace(extension, ".html");
            }

            var pageContext = PageContext.FromPage(_context, page, page.OutputFile);

            pageContext.Previous = previous;
            pageContext.Next = next;

            var pageContexts = new List<PageContext> { pageContext };
            object paginateObj;
            if (page.Bag.TryGetValue("paginate", out paginateObj))
            {
                var paginate = Convert.ToInt32(paginateObj);
                var totalPages = (int)Math.Ceiling(_context.Posts.Count / Convert.ToDouble(paginateObj));
                var paginator = new Paginator(_context, totalPages, paginate, 1);
                pageContext.Paginator = paginator;

                var paginateLink = "/page/:page/index.html";
                if (page.Bag.ContainsKey("paginate_link"))
                    paginateLink = Convert.ToString(page.Bag["paginate_link"]);

                var prevLink = page.Url;
                for (var i = 2; i <= totalPages; i++)
                {
                    var newPaginator = new Paginator(_context, totalPages, paginate, i) { PreviousPageUrl = prevLink };
                    var link = paginateLink.Replace(":page", Convert.ToString(i));
                    paginator.NextPageUrl = link;

                    paginator = newPaginator;
                    prevLink = link;

                    var path = Path.Combine(outputDirectory, link.ToRelativeFile());
                    if (path.EndsWith(FileSystem.Path.DirectorySeparatorChar.ToString()))
                    {
                        path = Path.Combine(path, "index.html");
                    }
                    var context = new PageContext(pageContext) { Paginator = newPaginator, OutputPath = path };
                    context.Bag["url"] = link;
                    pageContexts.Add(context);
                }
            }

            foreach (var context in pageContexts)
            {
                var metadata = page.Bag;

                var excerptSeparator = context.Bag.ContainsKey("excerpt_separator")
                    ? context.Bag["excerpt_separator"].ToString()
                    : _context.ExcerptSeparator;
                try
                {
                    context.Content = RenderContent(page.File, context.Content);
                    context.FullContent = context.Content;
                    context.Bag["excerpt"] = GetContentExcerpt(context.Content, excerptSeparator);
                }
                catch (Exception ex)
                {
                    if (!skipFileOnError)
                    {
                        var message = string.Format("Failed to process {0}, see inner exception for more details", context.OutputPath);
                        throw new PageProcessingException(message, ex);
                    }

                    Console.WriteLine(@"Failed to process {0}: {1}", context.OutputPath, ex);
                    continue;
                }
            }
        }

        private string RenderContent(string file, string contents)
        {
            string html;
            try
            {
                var contentsWithoutHeader = contents.ExcludeHeader();

                html = Path.GetExtension(file).IsMarkdownFile()
                       ? LightweightMarkupEngine.Convert(contentsWithoutHeader).Trim()
                       : contentsWithoutHeader;

                //if (ContentTransformers != null)
                //{
                //    html = ContentTransformers.Aggregate(html, (current, contentTransformer) => contentTransformer.Transform(current));
                //}
            }
            catch (Exception e)
            {
                Tracing.Info("Error ({0}) converting {1}", e.Message, file);
                Tracing.Debug(e.ToString());
                html = string.Format("<p><b>Error converting markdown:</b><br />{0}</p><p>Original content:<br /><pre>{1}</pre></p>", e.Message, contents);
            }
            return html;
        }

        private static string GetContentExcerpt(string content, string excerptSeparator)
        {
            var excerptSeparatorIndex = content.IndexOf(excerptSeparator, StringComparison.InvariantCulture);
            string excerpt = null;
            if (excerptSeparatorIndex == -1)
            {
                var match = paragraphRegex.Match(content);
                if (match.Success)
                {
                    excerpt = match.Groups[1].Value;
                }
            }
            else
            {
                excerpt = content.Substring(0, excerptSeparatorIndex);
                if (excerpt.StartsWith("<p>") && !excerpt.EndsWith("</p>"))
                {
                    excerpt += "</p>";
                }
            }
            return excerpt;
        }

        public void CopyFileIfSourceNewer(string sourceFileName, string destFileName, bool overwrite)
        {
            if (!FileSystem.File.Exists(destFileName) ||
                FileSystem.File.GetLastWriteTime(sourceFileName) > FileSystem.File.GetLastWriteTime(destFileName))
            {
                FileSystem.File.Copy(sourceFileName, destFileName, overwrite);
            }
        }

        private void CreateOutputDirectory(string outputFile)
        {
            var directory = Path.GetDirectoryName(outputFile);
            if (!FileSystem.Directory.Exists(directory))
                FileSystem.Directory.CreateDirectory(directory);
        }

        private static readonly string[] layoutExtensions = { ".html", ".htm" };

        protected virtual string[] LayoutExtensions
        {
            get { return layoutExtensions; }
        }

        private IDictionary<string, object> ProcessTemplate(PageContext pageContext, string path)
        {
            var templateFile = FileSystem.File.ReadAllText(path);
            var metadata = templateFile.YamlHeader();
            var templateContent = templateFile.ExcludeHeader();

            //pageContext.FullContent = RenderTemplate(templateContent, pageContext);

            return metadata;
        }

        private string MapToOutputPath(string file)
        {
            var temp = file.Replace(_context.SourceFolder, "")
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return temp;
        }

        public bool CanProcess(SiteContext context)
        {
            var engineInfo = GetType().GetCustomAttributes(typeof(SiteEngineInfoAttribute), true).SingleOrDefault() as SiteEngineInfoAttribute;
            if (engineInfo == null) return false;
            return context.Engine == engineInfo.Engine;
        }

        private string FindLayoutPath(string layout)
        {
            foreach (var extension in LayoutExtensions)
            {
                var path = Path.Combine(_context.SourceFolder, "_layouts", layout + extension);
                if (FileSystem.File.Exists(path))
                    return path;
            }

            return null;
        }

        public void Initialize()
        {

        }
    }
}
