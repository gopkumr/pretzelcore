using PretzelCore.Core.Extensibility;
using PretzelCore.Core.Templating.Context.DataParsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace PretzelCore.Core.Templating.Context
{
    public class Data 
    {
        private readonly IFileSystem fileSystem;
        private readonly string dataDirectory;
        private readonly Dictionary<string, Lazy<object>> cachedResults = new Dictionary<string, Lazy<object>>();
        private readonly IList<IDataParser> dataParsers;

        public Data(IFileSystem fileSystem, string dataDirectory)
        {
            this.fileSystem = fileSystem;
            this.dataDirectory = dataDirectory;
            dataParsers = new List<IDataParser>
            {
                new YamlJsonDataParser(fileSystem, "yml"),
                new YamlJsonDataParser(fileSystem, "json"),
                new CsvTsvDataParser(fileSystem, "csv"),
                new CsvTsvDataParser(fileSystem, "tsv", "\t")
            };
        }

        public dynamic this[object method]
        {
            get
            {
                if (!cachedResults.ContainsKey(method.ToString()))
                {
                    var cachedResult = new Lazy<object>(() =>
                    {
                        if (!fileSystem.Directory.Exists(dataDirectory))
                        {
                            return null;
                        }

                        var methodName = method.ToString();
                        foreach (var dataParser in dataParsers)
                        {
                            if (dataParser.CanParse(dataDirectory, methodName))
                            {
                                return dataParser.Parse(dataDirectory, methodName);
                            }
                        }

                        var subFolder = Path.Combine(dataDirectory, method.ToString());
                        if (fileSystem.Directory.Exists(subFolder))
                        {
                            return new Data(fileSystem, subFolder);
                        }

                        return null;
                    });
                    cachedResults[method.ToString()] = cachedResult;
                    return cachedResult.Value;
                }

                return cachedResults[method.ToString()].Value;
            }
        }
    }

}
