using System;
using System.Collections.Generic;

namespace TinySite.Models
{
    public class MetadataCollection : IEnumerable<KeyValuePair<string, object>>
    {
        private readonly Dictionary<string, object> _dictionary;

        public MetadataCollection()
        {
            _dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public MetadataCollection(MetadataCollection original)
        {
            _dictionary = new Dictionary<string, object>(original._dictionary);
        }

        public void Add(string key, object value)
        {
            _dictionary.Add(key, value);
        }

        internal void AssignFrom(string path, IDictionary<string, object> source)
        {
            foreach (var kvp in source)
            {
                try
                {
                    this.Add(kvp.Key, kvp.Value);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Additional metadata configuration metadata cannot overwrite file: {0} metadata: \"{1}\" with value: \"{2}\"", path, kvp.Key, kvp.Value);
                }
            }
        }

        public void AssignTo(string path, IDictionary<string, object> target)
        {
            foreach (var kvp in _dictionary)
            {
                if (!target.TryAdd(kvp.Key, kvp.Value))
                {
                    Console.WriteLine("Document metadata in: {0} cannot overwrite built in or existing metadata: \"{1}\" with value: \"{2}\"", path, kvp.Key, kvp.Value);
                }
            }
        }

        public bool Contains(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            return _dictionary.TryGetValue(key, out var result) ? (T)Convert.ChangeType(result, typeof(T)) : defaultValue;
        }

        public T GetAndRemove<T>(string key, T defaultValue = default)
        {
            if (!_dictionary.TryGetValue(key, out var result))
            {
                return defaultValue;
            }

            this.Remove(key);
            return (T)Convert.ChangeType(result, typeof(T));
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

        public void Overwrite(string key, object value)
        {
            _dictionary[key] = value;
        }

        public bool Remove(string key)
        {
            return _dictionary.Remove(key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }
    }
}
