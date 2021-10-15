using PretzelCore.Core.Commands;
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

        [Import]
        public PretzelSiteGenerator SiteGenerator { get; set; }

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

                SiteGenerator.SetSourceContentEngine(markdownEngine)
                             .SetTemplatingEngine(renderingEngine)
                             .GenerateSite(siteContext);

                watch.Stop();
                Tracing.Info("done - took {0}ms", watch.ElapsedMilliseconds);
            }
            else
            {
                Tracing.Error("Cannot find engine for input: '{0}'", arguments.Template);
                return Task.FromResult(1);
            }
            return Task.FromResult(0);
        }
    }
}
