using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;

namespace TinySite.Extensions
{
    public class CaseInsensitiveExpando : DynamicObject, IDictionary<string, object>
    {
        private readonly IDictionary<string, object> _dictionary;

        public CaseInsensitiveExpando()
        {
            _dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public CaseInsensitiveExpando(IEnumerable<KeyValuePair<string, object>> existing)
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
            if (!_dictionary.TryGetValue(binder.Name, out result))
            {
                if (binder.ReturnType.IsValueType)
                {
                    result = Activator.CreateInstance(binder.ReturnType);
                }
            }

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _dictionary[binder.Name] = value;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (_dictionary.TryGetValue(binder.Name, out var delegateObject) &&
                delegateObject is Delegate del)
            {
                result = del.DynamicInvoke(args);
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
            return this.GetEnumerator();
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

        public T GetOrDefault<T>(string key, T defaultValue = default)
        {
            return _dictionary.TryGetValue(key, out var result) ? (T)Convert.ChangeType(result, typeof(T)) : defaultValue;
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (_dictionary.TryGetValue(key, out var valueObject))
            {
                value = (T)valueObject;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public static object FromJson(string json)
        {
            return FromJToken(JToken.Parse(json));
        }

        public static object FromJToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    return new CaseInsensitiveExpando(
                                token.Children<JProperty>()
                                     .Select(prop => new KeyValuePair<string, object>(
                                         prop.Name, 
                                         FromJToken(prop.Value))
                                     ));

                case JTokenType.Array:
                    return token.Select(FromJToken).ToList();

                default:
                    return ((JValue)token).Value;
            }
        }

        protected T Get<T>([CallerMemberName] string key = null)
        {
            return _dictionary.TryGetValue(key, out var value) ? (T)value : default;
        }

        protected void Set<T>(T value, [CallerMemberName] string key = null)
        {
            _dictionary[key] = value;
        }
    }
}
