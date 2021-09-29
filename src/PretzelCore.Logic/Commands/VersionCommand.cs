using PretzelCore.Core.Commands;
using PretzelCore.Core.Commands.Interfaces;
using PretzelCore.Core.Extensions;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.Reflection;
using System.Threading.Tasks;

namespace PretzelCore.Services.Commands
{
    [Shared]
    [Export]
    [CommandArguments]
    public class VersionCommandArguments : BaseCommandArguments
    {
        protected override IEnumerable<Option> CreateOptions() => Array.Empty<Option>();
    }

    [Shared]
    [CommandInfo(
        Name = "version",
        Description = "display current Pretzel version",
        ArgumentsType = typeof(VersionCommandArguments),
        CommandType = typeof(VersionCommand)
        )]
    public sealed class VersionCommand : Command<VersionCommandArguments>
    {
        protected override Task<int> Execute(VersionCommandArguments arguments)
        {
            Tracing.Info("V{0}", Assembly.GetExecutingAssembly().GetName().Version);

            return Task.FromResult(0);
        }
    }
}
