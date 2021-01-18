using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using RazorEngineCore;
using TinySite.Models;
using TinySite.Rendering;
using TinySite.Services;

namespace TinySite.Renderers
{
    [Render("cshtml")]
    public class RazorRenderer : IRenderer
    {
        private readonly object _renderLock = new object();
        private readonly ConcurrentDictionary<string, IRazorEngineCompiledTemplate<RazorRendererModel>> _templateCache = new ConcurrentDictionary<string, IRazorEngineCompiledTemplate<RazorRendererModel>>();

        public string Render(SourceFile sourceFile, string template, object data)
        {
            lock (_renderLock)
            {
                try
                {
                    var compiledTemplate = _templateCache.GetOrAdd(sourceFile.SourcePath, key =>
                    {
                        var engine = new RazorEngine();
                        return engine.Compile<RazorRendererModel>(template, options =>
                        {
                            options.AddUsing("System");
                            options.AddUsing("System.Collections.Generic");
                            options.AddUsing("System.IO");
                            options.AddUsing("System.Linq");
                        });
                    });

                    var result = compiledTemplate.Run(instance =>
                    {
                        instance.Model = data;
                        instance.Render = this;
                    });

                    return result;
                }
                catch (RazorEngineCompilationException e)
                {
                    foreach (var error in e.Errors)
                    {
                        var line = error.Location.GetMappedLineSpan();
                        var msg = error.GetMessage();
                        Console.WriteLine("{0}({1}): {2}", sourceFile.SourceRelativePath, line.StartLinePosition.Line - 1, msg);
                    }

                    return String.Empty;
                }
            }
        }

        public void Unload(IEnumerable<string> paths)
        {
            lock (_renderLock)
            {
                foreach (var path in paths)
                {
                    _templateCache.TryRemove(path, out _);
                }
            }
        }
    }

    public class RazorRendererModel : RazorEngineTemplateBase
    {
        internal RazorRenderer Render { get; set; }

        [Obsolete]
        public RazorRendererModel Dynamic => this;

        public bool Defined(object value) => value != null;

        public bool Undefined(object value) => value is null;

        [Obsolete]
        public object Encode(object value) => value is string str ? WebUtility.HtmlEncode(str) : value;

        public object HtmlEncode(object value) => value is string str ? WebUtility.HtmlEncode(str) : value;

        public object UrlEncode(object value) => value is string str ? WebUtility.UrlEncode(str) : value;

        public object Raw(object value) => value;

        public string Include(string file) => this.Include(file, this.Model);

        public string Include(string file, dynamic model)
        {
            var id = Path.GetFileNameWithoutExtension(file);

            if (RenderingTransaction.Current.Layouts.Contains(id))
            {
                var layout = RenderingTransaction.Current.Layouts[id];
                return this.Render.Render(layout, layout.SourceContent, model);
            }

            return null;
        }
    }
}
