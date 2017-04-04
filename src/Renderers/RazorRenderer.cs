using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;
using SharpRazor;
using TinySite.Models;
using TinySite.Rendering;
using TinySite.Services;

namespace TinySite.Renderers
{
    [Render("cshtml")]
    public class RazorRenderer : IRenderer
    {
        private readonly object _renderLock = new object();

        private Razorizer _razor;

        public RazorRenderer()
        {
            this.InitializeRazorEngine();
        }

        public string Render(SourceFile sourceFile, string template, object data)
        {
            lock (_renderLock)
            {
                try
                {
                    var compilation = _razor.Compile(sourceFile.FileName, template, sourceFile.SourcePath);
                    compilation.Model = data;

                    var result = compilation.Run();
                    return result;
                }
                catch (TemplateCompilationException e)
                {
                    foreach (var error in e.Errors.Where(err => !err.IsWarning))
                    {
                        Console.WriteLine(error);
                    }

                    return String.Empty;
                }
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
            _razor = new Razorizer(typeof(DynamicPageTemplate))
            {
                TemplateResolver = this.ResolveTemplate
            };
        }

        private PageTemplate ResolveTemplate(string templatename)
        {
            var id = Path.GetFileNameWithoutExtension(templatename);

            if (RenderingTransaction.Current.Layouts.Contains(id))
            {
                var layout = RenderingTransaction.Current.Layouts[id];
                return _razor.Compile(templatename, layout.SourceContent, layout.SourcePath);
            }

            return null;
        }
    }

    public class DynamicPageTemplate : PageTemplate<dynamic>
    {
        [Obsolete]
        public RazorRenderDynamicHelper Dynamic { get; } = new RazorRenderDynamicHelper();

        public static bool Defined(object value)
        {
            return value != null;
        }

        public static bool Undefined(object value)
        {
            return value == null;
        }
    }

    public class RazorRenderDynamicHelper
    {
        public bool Defined(object value)
        {
            return value != null;
        }

        public bool Undefined(object value)
        {
            return !this.Defined(value);
        }

        public HtmlRawString Raw(object value)
        {
            if (value != null)
            {
                try
                {
                    var str = value.ToString();
                    return new HtmlRawString(str);
                }
                catch (RuntimeBinderException)
                {
                }
            }

            return new HtmlRawString(null);
        }
    }
}
