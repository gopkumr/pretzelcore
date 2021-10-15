using PretzelCore.Core.Extensibility;
using PretzelCore.Core.Templating.Context;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO.Abstractions;
using System.Text;

namespace PretzelCore.Services.Extensions
{
    [Export(typeof(IPlugin))]
    public class SitemapCompression : AbstractPlugin
    {
        IFileSystem _fileSystem;

        [ImportingConstructor]
        public SitemapCompression(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public override void PostProcessingTransform(SiteContext siteContext)
        {
            var sitemap = _fileSystem.Path.Combine(siteContext.OutputFolder, @"sitemap.xml");
            var compressedSitemap = sitemap + ".gz";

            if (_fileSystem.File.Exists(sitemap))
            {
                using (var sitemapStream = _fileSystem.File.OpenRead(sitemap))
                {
                    using (var compressedMap = _fileSystem.File.Create(compressedSitemap))
                    {
                        using (var gzip = new System.IO.Compression.GZipStream(compressedMap, System.IO.Compression.CompressionMode.Compress))
                        {
                            sitemapStream.CopyTo(gzip);
                        }
                    }
                }
            }
        }
    }
}
