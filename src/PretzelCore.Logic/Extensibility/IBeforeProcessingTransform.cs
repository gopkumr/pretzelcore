using PretzelCore.Services.Templating.Context;

namespace PretzelCore.Services.Extensibility
{
    public interface IBeforeProcessingTransform
    {
        void Transform(SiteContext context);
    }
}
