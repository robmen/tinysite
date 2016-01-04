using System.Collections.Generic;
using CommonMark;
using TinySite.Models;
using TinySite.Rendering;

namespace TinySite.Renderers
{
    [Render("md")]
    [Render("markdown")]
    public class MarkdownRenderer : IRenderer
    {
        public MarkdownRenderer()
        {
        }

        public string Render(SourceFile sourceFile, string template, object data)
        {
            var result = CommonMarkConverter.Convert(template);
            
            return result.Trim();
        }

        public void Unload(IEnumerable<string> paths)
        {
        }
    }
}
