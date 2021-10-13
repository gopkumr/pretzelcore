using PretzelCore.Core.Commands;
using PretzelCore.Core.Commands.Interfaces;
using PretzelCore.Core.Extensibility;
using PretzelCore.Core.Extensions;
using PretzelCore.Core.Telemetry;
using PretzelCore.Services.Templating;
using PretzelCore.Services.Templating.Context;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace PretzelCore.Services.Commands
{
    [Shared]
    [Export]
    [CommandArguments]
    public sealed class BakeCommandArguments : BakeBaseCommandArguments
    {
        [ImportingConstructor]
        public BakeCommandArguments(IFileSystem fileSystem) : base(fileSystem) { }
    }

    [Shared]
    [CommandInfo(
        Name = "bake",
        Description = "transforming content into a website",
        ArgumentsType = typeof(BakeCommandArguments),
        CommandType = typeof(BakeCommand)
        )]
    public sealed class BakeCommand : Command<BakeCommandArguments>
    {
        [Import]
        public TemplateEngineCollection TemplateEngines { get; set; }

        [Import]
        public SiteContextGenerator Generator { get; set; }

        [ImportMany]
        public IEnumerable<ITransform> Transforms { get; set; }

        [Import]
        public StaticContentHandler StaticContentHandler { get; set; }

        [Import]
        public IFileSystem FileSystem { get; set; }

        protected override Task<int> Execute(BakeCommandArguments arguments)
        {
            Tracing.Info("bake - transforming content into a website");

            var siteContext = Generator.BuildContext(arguments.Source, arguments.Destination, arguments.Drafts);

            if (arguments.CleanTarget && FileSystem.Directory.Exists(siteContext.OutputFolder))
            {
                FileSystem.Directory.Delete(siteContext.OutputFolder, true);
            }

            if (string.IsNullOrWhiteSpace(arguments.Template))
            {
                arguments.DetectFromDirectory(TemplateEngines.Engines, siteContext);
            }

            var renderingEngine = TemplateEngines[arguments.Template];
            var markdownEngine = TemplateEngines["markdown"];
            if (renderingEngine != null)
            {
                var watch = new Stopwatch();
                watch.Start();

                this.StaticContentHandler.Process(siteContext);
                markdownEngine.Initialize();
                markdownEngine.Process(siteContext);

                renderingEngine.Initialize();
                renderingEngine.Process(siteContext);
                foreach (var t in Transforms)
                    t.Transform(siteContext);

                renderingEngine.CompressSitemap(siteContext, FileSystem);

                watch.Stop();
                Tracing.Info("done - took {0}ms", watch.ElapsedMilliseconds);
            }
            else
            {
                Tracing.Info("Cannot find engine for input: '{0}'", arguments.Template);
                return Task.FromResult(1);
            }
            return Task.FromResult(0);
        }
    }
}
