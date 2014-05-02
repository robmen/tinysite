using System;
using System.IO;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using TinySite.Rendering;
using TinySite.Services;

namespace TinySite.Renderers
{
    [Render("cshtml")]
    public class RazorRenderer : IRenderer
    {
        public string Render(string template, object data)
        {
            var config = new TemplateServiceConfiguration() { Resolver = new TemplateResolver() };

            using (var service = new TemplateService(config))
            {
                Razor.SetTemplateService(service);

                var cacheName = template.GetHashCode().ToString();

                var result = Razor.Parse(template, data, cacheName);

                return result;
            }
        }

        private class TemplateResolver : ITemplateResolver
        {
            public string Resolve(string name)
            {
                var id = Path.GetFileNameWithoutExtension(name);

                var extension = Path.GetExtension(name).TrimStart('.');

                var layout = RenderingTransaction.Current.Layouts[id];

                if (!layout.Extension.Equals(String.IsNullOrEmpty(extension) ? "cshtml" : extension, StringComparison.OrdinalIgnoreCase))
                {
                    // TODO: throw new exception.
                }

                return layout.Content;
            }
        }
    }
}
