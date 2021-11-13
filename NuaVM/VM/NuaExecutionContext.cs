using System.Collections.Generic;
using NuaVM.Types;

namespace NuaVM.VM
{
    public class NuaExecutionContext
    {
        public NuaVirtualMachine VM { get; private set; }

        public NuaClosure Closure { get; private set; }

        public NuaCallingData CallingData { get; private set; }

        public NuaExecutionContext CallingContext { get; private set; }

        public NuaRegisterList Registers { get; private set; }

        public NuaObject[] Top { get; set; }

        public int TopOffset { get; set; }

        public int PC;

        public Stack<NuaExecutionContext> CallStack { get; private set; }

        public int CallStackDepth => CallStack.Count;

        public NuaExecutionContext(NuaVirtualMachine vm, NuaClosure closure, NuaExecutionContext callingContext)
        {
            VM = vm;
            Closure = closure;
            CallingContext = callingContext;

            if (closure != null)
                Registers = new NuaRegisterList(closure.Prototype.Header.NumOfRegisters);

            CallingData = new NuaCallingData();

            // initialize call stack
            if (callingContext != null)
            {
                CallStack = callingContext.CallStack;
                CallStack.Push(this);
            }
            else
            {
                CallStack = new Stack<NuaExecutionContext>();
                CallStack.Push(this);
            }
        }
    }
}
