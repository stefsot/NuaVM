using NuaVM.Types;

namespace NuaVM.VM
{
    public class NuaUpvalue
    {
        public bool IsClosed { get; private set; }

        public NuaRegister Register { get; private set; }

        public NuaObject Value
        {
            get => Register.Value;
            set => Register.Value = value;
        }

        public void Close()
        {
            IsClosed = true;
            Register = new NuaRegister(Register);
        }

        public NuaUpvalue(NuaRegister register)
        {
            Register = register;
        }
    }
}
