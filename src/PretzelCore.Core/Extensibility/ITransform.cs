using PretzelCore.Core.Templating.Context;

namespace PretzelCore.Core.Extensibility
{
    public interface ITransform
    {
        void Transform(SiteContext siteContext);
    }
}
