using PretzelCore.Core.Templating.Context;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.IO.Abstractions;
using System.Text;

namespace PretzelCore.Services.Templating
{
    [Export]
    public class StaticContentHandler
    {
        [Import]
        public IFileSystem FileSystem { get; set; }

        SiteContext _context;

        public void Process(SiteContext context, bool skipFileOnError = false)
        {
            _context = context;

            for (int index = 0; index < context.Posts.Count; index++)
            {
                var p = context.Posts[index];
                ProcessFile(context.OutputFolder, p, skipFileOnError, p.Filepath);
            }

            for (int index = 0; index < context.Pages.Count; index++)
            {
                var p = context.Pages[index];
                ProcessFile(context.OutputFolder, p, skipFileOnError);
            }
        }

        private void ProcessFile(string outputDirectory, Page page, bool skipFileOnError, string relativePath = "")
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                relativePath = MapToOutputPath(page.File);

            page.OutputFile = Path.Combine(outputDirectory, relativePath);
            var extension = Path.GetExtension(page.File);

            if (extension.IsImageFormat())
            {
                CreateOutputDirectory(page.OutputFile);
                CopyFileIfSourceNewer(page.File, page.OutputFile, true);
                return;
            }

            if (page is NonProcessedPage)
            {
                CreateOutputDirectory(page.OutputFile);
                CopyFileIfSourceNewer(page.File, page.OutputFile, true);
                return;
            }
        }

        public void CopyFileIfSourceNewer(string sourceFileName, string destFileName, bool overwrite)
        {
            if (!FileSystem.File.Exists(destFileName) ||
                FileSystem.File.GetLastWriteTime(sourceFileName) > FileSystem.File.GetLastWriteTime(destFileName))
            {
                FileSystem.File.Copy(sourceFileName, destFileName, overwrite);
            }
        }

        private void CreateOutputDirectory(string outputFile)
        {
            var directory = Path.GetDirectoryName(outputFile);
            if (!FileSystem.Directory.Exists(directory))
                FileSystem.Directory.CreateDirectory(directory);
        }
        private string MapToOutputPath(string file)
        {
            var temp = file.Replace(_context.SourceFolder, "")
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return temp;
        }
    }
}
