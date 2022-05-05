using PretzelCore.Core.Commands;
using PretzelCore.Core.Extensibility;
using PretzelCore.Core.Extensions;
using PretzelCore.Core.Telemetry;
using PretzelCore.Services.Recipes;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace PretzelCore.Services.Commands
{
    [Shared]
    [Export]
    [CommandArguments]
    public class IngredientCommandArguments : PretzelBaseCommandArguments
    {
        [ImportingConstructor]
        public IngredientCommandArguments(IFileSystem fileSystem) : base(fileSystem) { }

        protected override IEnumerable<Option> CreateOptions() => base.CreateOptions().Concat(new[]
        {
            new Option(new [] { "--newposttitle", "-n" }, description: "The title of the new post (\"New post\" by default", getDefaultValue: () =>  "New post")
        });

        public string NewPostTitle { get; set; }
    }

    [Shared]
    [CommandInfo(
        Name = "ingredient",
        Description = "create a new post",
        ArgumentsType = typeof(IngredientCommandArguments),
        CommandType = typeof(IngredientCommand)
        )]
    public sealed class IngredientCommand : Command<IngredientCommandArguments>
    {
        [Import]
        public IFileSystem FileSystem { get; set; }

        protected override Task<int> Execute(IngredientCommandArguments arguments)
        {
            Tracing.Info("ingredient - create a new post");

            var ingredient = new Ingredient(FileSystem, arguments.NewPostTitle, arguments.Source, arguments.Drafts);
            ingredient.Create();

            return Task.FromResult(0);
        }
    }
}