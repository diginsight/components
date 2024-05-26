#region using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Common
{
    public static class StringHelper
    {
        #region IsEmpty
        public static bool IsEmpty(string str)
        {
            return (str == null || str.Length == 0);
        }
        #endregion

        #region Truncate
        public static string Truncate(string message, int l, int leading, int trailing)
        {
            if (leading > l - 3) { leading = l - 3; }
            if (trailing > l - leading - 3) { trailing = l - leading - 3; }

            int lv = message != null ? message.Length : 0;
            if (lv > l)
            {
                int left = Math.Max(leading, l - trailing - 3);
                int right = l - left - 3;
                message = string.Format("{0}...{1}", message.Substring(0, left), message.Substring(lv - right + 1));
            }
            return message;
        }
        #endregion
        #region Truncate2
        public static string Truncate2(string message, int l, int leading, int trailing) { return Truncate2(message, l, leading, trailing, true, true); }
        public static string Truncate2(string message, int l, int leading, int trailing, bool trimblanks, bool escape)
        {
            if (leading > l - 3) { leading = l - 3; }
            if (trailing > l - leading - 3) { trailing = l - leading - 3; }

            if (message != null && trimblanks) { message = message.Trim(); }
            int lv = message != null ? message.Length : 0; // lunghezza vera
            if (lv > l)
            { // il messaggio è da troncare
                int leftlen = Math.Max(leading, l - trailing - 3);
                int rightlen = l - leftlen - 3;
                string left = message.Substring(0, leftlen);
                string right = message.Substring(lv - rightlen + 1);
                message = string.Format("{0}...{1}", left, right);
            }
            return escape ? Escape(message) : message;
        }
        #endregion
        #region Escape
        public static string Escape(string message)
        {
            if (message != null)
            {
                if (message.IndexOf("\r") > 0) { message = message.Replace("\r", "\\r"); }
                if (message.IndexOf("\n") > 0) { message = message.Replace("\n", "\\n"); }
            }
            return message;
        }

        public static ushort[] HexStringToWordArray(string v1, char[] v2)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
