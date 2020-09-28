using System;
using System.Collections.Generic;
using System.Linq;
using PepperDash.Core;
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
        }

        public override string ToString()
        {
            return Name;
        }

        private static readonly CCriticalSection _lock = new CCriticalSection();
        private static IEnumerable<TEnum> _all;
        public static IEnumerable<TEnum> GetAll()
        {
            CheckAll();
            return _all;
        }

        static void CheckAll()
        {
            _lock.Enter();
            try
            {
                if (_all == null)
                    _all = GetAllOptions();
            }
            finally
            {
                _lock.Leave();
            }
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

        /*static readonly object _fromNameLock = new object();
        static Dictionary<string, TEnum> _fromNameObject = null;
        static Dictionary<string, TEnum> _fromName
        {
            get
            {
                CMonitor.Enter(_fromNameLock);
                try
                {
                    if (_fromNameObject == null)
                    {
                        _fromNameObject = _all.ToDictionary(item => item.Name);
                    }
                }
                finally
                {
                    CMonitor.Exit(_fromNameLock);     
                }

                return _fromNameObject;
            }
        }

        static readonly object _fromNameIgnoreCaseLock = new object();
        static Dictionary<string, TEnum> _fromNameIgnoreCaseObject = null;
        static Dictionary<string, TEnum> _fromNameIgnoreCase
        {
            get
            {
                CMonitor.Enter(_fromNameIgnoreCaseLock);
                try
                {
                    if (_fromNameIgnoreCaseObject == null)
                    {
                        _fromNameIgnoreCaseObject = _all.ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase);
                    }
                }
                finally
                {
                    CMonitor.Exit(_fromNameIgnoreCaseLock);
                }

                return _fromNameObject;
            }
        }

        static readonly object _fromValueLock = new object();
        static Dictionary<int, TEnum> _fromValueObject = null;
        static Dictionary<int, TEnum> _fromValue
        {
            get
            {
                CMonitor.Enter(_fromValueLock);
                try
                {
                    if (_fromValueObject == null)
                    {
                        _fromValueObject = _all.ToDictionary(x => x.Value);
                    }
                }
                finally
                {
                    CMonitor.Exit(_fromValueLock);
                }

                return _fromValueObject;
            }
        }*/

        static IEnumerable<TEnum> GetAllOptions()
        {
            try
            {
                var baseType = typeof(TEnum).GetCType();
                var a = baseType.Assembly;

                Debug.Console(0, "Base type: {0}", baseType.Name);
                IEnumerable<CType> enumTypes = a.GetTypes().Where(t => baseType.IsAssignableFrom(t));

                List<TEnum> options = new List<TEnum>();
                foreach (var enumType in enumTypes)
                {
                    Debug.Console(2, "Found enum type: {0}", enumType.Name);
                    var fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
                        .Select(x => x.GetValue(null))
                        .Cast<TEnum>();

                    foreach (var field in fields)
                    {
                        if (field == null)
                            continue;

                        Debug.Console(1, "Adding field to this enum:{0} - {1}", field.Name, enumType.Name);
                        if (options.Contains(field))
                            throw new Exception("This enum already exists");

                        options.Add(field);
                    }
                }

                return options;
            }
            catch (Exception ex)
            {
                var error = "Error getting all options -" + string.Format("{0}\r{1}\r{2}", ex.Message, ex.InnerException, ex.StackTrace);
                CrestronConsole.PrintLine(error);
                Debug.Console(0, "{0}", error);
                throw;
            }
        }

        public static TEnum FromName(string name, bool ignoreCase)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(name);

            CheckAll();
            if (!ignoreCase)
            {
                var result = _all.FirstOrDefault(x => x.Name.Equals(name));
                if (result == null)
                    throw new ArgumentException(name);

                return result;
            }
            else
            {
                var result = _all.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (result == null)
                    throw new ArgumentException(name);

                return result;
            }
        }

        public static bool TryFromValue(int value, out TEnum result)
        {
            CheckAll();
            result = _all.FirstOrDefault(x => x.Value == value);
            if (result == null) return false;

            return true;
        }

        public static TEnum FromValue(int value)
        {
            CheckAll();
            var result = _all.FirstOrDefault(x => x.Value == value);
            if (result == null)
                throw new ArgumentException(value.ToString());

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