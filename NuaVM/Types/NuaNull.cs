namespace NuaVM.Types
{
    public class NuaNull : NuaObject
    {
        // NuaObject implementation
        // -------------------------------------

        public override NuaObjectType Type => NuaObjectType.nil;

        public override object Value { get; protected set; }

        // NuaNull implementation
        // -------------------------------------

        public override string ToString()
        {
            return "nil";
        }
    }
}
