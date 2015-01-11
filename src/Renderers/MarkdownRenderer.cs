using MarkdownDeep;
using TinySite.Rendering;

namespace TinySite.Renderers
{
    [Render("md")]
    [Render("markdown")]
    public class MarkdownRenderer : IRenderer
    {
        public MarkdownRenderer()
        {
            this.MarkdownEngine = new Markdown();
            this.MarkdownEngine.ExtraMode = true;
            this.MarkdownEngine.SafeMode = false;
        }

        private Markdown MarkdownEngine { get; set; }

        public string Render(string path, string template, object data)
        {
            lock (this.MarkdownEngine)
            {
                var result = this.MarkdownEngine.Transform(template).Trim();

                return result;
            }
        }
    }
}
