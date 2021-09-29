using System;
using System.Linq;

namespace PretzelCore.Services.Templating.Context.DataParsing
{
    internal interface IDataParser
    {
        string Extension { get; }

        bool CanParse(string folder, string method);

        object Parse(string folder, string method);
    }
}
