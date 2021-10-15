using PretzelCore.Core.Templating.Context;

namespace PretzelCore.Core.Extensibility
{
    /// <summary>
    /// Extensions that can be used to tranform the generated site after all
    /// the generation process is completed
    /// </summary>
    public interface ISiteTransform
    {
        /// <summary>
        /// Transforms the generated site using the information about the site passed in.
        /// </summary>
        /// <param name="siteContext">Context of the current generated site</param>
        void Transform(SiteContext siteContext);
    }
}
