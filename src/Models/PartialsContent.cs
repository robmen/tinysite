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
        private IDictionary<string, DocumentFile> _partials;

        public PartialsContent(IEnumerable<DocumentFile> partials, DocumentFile renderingDocument)
        {
            this.RenderingDocument = renderingDocument;

            _partials = partials.ToDictionary(IdForPartial, StringComparer.OrdinalIgnoreCase);
        }

        private DocumentFile RenderingDocument { get; }

        #region // IDictionary<string, object>

        public object this[string key]
        {
            get
            {
                object value;

                if (!this.TryGetValue(key, out value))
                {
                    value = String.Empty;
                }

                return value;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public int Count => _partials.Keys.Count;

        public bool IsReadOnly => true;

        public ICollection<string> Keys
        {
            get
            {
                return _partials.Keys.ToList();
            }
        }

        public ICollection<object> Values
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _partials.ContainsKey(item.Key);
        }

        public bool ContainsKey(string key)
        {
            return _partials.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return this.Remove(item.Key);
        }

        public bool Remove(string key)
        {
            return _partials.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            DocumentFile partial;

            var id = SafeId(key);

            if (_partials.TryGetValue(key, out partial))
            {
                if (!partial.Rendered)
                {
                    this.RenderPartial(partial);
                }

                this.RenderingDocument.AddContributingFile(partial);

                value = partial.RenderedContent;
                return true;
            }
            else
            {
                value = String.Empty;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region // DynamicObject

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return this.TryGetValue(binder.Name, out result);
        }

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

        private string IdForPartial(DocumentFile partial)
        {
            return SafeId(partial.Id);
        }

        private static string SafeId(string id)
        {
            return id.Replace('-', '_').Replace('\\', '_').Replace('/', '_');
        }
    }
}
