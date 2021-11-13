using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using NuaVM.Helpers;
using NuaVM.Types;
using NuaVM.Types.Exceptions;
using NuaVM.VM;

namespace NuaVM.CommonLibraries
{
    public static class NuaStringLib
    {
        public static NuaTable GetTable()
        {
            var table = new NuaTable();

            table.Set("sub", new NuaFunction(String_Sub));
            table.Set("gsub", new NuaFunction(String_GSub));
            table.Set("byte", new NuaFunction(String_Byte));
            table.Set("char", new NuaFunction(String_Char));
            table.Set("rep", new NuaFunction(String_Rep));

            return table;
        }

        public static NuaObject[] String_Sub(NuaExecutionContext context, params NuaObject[] args)
        {
            if(args.Length < 2)
                throw new NuaExecutionException(context, $"{nameof(String_GSub)}: expected 2 args got {args.Length}");

            var arg1 = args[0];
            var arg2 = args[1];

            if (arg1.Type != NuaObjectType.@string)
                throw new NuaExecutionException(context, $"wrong arg1, string expected got {arg1.Type}");

            if (arg2.Type != NuaObjectType.number)
                throw new NuaExecutionException(context, $"wrong arg2, number expected got {arg2.Type}");

            var str = arg1.AsString();
            var num = arg2.AsNumber();

            if (args.Length >= 3)
            {
                return new NuaObject[]
                {
                    new NuaString(str.String.Substring((int)num.Number - 1,
                        (int)(args[2].AsNumber().Number - num.Number) + 1))
                };
            }

            return new NuaObject[] { new NuaString(str.String.Substring((int)num.Number - 1)) };
        }

        public static NuaObject[] String_GSub(NuaExecutionContext context, params NuaObject[] args)
        {
            if (args.Length < 3)
                throw new NuaExecutionException(context, $"{nameof(String_GSub)}: expected 3 args got {args.Length}");

            var arg1 = args[0];
            var arg2 = args[1];
            var arg3 = args[2];

            var str1 = arg1.AsString();
            var str2 = arg2.AsString();
            var callback = arg3.AsFunction();

            var regex = new Regex(str2.String);
            var matches = regex.Matches(str1.String);

            var sb = new StringBuilder();

            Match match = null;

            for(var i = 0; i < matches.Count; i++)
            {
                match = matches[i];

                var results = callback.Invoke(context, new NuaString(match.Value));

                foreach (var result in results)
                {
                    switch (result.Type)
                    {
                        case NuaObjectType.number:
                        case NuaObjectType.@string:
                            sb.Append(result.Value);
                            break;

                        case NuaObjectType.nil:
                            sb.Append(match.Value);
                            break;

                        default:
                            throw new NuaExecutionException(context, $"invalid replacement value (a {result.Type})");
                    }
                }
            }

            if(match != null)
                sb.Append(str1.String, match.Index + match.Length, str1.String.Length - match.Index - match.Length);

            return new NuaObject[] {new NuaString(sb.ToString())};
        }

        public static NuaObject[] String_Byte(NuaExecutionContext context, params NuaObject[] args)
        {
            var arg1 = args[0];
            var arg2 = args[1];

            var str1 = arg1.AsString();
            var num2 = arg2.AsNumber();

            if (args.Length > 2)
            {
                var num3 = args[2].AsNumber().Number;

                //if (num3 > str1.Length)
                //    num3 = str1.Length;

                var list = new List<NuaObject>();

                for (var i = (int) num2.Number - 1; i <= (int)num3 - 1; i++)
                {
                    list.Add(new NuaNumber(str1.String[i]));
                }

                return list.ToArray();
            }

            var b = str1.String[(int) num2.Number - 1];
            return new NuaObject[] {new NuaNumber(b)};
        }

        public static NuaObject[] String_Char(NuaExecutionContext context, params NuaObject[] args)
        {
            var arg1 = args[0];
            var num1 = arg1.AsNumber();

            return new NuaObject[] {new NuaString(new string(new[] {(char) num1.Number}))};
        }

        public static NuaObject[] String_Rep(NuaExecutionContext context, params NuaObject[] args)
        {
            var arg1 = args[0];
            var arg2 = args[1];

            var str1 = arg1.AsString();
            var num2 = arg2.AsNumber();

            var sb = new StringBuilder();

            for (var i = 0; i < num2.Number; i++)
            {
                sb.Append(str1.String);
            }

            return new NuaObject[] {new NuaString(sb.ToString())};
        }
    }
}
