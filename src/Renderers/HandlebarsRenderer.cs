using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FuManchu;
using TinySite.Models;
using TinySite.Rendering;

namespace TinySite.Renderers
{
    [Render("hb")]
    [Render("handlebar")]
    [Render("handlebars")]
    public class HandlebarsRenderer : IRenderer
    {
        private readonly object _renderLock = new object();

        private readonly ConcurrentDictionary<string, HandlebarTemplate> _compiledTemplates = new ConcurrentDictionary<string, HandlebarTemplate>();

        public string Render(SourceFile sourceFile, string template, object data)
        {
            var path = sourceFile.SourcePath;

            lock (_renderLock)
            {
                try
                {
                    var compiledTemplate = _compiledTemplates.GetOrAdd(path, key =>
                    {
                       return Handlebars.Compile(path, template);
                    });

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
            lock (_renderLock)
            {
                foreach (var path in paths)
                {
                    _compiledTemplates.TryRemove(path, out _);
                }
            }
        }
    }
}
