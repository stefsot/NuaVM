using System;
using System.Globalization;

namespace NuaVM.Types
{
    public class NuaNumber : NuaObject
    {
        // NuaObject implementation
        // -------------------------------------

        public override NuaObjectType Type => NuaObjectType.number;

        public override object Value { get; protected set; }

        // NuaNumber implementation
        // -------------------------------------

        public double Number
        {
            get => (double) Value;
            set => Value = value;
        }

        public NuaNumber(double number)
        {
            Number = number;
        }

        public override string ToString()
        {
            return Number.ToString(CultureInfo.InvariantCulture);
        }

        public static NuaNumber operator +(NuaNumber b, NuaNumber c)
        {
           return new NuaNumber(b.Number + c.Number);
        }

        public static NuaNumber operator -(NuaNumber b, NuaNumber c)
        {
            return new NuaNumber(b.Number - c.Number);
        }

        public static NuaNumber operator /(NuaNumber b, NuaNumber c)
        {
            return new NuaNumber(b.Number / c.Number);
        }

        public static NuaNumber operator *(NuaNumber b, NuaNumber c)
        {
            return new NuaNumber(b.Number * c.Number);
        }

        public static NuaNumber operator ^(NuaNumber b, NuaNumber c)
        {
            return new NuaNumber(Math.Pow(b.Number, c.Number));
        }

        public static NuaNumber operator %(NuaNumber b, NuaNumber c)
        {
            return new NuaNumber(b.Number % c.Number);
        }

        public static NuaNumber operator -(NuaNumber b)
        {
            return new NuaNumber(-b.Number);
        }
    }
}
