using System.Collections.Generic;
using TinySite.Rendering;

namespace TinySite.Renderers
{
    [Render("mustache")]
    public class MustacheRenderer : IRenderer
    {
        public string Render(string path, string template, object data)
        {
            var result = Nustache.Core.Render.StringToString(template, data);

            return result;
        }

        public void Unload(IEnumerable<string> paths)
        {
        }
    }
}
