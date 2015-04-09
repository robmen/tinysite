using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace TinySite.Extensions
{
    public class CaseInsensitiveExpando : DynamicObject, IDictionary<string, object>
    {
        private IDictionary<string, object> _dictionary;

        public CaseInsensitiveExpando()
        {
            _dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public CaseInsensitiveExpando(IDictionary<string, object> existing)
        {
            _dictionary = new Dictionary<string, object>(existing, StringComparer.OrdinalIgnoreCase);
        }

        protected CaseInsensitiveExpando(CaseInsensitiveExpando original)
        {
            _dictionary = new Dictionary<string, object>(original, StringComparer.OrdinalIgnoreCase);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _dictionary.Add(item);
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _dictionary.Remove(item);
        }

        public int Count
        {
            get { return _dictionary.Keys.Count; }
        }

        public bool IsReadOnly
        {
            get { return _dictionary.IsReadOnly; }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _dictionary.TryGetValue(binder.Name, out result) || base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _dictionary[binder.Name] = value;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            object delegateObject;
            if (_dictionary.TryGetValue(binder.Name, out delegateObject) && delegateObject is Delegate)
            {
                result = ((Delegate)delegateObject).DynamicInvoke(args);
                return true;
            }

            return base.TryInvokeMember(binder, args, out result);
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            return _dictionary.Remove(binder.Name) || base.TryDeleteMember(binder);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void Add(string key, object value)
        {
            _dictionary.Add(key, value);
        }

        public bool Remove(string key)
        {
            return _dictionary.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public object this[string key]
        {
            get { return _dictionary[key]; }
            set { _dictionary[key] = value; }
        }

        public ICollection<string> Keys
        {
            get { return _dictionary.Keys; }
        }

        public ICollection<object> Values
        {
            get { return _dictionary.Values; }
        }

        public T GetOrDefault<T>(string key, T defaultValue = default(T))
        {
            object result;
            return _dictionary.TryGetValue(key, out result) ? (T)Convert.ChangeType(result, typeof(T)) : defaultValue;
        }

        public bool TryGet<T>(string key, out T value)
        {
            object valueObject;
            if (_dictionary.TryGetValue(key, out valueObject))
            {
                value = (T)valueObject;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        protected T Get<T>([CallerMemberName] string key = null)
        {
            object value;
            return _dictionary.TryGetValue(key, out value) ? (T)value : default(T);
        }

        protected void Set<T>(T value, [CallerMemberName] string key = null)
        {
            _dictionary[key] = value;
        }
    }
}
