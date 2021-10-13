using System.Collections.Generic;

namespace PretzelCore.Core.Templating.Context
{
    public class Category 
    {
        public IEnumerable<Page> Posts { get; set; }
        public string Name { get; set; }
    }
}
