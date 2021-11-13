using NuaVM.Types;

namespace NuaVM.VM
{
    public class NuaCallingData
    {
        public NuaObject[] CallArgs { get; set; }

        public NuaObject[] VarArgs { get; set; }

        internal NuaCallingData()
        {
        }
    }
}
