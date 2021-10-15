using PretzelCore.Core.Extensibility;
using PretzelCore.Core.Extensions;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Composition;
using System.Linq;

namespace PretzelCore.Core.Commands
{
    public abstract class BaseCommandArguments : ICommandArguments
    {
        readonly List<Option> options = new List<Option>();
        [Export]
        public IList<Option> Options => options;
        public IList<ICommandArgumentsExtension> Extensions { get; } = new List<ICommandArgumentsExtension>();

        public void BuildOptions()
        {
            options.AddRange(CreateOptions());
        }

        protected abstract IEnumerable<Option> CreateOptions();

        public virtual void BindingCompleted()
        {
        }
    }
}
