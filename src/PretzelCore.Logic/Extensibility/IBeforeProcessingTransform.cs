using PretzelCore.Core.Templating.Context;

namespace PretzelCore.Services.Extensibility
{
    public interface IBeforeProcessingTransform
    {
        void Transform(SiteContext context);
    }
}
