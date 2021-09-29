using PretzelCore.Core.Commands.Interfaces;
using System;
using System.Composition;

namespace PretzelCore.Core.Commands
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CommandArgumentsAttribute : ExportAttribute
    {
        public CommandArgumentsAttribute() : base(typeof(ICommandArguments))
        {
        }
    }
}
