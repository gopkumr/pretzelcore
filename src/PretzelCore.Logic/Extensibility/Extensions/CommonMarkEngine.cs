using System.Composition;

namespace PretzelCore.Core.Extensibility.Extensions
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
