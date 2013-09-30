using System;
using System.Collections.Generic;
using TinySite.Models;

namespace TinySite.Services
{
    public class RenderingTransaction : IDisposable
    {
        private bool _disposed = false;

        public RenderingTransaction(IDictionary<string, RenderingEngine> engines, Site site)
        {
            if (RenderingTransaction.Current != null)
            {
                throw new InvalidOperationException("Can only have one rendering transaction active at a time. Ensure the previous rendering transaction was disposed before creating this one.");
            }

            this.Engines = engines;
            this.Site = site;
            this.Documents = site.Documents;
            this.Files = site.Files;
            this.Layouts = site.Layouts;

            RenderingTransaction.Current = this;
        }

        public static RenderingTransaction Current { get; set; }

        public IDictionary<string, RenderingEngine> Engines { get; set; }

        public Site Site { get; set; }

        public IEnumerable<DocumentFile> Documents { get; set; }

        public IEnumerable<StaticFile> Files { get; set; }

        public LayoutFileCollection Layouts { get; set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (RenderingTransaction.Current == this)
                {
                    RenderingTransaction.Current = null;
                }
            }

            _disposed = true;
        }
    }
}
