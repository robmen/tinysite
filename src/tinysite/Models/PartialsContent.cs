using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using TinySite.Services;

namespace TinySite.Models
{
    public class PartialsContent : DynamicObject, IDictionary<string, object>
    {
        private readonly IDictionary<string, DocumentFile> _partials;

        public PartialsContent(IEnumerable<DocumentFile> partials, DocumentFile renderingDocument)
        {
            this.RenderingDocument = renderingDocument;

            _partials = partials.ToDictionary(IdForPartial, StringComparer.OrdinalIgnoreCase);
        }

        private DocumentFile RenderingDocument { get; }

        #region IDictionary<string, object>

        public object this[string key]
        {
            get => this.TryGetValue(key, out var value) ? value : String.Empty;
            set => throw new NotImplementedException();
        }

        public int Count => _partials.Keys.Count;

        public bool IsReadOnly => true;

        public ICollection<string> Keys => _partials.Keys;

        public ICollection<object> Values => throw new NotImplementedException();

        public void Add(KeyValuePair<string, object> item) => this.Add(item.Key, item.Value);

        public void Add(string key, object value) => throw new NotImplementedException();

        public void Clear() => throw new NotImplementedException();

        public bool Contains(KeyValuePair<string, object> item) => _partials.ContainsKey(item.Key);

        public bool ContainsKey(string key) => _partials.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new NotImplementedException();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => throw new NotImplementedException();

        public bool Remove(KeyValuePair<string, object> item) => this.Remove(item.Key);

        public bool Remove(string key) => _partials.Remove(key);

        public bool TryGetValue(string key, out object value)
        {
            var id = SafeId(key);

            if (!_partials.TryGetValue(id, out var partial))
            {
                value = String.Empty;
                return false;
            }

            if (!partial.Rendered)
            {
                this.RenderPartial(partial);
            }

            this.RenderingDocument.AddContributingFile(partial);

            value = partial.RenderedContent;
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        #endregion

        #region // DynamicObject

        public override bool TryGetMember(GetMemberBinder binder, out object result) => this.TryGetValue(binder.Name, out result);

        #endregion

        private void RenderPartial(DocumentFile partial)
        {
            var contentRendering = new ContentRendering(RenderingTransaction.Current);

            contentRendering.RenderDocumentContent(partial);

            var content = partial.Content;

            foreach (var layout in partial.Layouts)
            {
                content = contentRendering.RenderDocumentContentUsingLayout(partial, content, layout);
            }

            partial.RenderedContent = content;

            partial.Rendered = true;
        }

        private static string IdForPartial(DocumentFile partial) => SafeId(partial.Id);

        private static string SafeId(string id) => id.Replace('-', '_').Replace('\\', '_').Replace('/', '_');
    }
}
