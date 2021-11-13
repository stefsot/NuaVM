using System.Runtime.CompilerServices;
using NuaVM.Types;
using NuaVM.VM;

namespace NuaVM.Helpers
{
    public static class NuaTypeHelper
    {
        public static NuaTable AsTable(this NuaObject n)
        {
            return (NuaTable) n;
        }

        public static NuaFunction AsFunction(this NuaObject f)
        {
            return (NuaFunction) f;
        }

        public static NuaNumber AsNumber(this NuaObject n)
        {
            return (NuaNumber) n;
        }

        public static NuaBoolean AsBoolean(this NuaObject b)
        {
            return (NuaBoolean) b;
        }

        public static NuaString AsString(this NuaObject s)
        {
            return (NuaString) s;
        }

        public static NuaUserData AsUserData(this NuaObject s)
        {
            return (NuaUserData)s;
        }

        // attempt to retrieve the environment table from the current context
        public static NuaTable GetEnvironment(this NuaExecutionContext context)
        {
            // each lua assembly can be loaded with its own environment table
            // the environment table is represented inside the Lua assembly as the top level upvalue
            // the Nua VM does not care about the environment during execution as its only linked during the "load" code

            // retrieving the ENV from Net can be done only if the caller is a Lua function
            return context.CallingContext?.Closure?.Environment.Value as NuaTable;
        }


        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static Dictionary<NuaObject, NuaObject> GetValue(this NuaTable t)
        //{
        //    return t.Dictionary;
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static NuaClosure GetValue(this NuaFunction f)
        //{
        //    return f.Closure;
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static double GetValue(this NuaNumber n)
        //{
        //    return n.Number;
        //}
    }
}
