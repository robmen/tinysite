using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CSharp.RuntimeBinder;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;
using TinySite.Models;
using TinySite.Rendering;
using TinySite.Services;

namespace TinySite.Renderers
{
    [Render("cshtml")]
    public class RazorRenderer : IRenderer
    {
        private object _renderLock = new object();

        public RazorRenderer()
        {
            this.InitializeRazorEngine();
        }

        private RazorRendererTemplateManager TemplateManager { get; set; }

        public string Render(SourceFile sourceFile, string template, object data)
        {
            var path = sourceFile.SourcePath;

            lock (_renderLock)
            {
                LoadedTemplateSource loadedTemplate;

                if (!this.TemplateManager.TopLevelTemplates.TryGetValue(path, out loadedTemplate))
                {
                    loadedTemplate = new LoadedTemplateSource(template, path);

                    this.TemplateManager.TopLevelTemplates.Add(path, loadedTemplate);
                }

                try
                {
                    this.TemplateManager.CurrentSourceFile = sourceFile;

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
                finally
                {
                    this.TemplateManager.CurrentSourceFile = null;
                }

                return null;
            }
        }

        public void Unload(IEnumerable<string> paths)
        {
            lock (_renderLock)
            {
                this.InitializeRazorEngine();
            }
        }

        private void InitializeRazorEngine()
        {
            this.TemplateManager = new RazorRendererTemplateManager();

            var config = new TemplateServiceConfiguration();
            config.AllowMissingPropertiesOnDynamic = true;
            config.BaseTemplateType = typeof(RazorRendererTemplateBase<>);
            config.CachingProvider = new DefaultCachingProvider(t => { });
            config.Namespaces.Add("System.IO");
            config.Namespaces.Add("RazorEngine.Text");
            config.Namespaces.Add("TinySite.Renderers");
            config.TemplateManager = this.TemplateManager;

            var service = RazorEngineService.Create(config);
            Engine.Razor = service;
        }

        private class RazorRendererTemplateManager : ITemplateManager
        {
            public SourceFile CurrentSourceFile { get; set; }

            public Dictionary<string, LoadedTemplateSource> TopLevelTemplates { get; } = new Dictionary<string, LoadedTemplateSource>();

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

                var parentFile = this.GetParentSourceFile(key);

                // Always try to get a matching layout so the parent file gets a
                // chance to track the layout as a contributing file.
                LayoutFile layout = null;

                if (this.TryGetLayout(name, out layout) && parentFile != layout)
                {
                    parentFile.AddContributingFile(layout);
                }

                LoadedTemplateSource loadedTemplate;

                if (!this.TopLevelTemplates.TryGetValue(name, out loadedTemplate))
                {
                    loadedTemplate = new LoadedTemplateSource(layout.SourceContent, layout.SourcePath);

                    // Do not need to add this loaded template to our list of cached top level templates
                    // because RazorEngine will remember it for us and never ask again.
                }

                return loadedTemplate;
            }

            private SourceFile GetParentSourceFile(ITemplateKey key)
            {
                var parentFile = this.CurrentSourceFile;

                // If the template is actually an include in the parent then
                // the parent is the layout doing the including, not the current
                // source file being rendered.
                if (key.TemplateType == ResolveType.Include)
                {
                    var parentName = Path.GetFileName(key.Context.Name);

                    var parentId = Path.GetFileNameWithoutExtension(parentName);

                    parentFile = RenderingTransaction.Current.Layouts[parentId];
                }

                return parentFile;
            }

            private bool TryGetLayout(string name, out LayoutFile layout)
            {
                layout = null;

                var id = Path.GetFileNameWithoutExtension(name);

                if (RenderingTransaction.Current.Layouts.Contains(id))
                {
                    layout = RenderingTransaction.Current.Layouts[id];
                }

                return (layout != null);
            }
        }
    }

    public class DynamicHelper
    {
        public bool Defined(object value)
        {
            if (value != null)
            {
                try
                {
                    var exists = value.ToString();
                    return !String.IsNullOrEmpty(exists);
                }
                catch (RuntimeBinderException)
                {
                }
            }

            return false;
        }

        public bool Undefined(object value)
        {
            return !this.Defined(value);
        }

        public RawString Raw(object value)
        {
            if (value != null)
            {
                try
                {
                    var str = value.ToString();
                    return new RawString(str);
                }
                catch (RuntimeBinderException)
                {
                }
            }

            return new RawString(null);
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
