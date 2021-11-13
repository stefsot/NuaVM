using System;
using System.Runtime.CompilerServices;
using NuaVM.LuaDisasm;
using NuaVM.Types;
using NuaVM.VM;

namespace NuaVM.Helpers
{
    public static class ProcessorHelper
    {
        public static bool IsLuaIndex(int i)
        {
            return i < 0;
        }

        public static int LuaIndexToZeroIndex(int i)
        {
            if (IsLuaIndex(i))
                return i * -1 - 1;

            return i;
        }

        public static NuaObject GetConstant(NuaClosure closure, int i)
        {
            var constant = closure.Prototype.Constants[LuaIndexToZeroIndex(i)];

            switch (constant.CType)
            {
                case LuaConstant.ConstantType.Null:
                    return new NuaNull();

                case LuaConstant.ConstantType.Boolean:
                    return new NuaBoolean((bool) constant.Value);

                case LuaConstant.ConstantType.Number:
                    return new NuaNumber((double) constant.Value);

                case LuaConstant.ConstantType.String:
                    return new NuaString(constant.StringValue);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static NuaObject GetRK(NuaExecutionContext context, int i)
        {
            if (IsLuaIndex(i))
            {
                return GetConstant(context.Closure, i);
            }

            return context.Registers[i];
        }

        public static NuaNumber AsArithmetic(NuaObject value)
        {
            switch (value.Type)
            {
                case NuaObjectType.@string:
                    if (double.TryParse((string) value.Value, out var num))
                        return new NuaNumber(num);

                    return null;

                case NuaObjectType.number:
                    return (NuaNumber) value;

                default:
                    return null;
            }
        }
    }
}
