
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
        /// <param name="template">Template to render.</param>
        /// <param name="data">Data provided to template.</param>
        /// <returns>Rendered template content.</returns>
        string Render(string template, object data);
    }
}
