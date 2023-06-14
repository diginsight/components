using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class ObjectExtensions
    {
        //#region DoAction
        //public static IEnumerable<T> DoAction<T>(this IEnumerable<T> source, Action<T> action) {
        //    if (source == null || action == null) { return source; }
        //    foreach (var item in source) { action(item); }
        //    return source;
        //}
        //#endregion

        // Action( Action<T> action )
        // Action( Action<T> action, count )
        // Action( Action<T, bool> action, count )
        // Action( Action<T> action, Func<T, bool> while )
        // Action( Action<T> action, Func<T, bool> until )
        #region Action
        public static T Action<T>(this T t, Action<T> action)
        {
            if (action != null) { action(t); }
            return t;
        }
        #endregion
        #region Do
        public static T Do<T>(this T t, Action<T> action)
        {
            if (action != null) { action(t); }
            return t;
        }
        #endregion
    }
}
