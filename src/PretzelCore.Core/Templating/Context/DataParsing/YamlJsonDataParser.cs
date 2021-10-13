using PretzelCore.Core.Extensions;
using System.IO;
using System.IO.Abstractions;
using YamlDotNet.RepresentationModel;

namespace PretzelCore.Core.Templating.Context.DataParsing
{
    internal class YamlJsonDataParser : AbstractDataParser
    {
        internal YamlJsonDataParser(IFileSystem fileSystem, string extension) : base(fileSystem, extension)
        {

        }

        public override dynamic Parse(string folder, string method)
        {
            var text = FileSystem.File.ReadAllText(BuildFilePath(folder, method));

            var input = new StringReader(text);

            var yaml = new YamlStream();
            yaml.Load(input);

            if (yaml.Documents.Count == 0)
            {
                return null;
            }

            var root = yaml.Documents[0].RootNode;
            if (root is YamlSequenceNode seq)
            {
                return seq;
            }

            return text.ParseYaml();
        }
    }
}
