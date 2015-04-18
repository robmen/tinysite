using System.Collections.Generic;
using dotless.Core;
using TinySite.Rendering;

namespace TinySite.Renderers
{
    [Render("less")]
    public class LessRenderer : IRenderer
    {
        private object sync = new object();

        public string Render(string path, string template, object data)
        {
            lock (sync)
            {
                var result = Less.Parse(template);

                return result;
            }
        }

        public void Unload(IEnumerable<string> paths)
        {
        }
    }
}
