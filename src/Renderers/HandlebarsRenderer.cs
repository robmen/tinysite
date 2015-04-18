using System;
using System.Collections.Generic;
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

        private Dictionary<string, Func<object, string>> compiledTemplates = new Dictionary<string, Func<object, string>>();

        public string Render(string path, string template, object data)
        {
            lock (sync)
            {
                try
                {
                    Func<object, string> compiledTemplate;

                    if (!this.compiledTemplates.TryGetValue(path, out compiledTemplate))
                    {
                        compiledTemplate = Handlebars.Compile(path, template);

                        this.compiledTemplates.Add(path, compiledTemplate);
                    }

                    var result = compiledTemplate(data);

                    return result;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Handelbars failure while processing: {0}, error: {1}", path, e.Message);
                }

                return null;
            }
        }

        public void Unload(IEnumerable<string> paths)
        {
            lock (sync)
            {
                foreach (var path in paths)
                {
                    this.compiledTemplates.Remove(path);
                }
            }
        }
    }
}
