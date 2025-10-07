using System.Runtime.Serialization;

namespace Diginsight.Components;

[Serializable]
public class BreakLoopException : Exception
{
    private object Item { get; set; }

    public BreakLoopException() { }
    public BreakLoopException(string message) : base(message) { }
    public BreakLoopException(string message, Exception inner) : base(message, inner) { }
    //public BreakLoopException(string message, object item) : base(message) { this.Item = item; }
    //public BreakLoopException(string message, object item, Exception inner) : base(message, inner) { this.Item = item; }
    protected BreakLoopException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
