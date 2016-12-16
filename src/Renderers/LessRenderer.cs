using System.Collections.Generic;
using dotless.Core;
using TinySite.Models;
using TinySite.Rendering;

namespace TinySite.Renderers
{
    [Render("less")]
    public class LessRenderer : IRenderer
    {
        public string Render(SourceFile sourceFile, string template, object data)
        {
            var result = Less.Parse(template);

            return result;
        }

        public void Unload(IEnumerable<string> paths)
        {
        }
    }
}
