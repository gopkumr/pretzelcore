using PretzelCore.Core.Templating.Context;

namespace PretzelCore.Core.Extensibility
{
    public interface ISiteEngine
    {
        void Initialize();

        bool CanProcess(SiteContext context);

        void Process(SiteContext context, bool skipFileOnError = false);
    }
}
