using System.Collections.Generic;
using DotLiquid;

namespace PretzelCore.Services.Templating.Context
{
    public class Category : Drop
    {
        public IEnumerable<Page> Posts { get; set; }
        public string Name { get; set; }
    }
}
