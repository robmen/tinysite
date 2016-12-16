using System.Collections.Generic;
using Markdig;
using TinySite.Models;
using TinySite.Rendering;

namespace TinySite.Renderers
{
    [Render("md")]
    [Render("markdown")]
    public class MarkdownRenderer : IRenderer
    {
        public string Render(SourceFile sourceFile, string template, object data)
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            var result = Markdown.ToHtml(template, pipeline);

            return result.Trim();
        }

        public void Unload(IEnumerable<string> paths)
        {
        }
    }
}
