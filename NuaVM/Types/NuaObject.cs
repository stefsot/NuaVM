using System;

namespace NuaVM.Types
{
    // ReSharper disable InconsistentNaming
    public enum NuaObjectType
    {
        nil,
        boolean,
        number,
        @string,
        function,
        table,
        userdata
    }

    public class NuaObjectMetadata
    {
        public string Name { get; private set; }

        public string DebugName { get; set; }

        public NuaObjectMetadata(string name)
        {
            Name = name;
        }
    }

    public abstract class NuaObject
    {
        public abstract NuaObjectType Type { get; }

        public NuaObjectMetadata Metadata { get; protected set; } = new NuaObjectMetadata(string.Empty);

        public abstract object Value { get; protected set; }

        public virtual int Length => 0;

        public bool IsNull
        {
            get
            {
                if (Type == NuaObjectType.function)
                    return false;

                return Value == null;
            }
        }

        public override string ToString()
        {
            return Type.ToString();
        }

        public static NuaObject operator !(NuaObject b)
        {
            return new NuaBoolean(false);
        }

        public static bool operator ==(NuaObject b, NuaObject c)
        {
            if (ReferenceEquals(b, null))
            {
                return ReferenceEquals(c, null);
            }

            if (ReferenceEquals(c, null))
            {
                return false;
            }

            if (b.Type != c.Type)
                return false;

            switch (b.Type)
            {
                case NuaObjectType.nil:
                    return true;

                case NuaObjectType.boolean:
                    return (bool) b.Value == (bool) c.Value;

                case NuaObjectType.number:
                    return (double) b.Value == (double) c.Value;

                case NuaObjectType.@string:
                    return string.Equals((string) b.Value, (string) c.Value);

                case NuaObjectType.function:
                    var bF = b as NuaFunction;
                    var cF = c as NuaFunction;

                    // ReSharper disable once PossibleNullReferenceException
                    if (bF.IsNet)
                        return cF.IsNet && ReferenceEquals(bF.Delegate.Method, cF.Delegate.Method);

                    if (!ReferenceEquals(bF.Closure.Prototype, cF.Closure.Prototype))
                        return false;

                    if (bF.Closure.Upvalues.Length != cF.Closure.Upvalues.Length)
                        return false;

                    for (var i = 0; i < bF.Closure.Upvalues.Length; i++)
                        if (!ReferenceEquals(bF.Closure.Upvalues[i].Register, cF.Closure.Upvalues[i].Register))
                            return false;

                    return true;

                case NuaObjectType.table:
                    return ReferenceEquals(b.Value, c.Value);

                default:
                    return b.Value == c.Value;
            }
        }

        protected bool Equals(NuaObject other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NuaObject)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)Type;
                hashCode = (hashCode * 397) ^ (Metadata != null ? Metadata.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator !=(NuaObject b, NuaObject c)
        {
            return !(b == c);
        }

        public static implicit operator NuaObject(string s)
        {
            return new NuaString(s);
        }

        public static implicit operator NuaObject(double d)
        {
            return new NuaNumber(d);
        }

        public static implicit operator NuaObject(bool b)
        {
            return new NuaBoolean(b);
        }

        public static readonly NuaObject[] EmptyArgs = Array.Empty<NuaObject>();
    }
}
