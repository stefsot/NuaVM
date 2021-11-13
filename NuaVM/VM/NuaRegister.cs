using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NuaVM.Types;

namespace NuaVM.VM
{
    public class NuaRegister
    {
        private NuaObject _value;

        public NuaObject Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value == null)
                    value = new NuaNull();

                _value = value;
            }
        }

        public NuaRegister()
        {
            Value = new NuaNull();
        }

        public NuaRegister(NuaObject value)
        {
            Value = value;
        }

        public NuaRegister(NuaRegister nuaRegister) : this(nuaRegister.Value)
        {
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
