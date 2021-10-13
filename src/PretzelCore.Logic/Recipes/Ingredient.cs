using PretzelCore.Core.Extensions;
using PretzelCore.Core.Telemetry;
using PretzelCore.Services.Extensions;
using System;
using System.IO.Abstractions;

namespace PretzelCore.Services.Recipes
{
    public class Ingredient
    {
        private readonly string directory;

        private readonly IFileSystem fileSystem;

        private readonly string title;

        private readonly bool withDrafts;

        public Ingredient(IFileSystem fileSystem, string title, string directory, bool withDrafts)
        {
            this.fileSystem = fileSystem;
            this.title = title;
            this.directory = directory;
            this.withDrafts = withDrafts;
        }

        public void Create()
        {
            var postPath = fileSystem.Path.Combine(directory, !withDrafts ? @"_posts" : @"_drafts");

            var postName = string.Format("{0}-{1}.md", DateTime.Today.ToString("yyyy-MM-dd"), SlugifyFilter.Slugify(title));
            var pageContents = string.Format("---\r\n layout: post \r\n title: {0}\r\n comments: true\r\n---\r\n", title);

            if (!fileSystem.Directory.Exists(postPath))
            {
                Tracing.Info("{0} folder not found", postPath);
                return;
            }

            if (fileSystem.File.Exists(fileSystem.Path.Combine(postPath, postName)))
            {
                Tracing.Info("The \"{0}\" file already exists", postName);
                return;
            }

            fileSystem.File.WriteAllText(fileSystem.Path.Combine(postPath, postName), pageContents);

            Tracing.Info("Created the \"{0}\" post ({1})", title, postName);
        }
    }
}
