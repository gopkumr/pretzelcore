using System;

namespace PretzelCore.Core.Modules
{
    public interface IFileSystemWatcher
    {
        void OnChange(string path, Action<string> fileChangedCallback);
    }
}
