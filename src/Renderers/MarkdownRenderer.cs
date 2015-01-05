using MarkdownSharp;
using TinySite.Rendering;

namespace TinySite.Renderers
{
    [Render("md")]
    [Render("markdown")]
    public class MarkdownRenderer : IRenderer
    {
        public MarkdownRenderer()
        {
            this.MarkdownEngine = new Markdown(new MarkdownOptions() { AutoHyperlink = true });
        }

        private Markdown MarkdownEngine { get; set; }

        public string Render(string template, object data)
        {
            lock (this.MarkdownEngine)
            {
                var result = this.MarkdownEngine.Transform(template).Trim();

                return result;
            }
        }
    }
}
