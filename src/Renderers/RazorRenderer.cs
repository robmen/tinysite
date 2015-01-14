using System;
using System.Collections.Generic;
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
        public RazorRenderer()
        {
            this.TopLevelTemplates = new Dictionary<string, LoadedTemplateSource>();

            var manager = new TemplateManager();
            manager.TopLevelTemplates = this.TopLevelTemplates;

            var config = new TemplateServiceConfiguration();
            config.AllowMissingPropertiesOnDynamic = true;
            config.Namespaces.Add("System.IO");
            config.Namespaces.Add("RazorEngine.Text");
            config.TemplateManager = manager;

            var service = RazorEngineService.Create(config);
            Engine.Razor = service;
        }

        private Dictionary<string, LoadedTemplateSource> TopLevelTemplates { get; set; }

        public string Render(string path, string template, object data)
        {
            lock (Engine.Razor)
            {
                LoadedTemplateSource loadedTemplate;

                if (!this.TopLevelTemplates.TryGetValue(path, out loadedTemplate))
                {
                    loadedTemplate = new LoadedTemplateSource(template, path);

                    this.TopLevelTemplates.Add(path, loadedTemplate);
                }

                try
                {
                    var result = Engine.Razor.RunCompile(loadedTemplate, path, null, data);

                    return result;
                }
                catch (TemplateCompilationException e)
                {
                    foreach (var error in e.CompilerErrors)
                    {
                        Console.Error.WriteLine(error.ErrorText);
                    }
                }

                return null;
            }
        }

        private class TemplateManager : ITemplateManager
        {
            public Dictionary<string, LoadedTemplateSource> TopLevelTemplates { get; set; }

            public void AddDynamic(ITemplateKey key, ITemplateSource source)
            {
            }

            public ITemplateKey GetKey(string name, ResolveType resolveType, ITemplateKey context)
            {
                return new NameOnlyTemplateKey(name, resolveType, context);
            }

            public ITemplateSource Resolve(ITemplateKey key)
            {
                var name = key.Name;

                LoadedTemplateSource loadedTemplate;

                if (!this.TopLevelTemplates.TryGetValue(name, out loadedTemplate))
                {
                    var id = Path.GetFileNameWithoutExtension(name);

                    var extension = Path.GetExtension(name).TrimStart('.');

                    var layout = RenderingTransaction.Current.Layouts[id];

                    if (!layout.Extension.Equals(String.IsNullOrEmpty(extension) ? "cshtml" : extension, StringComparison.OrdinalIgnoreCase))
                    {
                        // TODO: throw new exception.
                    }

                    loadedTemplate = new LoadedTemplateSource(layout.SourceContent, layout.SourcePath);

                    // Do not need to add this loaded template to our list of cached top level templates
                    // because RazorEngine will remember it for us and never ask again.
                }

                return loadedTemplate;
            }
        }
    }
}
