using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TinySite.Rendering;

namespace TinySite.Services
{
    public class RenderingEngine
    {
        private RenderingEngine(Type type)
        {
            this.Type = type;
        }

        public Type Type { get; set; }

        private IRenderer Renderer { get; set; }

        public static IDictionary<string, RenderingEngine> Load(params Assembly[] assemblies)
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
                var engine = new RenderingEngine(renderType.Type);

                foreach (var extension in renderType.Attributes.Select(a => a.Extension))
                {
                    engines.Add(extension.ToLowerInvariant(), engine);
                }
            }

            return engines;
        }

        public string Render(string path, string template, object data)
        {
            if (this.Renderer == null)
            {
                this.Renderer = Activator.CreateInstance(this.Type) as IRenderer;

                if (this.Renderer == null)
                {
                    throw new InvalidCastException(String.Format("The render of type '{0}' does not impliement IRender. Ensure your renderer inherits from IRender.", this.Type));
                }
            }

            return this.Renderer.Render(path, template, data);
        }
    }
}
