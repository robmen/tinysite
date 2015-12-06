using System.Collections.Generic;
using TinySite.Models;
using TinySite.Rendering;

namespace TinySite.Renderers
{
    [Render("mustache")]
    public class MustacheRenderer : IRenderer
    {
        public string Render(SourceFile sourceFile, string template, object data)
        {
            var result = Nustache.Core.Render.StringToString(template, data);

            return result;
        }

        public void Unload(IEnumerable<string> paths)
        {
        }
    }
}
