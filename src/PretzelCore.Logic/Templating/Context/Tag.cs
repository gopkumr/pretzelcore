using DotLiquid;
using System.Collections.Generic;

namespace PretzelCore.Services.Templating.Context
{
    public class Tag : Drop
    {
        public IEnumerable<Page> Posts { get; set; }

        public string Name { get; set; }

        public Tag()
        {
            Posts = new List<Page>();
        }
    }
}
