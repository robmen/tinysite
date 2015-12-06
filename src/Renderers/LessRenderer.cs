using System.Collections.Generic;
using dotless.Core;
using TinySite.Models;
using TinySite.Rendering;

namespace TinySite.Renderers
{
    [Render("less")]
    public class LessRenderer : IRenderer
    {
        private object _renderLock = new object();

        public string Render(SourceFile sourceFile, string template, object data)
        {
            lock (_renderLock)
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
