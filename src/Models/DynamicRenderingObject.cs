using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace TinySite.Models
{
    public abstract class DynamicRenderingObject : DynamicObject, IDictionary<string, object>
    {
        protected DynamicRenderingObject()
        {
            this.Data = new Lazy<IDictionary<string, object>>(GetData);
        }

        private Lazy<IDictionary<string, object>> Data { get; }

        protected abstract IDictionary<string, object> GetData();

        protected virtual bool TrySetValue(string key, object value)
        {
            try
            {
                this.Data.Value.Add(key, value);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        #region DynamicObject

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return this.TryGetValue(binder.Name, out result) || base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return this.TrySetValue(binder.Name, value) || base.TrySetMember(binder, value);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            object delegateObject;
            if (this.TryGetValue(binder.Name, out delegateObject) && delegateObject is Delegate)
            {
                result = ((Delegate)delegateObject).DynamicInvoke(args);
                return true;
            }

            return base.TryInvokeMember(binder, args, out result);
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            return this.Remove(binder.Name);
        }

        #endregion

        #region IDictionary<string, object>

        public object this[string key]
        {
            get
            {
                object value = null;
                this.TryGetValue(key, out value);
                // TODO: should this throw if key not found?
                return value;
            }

            set
            {
                this.Add(key, value);
            }
        }

        public int Count => this.Data.Value.Count;

        public bool IsReadOnly => true;

        public ICollection<string> Keys => this.Data.Value.Keys;

        public ICollection<object> Values => new DelayLoadReadOnlyCollection(this.Data.Value.Values);

        public void Add(KeyValuePair<string, object> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Add(string key, object value)
        {
            if (!this.TrySetValue(key, value))
            { 
                // TODO: warn that metadata cannot be set
            }
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            return this.Data.Value.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.Data.Value.ToDictionary(kvp => kvp.Key, kvp => GetPossibleLazyValue(kvp.Value)).GetEnumerator();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(string key)
        {
            throw new NotSupportedException();
        }

        public bool TryGetValue(string key, out object value)
        {
            if (this.Data.Value.TryGetValue(key, out value))
            {
                var lazy = value as Lazy<object>;

                if (lazy != null)
                {
                    value = lazy.Value;
                }

                return true;
            }

            value = null;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private object GetPossibleLazyValue(object value)
        {
            var lazy = value as Lazy<object>;

            return (lazy != null) ? lazy.Value : value;
        }

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

            public void Add(object item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(object item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(object[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<object> GetEnumerator()
            {
                foreach (var value in this.Values)
                {
                    var lazy = value as Lazy<object>;

                    yield return (lazy != null) ? lazy.Value : value;
                }
            }

            public bool Remove(object item)
            {
                throw new NotSupportedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            #endregion
        }

        #endregion
    }
}
