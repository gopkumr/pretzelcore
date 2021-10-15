using PretzelCore.Core.Templating.Context;

namespace PretzelCore.Core.Extensibility
{
    /// <summary>
    /// Extensions used to trasform the content of the source file before
    /// the content is passsed through html conversion.
    /// </summary>
    public interface IContentTransform
    {
        /// <summary>
        /// Trasforms the content passed in 
        /// </summary>
        /// <param name="content">Raw contents from the source file</param>
        /// <param name="file">file for which the transformation is being executed</param>
        /// <returns>Transformed content</returns>
        string Transform(string file, string content);
    }
}
