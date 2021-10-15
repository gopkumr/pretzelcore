using PretzelCore.Core.Extensibility;
using PretzelCore.Core.Extensions;
using PretzelCore.Core.Telemetry;
using PretzelCore.Core.Templating.Context;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO.Abstractions;

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
        public IEnumerable<ITransform> Transforms { get; set; }

        public void GenerateSite(SiteContext siteContext)
        {
            if (_sourceContentEngine == null || _templateEngine == null)
            {
                Tracing.Error("Cannot find engine for input or template");
                return;
            }

            this.StaticContentHandler.Process(siteContext);

            Tracing.Info("Completed static content processing");

            _sourceContentEngine.Initialize();
            _sourceContentEngine.Process(siteContext);

            _templateEngine.Initialize();
            _templateEngine.Process(siteContext);

            foreach (var t in Transforms)
                t.Transform(siteContext);
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
    }
}
