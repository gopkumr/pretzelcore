using PretzelCore.Core.Extensibility;
using System;
using System.Composition;

namespace PretzelCore.Services.Extensions
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
