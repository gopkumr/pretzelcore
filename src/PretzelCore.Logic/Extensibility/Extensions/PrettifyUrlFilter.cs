using System;
using System.Composition;

namespace PretzelCore.Core.Extensibility.Extensions
{
    [Export(typeof(IFilter))]
    public class PrettifyUrlFilter : IFilter
    {
        public string Name
        {
            get { return "PrettifyUrl"; }
        }

        public static string PrettifyUrl(string input)
        {
            return input.Replace("index.html", string.Empty);
        }
    }
}
