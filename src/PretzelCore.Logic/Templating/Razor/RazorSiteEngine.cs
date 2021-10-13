using PretzelCore.Core.Exceptions;
using PretzelCore.Core.Extensibility;
using PretzelCore.Core.Extensions;
using PretzelCore.Core.Telemetry;
using PretzelCore.Core.Templating;
using PretzelCore.Core.Templating.Context;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;

namespace PretzelCore.Services.Templating.Razor
{
    [Shared]
    [SiteEngineInfo(Engine = "razor")]
    public class RazorSiteEngine : ISiteEngine
    {
        private static readonly string[] layoutExtensions = { ".cshtml" };

        private string includesPath;

        private readonly List<ITag> _allTags = new List<ITag>();

        private SiteContext _context;

        [Import]
        public IFileSystem FileSystem { get; set; }

        [ImportMany]
        public IEnumerable<IFilter> Filters { get; set; }

        [ImportMany]
        public IEnumerable<ITag> Tags { get; set; }

        [ImportMany]
        public IEnumerable<TagFactoryBase> TagFactories { get; set; }

        public void Initialize()
        {
        }

        public bool CanProcess(SiteContext context)
        {
            var engineInfo = GetType().GetCustomAttributes(typeof(SiteEngineInfoAttribute), true).SingleOrDefault() as SiteEngineInfoAttribute;
            if (engineInfo == null) return false;
            return context.Engine == engineInfo.Engine;
        }

        public void Process(SiteContext context, bool skipFileOnError = false)
        {
            Tracing.Debug("Rendering Engine: {0}", this.GetType().Name);

            _context = context;
            PreProcess();

            for (int index = 0; index < context.Posts.Count; index++)
            {
                var p = context.Posts[index];
                ProcessFile(context.OutputFolder, p, skipFileOnError, p.Filepath);
            }

            for (int index = 0; index < context.Pages.Count; index++)
            {
                var p = context.Pages[index];
                ProcessFile(context.OutputFolder, p, skipFileOnError);
            }
        }

        private void ProcessFile(string outputDirectory, Page page, bool skipFileOnError, string relativePath = "")
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                relativePath = MapToOutputPath(page.File);

            page.OutputFile = Path.Combine(outputDirectory, relativePath);
            var extension = Path.GetExtension(page.File);

            if (extension.IsImageFormat() || (page is NonProcessedPage))
            {
                return;
            }

            if (extension.IsMarkdownFile() || extension.IsRazorFile())
                page.OutputFile = page.OutputFile.Replace(extension, ".html");

            var pageContext = PageContext.FromPage(_context, page, outputDirectory, page.OutputFile);
            pageContext.Content = RenderTemplate(pageContext.Content, pageContext);
            pageContext.FullContent = pageContext.Content;

            var metadata = page.Bag;
            while (metadata.ContainsKey("layout"))
            {
                var layout = metadata["layout"];
                if ((string)layout == "nil" || layout == null)
                    break;

                var path = FindLayoutPath(layout.ToString());

                if (path == null)
                    break;

                try
                {
                    var layoutFile = FileSystem.File.ReadAllText(path);
                    metadata = layoutFile.YamlHeader();
                    var layoutFileContent = layoutFile.ExcludeHeader();
                    pageContext.FullContent = RenderTemplate(layoutFileContent, pageContext);
                }
                catch (Exception ex)
                {
                    if (!skipFileOnError)
                    {
                        var message = string.Format("Failed to process layout {0} for {1}, see inner exception for more details", layout, pageContext.OutputPath);
                        throw new PageProcessingException(message, ex);
                    }

                    Console.WriteLine(@"Failed to process layout {0} for {1} because '{2}'. Skipping file", layout, pageContext.OutputPath, ex.Message);
                    break;
                }
            }

            CreateOutputDirectory(pageContext.OutputPath);
            FileSystem.File.WriteAllText(pageContext.OutputPath, pageContext.FullContent);
        }


        private class TagComparer : IEqualityComparer<ITag>
        {
            public bool Equals(ITag x, ITag y)
            {
                if (x == null || y == null)
                {
                    return false;
                }

                return x.Name == y.Name;
            }

            public int GetHashCode(ITag obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        protected void PreProcess()
        {
            includesPath = Path.Combine(_context.SourceFolder, "_includes");

            if (Tags != null)
            {
                var toAdd = Tags.Except(_allTags, new TagComparer()).ToList();
                _allTags.AddRange(toAdd);
            }

            if (TagFactories != null)
            {
                var toAdd = TagFactories.Select(factory =>
                {
                    factory.Initialize(_context);
                    return factory.CreateTag();
                }).Except(_allTags, new TagComparer()).ToList();

                _allTags.AddRange(toAdd);
            }
        }

        protected string[] LayoutExtensions
        {
            get { return layoutExtensions; }
        }

        protected string RenderTemplate(string content, PageContext pageData)
        {
            var serviceConfiguration = new TemplateServiceConfiguration
            {
                TemplateManager = new IncludesResolver(FileSystem, includesPath),
                BaseTemplateType = typeof(ExtensibleTemplate<>),
                DisableTempFileLocking = true,
                CachingProvider = new DefaultCachingProvider(t => { }),
                //ConfigureCompilerBuilder = builder => ModelDirective.Register(builder)
            };
            serviceConfiguration.Activator = new ExtensibleActivator(serviceConfiguration.Activator, Filters, _allTags);

            Engine.Razor = RazorEngineService.Create(serviceConfiguration);

            content = Regex.Replace(content, "(@model \\w*.*)", "");

            var pageContent = pageData.Content;
            pageData.Content = pageData.FullContent;

            try
            {
                content = Engine.Razor.RunCompile(content, pageData.Page.File, typeof(PageContext), pageData);
                pageData.Content = pageContent;
                return content;
            }
            catch (Exception e)
            {
                Tracing.Error(@"Failed to render template, falling back to direct content");
                Tracing.Debug(e.Message);
                Tracing.Debug(e.StackTrace);
                return content;
            }
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

        private string MapToOutputPath(string file)
        {
            var temp = file.Replace(_context.SourceFolder, "")
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return temp;
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
    }
}

