using System;
using System.Collections.Generic;

namespace TinySite.Models
{
    public class MetadataCollection : IEnumerable<KeyValuePair<string, object>>
    {
        public MetadataCollection()
        {
            this.Dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public MetadataCollection(MetadataCollection original)
        {
            this.Dictionary = new Dictionary<string, object>(original.Dictionary);
        }

        private Dictionary<string, object> Dictionary { get; set; }

        public void Add(string key, object value)
        {
            this.Dictionary.Add(key, value);
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
            foreach (var kvp in this.Dictionary)
            {
                try
                {
                    target.Add(kvp);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Document metadata in: {0} cannot overwrite built in or existing metadata: \"{1}\" with value: \"{2}\"", path, kvp.Key, kvp.Value);
                }
            }
        }

        public bool Contains(string key)
        {
            return this.Dictionary.ContainsKey(key);
        }

        public T Get<T>(string key, T defaultValue = default(T))
        {
            object result;
            return this.Dictionary.TryGetValue(key, out result) ? (T)Convert.ChangeType(result, typeof(T)) : defaultValue;
        }

        public bool TryGet<T>(string key, out T value)
        {
            object valueObject;
            if (this.Dictionary.TryGetValue(key, out valueObject))
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

        public void Overwrite(string key, object value)
        {
            this.Dictionary[key] = value;
        }

        public bool Remove(string key)
        {
            return this.Dictionary.Remove(key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.Dictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.Dictionary.GetEnumerator();
        }
    }
}
