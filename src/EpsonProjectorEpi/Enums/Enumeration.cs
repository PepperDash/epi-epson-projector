using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;

namespace EpsonProjectorEpi.Enums
{
    public abstract class Enumeration<TEnum> : IComparable<Enumeration<TEnum>> where TEnum : Enumeration<TEnum>
    {
        public string Name { get; private set; }

        public int Value { get; private set; }

        protected Enumeration(int value, string name)
        {
            Value = value;
            Name = name;

            _all.Add(this as TEnum);
        }

        public override string ToString()
        {
            return Name;
        }

        static readonly List<TEnum> _all = new List<TEnum>();
        public static IEnumerable<TEnum> GetAll()
        {
            return _all;
        }

        public override bool Equals(object obj)
        {
            var otherValue = obj as Enumeration<TEnum>;

            if (otherValue == null)
                return false;

            var typeMatches = GetType().Equals(obj.GetType());
            var valueMatches = Value.Equals(otherValue.Value);

            return typeMatches && valueMatches;
        }

        public int CompareTo(object other)
        {
            if (other == null) return 1;

            Enumeration<TEnum> otherEnum = other as Enumeration<TEnum>;
            if (otherEnum != null)
                return this.Value.CompareTo(otherEnum.Value);
            else
                throw new ArgumentException("Object is not an Enum Class");
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        public static TEnum FromName(string name, bool ignoreCase)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(name);

            if (ignoreCase)
            {
                var result = _all.FirstOrDefault(x => x.Name.Equals(name));
                if (result == null)
                    throw new ArgumentNullException(name);

                return result;
            }
            else
            {
                var result = _all.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (result == null)
                    throw new ArgumentNullException(name);

                return result;
            }
        }

        public static bool TryFromValue(int value, out TEnum result)
        {
            result = _all.FirstOrDefault(x => x.Value == value);
            return result != null; 
        }

        public static TEnum FromValue(int value)
        {
            var result = _all.FirstOrDefault(x => x.Value == value);
            if (result == null)
                throw new ArgumentNullException(value.ToString());

            return result;
        }

        #region IComparable<Enumeration<TEnum>> Members

        public int CompareTo(Enumeration<TEnum> other)
        {
            if (other == null) return 1;
            return Value.CompareTo(other.Value);
        }

        #endregion
    }
}