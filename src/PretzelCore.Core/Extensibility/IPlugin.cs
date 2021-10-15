using PretzelCore.Core.Templating.Context;

namespace PretzelCore.Core.Extensibility
{
    public interface IPlugin
    {
        /// <summary>
        /// Trasforms the content passed in 
        /// </summary>
        /// <param name="content">Raw contents from the source file</param>
        /// <param name="file">file for which the transformation is being executed</param>
        /// <returns>Transformed content</returns>
        string ContentTransform(string file, string content);

        /// <summary>
        /// Transforms the generated site using the information about the site passed in.
        /// </summary>
        /// <param name="siteContext">Context of the current generated site</param>
        void PostProcessingTransform(SiteContext siteContext);

        /// <summary>
        /// Transforms the site context before the site generation process starts.
        /// </summary>
        /// <param name="siteContext">Context of the current generated site</param>
        void PreProcessingTransform(SiteContext context);
    }
}
