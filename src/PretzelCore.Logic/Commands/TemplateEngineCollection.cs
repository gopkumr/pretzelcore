using PretzelCore.Core.Extensibility;
using PretzelCore.Core.Templating;
using System.Collections.Generic;
using System.Composition;

namespace PretzelCore.Services.Commands
{
    [Export]
    [Shared]
    public sealed class TemplateEngineCollection
    {
        [ImportMany]
#pragma warning disable 649
        public ExportFactory<ISiteEngine, SiteEngineInfoAttribute>[] templateEngineMap { get; set; }
#pragma warning restore 649

        public Dictionary<string, ISiteEngine> Engines { get; private set; }

        public ISiteEngine this[string name]
        {
            get
            {
                ISiteEngine engine;
                Engines.TryGetValue(name.ToLower(System.Globalization.CultureInfo.InvariantCulture), out engine);
                return engine;
            }
        }

        [OnImportsSatisfied]
        public void OnImportsSatisfied()
        {
            Engines = new Dictionary<string, ISiteEngine>(templateEngineMap.Length);

            foreach (var command in templateEngineMap)
            {
                if (!Engines.ContainsKey(command.Metadata.Engine))
                    Engines.Add(command.Metadata.Engine, command.CreateExport().Value);
            }
        }
    }
}
