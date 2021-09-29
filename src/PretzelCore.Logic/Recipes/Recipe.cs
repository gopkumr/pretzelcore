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
        public Recipe(IFileSystem fileSystem, string engine, string directory, IEnumerable<IAdditionalIngredient> additionalIngredients, bool withProject, bool wiki, bool withDrafts = false)
        {
            this.fileSystem = fileSystem;
            this.engine = engine;

            this.directory = FixDirectoryPath(directory);
            this.additionalIngredients = additionalIngredients;
            this.withProject = withProject;
            this.wiki = wiki;
            this.withDrafts = withDrafts;
        }

        private readonly IFileSystem fileSystem;
        private readonly string engine;
        private readonly string directory;
        private readonly IEnumerable<IAdditionalIngredient> additionalIngredients;
        private readonly bool withProject;
        private readonly bool wiki;
        private readonly bool withDrafts;

        public void Create()
        {
            try
            {
                if (!fileSystem.Directory.Exists(directory))
                    fileSystem.Directory.CreateDirectory(directory);
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

        private void CreateProject()
        {
            var layoutDirectory = Path.Combine(directory, "_layouts");
            fileSystem.Directory.CreateDirectory(Path.Combine(layoutDirectory, @"Properties"));
            fileSystem.Directory.CreateDirectory(Path.Combine(layoutDirectory, @"PretzelClasses"));
            fileSystem.Directory.CreateDirectory(Path.Combine(layoutDirectory, @".nuget"));

            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"Properties", "AssemblyInfo.cs"), Services.Properties.RazorCsProject.AssemblyInfo_cs);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"PretzelClasses", "Category.cs"), Services.Properties.RazorCsProject.Category_cs);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"LayoutProject.csproj"), Services.Properties.RazorCsProject.LayoutProject_csproj);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"layoutSolution.sln"), Services.Properties.RazorCsProject.LayoutSolution_sln);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"PretzelClasses", "NonProcessedPage.cs"), Services.Properties.RazorCsProject.NonProcessedPage_cs);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @".nuget", "NuGet.config"), Services.Properties.RazorCsProject.NuGet_Config);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @".nuget", "NuGet.exe"), Services.Properties.RazorCsProject.NuGet_exe);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @".nuget", "NuGet.targets"), Services.Properties.RazorCsProject.NuGet_targets);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"PretzelClasses", "PageContext.cs"), Services.Properties.RazorCsProject.PageContext_cs);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"PretzelClasses", "Page.cs"), Services.Properties.RazorCsProject.Page_cs);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"PretzelClasses", "Paginator.cs"), Services.Properties.RazorCsProject.Paginator_cs);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"PretzelClasses", "SiteContext.cs"), Services.Properties.RazorCsProject.SiteContext_cs);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"PretzelClasses", "Tag.cs"), Services.Properties.RazorCsProject.Tag_cs);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"web.config"), Services.Properties.RazorCsProject.Web_config);
            fileSystem.File.WriteAllBytes(Path.Combine(layoutDirectory, @"packages.config"),
                                          Services.Properties.RazorCsProject.packages_config);
        }

        private void CreateImages()
        {
            CreateImage(@"Resources\25.png", directory, @"img", "25.png");
            CreateImage(@"Resources\favicon.png", directory, @"img", "favicon.png");
            CreateImage(@"Resources\logo.png", directory, @"img", "logo.png");

            CreateFavicon();
        }

        private void CreateFavicon()
        {
            CreateImage(@"Resources\favicon.ico", directory, @"img", "favicon.ico");
        }

        private void CreateImage(string resourceName, params string[] pathSegments)
        {
            using (var ms = new MemoryStream())
            using (var resourceStream = GetResourceStream(resourceName))
            {
                resourceStream.CopyTo(ms);
                fileSystem.File.WriteAllBytes(Path.Combine(pathSegments), ms.ToArray());
            }
        }

        //https://github.com/dotnet/corefx/issues/12565
        private Stream GetResourceStream(string path)
        {
            var assembly = GetType().Assembly;
            var name = GetType().Assembly.GetName().Name;

            path = path.Replace("/", ".").Replace("\\", ".");

            var fullPath = $"{name}.{path}";
            var stream = assembly.GetManifestResourceStream(fullPath);

            return stream;
        }

        private void CreateDirectories()
        {
            fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"_posts"));
            fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"_layouts"));
            fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"_includes"));
            fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"css"));
            fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"img"));
            if (withDrafts)
            {
                fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"_drafts"));
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
