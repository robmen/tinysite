using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace TinySite.Models.Dynamic
{
    public abstract class DynamicBase : DynamicObject, IDictionary<string, object>
    {
        protected DynamicBase()
        {
            this.Data = new Lazy<IDictionary<string, object>>(this.GetData);
        }

        protected Lazy<IDictionary<string, object>> Data { get; }

        protected virtual IDictionary<string, object> GetData() => new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        protected virtual bool TrySetValue(string key, object value) => this.Data.Value.TryAdd(key, value);

        #region DynamicObject

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this.TryGetValue(binder.Name, out var value) ? value : null;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) => this.TrySetValue(binder.Name, value);

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (this.TryGetValue(binder.Name, out var value) && value is Delegate delegated)
            {
                result = delegated.DynamicInvoke(args);
                return true;
            }

            result = null;
            return false;
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder) => this.Remove(binder.Name);

        #endregion

        #region IDictionary<string, object>

        public object this[string key]
        {
            get => this.TryGetValue(key, out var value) ? value : null;
            set => this.Add(key, value);
        }

        public int Count => this.Data.Value.Count;

        public bool IsReadOnly => true;

        public ICollection<string> Keys => this.Data.Value.Keys;

        public ICollection<object> Values => new DelayLoadReadOnlyCollection(this.Data.Value.Values);

        public void Add(KeyValuePair<string, object> item) => this.Add(item.Key, item.Value);

        public virtual void Add(string key, object value)
        {
            this.Data.Value.Add(key, value);
        }

        public void Clear() => throw new NotSupportedException();

        public bool Contains(KeyValuePair<string, object> item) => throw new NotImplementedException();

        public bool ContainsKey(string key) => this.Data.Value.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new NotImplementedException();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach (var kvp in this.Data.Value)
            {
                yield return kvp.Value is Lazy<object> lazy ?
                    new KeyValuePair<string, object>(kvp.Key, lazy.Value) :
                    kvp;
            }
        }

        public bool Remove(KeyValuePair<string, object> item) => throw new NotSupportedException();

        public bool Remove(string key) => throw new NotSupportedException();

        public bool TryGetValue(string key, out object value)
        {
            if (this.Data.Value.TryGetValue(key, out value))
            {
                if (value is Lazy<object> lazy)
                {
                    value = lazy.Value;
                }

                return true;
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        private class DelayLoadReadOnlyCollection : ICollection<object>
        {
            public DelayLoadReadOnlyCollection(ICollection<object> values)
            {
                this.Values = values;
            }

            private ICollection<object> Values { get; }

            #region ICollection

            public int Count => this.Values.Count;

            public bool IsReadOnly => true;

            public void Add(object item) => throw new NotSupportedException();

            public void Clear() => throw new NotSupportedException();

            public bool Contains(object item) => throw new NotImplementedException();

            public void CopyTo(object[] array, int arrayIndex) => throw new NotImplementedException();

            public IEnumerator<object> GetEnumerator()
            {
                foreach (var value in this.Values)
                {
                    yield return (value is Lazy<object> lazy) ? lazy.Value : value;
                }
            }

            public bool Remove(object item) => throw new NotSupportedException();

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

            #endregion
        }

        #endregion
    }
}
