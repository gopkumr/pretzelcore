using PretzelCore.Services.Templating.Context;

namespace PretzelCore.Services.Extensibility
{
    public interface ITransform
    {
        void Transform(SiteContext siteContext);
    }
}
