using NuaVM.LuaDisasm;
using NuaVM.Types;

namespace NuaVM.VM
{
    public class NuaClosure
    {
        public LuaFunction Prototype { get; private set; }

        public NuaUpvalue[] Upvalues { get; private set; }

        public NuaUpvalue Environment { get; set; }

        // link upvalues
        internal NuaRegister GetUpvalueRegister(NuaExecutionContext context, int index)
        {
            var upvalueProto = Prototype.Upvalues[index];

            return upvalueProto.IsInStack > 0 ?
                context.Registers.GetRegister(upvalueProto.Position) :
                context.Closure.Upvalues[upvalueProto.Position].Register;
        }

        // link upvalues from proper execution context
        private void LinkUpvalues(NuaExecutionContext context)
        {
            Upvalues = new NuaUpvalue[Prototype.Upvalues.Count];

            for (var i = 0; i < Upvalues.Length; i++)
                Upvalues[i] = new NuaUpvalue(GetUpvalueRegister(context, i));

            if (context.Closure != null)
                Environment = context.Closure.Environment;
        }

        // link predefined upvalues
        // note: environment should be a NuaTable, what happens if it's not? expect undefined behavior
        private void LinkUpvalues(NuaObject environment)
        {
            // upvalue of the top most function always points to _ENV
            Upvalues = new[] { new NuaUpvalue(new NuaRegister(environment)) };
            Environment = Upvalues[0];
        }


        // creates a closure for the current calling context
        // it's usually used by a parent function
        public NuaClosure(LuaFunction prototype, NuaExecutionContext context)
        {
            Prototype = prototype;
            LinkUpvalues(context);
        }

        // create a standalone closure without parent
        // a valid "environment" table needs to be provided as a top level upvalue
        public NuaClosure(LuaFunction prototype, NuaObject env)
        {
            Prototype = prototype;
            LinkUpvalues(env);
        }
    }
}
