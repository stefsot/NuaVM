using System;
using System.Collections.Generic;
using System.Text;
using NuaVM.Helpers;
using NuaVM.LuaDisasm;
using NuaVM.Types;
using NuaVM.Types.Exceptions;
using NuaVM.VM;

namespace NuaVM.OpcodeProcessor.Implementations
{
    public class DefaultOpcodeProcessor : OpcodeProcessorDefinition
    {
        // ReSharper disable once InconsistentNaming
        public const int FPF = 50;


        public override void SetupProcessors()
        {
            Processors[(int)LuaOpcode.Opcode.OP_MOVE] = processor_Move;
            Processors[(int)LuaOpcode.Opcode.OP_LOADK] = processor_LoadK;
            Processors[(int)LuaOpcode.Opcode.OP_LOADKX] = processor_LoadKX;
            Processors[(int)LuaOpcode.Opcode.OP_LOADBOOL] = processor_LoadBool;
            Processors[(int)LuaOpcode.Opcode.OP_LOADNIL] = processor_LoadNil;
            Processors[(int)LuaOpcode.Opcode.OP_GETUPVAL] = processor_GetUpVal;
            Processors[(int)LuaOpcode.Opcode.OP_GETTABUP] = processor_GetTabUp;
            Processors[(int)LuaOpcode.Opcode.OP_GETTABLE] = processor_GetTable;
            Processors[(int)LuaOpcode.Opcode.OP_SETTABUP] = processor_SetTabUp;
            Processors[(int)LuaOpcode.Opcode.OP_SETUPVAL] = processor_SetUpVal;
            Processors[(int)LuaOpcode.Opcode.OP_SETTABLE] = processor_SetTable;
            Processors[(int)LuaOpcode.Opcode.OP_NEWTABLE] = processor_NewTable;
            Processors[(int)LuaOpcode.Opcode.OP_SELF] = processor_Self;
            Processors[(int)LuaOpcode.Opcode.OP_ADD] = processor_Add;
            Processors[(int)LuaOpcode.Opcode.OP_SUB] = processor_Sub;
            Processors[(int)LuaOpcode.Opcode.OP_MUL] = processor_Mul;
            Processors[(int)LuaOpcode.Opcode.OP_DIV] = processor_Div;
            Processors[(int)LuaOpcode.Opcode.OP_MOD] = processor_Mod;
            Processors[(int)LuaOpcode.Opcode.OP_POW] = processor_Pow;
            Processors[(int)LuaOpcode.Opcode.OP_UNM] = processor_Unm;
            Processors[(int)LuaOpcode.Opcode.OP_NOT] = processor_Not;
            Processors[(int)LuaOpcode.Opcode.OP_LEN] = processor_Len;
            Processors[(int)LuaOpcode.Opcode.OP_CONCAT] = processor_Concat;
            Processors[(int)LuaOpcode.Opcode.OP_JMP] = processor_Jmp;
            Processors[(int)LuaOpcode.Opcode.OP_EQ] = processor_Eq;
            Processors[(int)LuaOpcode.Opcode.OP_LT] = processor_Lt;
            Processors[(int)LuaOpcode.Opcode.OP_LE] = processor_Le;
            Processors[(int)LuaOpcode.Opcode.OP_TEST] = processor_Test;
            Processors[(int)LuaOpcode.Opcode.OP_TESTSET] = processor_TestSet;

            Processors[(int)LuaOpcode.Opcode.OP_CALL] = processor_Call;
            Processors[(int)LuaOpcode.Opcode.OP_TAILCALL] = processor_TailCall;

            Processors[(int)LuaOpcode.Opcode.OP_RETURN] = processor_Return;
            Processors[(int)LuaOpcode.Opcode.OP_FORLOOP] = processor_ForLoop;
            Processors[(int)LuaOpcode.Opcode.OP_FORPREP] = processor_ForPrep;


            Processors[(int)LuaOpcode.Opcode.OP_SETLIST] = processor_SetList;
            Processors[(int)LuaOpcode.Opcode.OP_CLOSURE] = processor_Closure;
            Processors[(int)LuaOpcode.Opcode.OP_VARARG] = processor_VarArg;
        }

        private NuaObject[] processor_Move(NuaExecutionContext context, ref int pc)
        {
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];

            registers[a] = registers[b];

            return null;
        }

        private NuaObject[] processor_LoadK(NuaExecutionContext context, ref int pc)
        {
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var bx = opcode.Operands[LuaOpcode.Operand.Bx];

            registers[a] = ProcessorHelper.GetConstant(context.Closure, bx);
            return null;
        }

        private NuaObject[] processor_LoadKX(NuaExecutionContext context, ref int pc)
        {
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];
            var next = function.Opcodes[++pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var ax = next.Operands[LuaOpcode.Operand.Ax];

            registers[a] = ProcessorHelper.GetConstant(context.Closure, ProcessorHelper.LuaIndexToZeroIndex(ax));

            return null;
        }

        private NuaObject[] processor_LoadBool(NuaExecutionContext context, ref int pc)
        {
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            registers[a] = new NuaBoolean(b > 0);

            if (c > 0)
            {
                pc++;
            }

            return null;
        }

        private NuaObject[] processor_LoadNil(NuaExecutionContext context, ref int pc)
        {
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];

            for (var j = a; j <= a + b; j++)
                registers[j] = new NuaNull();

            return null;
        }

        private NuaObject[] processor_GetUpVal(NuaExecutionContext context, ref int pc)
        {
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];

            registers[a] = context.Closure.Upvalues[ProcessorHelper.LuaIndexToZeroIndex(b)].Value;

            return null;
        }

        private NuaObject[] processor_GetTabUp(NuaExecutionContext context, ref int pc)
        {
            // R(A) := UpValue[B][RK(C)]     
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];
            var cValue = ProcessorHelper.GetRK(context, c);

            var upvalue = context.Closure.Upvalues[ProcessorHelper.LuaIndexToZeroIndex(b)].Value;

            switch (upvalue.Type)
            {
                case NuaObjectType.table:
                    {
                        var table = upvalue.AsTable();
                        registers[a] = table[context, cValue];
                        break;
                    }
                case NuaObjectType.@string:
                    {
                        if (!context.VM.StandardLibraries.TryGetValue("string", out var table))
                            throw new NuaExecutionException(context, "could not load lib string");

                        registers[a] = table[context, cValue];
                        break;
                    }
                case NuaObjectType.userdata:
                {
                    var userdata = upvalue.AsUserData();
                    registers[a] = userdata.Get(cValue);
                    break;
                }

                default:
                    throw new NuaExecutionException(context, $"attempt to index a {upvalue.Type} value");
            }

            // debug info
            if (cValue.Type == NuaObjectType.@string)
                registers[a].Metadata.DebugName = (string)cValue.Value;

            return null;
        }

        private NuaObject[] processor_GetTable(NuaExecutionContext context, ref int pc)
        {
            // R(A) := R(B)[RK(C)]
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            var bValue = registers[b];
            var cValue = ProcessorHelper.GetRK(context, c);

            switch (bValue.Type)
            {
                case NuaObjectType.table:
                    {
                        var table = bValue.AsTable();
                        registers[a] = table[context, cValue];
                        break;
                    }
                case NuaObjectType.@string:
                    {
                        if (!context.VM.StandardLibraries.TryGetValue("string", out var table))
                            throw new NuaExecutionException(context, "could not load lib string");

                        registers[a] = table[context, cValue];
                        break;
                    }
                case NuaObjectType.userdata:
                {
                    var userdata = bValue.AsUserData();
                    registers[a] = userdata.Get(cValue);
                    break;
                }

                default:
                    throw new NuaExecutionException(context, $"attempt to index a {bValue.Type} value");
            }

            // debug info
            if (cValue.Type == NuaObjectType.@string)
                registers[a].Metadata.DebugName = (string)cValue.Value;

            return null;
        }

        private NuaObject[] processor_SetTabUp(NuaExecutionContext context, ref int pc)
        {
            // UpValue[A][RK(B)] := RK(C)
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            var aValue = context.Closure.Upvalues[a].Value;
            var bValue = ProcessorHelper.GetRK(context, b);
            var cValue = ProcessorHelper.GetRK(context, c);

            switch (aValue.Type)
            {
                case NuaObjectType.table:
                    {
                        var table = aValue.AsTable();
                        table[context, bValue] = cValue;
                        break;
                    }
                case NuaObjectType.@string:
                    {
                        if (!context.VM.StandardLibraries.TryGetValue("string", out var table))
                            throw new NuaExecutionException(context, "could not load lib string");

                        table[context, bValue] = cValue;
                        break;
                    }

                default:
                    throw new NuaExecutionException(context, $"attempt to index a {aValue.Type} value");
            }

            return null;
        }

        private NuaObject[] processor_SetUpVal(NuaExecutionContext context, ref int pc)
        {
            // UpValue[B] := R(A)    
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];

            context.Closure.Upvalues[b].Value = registers[a];

            return null;
        }

        private NuaObject[] processor_SetTable(NuaExecutionContext context, ref int pc)
        {
            // R(A)[RK(B)] := RK(C)  
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            var aValue = (NuaTable)registers[a];
            var bValue = ProcessorHelper.GetRK(context, b);
            var cValue = ProcessorHelper.GetRK(context, c);
            aValue[context, bValue] = cValue;

            return null;
        }

        private NuaObject[] processor_NewTable(NuaExecutionContext context, ref int pc)
        {
            //R(A) := {} (size = B,C)     
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            registers[a] = new NuaTable();

            return null;
        }

        private NuaObject[] processor_Self(NuaExecutionContext context, ref int pc)
        {
            //R(A+1) := R(B); R(A) := R(B)[RK(C)]    
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            var bValue = registers[b];
            var cValue = ProcessorHelper.GetRK(context, c);

            registers[a + 1] = bValue;

            switch (bValue.Type)
            {
                case NuaObjectType.table:
                    {
                        var table = (NuaTable)bValue;
                        registers[a] = table[context, cValue];
                        break;
                    }
                case NuaObjectType.@string:
                    {
                        if (!context.VM.StandardLibraries.TryGetValue("string", out var table))
                            throw new NuaExecutionException(context, "could not load lib string");

                        registers[a] = table[context, cValue];
                        break;
                    }

                default:
                    throw new NuaExecutionException(context, $"attempt to index a {bValue.Type} value");
            }

            // debug info
            if (cValue.Type == NuaObjectType.@string)
                registers[a].Metadata.DebugName = (string)cValue.Value;

            return null;
        }

        private NuaObject[] processor_Add(NuaExecutionContext context, ref int pc)
        {
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            var valB = ProcessorHelper.GetRK(context, b);
            var valC = ProcessorHelper.GetRK(context, c);

            var numB = ProcessorHelper.AsArithmetic(valB);
            var numC = ProcessorHelper.AsArithmetic(valC);

            if (numB == null)
                throw new NuaExecutionException(context, $"attempt to perform arithmetic on a {valB.Type} value");

            if (numC == null)
                throw new NuaExecutionException(context, $"attempt to perform arithmetic on a {valC.Type} value");

            registers[a] = numB + numC;

            return null;
        }

        private NuaObject[] processor_Sub(NuaExecutionContext context, ref int pc)
        {
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            var valB = ProcessorHelper.GetRK(context, b);
            var valC = ProcessorHelper.GetRK(context, c);

            var numB = ProcessorHelper.AsArithmetic(valB);
            var numC = ProcessorHelper.AsArithmetic(valC);

            if (numB == null)
                throw new NuaExecutionException(context, $"attempt to perform arithmetic on a {valB.Type} value");

            if (numC == null)
                throw new NuaExecutionException(context, $"attempt to perform arithmetic on a {valC.Type} value");

            registers[a] = numB - numC;

            return null;
        }

        private NuaObject[] processor_Mul(NuaExecutionContext context, ref int pc)
        {
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            var valB = ProcessorHelper.GetRK(context, b);
            var valC = ProcessorHelper.GetRK(context, c);

            var numB = ProcessorHelper.AsArithmetic(valB);
            var numC = ProcessorHelper.AsArithmetic(valC);

            if (numB == null)
                throw new NuaExecutionException(context, $"attempt to perform arithmetic on a {valB.Type} value");

            if (numC == null)
                throw new NuaExecutionException(context, $"attempt to perform arithmetic on a {valC.Type} value");

            registers[a] = numB * numC;

            return null;
        }

        private NuaObject[] processor_Div(NuaExecutionContext context, ref int pc)
        {
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            var valB = ProcessorHelper.GetRK(context, b);
            var valC = ProcessorHelper.GetRK(context, c);

            var numB = ProcessorHelper.AsArithmetic(valB);
            var numC = ProcessorHelper.AsArithmetic(valC);

            if (numB == null)
                throw new NuaExecutionException(context, $"attempt to perform arithmetic on a {valB.Type} value");

            if (numC == null)
                throw new NuaExecutionException(context, $"attempt to perform arithmetic on a {valC.Type} value");

            registers[a] = numB / numC;

            return null;
        }

        private NuaObject[] processor_Mod(NuaExecutionContext context, ref int pc)
        {
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            var valB = ProcessorHelper.GetRK(context, b);
            var valC = ProcessorHelper.GetRK(context, c);

            var numB = ProcessorHelper.AsArithmetic(valB);
            var numC = ProcessorHelper.AsArithmetic(valC);

            if (numB == null)
                throw new NuaExecutionException(context, $"attempt to perform arithmetic on a {valB.Type} value");

            if (numC == null)
                throw new NuaExecutionException(context, $"attempt to perform arithmetic on a {valC.Type} value");

            registers[a] = numB % numC;

            return null;
        }

        private NuaObject[] processor_Pow(NuaExecutionContext context, ref int pc)
        {
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            var valB = ProcessorHelper.GetRK(context, b);
            var valC = ProcessorHelper.GetRK(context, c);

            var numB = ProcessorHelper.AsArithmetic(valB);
            var numC = ProcessorHelper.AsArithmetic(valC);

            if (numB == null)
                throw new NuaExecutionException(context, $"attempt to perform arithmetic on a {valB.Type} value");

            if (numC == null)
                throw new NuaExecutionException(context, $"attempt to perform arithmetic on a {valC.Type} value");

            registers[a] = numB ^ numC;

            return null;
        }

        private NuaObject[] processor_Unm(NuaExecutionContext context, ref int pc)
        {
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];

            var valB = ProcessorHelper.GetRK(context, b);
            var numB = ProcessorHelper.AsArithmetic(valB);

            if (numB == null)
                throw new NuaExecutionException(context, $"attempt to perform arithmetic on a {valB.Type} value");

            registers[a] = -numB;

            return null;
        }

        private NuaObject[] processor_Not(NuaExecutionContext context, ref int pc)
        {
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];

            var valB = ProcessorHelper.GetRK(context, b);

            registers[a] = !valB;

            return null;
        }

        private NuaObject[] processor_Len(NuaExecutionContext context, ref int pc)
        {
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];

            var bValue = registers[b];

            switch (bValue.Type)
            {
                case NuaObjectType.@string:
                case NuaObjectType.table:
                case NuaObjectType.userdata:
                    registers[a] = new NuaNumber(bValue.Length);
                    break;

                default:
                    throw new NuaExecutionException(context, $"attempt to get length of a {bValue.Type} value");
            }

            return null;
        }

        private NuaObject[] processor_Concat(NuaExecutionContext context, ref int pc)
        {
            // R(A) := R(B).. ... ..R(C)   
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            var sb = new StringBuilder();

            for (var i = b; i <= c; i++)
            {
                var iValue = registers[i];

                switch (iValue.Type)
                {
                    case NuaObjectType.number:
                    case NuaObjectType.@string:
                        sb.Append(iValue.Value);
                        break;

                    default:
                        throw new NuaExecutionException(context, $"attempt to concatenate a {iValue.Type} value");
                }
            }

            registers[a] = new NuaString(sb.ToString());

            return null;
        }

        private NuaObject[] processor_Jmp(NuaExecutionContext context, ref int pc)
        {
            // pc+=sBx; if (A) close all upvalues >= R(A - 1)
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var sbx = opcode.Operands[LuaOpcode.Operand.sBx];

            pc += sbx;

            if (a > 0)
            {
                for (var i = a - 1; i < registers.Count; i++)
                    registers.CloseRegister(i);
            }

            return null;
        }

        private NuaObject[] processor_Eq(NuaExecutionContext context, ref int pc)
        {
            // if ((RK(B) == RK(C)) ~= A) then pc++
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            if (ProcessorHelper.GetRK(context, b) == ProcessorHelper.GetRK(context, c) != a > 0)
                pc++;

            return null;
        }

        private NuaObject[] processor_Lt(NuaExecutionContext context, ref int pc)
        {
            // if ((RK(B) <  RK(C)) ~= A) then pc++
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            var bValue = ProcessorHelper.GetRK(context, b);
            var cValue = ProcessorHelper.GetRK(context, c);

            if (bValue.Type != cValue.Type)
            {
                throw new NuaExecutionException(context, $"attempt to compare {bValue.Type} with {cValue.Type}");
            }

            switch (bValue.Type)
            {
                case NuaObjectType.number:
                case NuaObjectType.@string:
                    var bNumber = ProcessorHelper.AsArithmetic(bValue);
                    var cNumber = ProcessorHelper.AsArithmetic(cValue);

                    if (bNumber.Number < cNumber.Number != a > 0)
                        pc++;

                    break;

                default:
                    throw new NuaExecutionException(context, $"attempt to compare two {bValue.Type} values");
            }

            return null;
        }

        private NuaObject[] processor_Le(NuaExecutionContext context, ref int pc)
        {
            // if ((RK(B) <= RK(C)) ~= A) then pc++
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            var bValue = ProcessorHelper.GetRK(context, b);
            var cValue = ProcessorHelper.GetRK(context, c);

            if (bValue.Type != cValue.Type)
            {
                throw new NuaExecutionException(context, $"attempt to compare {bValue.Type} with {cValue.Type}");
            }

            switch (bValue.Type)
            {
                case NuaObjectType.number:
                case NuaObjectType.@string:
                    var bNumber = ProcessorHelper.AsArithmetic(bValue);
                    var cNumber = ProcessorHelper.AsArithmetic(cValue);

                    if (bNumber.Number <= cNumber.Number != a > 0)
                        pc++;

                    break;

                default:
                    throw new NuaExecutionException(context, $"attempt to compare two {bValue.Type} values");
            }

            return null;
        }

        private NuaObject[] processor_Test(NuaExecutionContext context, ref int pc)
        {
            // if not (R(A) <=> C) then pc++ 
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            var aValue = registers[a];
            var cBool = c > 0;

            if (aValue.IsNull)
            {
                if (cBool)
                    pc++;
            }
            else if (aValue.Type == NuaObjectType.boolean)
            {
                if (((NuaBoolean)aValue).Boolean == !cBool)
                    pc++;
            }
            else if (!cBool)
            {
                pc++;
            }

            return null;
        }

        private NuaObject[] processor_TestSet(NuaExecutionContext context, ref int pc)
        {
            // if (R(B) <=> C) then R(A) := R(B) else pc++
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            var bValue = registers[b];
            var cBool = (c > 0);

            if (bValue.IsNull == !cBool)
            {
                registers[a] = bValue;
            }
            else if (bValue.Type == NuaObjectType.boolean && ((NuaBoolean)bValue).Boolean == !cBool)
            {
                registers[a] = bValue;
            }
            else
            {
                if (!cBool)
                    registers[a] = bValue;
                else
                    pc++;
            }

            return null;
        }

        private NuaObject[] processor_Call(NuaExecutionContext context, ref int pc)
        {
            // R(A), ... ,R(A+C-2) := R(A)(R(A+1), ... ,R(A+B-1))
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            var callArgs = new List<NuaObject>();
            var callStart = a + 1;
            var callEnd = b > 0 ? a + b - 1 : context.TopOffset - 1;

            var aValue = registers[a];

            // null check
            if (aValue.IsNull)
                throw new NuaExecutionException(context, $"attempt to call a nil value (global \'{aValue.Metadata.DebugName}\')");

            NuaFunction callFunction;

            switch (aValue.Type)
            {
                // meta table __call behaviour 
                case NuaObjectType.table:
                    var table = (NuaTable)aValue;
                    var metatable = table.Metatable;

                    if (metatable != null)
                    {
                        var metatableCall = metatable[context, "__call"];

                        if (metatableCall.IsNull || metatableCall.Type != NuaObjectType.function)
                            throw new NuaExecutionException(context, $"attempt to call a table with no valid '__call' metamethod");

                        callFunction = metatableCall.AsFunction();
                    }
                    else
                    {
                        throw new NuaExecutionException(context, $"attempt to call a table with no metatable");
                    }
                    break;

                case NuaObjectType.function:
                    callFunction = aValue.AsFunction();
                    break;

                default:
                    throw new NuaExecutionException(context, $"attempt to call a {aValue.Type} value (global \'{aValue.Metadata.DebugName}\')");
            }

            // setup call args
            for (var i = callStart; i <= callEnd; i++)
                callArgs.Add(registers[i]);

            if (b == 0)
                callArgs.AddRange(context.Top);

            var result = context.VM.InternalCall(context, callFunction, callArgs.ToArray());

            if (c == 0)
            {
                context.TopOffset = a;
                context.Top = result;
            }
            else
            {
                for (var i = a; i <= a + c - 2; i++)
                    registers[i] = i - a < result.Length ? result[i - a] : null;
            }

            return null;
        }

        private NuaObject[] processor_TailCall(NuaExecutionContext context, ref int pc)
        {
            // return R(A)(R(A+1), ... ,R(A+B-1)) 
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            var callArgs = new List<NuaObject>();
            var callStart = a + 1;
            var callEnd = b > 0 ? a + b - 1 : context.TopOffset - 1;

            var aValue = registers[a];

            // null check
            if (aValue.IsNull)
                throw new NuaExecutionException(context, $"attempt to call {aValue.Metadata.Name} a nil value");

            NuaFunction callFunction;

            switch (aValue.Type)
            {
                // meta table __call behaviour 
                case NuaObjectType.table:
                    var table = (NuaTable)aValue;
                    var metatable = table.Metatable;

                    if (metatable != null)
                    {
                        var metatableCall = metatable[context, "__call"];

                        if (metatableCall.IsNull || metatableCall.Type != NuaObjectType.function)
                            throw new NuaExecutionException(context, "attempt to call a table with no valid '__call' metamethod");

                        callFunction = metatableCall.AsFunction();
                    }
                    else
                    {
                        throw new NuaExecutionException(context, "attempt to call a table with no metatable");
                    }
                    break;

                case NuaObjectType.function:
                    callFunction = aValue.AsFunction();
                    break;

                default:
                    throw new NuaExecutionException(context, $"attempt to call {aValue.Metadata.Name} a {aValue.Type} value");
            }

            // setup call args
            for (var i = callStart; i <= callEnd; i++)
                callArgs.Add(registers[i]);

            if (b == 0)
                callArgs.AddRange(context.Top);

            return context.VM.InternalCall(context, callFunction, callArgs.ToArray());
        }

        private NuaObject[] processor_Return(NuaExecutionContext context, ref int pc)
        {
            // return R(A), ... ,R(A+B-2)
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];

            var returnLength = (b > 0 ? b - 1 : context.TopOffset - a + context.Top.Length);

            if (returnLength > 0)
            {
                var returnvalues = new NuaObject[returnLength];

                for (var i = a; i <= a + b - 2; i++)
                    returnvalues[i - a] = registers[i];

                if (b == 0)
                {
                    for (var i = a; i < context.TopOffset; i++)
                        returnvalues[i - a] = registers[i];

                    Array.Copy(context.Top, 0, returnvalues, context.TopOffset - 1, context.Top.Length);
                }

                return returnvalues;
            }

            return Array.Empty<NuaObject>();
        }

        private NuaObject[] processor_ForLoop(NuaExecutionContext context, ref int pc)
        {
            // R(A)+=R(A+2);
            // if R(A) <?= R(A + 1) then { pc += sBx; R(A + 3) = R(A) }

            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var sBx = opcode.Operands[LuaOpcode.Operand.sBx];

            var aValue = registers[a];
            var a2Value = registers[a + 2];
            var a1Value = registers[a + 1];

            var aNum = ProcessorHelper.AsArithmetic(aValue);
            var a2Num = ProcessorHelper.AsArithmetic(a2Value);
            var a1Num = ProcessorHelper.AsArithmetic(a1Value);

            if (aNum == null)
                throw new NuaExecutionException(context, "'for' intenral index must be number");

            if (a2Num == null)
                throw new NuaExecutionException(context, "'for' step must be a number");

            if (a1Num == null)
                throw new NuaExecutionException(context, "'for' limit must be a number");

            var index = aNum + a2Num;
            var step = a2Num.Number;

            registers[a] = index;

            if (step > 0)
            {
                if (index.Number <= a1Num.Number)
                {
                    pc += sBx;
                    registers[a + 3] = index;
                }
            }
            else
            {
                if (index.Number >= a1Num.Number)
                {
                    pc += sBx;
                    registers[a + 3] = index;
                }
            }

            return null;
        }

        private NuaObject[] processor_ForPrep(NuaExecutionContext context, ref int pc)
        {
            // R(A) -= R(A + 2); pc += sBx
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var sBx = opcode.Operands[LuaOpcode.Operand.sBx];

            var aValue = registers[a];
            var a2Value = registers[a + 2];

            var aNum = ProcessorHelper.AsArithmetic(aValue);
            var a2Num = ProcessorHelper.AsArithmetic(a2Value);

            if (aNum == null)
                throw new NuaExecutionException(context, "'for' intenral index must be number");

            if (a2Num == null)
                throw new NuaExecutionException(context, "'for' step must be a number");

            registers[a] = aNum - a2Num;

            pc += sBx;

            return null;
        }













        private NuaObject[] processor_SetList(NuaExecutionContext context, ref int pc)
        {
            //R(A)[(C-1)*FPF+i] := R(A+i), 1 <= i <= B
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];
            var c = opcode.Operands[LuaOpcode.Operand.C];

            if (c == 0)
                c = function.Opcodes[++pc].Operands[LuaOpcode.Operand.Ax];

            var table = (NuaTable)registers[a];
            var end = b > 0 ? b : context.TopOffset - a;

            for (var i = 1; i <= end; i++)
                table[context, new NuaNumber((c - 1) * FPF + i)] = registers[a + i];

            if (b == 0)
                for (var i = 0; i < context.Top.Length; i++)
                    table[context, new NuaNumber((c - 1) * FPF + i + end)] = context.Top[i];

            return null;
        }














        private NuaObject[] processor_Closure(NuaExecutionContext context, ref int pc)
        {
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var bx = opcode.Operands[LuaOpcode.Operand.Bx];

            registers[a] = context.VM.NuaFunctionFromClosure(
                context.VM.CreateClosure(
                    context,
                    function.FunctionPrototypes[ProcessorHelper.LuaIndexToZeroIndex(bx)]
                    )
                );

            return null;
        }









        private NuaObject[] processor_VarArg(NuaExecutionContext context, ref int pc)
        {
            //R(A), R(A+1), ..., R(A+B-2) = vararg
            var registers = context.Registers;
            var function = context.Closure.Prototype;
            var opcode = function.Opcodes[pc];

            var a = opcode.Operands[LuaOpcode.Operand.A];
            var b = opcode.Operands[LuaOpcode.Operand.B];

            for (var i = a; i <= a + b - 2; i++)
                registers[i] = i - a < context.CallingData.VarArgs.Length ? context.CallingData.VarArgs[i - a] : null;

            if (b == 0)
            {
                context.TopOffset = a;
                context.Top = context.CallingData.VarArgs;
            }

            return null;
        }
    }
}
