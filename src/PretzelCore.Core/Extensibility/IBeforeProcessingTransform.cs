using PretzelCore.Core.Templating.Context;

namespace PretzelCore.Core.Extensibility
{
    public interface IBeforeProcessingTransform
    {
        void Transform(SiteContext context);
    }
}
