using System;
using System.Collections.Generic;
using NuaVM.CommonLibraries;
using NuaVM.LuaDisasm;
using NuaVM.OpcodeProcessor;
using NuaVM.OpcodeProcessor.Implementations;
using NuaVM.Types;
using NuaVM.Types.Exceptions;

// ReSharper disable IdentifierTypo

namespace NuaVM.VM
{
    public class NuaVirtualMachine
    {
        // events
        // ---------------------

        public delegate void OnCallDelegate(ref NuaExecutionContext callingContext, ref NuaFunction callee,
            ref NuaObject[] callArgs);
        public event OnCallDelegate OnCallFunction;

        public delegate void OnProcessOpcodeDelegate(NuaExecutionContext context, ref OpcodeProcessorDelegate processor);
        public event OnProcessOpcodeDelegate OnExecuteOpcode;

        public delegate bool OnExceptionDelegate(NuaExecutionException e, NuaExecutionContext context);
        public event OnExceptionDelegate OnExecutionException;

        // ---------------------

        public int MaxCallStackDepth { get; set; } = 1024;

        public Dictionary<string, NuaTable> StandardLibraries = new Dictionary<string, NuaTable>();

        public NuaTable GlobalTable { get; set; }

        private OpcodeProcessorDefinition OpcodeProcessor { get; set; }

        public NuaVirtualMachine()
        {
            OpcodeProcessor = new DefaultOpcodeProcessor();
            GlobalTable = new NuaTable();

            LoadCommonLibraries();
            SetCommonLibraries();
        }

        private void LoadCommonLibraries()
        {
            StandardLibraries["string"] = NuaStringLib.GetTable();
        }

        private void SetCommonLibraries()
        {
            foreach (var pair in StandardLibraries)
                GlobalTable.Set(pair.Key, pair.Value);

            foreach (var pair in NuaDefaultLib.GetTable().Dictionary)
                GlobalTable.Set((string)pair.Key, pair.Value);
        }

        public NuaClosure CreateClosure(NuaExecutionContext context, LuaFunction prototype)
        {
            return new NuaClosure(prototype, context);
        }

        public NuaClosure CreateClosure(LuaFunction prototype, NuaObject env)
        {
            return new NuaClosure(prototype, env);
        }

        public NuaFunction Load(LuaAssembly assembly, NuaTable env)
        {
            env.Metatable = GlobalTable;

            var closure = CreateClosure(assembly.EntryPoint, env);
            return NuaFunctionFromClosure(closure);
        }

        // executes a function with no initial context
        public NuaObject[] Run(NuaFunction f, params NuaObject[] callArgs)
        {
            return InternalCall(null, f, callArgs);
        }

        public NuaFunction NuaFunctionFromClosure(NuaClosure closure, string name = "anonymous_LuaFunction")
        {
            return new NuaFunction(
                this,
                (context, args) => ProcessContext(context),
                closure,
                name);
        }

        private void SetCallData(NuaExecutionContext context, NuaObject[] callArgs)
        {
            if(callArgs == null)
                callArgs = NuaObject.EmptyArgs;

            // preserve a copy of call data
            context.CallingData.CallArgs = callArgs;

            // copy callArgs to registers
            for (var i = 0; i < context.Closure.Prototype.Header.NumOfArgs; i++)
            {
                var value = i < callArgs?.Length ? callArgs[i] : null;
                context.Registers[i] = value;
            }

            // create varArg data
            if (context.Closure.Prototype.Header.VarargFlag > 0)
            {
                var numOfArgs = context.Closure.Prototype.Header.NumOfArgs;
                var numOfVarArgs = callArgs.Length - numOfArgs;

                var varArgs = new NuaObject[numOfVarArgs];
                Array.Copy(callArgs, numOfArgs, varArgs, 0, numOfVarArgs);

                context.CallingData.VarArgs = varArgs;
            }
        }

        private NuaObject[] ExecuteContext(NuaExecutionContext context)
        {
            // stack overflow check limit
            if (context.CallStackDepth >= MaxCallStackDepth)
                throw new NuaExecutionException(context, "stack overflow");

            var closure = context.Closure;

            // execution loop
            while (context.PC >= 0)
            {
                var processor = OpcodeProcessor.GetProcessor(closure.Prototype.Opcodes[context.PC]);

                // event handler
                OnExecuteOpcode?.Invoke(context, ref processor);

                try
                {
                    var returnStack = processor(context, ref context.PC);

                    if (returnStack != null)
                        return returnStack;

                    context.PC++;
                }
                catch (NuaExecutionException e)
                {
                   // pass exception to event handler
                   // handler can return true to suppress the exception propagation
                   var result = OnExecutionException?.Invoke(e, context);

                   // rethrow exception if event handler returns false
                   if(!result.HasValue || !result.Value)
                       throw;
                }
            }

            return NuaObject.EmptyArgs;
        }

        private NuaObject[] ProcessContext(NuaExecutionContext context)
        {
            // execute context
            var result = ExecuteContext(context);

            // pop context from call stack
            context.CallStack.Pop();

            return result;
        }

        internal NuaObject[] InternalCall(NuaExecutionContext callingContext, NuaFunction callee, NuaObject[] callArgs)
        {
            // call args cant be null
            if (callArgs == null)
                callArgs = NuaObject.EmptyArgs;

            // event handler
            OnCallFunction?.Invoke(ref callingContext, ref callee, ref callArgs);

            // prepare new execution context
            var newContext = new NuaExecutionContext(this, callee.Closure, callingContext);

            // set up call data
            if(callee.IsLua)
                SetCallData(newContext, callArgs);

            return callee.Delegate(newContext, callArgs);
        }
    }
}
