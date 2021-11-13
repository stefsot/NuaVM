using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using NuaVM.Helpers;

namespace NuaVM.Types
{
    public abstract class NuaUserData : NuaObject
    {
        // NuaObject implementation
        // -------------------------------------

        public override NuaObjectType Type => NuaObjectType.userdata;

        public override object Value { get; protected set; }

        public abstract NuaObject Get(NuaObject key);

        public abstract void Set(NuaObject key, NuaObject value);

        public static NuaUserData<T> Create<T>(T data)
        {
            return new NuaUserData<T>(data);
        }
    }

    public class NuaUserData<T> : NuaUserData
    {
        public T Data
        {
            get => (T)Value;
            set => Value = value;
        }

        private PropertyInfo[] _cachedProperties;

        private NuaObject GetPropertyValue(PropertyInfo property)
        {
            var value = property.GetValue(Data);

            if (value is NuaObject nuaObject)
                return nuaObject;

            switch(value.GetType().Name)
            {
                case "String":
                    return new NuaString((string)value);
                case "Int32":
                    return new NuaNumber((int)value);
                case "Double":
                    return new NuaNumber((double)value);
                case "Single":
                    return new NuaNumber((float)value);
                case "Boolean":
                    return new NuaBoolean((bool)value);
                default:
                    var t = typeof(NuaUserData<>).MakeGenericType(value.GetType());
                    var ctor = t.GetConstructor(new[] { value.GetType() });

                    return (NuaObject)ctor.Invoke(new[] { value });
            }
        }

        private void SetPropertyValue(PropertyInfo property, NuaObject value)
        {
            if(value.Value.GetType() != property.PropertyType && value.Value is IConvertible)
                property.SetValue(Data, Convert.ChangeType(value.Value, property.PropertyType));

            property.SetValue(Data, value.Value);
        }

        private PropertyInfo GetProperty(NuaString key)
        {
            if (_cachedProperties == null)
                _cachedProperties = typeof(T).GetProperties();

            return _cachedProperties.FirstOrDefault(p =>
                string.Compare(p.Name, (string)key.Value, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        public override NuaObject Get(NuaObject key)
        {
            var property = GetProperty(key.AsString());

            if (property == null)
                return new NuaNull();

            return GetPropertyValue(property);
        }

        public override void Set(NuaObject key, NuaObject value)
        {
            var property = GetProperty(key.AsString());

            if (property == null)
                return;

            SetPropertyValue(property, value);
        }

        public NuaObject this[NuaObject key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        public NuaUserData()
        {
        }

        public NuaUserData(T data)
        {
            Data = data;
        }
    }
}
