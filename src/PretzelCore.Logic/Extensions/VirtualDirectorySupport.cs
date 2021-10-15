using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using PretzelCore.Core.Commands;
using PretzelCore.Core.Extensibility;
using PretzelCore.Core.Extensions;
using PretzelCore.Core.Templating.Context;
using PretzelCore.Services.Commands;

namespace PretzelCore.Services.Extensions
{
    [Export]
    [Shared]
    [CommandArgumentsExtension(CommandTypes = new[] { typeof(BakeCommand), typeof(TasteCommand) })]
    public class VirtualDirectorySupportArguments : ICommandArgumentsExtension
    {
        public IList<Option> Options { get; } = new[]
        {
            new Option(new [] { "--virtualdirectory", "-vDir" }, "Rewrite url's to work inside the specified virtual directory", argumentType: typeof(string))
        };

        public void BindingCompleted()
        {
            //Not used
        }

        public string VirtualDirectory { get; set; }

    }

    [Export(typeof(IPlugin))]
    public class VirtualDirectorySupport : IPlugin
    {
        readonly IFileSystem fileSystem;

        [ImportingConstructor]
        public VirtualDirectorySupport(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        [Import]
        public VirtualDirectorySupportArguments Arguments { get; set; }

        public string ContentTransform(string file, string content)
        {
            return content;
        }

        public void PreProcessingTransform(SiteContext context)
        {
            //Ignore
        }

        public void PostProcessingTransform(SiteContext siteContext)
        {
            if (string.IsNullOrEmpty(Arguments.VirtualDirectory)) return;

            var href = new Regex("href=\"(?<url>/.*?)\"", RegexOptions.Compiled);
            var src = new Regex("src=\"(?<url>/.*?)\"", RegexOptions.Compiled);
            var hrefReplacement = string.Format("href=\"/{0}${{url}}\"", Arguments.VirtualDirectory);
            var srcReplacement = string.Format("src=\"/{0}${{url}}\"", Arguments.VirtualDirectory);

            foreach (var page in siteContext.Pages.Where(p => p.OutputFile.EndsWith(".html") || p.OutputFile.EndsWith(".htm") || p.OutputFile.EndsWith(".css")))
            {
                var fileContents = fileSystem.File.ReadAllText(page.OutputFile);

                var processedContents = href.Replace(fileContents, hrefReplacement);
                processedContents = src.Replace(processedContents, srcReplacement);

                if (fileContents != processedContents)
                {
                    fileSystem.File.WriteAllText(page.OutputFile, processedContents);
                }
            }
        }
    }
}
