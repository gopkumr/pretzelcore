using PretzelCore.Core.Templating.Context;

namespace PretzelCore.Core.Extensibility
{
    public interface ISiteGenerator
    {
        ISiteGenerator SetTemplatingEngine(ISiteEngine engine);

        ISiteGenerator SetSourceContentEngine(ISiteEngine engine);

        void GenerateSite(SiteContext siteContext);
    }
}
