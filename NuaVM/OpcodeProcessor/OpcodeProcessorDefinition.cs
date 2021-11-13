using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NuaVM.LuaDisasm;
using NuaVM.Types;
using NuaVM.VM;

namespace NuaVM.OpcodeProcessor
{
    public delegate NuaObject[] OpcodeProcessorDelegate(NuaExecutionContext context, ref int pc);

    public abstract class OpcodeProcessorDefinition
    {
        public Dictionary<int, OpcodeProcessorDelegate> Processors { get; private set; }

        public abstract void SetupProcessors();

        public OpcodeProcessorDelegate GetProcessor(int opcode)
        {
            return Processors[opcode];
        }

        public OpcodeProcessorDelegate GetProcessor(LuaOpcode opcode)
        {
            return GetProcessor((int) opcode.Op);
        }

        protected OpcodeProcessorDefinition()
        {
            Processors = new Dictionary<int, OpcodeProcessorDelegate>();
            SetupProcessors();
        }
    }
}
