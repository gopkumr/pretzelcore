using PretzelCore.Services.Templating.Context;

namespace PretzelCore.Services.Templating
{
    public interface ISiteEngine
    {
        void Initialize();

        bool CanProcess(SiteContext context);

        void Process(SiteContext context, bool skipFileOnError = false);
    }
}
