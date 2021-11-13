using System;

namespace NuaVM.Types.Exceptions
{
    public class NuaException : Exception
    {
        public NuaException(string message) : base(message)
        {

        }
    }
}
