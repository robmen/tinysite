using System.Collections.Generic;

namespace TinySite.Rendering
{
    /// <summary>
    /// Interface implemented by classes that can render templates into content.
    /// </summary>
    public interface IRenderer
    {
        /// <summary>
        /// Renders the template with the provided data.
        /// </summary>
        /// <param name="path">Path of template to render.</param>
        /// <param name="template">Template to render.</param>
        /// <param name="data">Data provided to template.</param>
        /// <returns>Rendered template content.</returns>
        string Render(string path, string template, object data);

        /// <summary>
        /// Unloads the specified template.
        /// </summary>
        /// <param name="paths">Paths of templates to unload.</param>
        void Unload(IEnumerable<string> paths);
    }
}
