using TinySite.Extensions;

namespace TinySite.Models
{
    public class Page : CaseInsensitiveExpando
    {
        public bool Active { get { return this.Get<bool>(); } set { this.Set<bool>(value); } }

        public int Number { get { return this.Get<int>(); } set { this.Set<int>(value); } }

        public string Url { get { return this.Get<string>(); } set { this.Set<string>(value); } }
    }
}
