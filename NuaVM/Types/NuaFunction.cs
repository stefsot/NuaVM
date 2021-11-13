using System;
using NuaVM.VM;

namespace NuaVM.Types
{
    public class NuaFunction : NuaObject
    {
        // NuaObject implementation
        // -------------------------------------

        public override NuaObjectType Type => NuaObjectType.function;

        public override object Value { get; protected set; }

        // NuaFunction implementation
        // -------------------------------------

        public delegate NuaObject[] NuaFunctionDelegate(NuaExecutionContext context, params NuaObject[] args);

        public NuaClosure Closure
        {
            get => (NuaClosure) Value;
            private set => Value = value;
        }

        public NuaFunctionDelegate Delegate { get; private set; }

        private NuaVirtualMachine VM { get; set; }

        public bool IsLua => Closure != null;

        public bool IsNet => !IsLua;

        public NuaObject[] Invoke(NuaExecutionContext callingContext, params NuaObject[] args)
        {
            // allow call of native functions without a vm present
            if (IsNet && VM == null)
                return Delegate.Invoke(callingContext, args);

            return VM.InternalCall(callingContext, this, args);
        }

        public NuaObject[] Invoke(params NuaObject[] args)
        {
            return Invoke(null, args);
        }

        public NuaFunction(NuaVirtualMachine vm, NuaFunctionDelegate @delegate, NuaClosure closure, string name = "anonymous_LuaFunction")
        {
            Metadata = new NuaObjectMetadata(name);

            VM = vm ?? throw new NullReferenceException($"{nameof(vm)} cannot be null");
            Delegate = @delegate;
            Closure = closure;       
        }

        public NuaFunction(NuaFunctionDelegate @delegate, string name = "anonymous_NetFunction")
        {
            Metadata = new NuaObjectMetadata(name);
            Delegate = @delegate;
        }
    }
}
