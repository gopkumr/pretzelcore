using System;
using System.Collections.Generic;
using System.Text;

namespace PretzelCore.Core.Configuration.Interfaces
{
    public interface IConfiguration
    {
        object this[string key] { get; }

        bool ContainsKey(string key);

        bool TryGetValue(string key, out object value);

        IDictionary<string, object> ToDictionary();

        IDefaultsConfiguration Defaults { get; }

        void ReadFromFile(string path);
    }

    public interface IDefaultsConfiguration
    {
        IDictionary<string, object> ForScope(string path);
    }

    public interface ISourcePathProvider
    {
        string Source { get; }
    }
}
