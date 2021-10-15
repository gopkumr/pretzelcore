using PretzelCore.Core.Templating.Context;

namespace PretzelCore.Core.Extensibility
{
    public class AbstractPlugin : IPlugin
    {
        public virtual string ContentTransform(string file, string content)
        {
            return content;
        }

        public virtual void PostProcessingTransform(SiteContext siteContext)
        {
            //ignore
        }

        public virtual void PreProcessingTransform(SiteContext context)
        {
            //ignore
        }
    }
}
