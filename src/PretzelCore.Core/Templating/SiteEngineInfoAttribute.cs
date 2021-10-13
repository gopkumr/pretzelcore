using PretzelCore.Core.Extensibility;
using System;
using System.Composition;

namespace PretzelCore.Core.Templating
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SiteEngineInfoAttribute : ExportAttribute
    {
        public string Engine { get; set; }

        public SiteEngineInfoAttribute() : base(typeof(ISiteEngine))
        {
        }
    }
}
