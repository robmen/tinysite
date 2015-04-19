using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CSharp.RuntimeBinder;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;
using TinySite.Rendering;
using TinySite.Services;

namespace TinySite.Renderers
{
    [Render("cshtml")]
    public class RazorRenderer : IRenderer
    {
        private object sync = new object();

        public RazorRenderer()
        {
            this.InitializeRazorEngine();
        }

        private Dictionary<string, LoadedTemplateSource> TopLevelTemplates { get; set; }

        public string Render(string path, string template, object data)
        {
            lock (sync)
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
                catch (TemplateParsingException e)
                {
                    Console.Error.WriteLine("{0}{1}", path, e.Message);
                }
                catch (TemplateCompilationException e)
                {
                    foreach (var error in e.CompilerErrors)
                    {
                        Console.Error.WriteLine("{0}({1},{2}): {3} {4}: {5}", error.FileName, error.Line, error.Column, error.IsWarning ? "warning" : "error", error.ErrorNumber, error.ErrorText);
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Razor failure while processing: {0}, error: {1}", path, e.Message);
                }

                return null;
            }
        }

        public void Unload(IEnumerable<string> paths)
        {
            lock (sync)
            {
                this.InitializeRazorEngine();
            }
        }

        private void InitializeRazorEngine()
        {
            this.TopLevelTemplates = new Dictionary<string, LoadedTemplateSource>();

            var manager = new TemplateManager();
            manager.TopLevelTemplates = this.TopLevelTemplates;

            var config = new TemplateServiceConfiguration();
            config.AllowMissingPropertiesOnDynamic = true;
            config.BaseTemplateType = typeof(RazorRendererTemplateBase<>);
            config.CachingProvider = new DefaultCachingProvider(t => { });
            config.Namespaces.Add("System.IO");
            config.Namespaces.Add("RazorEngine.Text");
            config.Namespaces.Add("TinySite.Renderers");
            config.TemplateManager = manager;

            var service = RazorEngineService.Create(config);
            Engine.Razor = service;
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

    public class DynamicHelper
    {
        public bool Defined(object value)
        {
            try
            {
                var exists = value.ToString();
                return !String.IsNullOrEmpty(exists);
            }
            catch (RuntimeBinderException)
            {
                return false;
            }
        }

        public bool Undefined(object value)
        {
            return !this.Defined(value);
        }
    }

    public abstract class RazorRendererTemplateBase<T> : TemplateBase<T>
    {
        public RazorRendererTemplateBase()
        {
            Dynamic = new DynamicHelper();
        }

        public DynamicHelper Dynamic { get; set; }

        public override string ResolveUrl(string path)
        {
            // TODO: consider using this to replace the "~" with Site.RootUrl.
            return base.ResolveUrl(path);
        }
    }
}
