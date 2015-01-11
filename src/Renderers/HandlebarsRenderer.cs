using FuManchu;
using TinySite.Rendering;

namespace TinySite.Renderers
{
    [Render("hb")]
    [Render("handlebar")]
    [Render("handlebars")]
    public class HandlebarsRenderer : IRenderer
    {
        private object sync = new object();

        public string Render(string path, string template, object data)
        {
            lock (sync)
            {
                var result = Handlebars.CompileAndRun(path, template, data);

                return result;
            }
        }
    }
}
