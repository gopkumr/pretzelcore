using Microsoft.Extensions.FileProviders;
using PretzelCore.Core.Extensibility;
using PretzelCore.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Reflection;

namespace PretzelCore.Services.Recipes
{
    public class Recipe
    {
        public Recipe(IFileSystem fileSystem, string engine, string directory, IEnumerable<IAdditionalIngredient> additionalIngredients)
        {
            this.fileSystem = fileSystem;
            this.engine = engine;
            this.directory = FixDirectoryPath(directory);
            this.additionalIngredients = additionalIngredients;
        }

        private readonly IFileSystem fileSystem;
        private readonly string engine;
        private readonly string directory;
        private readonly IEnumerable<IAdditionalIngredient> additionalIngredients;

        public void Create()
        {
            try
            {
                if (!fileSystem.Directory.Exists(directory))
                    fileSystem.Directory.CreateDirectory(directory);

                if(fileSystem.DirectoryInfo.FromDirectoryName(directory).GetFiles().Length>0
                    || fileSystem.DirectoryInfo.FromDirectoryName(directory).GetDirectories().Length > 0)
                {
                    Tracing.Info("Destination directory is not empty. Please clear the content and try again.");
                    return;
                }

                var defaultTemplate = string.Empty;
                if (string.Equals("razor", engine, StringComparison.InvariantCultureIgnoreCase))
                    defaultTemplate = "Resources.Razor.RazorTemplate.zip";
                else if (string.Equals("liquid", engine, StringComparison.InvariantCultureIgnoreCase))
                    defaultTemplate = "Resources.Liquid.LiquidTemplate.zip";
                else
                {
                    Tracing.Info("Templating Engine not found");
                    return;
                }

                var embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
                using (var reader = embeddedProvider.GetFileInfo(defaultTemplate).CreateReadStream())
                {
                    using ZipArchive archive = new ZipArchive(reader);
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string destinationPath = Path.GetFullPath(Path.Combine(directory, entry.FullName));
                        if (destinationPath.StartsWith(directory, StringComparison.Ordinal) && !destinationPath.EndsWith("\\"))
                            entry.ExtractToFile(destinationPath, true);
                        else if (!Directory.Exists(destinationPath))
                            Directory.CreateDirectory(destinationPath);
                    }
                }
                Tracing.Info("Pretzel site template has been created");

                foreach (var additionalIngredient in additionalIngredients)
                {
                    additionalIngredient.MixIn(directory);
                }
            }
            catch (Exception ex)
            {
                Tracing.Error("Error trying to create template: {0}", ex);
            }
        }

        // Ensures that the last character on the extraction path
        // is the directory separator char.
        // Without this, a malicious zip file could try to traverse outside of the expected
        // extraction path.
        private string FixDirectoryPath(string directory)
        {
            if (!directory.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                directory += Path.DirectorySeparatorChar;

            return directory;
        }
    }
}
