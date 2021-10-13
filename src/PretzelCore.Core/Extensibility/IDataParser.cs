using System;
using System.Linq;

namespace PretzelCore.Core.Extensibility
{
    internal interface IDataParser
    {
        string Extension { get; }

        bool CanParse(string folder, string method);

        dynamic Parse(string folder, string method);
    }
}
