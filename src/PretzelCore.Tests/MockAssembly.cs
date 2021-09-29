using PretzelCore.Services.Extensions;

namespace Pretzel.Tests
{
    public class MockAssembly : IAssembly
    {
        public string EntryAssemblyLocation { get; set; }

        public string GetEntryAssemblyLocation()
        {
            return EntryAssemblyLocation;
        }
    }
}
