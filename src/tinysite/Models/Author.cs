using TinySite.Extensions;

namespace TinySite.Models
{
    public class Author : CaseInsensitiveExpando
    {
        public string Id { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string Name { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string Email { get { return this.Get<string>(); } set { this.Set<string>(value); } }

        public string Url { get { return this.Get<string>(); } set { this.Set<string>(value); } }
    }
}
