using System;

namespace Common.Abstractions
{
    public interface DebugService
    {
        void IfDebug(Action action);
    } 
}
