using NUglify;
using PretzelCore.Core.Extensions;
using PretzelCore.Core.Telemetry;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace PretzelCore.Services.Minification
{
    public class JsMinifier
    {
        private readonly IFileSystem fileSystem;
        private readonly IEnumerable<FileInfo> files;
        private readonly string outputPath;

        public JsMinifier(IFileSystem fileSystem, IEnumerable<FileInfo> files, string outputPath)
        {
            this.files = files;
            this.outputPath = outputPath;
            this.fileSystem = fileSystem;
        }

        public void Minify()
        {
            var content = fileSystem.BundleFiles(files);
            var minified = Uglify.Js(content);
            if (minified.HasErrors)
            {
                foreach (var error in minified.Errors)
                {
                    Tracing.Error(error.ToString());
                }
            }
            fileSystem.File.WriteAllText(outputPath, minified.Code);
        }
    }
}
