#region using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
#endregion

namespace Common
{
    public static class TreeHelper
    {
        public static IEnumerable<T> Traverse<T>(T root, Func<T, IEnumerable<T>> children)
        {
            var stack = new Stack<T>();
            stack.Push(root);
            while (stack.Count != 0)
            {
                T item = stack.Pop();
                yield return item;
                foreach (var child in children(item))
                    stack.Push(child);
            }
        }

        private static void DoAction<T>(T root, Func<T, IEnumerable<T>> children, Action<T> action)
        {
            var stack = new Stack<T>();
            stack.Push(root);
            while (stack.Count != 0)
            {
                T item = stack.Pop();
                action.Invoke(item);
                foreach (var child in children(item))
                    stack.Push(child);
            }
        }



    }
}
