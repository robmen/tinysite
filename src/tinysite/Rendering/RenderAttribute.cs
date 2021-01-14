using System;

namespace TinySite.Rendering
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RenderAttribute : Attribute
    {
        public RenderAttribute(string extension)
        {
            this.Extension = extension;
        }

        public string Extension { get; private set; }
    }
}
