using NuaVM.VM;

namespace NuaVM.Types.Exceptions
{
    public class NuaExecutionException : NuaException
    {
        public NuaExecutionContext Context { get; private set; }

        public NuaObject ErrorObject { get; private set; }

        public NuaExecutionException(NuaExecutionContext context, string message) : base(message)
        {
            Context = context;
            ErrorObject = message;
        }

        public NuaExecutionException(NuaExecutionContext context, NuaObject errorObject) : base(errorObject.Value?.ToString())
        {
            Context = context;
            ErrorObject = errorObject;
        }
    }
}