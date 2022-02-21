using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TinySite.Models.Dynamic
{
    public class DynamicDateTime : DynamicBase, IComparable, IComparable<DateTime>, IEquatable<DateTime>
    {
        private readonly DateTime _date;

        public DynamicDateTime(DateTime date)
        {
            _date = date;
        }

        protected override IDictionary<string, object> GetData()
        {
            var data = base.GetData();

            data.Add(nameof(_date.Day), _date.Day);
            data.Add(nameof(_date.Month), _date.Month);
            data.Add(nameof(_date.Year), _date.Year);
            data.Add(nameof(_date.Hour), _date.Hour);
            data.Add(nameof(_date.Minute), _date.Minute);
            data.Add(nameof(_date.Second), _date.Second);
            data.Add(nameof(_date.Ticks), _date.Ticks);
            data.Add(nameof(_date.DayOfWeek), _date.DayOfWeek.ToString());
            data.Add(nameof(_date.DayOfYear), _date.DayOfYear);

            return data;
        }

        public int CompareTo(object obj) => (obj is DynamicDateTime d) ? _date.CompareTo(d._date) : 1;

        public int CompareTo([AllowNull] DateTime other) => _date.CompareTo(other);

        public bool Equals([AllowNull] DateTime other) => _date.Equals(other);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null)
            {
                return false;
            }

            if (obj is DynamicDateTime ddt)
            {
                return _date.Equals(ddt._date);
            }

            if (obj is DateTime date)
            {
                return _date.Equals(date);
            }

            return false;
        }

        public override int GetHashCode() => _date.GetHashCode();

        public override string ToString() => _date.ToString();

        public string ToString(string format) => _date.ToString(format);

        public string ToString(IFormatProvider provider) => _date.ToString(provider);

        public string ToString(string format, IFormatProvider provider) => _date.ToString(format, provider);

        public static implicit operator DynamicDateTime(DateTime value)
        {
            return new DynamicDateTime(value);
        }

        public static bool operator ==(DynamicDateTime left, DynamicDateTime right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(DynamicDateTime left, DynamicDateTime right)
        {
            return !(left == right);
        }

        public static bool operator <(DynamicDateTime left, DynamicDateTime right)
        {
            return left is null ? right is not null : left.CompareTo(right) < 0;
        }

        public static bool operator <=(DynamicDateTime left, DynamicDateTime right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(DynamicDateTime left, DynamicDateTime right)
        {
            return left is not null && left.CompareTo(right) > 0;
        }

        public static bool operator >=(DynamicDateTime left, DynamicDateTime right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }
}
