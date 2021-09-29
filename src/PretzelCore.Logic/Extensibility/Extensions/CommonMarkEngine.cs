

namespace PretzelCore.Core.Extensibility.Extensions
{
    internal class CommonMarkEngine : ILightweightMarkupEngine
    {
        public string Convert(string source)
        {
            return CommonMark.CommonMarkConverter.Convert(source);
        }
    }
}
