using PretzelCore.Core.Extensibility;
using PretzelCore.Core.Templating.Context;
using PretzelCore.Services.Templating;
using System.IO.Abstractions;

namespace PretzelCore.Core.Extensions
{
    public static class Sitemap
    {
        public static void CompressSitemap(this ISiteEngine engine, SiteContext siteContext, IFileSystem fileSystem)
        {
            var sitemap = fileSystem.Path.Combine(siteContext.OutputFolder, @"sitemap.xml");
            var compressedSitemap = sitemap + ".gz";

            if (fileSystem.File.Exists(sitemap))
            {
                using (var sitemapStream = fileSystem.File.OpenRead(sitemap))
                {
                    using (var compressedMap = fileSystem.File.Create(compressedSitemap))
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
