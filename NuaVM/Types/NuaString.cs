namespace NuaVM.Types
{
    public class NuaString : NuaObject
    {
        // NuaObject implementation
        // -------------------------------------

        public override NuaObjectType Type => NuaObjectType.@string;

        public override object Value { get; protected set; }

        // NuaString implementation
        // -------------------------------------

        public string String
        {
            get => (string)Value;
            set => Value = value;
        }

        public override int Length => String.Length;

        public NuaString(string @string)
        {
            String = @string;
        }

        public override string ToString()
        {
            return String;
        }

        public static implicit operator NuaString(string @string)
        {
            return new NuaString(@string);
        }
    }
}
