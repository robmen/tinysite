using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TinySite.Models;
using TinySite.Rendering;

namespace TinySite.Services
{
    public class RenderingEngine
    {
        private readonly object _sync = new object();

        private RenderingEngine(SiteConfig config, Type type)
        {
            this.Config = config;
            this.Type = type;
        }

        public SiteConfig Config { get; }

        public Type Type { get; set; }

        private IRenderer Renderer { get; set; }

        public static IDictionary<string, RenderingEngine> Load(SiteConfig config, params Assembly[] assemblies)
        {
            if (assemblies.Length == 0)
            {
                assemblies = new[] { Assembly.GetCallingAssembly() };
            }

            var engines = new Dictionary<string, RenderingEngine>();

            var renderTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .AsParallel()
                .Select(t => new { Type = t, Attributes = t.GetCustomAttributes(typeof(RenderAttribute), true).OfType<RenderAttribute>().ToList() })
                .Where(ta => ta.Attributes != null && ta.Attributes.Count > 0)
                .ToList();

            foreach (var renderType in renderTypes)
            {
                var engine = new RenderingEngine(config, renderType.Type);

                foreach (var extension in renderType.Attributes.Select(a => a.Extension))
                {
                    engines.Add(extension.ToLowerInvariant(), engine);
                }
            }

            return engines;
        }

        public string Render(SourceFile sourceFile, string template, object data)
        {
            if (this.Renderer == null)
            {
                lock (_sync)
                {
                    if (this.Renderer == null)
                    {
                        var constructor = this.Type.GetConstructor(new[] { typeof(SiteConfig) });
                        if (constructor != null)
                        {
                            this.Renderer = (IRenderer)constructor.Invoke(new[] { this.Config });
                        }
                        else
                        {
                            this.Renderer = (IRenderer)Activator.CreateInstance(this.Type);
                        }
                    }
                }

                if (this.Renderer == null)
                {
                    throw new InvalidCastException($"The render of type '{this.Type}' does not implement IRenderer. Ensure your renderer inherits from IRenderer.");
                }
            }

            return this.Renderer.Render(sourceFile, template, data);
        }

        internal void Unload(IEnumerable<string> paths) => this.Renderer?.Unload(paths);
    }
}
