using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace NuaVM.LuaDisasm
{
    public abstract class LuaSerializable : ICloneable
    {
        public virtual object Tag { get; set; }

        public LuaSerializable(byte[] buffer)
        {
            using (var steam = new MemoryStream(buffer))
            {
                using (var binaryReader = new BinaryReader(steam))
                {
                    Deserialize(binaryReader);
                }
            }
        }

        public LuaSerializable(BinaryReader binaryReader)
        {
            Deserialize(binaryReader);
        }

        public LuaSerializable()
        {
        }

        protected virtual void Deserialize(BinaryReader binaryReader)
        {
            throw new NotImplementedException();
        }

        public virtual byte[] Serialize()
        {
            throw new NotImplementedException();
        }

        public void Serialize(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Serialize());
        }

        public virtual object Clone()
        {
            return Activator.CreateInstance(GetType(), Serialize());
        }
    }

    public abstract class LuaType : LuaSerializable
    {
        public LuaType()
            : base()
        {
        }

        public LuaType(byte[] buffer)
            : base(buffer)
        {
        }

        public LuaType(BinaryReader binaryReader)
            : base(binaryReader)
        {
        }

        public virtual object Value { get; set; }

        public virtual string StringValue
        {
            get { return Value.ToString(); }
            set { throw new NotSupportedException("Cannot assign string value to the current lua type!"); }
        }

        public static bool operator ==(LuaType left, LuaType right)
        {
            if ((object)left == null || (object)right == null)
            {
                return false;
            }

            if (left.Value == null || right.Value == null)
            {
                return false;
            }

            if (left.Value.GetType() == right.Value.GetType())
            {
                if (left.Value is int)
                {
                    return (int)left.Value == (int)right.Value;
                }

                if (left.Value is bool)
                {
                    return (bool)left.Value == (bool)right.Value;
                }

                if (left.Value is double)
                {
                    return BitConverter.GetBytes((double)left.Value).SequenceEqual(BitConverter.GetBytes((double)right.Value));
                }

                if (left.Value is byte[])
                {
                    return ((byte[])left.Value).SequenceEqual((byte[])right.Value);
                }
            }

            return left.Value.Equals(right.Value);
        }

        public static bool operator !=(LuaType left, LuaType right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public abstract class LuaType<TManagedType> : LuaType
    {
        public LuaType()
            : base()
        {
        }

        public LuaType(byte[] buffer)
            : base(buffer)
        {
        }

        public LuaType(BinaryReader binaryReader)
            : base(binaryReader)
        {
        }

        public Type ManagedType
        {
            get { return typeof(TManagedType); }
        }

        public virtual TManagedType ManagedValue
        {
            get { return (TManagedType)Value; }
            set { Value = value; }
        }

        private object _value;

        public override object Value
        {
            get { return _value; }
            set
            {
                if (value.GetType() != ManagedType)
                {
                    throw new InvalidCastException("Invalid value type");
                }

                _value = value;
            }
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    public class LuaNull : LuaType<object>
    {
        public LuaNull()
        {
        }

        public LuaNull(byte[] buffer)
            : base(buffer)
        {
        }

        public LuaNull(BinaryReader binaryReader)
            : base(binaryReader)
        {
        }

        public override object Value
        {
            get { return null; }
        }

        public override object ManagedValue
        {
            get { return null; }
        }

        public override string StringValue
        {
            get { return string.Empty; }
            set
            {
                /*nothing*/
            }
        }

        protected override void Deserialize(BinaryReader binaryReader)
        {
        }

        public override byte[] Serialize()
        {
            return new byte[] { };
        }

        public override object Clone()
        {
            return new LuaNull();
        }
    }

    public class LuaBoolean : LuaType<bool>
    {
        public LuaBoolean()
        {
        }

        public LuaBoolean(bool value)
        {
            ManagedValue = value;
        }

        public LuaBoolean(byte[] buffer)
            : base(buffer)
        {
        }

        public LuaBoolean(BinaryReader binaryReader)
            : base(binaryReader)
        {
        }

        public override string StringValue
        {
            get { return ManagedValue.ToString(); }
            set { ManagedValue = Convert.ToBoolean(value); }
        }

        protected override void Deserialize(BinaryReader binaryReader)
        {
            var b = binaryReader.ReadByte();
            ManagedValue = b != 0;
        }

        public override byte[] Serialize()
        {
            return new[] { (byte)(ManagedValue ? 1 : 0) };
        }

        public override object Clone()
        {
            return new LuaBoolean { Value = Value };
        }
    }

    public class LuaNumber : LuaType<double>
    {
        public LuaNumber()
        {
        }

        public LuaNumber(double value)
        {
            ManagedValue = value;
        }

        public LuaNumber(byte[] buffer)
            : base(buffer)
        {
        }

        public LuaNumber(BinaryReader binaryReader)
            : base(binaryReader)
        {
        }

        public override string StringValue
        {
            get { return ManagedValue.ToString(CultureInfo.InvariantCulture); }
            set { ManagedValue = Convert.ToDouble(value); }
        }

        protected override void Deserialize(BinaryReader binaryReader)
        {
            var buffer = binaryReader.ReadBytes(8);
            ManagedValue = BitConverter.ToDouble(buffer, 0);
        }

        public override byte[] Serialize()
        {
            return BitConverter.GetBytes(ManagedValue);
        }

        public override object Clone()
        {
            return new LuaNumber { Value = Value };
        }
    }

    public class LuaString : LuaType<byte[]>
    {
        public LuaString()
        {
            ManagedValue = new byte[0];
        }

        public LuaString(string value)
        {
            StringValue = value;
        }

        public LuaString(byte[] buffer)
            : base(buffer)
        {
        }

        public LuaString(BinaryReader binaryReader)
            : base(binaryReader)
        {
        }

        public override string StringValue
        {
            get
            {
                if (ManagedValue == null)
                {
                    return null;
                }

                return Encoding.UTF8.GetString(ManagedValue);
            }
            set { ManagedValue = Encoding.UTF8.GetBytes(value); }
        }

        protected override void Deserialize(BinaryReader binaryReader)
        {
            var buffer = binaryReader.ReadBytes(4);
            var length = BitConverter.ToInt32(buffer, 0);

            if (length > 0)
            {
                ManagedValue = binaryReader.ReadBytes(length - 1);
                binaryReader.ReadByte();
            }
        }

        public override byte[] Serialize()
        {
            if (ManagedValue == null)
            {
                return new byte[] { 0, 0, 0, 0 };
            }

            return BitConverter.GetBytes(ManagedValue.Length + 1).
                Concat(ManagedValue).
                Concat(new byte[] { 0 }).
                ToArray();
        }

        public override string ToString()
        {
            return StringValue;
        }

        public override object Clone()
        {
            return new LuaString(Serialize());
            //return new LuaString { Value = ((byte[]) Value).ToArray() };

            //if (ManagedType.IsValueType || ManagedType == typeof(string))
            //{
            //    return new LuaType<TManagedType> {Value = Value};
            //}

            //return new LuaType<TManagedType>(Serialize());

            //return new LuaType<TManagedType>(Serialize());
            //----------
            //var newValue = (ManagedType.IsValueType || ManagedType == typeof(string))
            //    ? Value
            //    : Activator.CreateInstance<TManagedType>(); //??

            //return new LuaType<TManagedType>() { Value = newValue };
            //----------

            //return new LuaType<TManagedType> {Value = Value};
        }
    }

    public class LuaInt : LuaType<int>
    {
        public LuaInt()
        {
        }

        public LuaInt(int value)
        {
            ManagedValue = value;
        }

        public LuaInt(byte[] buffer)
            : base(buffer)
        {
        }

        public LuaInt(BinaryReader binaryReader)
            : base(binaryReader)
        {
        }

        public override string StringValue
        {
            get { return ManagedValue.ToString(CultureInfo.InvariantCulture); }
            set { ManagedValue = Convert.ToInt32(value); }
        }

        protected override void Deserialize(BinaryReader binaryReader)
        {
            var buffer = binaryReader.ReadBytes(4);
            ManagedValue = BitConverter.ToInt32(buffer, 0);
        }

        public override byte[] Serialize()
        {
            return BitConverter.GetBytes(ManagedValue);
        }

        public override object Clone()
        {
            return new LuaInt { Value = Value };
        }
    }

    public class LuaConstant : LuaSerializable
    {
        public enum ConstantType : byte
        {
            Null = 0,
            Boolean = 1,
            Number = 3,
            String = 4
        }

        private ConstantType _ctype;

        public ConstantType CType
        {
            get { return _ctype; }
            internal set
            {
                if (!Enum.IsDefined(typeof(ConstantType), value))
                {
                    throw new TypeLoadException(string.Format("Invalid constant type {0}", value));
                }

                _ctype = value;
            }
        }

        public Type ManagedType
        {
            get
            {
                switch (CType)
                {
                    case ConstantType.Null:
                        return typeof(object);
                    case ConstantType.Boolean:
                        return typeof(bool);
                    case ConstantType.Number:
                        return typeof(double);
                    case ConstantType.String:
                        return typeof(byte[]);
                }

                return null;
            }
        }

        public LuaType LuaType { get; internal set; }

        public object Value
        {
            get { return LuaType.Value; }
            set { LuaType.Value = value; }
        }

        public string StringValue
        {
            get { return LuaType.StringValue; }
            set { LuaType.StringValue = value; }
        }

        public LuaConstant(ConstantType ctype)
        {
            CType = ctype;

            LuaType luaType = null;
            switch (CType)
            {
                case ConstantType.Null:
                    luaType = new LuaNull();
                    break;
                case ConstantType.Boolean:
                    luaType = new LuaBoolean();
                    break;
                case ConstantType.Number:
                    luaType = new LuaNumber();
                    break;
                case ConstantType.String:
                    luaType = new LuaString();
                    break;
            }

            LuaType = luaType;
        }

        public LuaConstant(LuaType luaType)
        {
            if (luaType == null)
            {
                throw new ArgumentNullException("luaType");
            }

            if (luaType is LuaNull)
            {
                CType = ConstantType.Null;
            }
            else if (luaType is LuaBoolean)
            {
                CType = ConstantType.Boolean;
            }
            else if (luaType is LuaNumber)
            {
                CType = ConstantType.Number;
            }
            else if (luaType is LuaString)
            {
                CType = ConstantType.String;
            }
            else
            {
                throw new Exception(string.Format("Invalid lua type: {0}", luaType.GetType().Name));
            }

            LuaType = luaType;
        }

        public LuaConstant(byte[] buffer)
            : base(buffer)
        {
        }

        public LuaConstant(BinaryReader binaryReader)
            : base(binaryReader)
        {
        }

        protected override void Deserialize(BinaryReader binaryReader)
        {
            CType = (ConstantType)binaryReader.ReadByte();

            LuaType luaType;
            switch (CType)
            {
                case ConstantType.Null:
                    luaType = new LuaNull(binaryReader);
                    break;
                case ConstantType.Boolean:
                    luaType = new LuaBoolean(binaryReader);
                    break;
                case ConstantType.Number:
                    luaType = new LuaNumber(binaryReader);
                    break;
                case ConstantType.String:
                    luaType = new LuaString(binaryReader);
                    break;
                default:
                    throw new TypeLoadException(string.Format("Byte 0x{0} is invalid!", CType));
            }

            LuaType = luaType;
        }

        public override byte[] Serialize()
        {
            return new[] { (byte)CType }.
                Concat(LuaType.Serialize()).
                ToArray();
        }

        public override object Clone()
        {
            return new LuaConstant(LuaType.Clone() as LuaType);
        }

        public override string ToString()
        {
            var value = CType == ConstantType.String ? Encoding.UTF8.GetString((byte[])Value) : Value.ToString();
            return string.Format("{0} {1}", CType, value);
        }
    }

    public class LuaList<T> : List<T>, ICloneable where T : LuaSerializable
    {
        public LuaList(byte[] buffer)
        {
            Deserialize(new BinaryReader(new MemoryStream(buffer)));
        }

        public LuaList(BinaryReader binaryReader)
        {
            Deserialize(binaryReader);
        }

        public LuaList()
        {
        }

        protected void Deserialize(BinaryReader binaryReader)
        {
            var length = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);

            for (var i = 0; i < length; i++)
            {
                Add((T)Activator.CreateInstance(typeof(T), binaryReader));
            }
        }

        public byte[] Serialize()
        {
            return BitConverter.GetBytes(Count).Concat(this.SelectMany(t => t.Serialize())).ToArray();
        }

        public object Clone()
        {
            var obj = new LuaList<T>();
            obj.AddRange(this.Select(t => t.Clone()).Cast<T>());
            return obj;
        }
    }

    public class LuaAssemblyHeader : LuaSerializable
    {
        public const int Length = 18;

        public byte[] LuaSignature { get; private set; }

        public byte LuaVersion { get; set; }

        public byte BigIndian { get; set; }

        public byte[] SystemParams { get; private set; } //intlen, instrlen, sizelen, numlen, usesfloat, 

        public byte[] Unknown1 { get; private set; }

        public bool IsValid
        {
            get
            {
                if (LuaSignature == null || LuaSignature.Length < 4)
                {
                    return false;
                }

                return LuaSignature[0] == 0x1B && LuaSignature[1] == 0x4C && LuaSignature[2] == 0x75 && LuaSignature[3] == 0x61;
            }
        }

        protected override void Deserialize(BinaryReader binaryReader)
        {
            LuaSignature = binaryReader.ReadBytes(4);
            LuaVersion = binaryReader.ReadByte();
            BigIndian = binaryReader.ReadByte();
            SystemParams = binaryReader.ReadBytes(6);
            Unknown1 = binaryReader.ReadBytes(6);
        }

        public override byte[] Serialize()
        {
            var list = new List<byte>();

            list.AddRange(LuaSignature);
            list.Add(LuaVersion);
            list.Add(BigIndian);
            list.AddRange(SystemParams);
            list.AddRange(Unknown1);

            return list.ToArray();
        }

        public LuaAssemblyHeader(byte[] buffer)
            : base(buffer)
        {
        }

        public LuaAssemblyHeader(BinaryReader binaryReader)
            : base(binaryReader)
        {
        }

        public LuaAssemblyHeader()
        {
            LuaSignature = new byte[] { 0x1B, 0x4C, 0x75, 0x61 };
            LuaVersion = 0x52;
            BigIndian = 0x0;
            SystemParams = new byte[] { 0x01, 0x04, 0x04, 0x04, 0x8, 0x0 };
            Unknown1 = new byte[] { 0x19, 0x93, 0x0D, 0x0A, 0x1A, 0x0A };
        }

        public override object Clone()
        {
            return base.Clone();
        }
    }

    public class LuaFunctionHeader : LuaSerializable
    {
        public const int Length = 11;

        public uint SourceLineStart { get; set; }
        public uint SourceLineEnd { get; set; }

        public byte NumOfArgs { get; set; }
        public byte VarargFlag { get; set; }
        public byte NumOfRegisters { get; set; }

        protected override void Deserialize(BinaryReader binaryReader)
        {
            SourceLineStart = BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
            SourceLineEnd = BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
            NumOfArgs = binaryReader.ReadByte();
            VarargFlag = binaryReader.ReadByte();
            NumOfRegisters = binaryReader.ReadByte();
        }

        public override byte[] Serialize()
        {
            var list = new List<byte>();

            list.AddRange(BitConverter.GetBytes(SourceLineStart));
            list.AddRange(BitConverter.GetBytes(SourceLineEnd));
            list.Add(NumOfArgs);
            list.Add(VarargFlag);
            list.Add(NumOfRegisters);

            return list.ToArray();
        }

        public LuaFunctionHeader(byte[] buffer)
            : base(buffer)
        {
            Deserialize(new BinaryReader(new MemoryStream(buffer)));
        }

        public LuaFunctionHeader(BinaryReader binaryReader)
            : base(binaryReader)
        {
        }

        public LuaFunctionHeader()
        {
            VarargFlag = 1;
        }

        public override object Clone()
        {
            return base.Clone();
        }
    }

    public class Upvalue : LuaSerializable
    {
        public const int Length = 2;

        public byte Position { get; set; }
        public byte IsInStack { get; set; }

        protected override void Deserialize(BinaryReader binaryReader)
        {
            IsInStack = binaryReader.ReadByte();
            Position = binaryReader.ReadByte();
        }

        public override byte[] Serialize()
        {
            return new[] { IsInStack, Position };
        }

        public Upvalue(byte[] buffer)
            : base(buffer)
        {
        }

        public Upvalue(BinaryReader binaryReader)
            : base(binaryReader)
        {
        }

        public Upvalue()
        {
        }

        public override object Clone()
        {
            return new Upvalue() { Position = Position, IsInStack = IsInStack };
        }
    }

    public class LuaOpcode : LuaSerializable
    {
        public const int Length = 4;

        public enum Opcode : byte //6 bit
        {
            OP_MOVE = 0,
            OP_LOADK = 1,
            OP_LOADKX = 2,
            OP_LOADBOOL = 3,
            OP_LOADNIL = 4,
            OP_GETUPVAL = 5,
            OP_GETTABUP = 6,
            OP_GETTABLE = 7,
            OP_SETTABUP = 8,
            OP_SETUPVAL = 9,
            OP_SETTABLE = 10,
            OP_NEWTABLE = 11,
            OP_SELF = 12,
            OP_ADD = 13,
            OP_SUB = 14,
            OP_MUL = 15,
            OP_DIV = 16,
            OP_MOD = 17,
            OP_POW = 18,
            OP_UNM = 19,
            OP_NOT = 20,
            OP_LEN = 21,
            OP_CONCAT = 22,
            OP_JMP = 23,
            OP_EQ = 24,
            OP_LT = 25,
            OP_LE = 26,
            OP_TEST = 27,
            OP_TESTSET = 28,
            OP_CALL = 29,
            OP_TAILCALL = 30,
            OP_RETURN = 31,
            OP_FORLOOP = 32,
            OP_FORPREP = 33,
            OP_TFORCALL = 34,
            OP_TFORLOOP = 35,
            OP_SETLIST = 36,
            OP_CLOSURE = 37,
            OP_VARARG = 38,
            OP_EXTRAARG = 39,
            LAST = 39,
            MAX = 63,
            INVALID = 50,
        }

        public enum OpcodeType
        {
            Unknown,
            A,
            AB,
            ABx,
            ABC,
            AsBx,
            Ax,
            AC,
        }

        public enum Operand
        {
            A, //8 bit
            B, //9 bit
            Bx, //18 bit
            sBx, //18 bit
            C, //9 bit
            Ax, //26 bit
        }

        #region definitions

        public static readonly Opcode[] A_Opcodes = { Opcode.OP_LOADKX };
        public static readonly Opcode[] AB_Opcodes = { Opcode.OP_MOVE, Opcode.OP_LOADNIL, Opcode.OP_GETUPVAL, Opcode.OP_SETUPVAL, Opcode.OP_UNM, Opcode.OP_NOT, Opcode.OP_LEN, Opcode.OP_RETURN, Opcode.OP_VARARG };
        public static readonly Opcode[] ABx_Opcodes = { Opcode.OP_LOADK, Opcode.OP_CLOSURE };
        public static readonly Opcode[] ABC_Opcodes = { Opcode.OP_LOADBOOL, Opcode.OP_GETTABUP, Opcode.OP_GETTABLE, Opcode.OP_SETTABUP, Opcode.OP_SETTABLE, Opcode.OP_NEWTABLE,
                                                 Opcode.OP_SELF, Opcode.OP_ADD, Opcode.OP_SUB, Opcode.OP_MUL, Opcode.OP_DIV, Opcode.OP_MOD, Opcode.OP_POW, Opcode.OP_CONCAT,
                                                 Opcode.OP_EQ, Opcode.OP_LT, Opcode.OP_LE, Opcode.OP_TESTSET, Opcode.OP_CALL, Opcode.OP_TAILCALL, Opcode.OP_SETLIST};
        public static readonly Opcode[] AsBx_Opcodes = { Opcode.OP_JMP, Opcode.OP_FORLOOP, Opcode.OP_FORPREP, Opcode.OP_TFORLOOP };
        public static readonly Opcode[] Ax_Opcodes = { Opcode.OP_EXTRAARG };
        public static readonly Opcode[] AC_Opcodes = { Opcode.OP_TEST, Opcode.OP_TFORCALL };

        #endregion

        public Opcode Op { get; set; }
        public readonly Dictionary<Operand, int> Operands = new Dictionary<Operand, int>();

        public OpcodeType Type
        {
            get
            {
                if (A_Opcodes.Contains(Op))
                    return OpcodeType.A;
                if (AB_Opcodes.Contains(Op))
                    return OpcodeType.AB;
                if (ABx_Opcodes.Contains(Op))
                    return OpcodeType.ABx;
                if (ABC_Opcodes.Contains(Op))
                    return OpcodeType.ABC;
                if (AsBx_Opcodes.Contains(Op))
                    return OpcodeType.AsBx;
                if (Ax_Opcodes.Contains(Op))
                    return OpcodeType.Ax;
                if (AC_Opcodes.Contains(Op))
                    return OpcodeType.AC;

                return OpcodeType.Unknown;
            }
        }

        public bool IsValid
        {
            get { return !(Op > Opcode.LAST); }
        }

        public bool HasOperand(Operand a)
        {
            var type = Type;

            if (!Enum.IsDefined(typeof(OpcodeType), type) || type == OpcodeType.Unknown)
            {
                return false;
            }

            switch (a)
            {
                case Operand.A:
                    return type != OpcodeType.Ax;

                case Operand.Ax:
                    return type == OpcodeType.Ax;

                case Operand.B:
                    return type == OpcodeType.AB || type == OpcodeType.ABC;

                case Operand.Bx:
                    return type == OpcodeType.ABx;

                case Operand.C:
                    return type == OpcodeType.ABC || type == OpcodeType.AC;

                case Operand.sBx:
                    return type == OpcodeType.AsBx;

                default:
                    return false;
            }
        }

        public Operand[] GetOperands()
        {
            return Enum.GetValues(typeof(Operand)).Cast<Operand>().Where(HasOperand).ToArray();
        }

        public int GetOperandValue(Operand operand)
        {
            if (!Enum.IsDefined(typeof(Operand), operand))
            {
                return 0;
            }

            int value;
            Operands.TryGetValue(operand, out value);

            return value;
        }

        public int GetOperandValue(int operandIndex)
        {
            try
            {
                return GetOperandValue(GetOperands()[operandIndex]);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public void SetOperandValue(Operand operand, int value)
        {
            if (!Enum.IsDefined(typeof(Operand), operand))
            {
                return;
            }

            Operands[operand] = value;
        }

        public void SetOperandValue(int index, int value)
        {
            SetOperandValue(GetOperands()[index], value);
        }

        public byte[] Assemble()
        {
            int Lshift(int num1, int num2)
            {
                if (num2 >= 32)
                {
                    return 0;
                }

                return num1 << num2;
            }

            long Mod(long num1, long num2)
            {
                return (num1 - (long)Math.Floor(num1 / (double)num2) * num2);
            }

            long Not(int num1) => Mod(-1L - num1, uint.MaxValue + 1L);

            Func<Operand, int, long> put = (op, field) =>
            {
                switch (op)
                {
                    case Operand.A:
                        return (field << 6) & Not(Lshift(-1, (8 + 6)));
                    case Operand.Ax:
                        return ((-1 - field) << 6) & Not(Lshift(-1, (9 + 9 + 8 + 6)));
                    case Operand.B:
                        return ((field < 0 ? 255 - field : field) << (6 + 8 + 9)) & Not(Lshift(-1, (9 + 6 + 8 + 9)));
                    case Operand.Bx:
                        return ((-1 - field) << (6 + 8)) & Not(Lshift(-1, (9 + 9 + 6 + 8)));
                    case Operand.sBx:
                        return ((131071 + field) << (6 + 8)) & Not(Lshift(-1, (9 + 9 + 6 + 8)));
                    case Operand.C:
                        return ((field < 0 ? 255 - field : field) << (6 + 8)) & Not(Lshift(-1, (9 + 6 + 8)));
                }

                return 0;
            };

            var operands = GetOperands();
            var opcode = (long)Op;
            opcode = operands.Aggregate(opcode, (current, operand) => current | put(operand, Operands[operand]));

            return BitConverter.GetBytes((int)opcode);
        }

        private int Decode(uint num, int n, int? n2 = null)
        {
            if (n2.HasValue)
            {
                var total = 0;
                var digitn = 0;

                for (var i = n; i <= n2; i++)
                {
                    total += (int)Math.Pow(2, digitn) * Decode(num, i);
                    digitn += 1;
                }

                return total;
            }

            var pn = (long)Math.Pow(2, n - 1);
            return (num % (pn + pn) >= pn) ? 1 : 0;
        }

        protected override void Deserialize(BinaryReader binaryReader)
        {
            var bytes = binaryReader.ReadBytes(4);
            var number = BitConverter.ToUInt32(bytes, 0);

            Op = (Opcode)(number & 0b00111111u); //Convert.ToUInt32("00111111", 2)
            Operands[Operand.A] = (int)(number >> 6 & 0b0011111111u); // Convert.ToUInt32("0011111111", 2)
            Operands[Operand.B] = (number & 0b10000000000000000000000000000000u) > 0 // Convert.ToUInt32("10000000000000000000000000000000", 2)
                ? (int)(number >> 23 & 0b0011111111u) * -1 - 1 // Convert.ToUInt32("0011111111", 2)
                : (int)(number >> 23 & 0b0011111111u); // Convert.ToUInt32("0011111111", 2)
            Operands[Operand.C] = (number & 0b00000000010000000000000000000000u) > 0 // Convert.ToUInt32("00000000010000000000000000000000", 2)
                ? (int)(number >> 14 & 0b0011111111u) * -1 - 1 // Convert.ToUInt32("0011111111", 2)
                : (int)(number >> 14 & 0b0011111111u); // Convert.ToUInt32("0011111111", 2)
            Operands[Operand.Bx] = (int)(number >> 14 & 0b00111111111111111111) * -1 - 1; // Convert.ToUInt32("00111111111111111111", 2)
            Operands[Operand.sBx] = Decode(number, 15, 32) - 131071;
            Operands[Operand.Ax] = (int)(number >> 6 & 0b0011111111111111111111111111u) * -1 - 1; // Convert.ToUInt32("0011111111111111111111111111", 2)
        }

        public override byte[] Serialize()
        {
            return Assemble();
        }

        public LuaOpcode(byte[] buffer)
            : base(buffer)
        {
        }

        public LuaOpcode(BinaryReader binaryReader)
            : base(binaryReader)
        {
        }

        public LuaOpcode(Opcode opcode)
        {
            Op = opcode;

            foreach (var operand in Enum.GetValues(typeof(Operand)))
            {
                Operands[(Operand)operand] = 0;
            }
        }

        public LuaOpcode()
        {
        }

        public override string ToString()
        {
            if (!IsValid)
            {
                return "INVALID";
            }

            var str = Op.ToString().Replace("OP_", "");
            return GetOperands().Aggregate(str, (current, operand) => current + string.Format(" {0}={1}", operand, GetOperandValue(operand)));
        }

        public override object Clone()
        {
            var opcode = new LuaOpcode(Op);
            foreach (var operand in GetOperands())
            {
                opcode.SetOperandValue(operand, GetOperandValue(operand));
            }

            return opcode;
        }
    }

    public class LuaLocal : LuaSerializable
    {
        public LuaString Name { get; set; }
        public uint ScopeStart { get; set; }
        public uint ScopeEnd { get; set; }

        protected override void Deserialize(BinaryReader binaryReader)
        {
            Name = new LuaString(binaryReader);
            ScopeStart = BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
            ScopeEnd = BitConverter.ToUInt32(binaryReader.ReadBytes(4), 0);
        }

        public override byte[] Serialize()
        {
            return Name.Serialize().
                Concat(BitConverter.GetBytes(ScopeStart)).
                Concat(BitConverter.GetBytes(ScopeEnd)).
                ToArray();
        }

        public LuaLocal(byte[] buffer)
            : base(buffer)
        {
        }

        public LuaLocal(BinaryReader binaryReader)
            : base(binaryReader)
        {
        }

        public LuaLocal()
        {
            Name = new LuaString();
        }

        public override object Clone()
        {
            return new LuaLocal() { Name = (LuaString)Name.Clone(), ScopeStart = ScopeStart, ScopeEnd = ScopeEnd };
        }
    }

    public class LuaFunction : LuaSerializable
    {
        public LuaFunctionHeader Header { get; set; }

        public LuaList<LuaOpcode> Opcodes { get; set; }

        public LuaList<LuaConstant> Constants { get; set; }

        public LuaList<LuaFunction> FunctionPrototypes { get; set; }

        public LuaList<Upvalue> Upvalues { get; set; }

        public LuaString SourceCode { get; set; }

        public LuaList<LuaInt> SourceLines { get; set; }

        public LuaList<LuaLocal> Locals { get; set; }

        public LuaList<LuaString> UpvalueNames { get; set; }

        protected override void Deserialize(BinaryReader binaryReader)
        {
            Header = new LuaFunctionHeader(binaryReader);
            Opcodes = new LuaList<LuaOpcode>(binaryReader);
            Constants = new LuaList<LuaConstant>(binaryReader);
            FunctionPrototypes = new LuaList<LuaFunction>(binaryReader);
            Upvalues = new LuaList<Upvalue>(binaryReader);
            SourceCode = new LuaString(binaryReader);
            SourceLines = new LuaList<LuaInt>(binaryReader);
            Locals = new LuaList<LuaLocal>(binaryReader);
            UpvalueNames = new LuaList<LuaString>(binaryReader);
        }

        public override byte[] Serialize()
        {
            return Header.Serialize().
                Concat(Opcodes.Serialize()).
                Concat(Constants.Serialize()).
                Concat(FunctionPrototypes.Serialize()).
                Concat(Upvalues.Serialize()).
                Concat(SourceCode.Serialize()).
                Concat(SourceLines.Serialize()).
                Concat(Locals.Serialize()).
                Concat(UpvalueNames.Serialize()).
                ToArray();
        }

        public LuaFunction(byte[] buffer)
            : base(buffer)
        {
        }

        public LuaFunction(BinaryReader binaryReader)
            : base(binaryReader)
        {
        }

        public LuaFunction()
        {
            Header = new LuaFunctionHeader();
            Opcodes = new LuaList<LuaOpcode>();
            Constants = new LuaList<LuaConstant>();
            FunctionPrototypes = new LuaList<LuaFunction>();
            Upvalues = new LuaList<Upvalue>();
            SourceCode = new LuaString();
            SourceLines = new LuaList<LuaInt>();
            Locals = new LuaList<LuaLocal>();
            UpvalueNames = new LuaList<LuaString>();
        }

        public override object Clone()
        {
            return new LuaFunction
            {
                Header = (LuaFunctionHeader)Header.Clone(),
                Opcodes = (LuaList<LuaOpcode>)Opcodes.Clone(),
                Constants = (LuaList<LuaConstant>)Constants.Clone(),
                FunctionPrototypes = (LuaList<LuaFunction>)FunctionPrototypes.Clone(),
                Upvalues = (LuaList<Upvalue>)Upvalues.Clone(),
                SourceCode = (LuaString)SourceCode.Clone(),
                SourceLines = (LuaList<LuaInt>)SourceLines.Clone(),
                Locals = (LuaList<LuaLocal>)Locals.Clone(),
                UpvalueNames = (LuaList<LuaString>)UpvalueNames.Clone()
            };
        }
    }

    public class LuaAssembly : LuaSerializable
    {
        public static LuaAssembly EmptyAssembly
        {
            get
            {
                return
                    new LuaAssembly(
                        Convert.FromBase64String(
                            "G0x1YVIAAQQEBAgAGZMNChoKAAAAAAAAAAAAAQIBAAAAHwCAAAAAAAAAAAAAAQAAAAEAAAAAAAAAAAAAAAAAAAAAAA=="));
            }
        }

        public LuaAssemblyHeader Header { get; set; }

        public LuaFunction EntryPoint { get; set; }

        protected override void Deserialize(BinaryReader binaryReader)
        {
            Header = new LuaAssemblyHeader(binaryReader);
            EntryPoint = new LuaFunction(binaryReader);
        }

        public override byte[] Serialize()
        {
            return Header.Serialize().Concat(EntryPoint.Serialize()).ToArray();
        }

        public LuaAssembly(byte[] buffer)
            : base(buffer)
        {
        }

        public LuaAssembly(BinaryReader binaryReader)
            : base(binaryReader)
        {
        }

        public override object Clone()
        {
            var assembly = EmptyAssembly;
            assembly.Header = (LuaAssemblyHeader)Header.Clone();
            assembly.EntryPoint = (LuaFunction)EntryPoint.Clone();
            return assembly;
        }
    }
}