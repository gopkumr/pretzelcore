using PretzelCore.Core.Extensibility;
using PretzelCore.Core.Telemetry;
using PretzelCore.Core.Templating.Context;
using System.Collections.Generic;
using System.Composition;
using System.IO.Abstractions;
using System.Linq;

namespace PretzelCore.Services.Templating
{
    [Export]
    public class PretzelSiteGenerator : ISiteGenerator
    {
        ISiteEngine _sourceContentEngine;
        ISiteEngine _templateEngine;

        [Import]
        public StaticContentHandler StaticContentHandler { get; set; }

        [Import]
        public IFileSystem FileSystem { get; set; }

        [ImportMany]
        public IEnumerable<IPlugin> Plugins { get; set; }

        public void GenerateSite(SiteContext siteContext)
        {
            if (_sourceContentEngine == null || _templateEngine == null)
            {
                Tracing.Error("Cannot find engine for input or template");
                return;
            }

            this.StaticContentHandler.Process(siteContext);

            Tracing.Info("Completed static content processing");

            if (Plugins != null)
            {
                ExecuteContentTrasformations(siteContext);
            }

            _sourceContentEngine.Initialize();
            _sourceContentEngine.Process(siteContext);

            _templateEngine.Initialize();
            _templateEngine.Process(siteContext);

            foreach (var t in Plugins)
                t.PostProcessingTransform(siteContext);
        }


        public ISiteGenerator SetSourceContentEngine(ISiteEngine engine)
        {
            _sourceContentEngine = engine;
            return this;
        }

        public ISiteGenerator SetTemplatingEngine(ISiteEngine engine)
        {
            _templateEngine = engine;
            return this;
        }

        private void ExecuteContentTrasformations(SiteContext siteContext)
        {
            foreach (var post in siteContext.Posts)
                post.Content = Plugins.Aggregate(post.Content, (currentContent, contentTransformer) => contentTransformer.ContentTransform(post.File, currentContent));

            foreach (var page in siteContext.Pages)
                page.Content = Plugins.Aggregate(page.Content, (currentContent, contentTransformer) => contentTransformer.ContentTransform(page.File, currentContent));
        }
    }
}
