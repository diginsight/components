using System;

namespace Common.Abstractions
{
    public interface IDebugService
    {
        void IfDebug(Action action);
    } 
}
