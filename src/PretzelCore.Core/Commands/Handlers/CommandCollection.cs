using System;
using System.CommandLine;
using System.Composition;
using System.Linq;
using PretzelCore.Core.Configuration;
using PretzelCore.Core.Extensibility;
using PretzelCore.Core.Configuration.Interfaces;
using PretzelCore.Core.Extensions;

namespace PretzelCore.Core.Commands.Handlers
{
    [Export]
    [Shared]
    public sealed class CommandCollection
    {
        ExportFactory<Extensibility.ICommand, CommandInfoAttribute>[] commands;
        [ImportMany]
        public ExportFactory<Extensibility.ICommand, CommandInfoAttribute>[] Commands
        {
            get => commands ?? Array.Empty<ExportFactory<Extensibility.ICommand, CommandInfoAttribute>>();
            set => commands = value;
        }

        ICommandArguments[] commandArguments;
        [ImportMany]
        public ICommandArguments[] CommandArguments
        {
            get => commandArguments ?? Array.Empty<ICommandArguments>();
            set => commandArguments = value;
        }

        ExportFactory<ICommandArgumentsExtension, CommandArgumentsExtensionAttribute>[] argumentExtensions;
        [ImportMany]
        public ExportFactory<ICommandArgumentsExtension, CommandArgumentsExtensionAttribute>[] ArgumentExtensions
        {
            get => argumentExtensions ?? Array.Empty<ExportFactory<ICommandArgumentsExtension, CommandArgumentsExtensionAttribute>>();
            set => argumentExtensions = value;
        }

        public RootCommand RootCommand { get; set; }

        [Import]
        public IConfiguration Configuration { get; set; }

        [OnImportsSatisfied]
        public void OnImportsSatisfied()
        {
            RootCommand = new RootCommand();

            foreach (var command in Commands)
            {
                var subCommand = new Command(command.Metadata.Name, command.Metadata.Description);

                var argument = CommandArguments.First(c => command.Metadata.ArgumentsType.IsAssignableFrom(c.GetType()));

                if (argument is BaseCommandArguments baseCommandArguments)
                {
                    baseCommandArguments.BuildOptions();
                }

                foreach (var argumentExtensionsExport in ArgumentExtensions.Where(a => a.Metadata.CommandTypes.Any(type => type == command.Metadata.CommandType)))
                {
                    var arugumentExtension = argumentExtensionsExport.CreateExport().Value;
                    argument.Extensions.Add(arugumentExtension);
                }

                foreach (var arugumentExtension in argument.Extensions)
                {
                    foreach (var option in arugumentExtension.Options)
                        argument.Options.Add(option);
                }

                foreach (var option in argument.Options)
                {
                    subCommand.AddOption(option);
                }

                subCommand.Handler = new PretzelCommandHandler(Configuration, argument, command);

                RootCommand.AddCommand(subCommand);
            }
        }
    }
}
