using PretzelCore.Core.Extensibility;
using System.Composition;

namespace PretzelCore.Services.Templating.Markdown
{
    [Export(typeof(ILightweightMarkupEngine))]
    internal class CommonMarkEngine : ILightweightMarkupEngine
    {
        public string Convert(string source)
        {
            return CommonMark.CommonMarkConverter.Convert(source);
        }
    }
}
