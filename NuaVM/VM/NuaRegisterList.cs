using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NuaVM.Types;

namespace NuaVM.VM
{
    public class NuaRegisterList : List<NuaRegister>
    {
        public new NuaObject this[int index]
        {
            get => Get(index).Value;
            set => Get(index).Value = value;
        }

        public NuaRegister GetRegister(int index)
        {
            return Get(index);
        }

        public void CloseRegister(int index)
        {
            base[index] = new NuaRegister(Get(index));
        }

        private NuaRegister Get(int index)
        {
            return base[index] ?? (base[index] = new NuaRegister());
        }

        public NuaRegisterList(int size) : base(size)
        {
        }
    }
}
