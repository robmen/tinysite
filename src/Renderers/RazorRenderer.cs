using RazorEngine;
using TinySite.Rendering;

namespace TinySite.Renderers
{
    [Render("cshtml")]
    [Render("razor")]
    public class RazorRenderer : IRenderer
    {
        public string Render(string template, object data)
        {
            var cacheName = template.GetHashCode().ToString();

            var result = Razor.Parse(template, data, cacheName);

            return result;
        }
    }
}
