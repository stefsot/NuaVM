using System;
using NuaVM.Helpers;
using NuaVM.Types;
using NuaVM.Types.Exceptions;
using NuaVM.VM;

namespace NuaVM.CommonLibraries
{
    public class NuaDefaultLib
    {
        public static NuaTable GetTable()
        {
            var table = new NuaTable();

            table.Set("setmetatable", new NuaFunction(Setmetatable));
            table.Set("pcall", new NuaFunction(Pcall));
            table.Set("error", new NuaFunction(Error));

            return table;
        }

        public static NuaObject[] Setmetatable(NuaExecutionContext context, params NuaObject[] args)
        {
            if (args.Length < 2)
                return new NuaObject[0];

            if (args[0].Type != NuaObjectType.table || args[1].Type != NuaObjectType.table)
                return new NuaObject[0];

            args[0].AsTable().Metatable = args[1].AsTable();

            return new[] { args[0] };
        }

        public static NuaObject[] Pcall(NuaExecutionContext context, params NuaObject[] args)
        {
            if (args.Length == 0)
                return new NuaObject[0];

            if (args[0].Type != NuaObjectType.function)
                throw new NuaExecutionException(context, "Argument to pcall must be a function");

            var func = args[0].AsFunction();
            var argList = new NuaObject[args.Length - 1];
            Array.Copy(args, 1, argList, 0, argList.Length);

            try
            {
                _ = func.Invoke(context, argList);
                return new NuaObject[] { new NuaBoolean(true), new NuaBoolean(false) };
            }
            catch (NuaExecutionException e)
            {
                return new[] { new NuaBoolean(false), e.ErrorObject };
            }
        }

        public static NuaObject[] Error(NuaExecutionContext context, params NuaObject[] args)
        {
            if (args.Length == 0)
                throw new NuaExecutionException(context, "");

            throw new NuaExecutionException(context, args[0]);
        }
    }
}
