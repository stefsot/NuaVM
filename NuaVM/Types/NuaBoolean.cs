namespace NuaVM.Types
{
    public class NuaBoolean : NuaObject
    {
        // NuaObject implementation
        // -------------------------------------

        public override NuaObjectType Type => NuaObjectType.boolean;

        public override object Value { get; protected set; }

        // NuaBoolean implementation
        // -------------------------------------

        public bool Boolean
        {
            get => (bool) Value;
            set => Value = value;
        }

        public NuaBoolean(bool boolean)
        {
            Boolean = boolean;
        }

        public static NuaBoolean operator !(NuaBoolean b)
        {
            return new NuaBoolean(!b.Boolean);
        }

        //public static bool operator ==(NuaBoolean b, NuaBoolean c)
        //{
        //    if(b == null && c == null)

        //    return b.boolean == c.boolean;
        //}

        //public static bool operator !=(NuaBoolean b, NuaBoolean c)
        //{
        //    return !(b == c);
        //}
    }
}
