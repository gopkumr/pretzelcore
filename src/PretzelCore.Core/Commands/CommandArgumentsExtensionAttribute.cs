using PretzelCore.Core.Extensions;
using System;
using System.Composition;

namespace PretzelCore.Core.Commands
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CommandArgumentsExtensionAttribute : ExportAttribute
    {
        public Type[] CommandTypes { get; set; }

        public CommandArgumentsExtensionAttribute() : base(typeof(ICommandArgumentsExtension))
        {
        }
    }
}
