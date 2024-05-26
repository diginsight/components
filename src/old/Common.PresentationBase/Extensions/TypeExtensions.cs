#region using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows; 
#endregion

namespace Common
{
    public static class TypeExtension
    {

        public static bool IsAnonymousType(this Type type)
        {
            var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Count() > 0;
            var nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            var isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }
    }

    public static class TcpClientExtension
    {
        public static TcpState GetState(this TcpClient tcpClient)
        {
            var foo = IPGlobalProperties.GetIPGlobalProperties()
              .GetActiveTcpConnections()
              .SingleOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint));
            return foo != null ? foo.State : TcpState.Unknown;
        }
    }

    public static class TypeHelper {
        public static Type[] GetTypesByName(string className)
        {
            List<Type> returnVal = new List<Type>();

            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] assemblyTypes = a.GetTypes();
                for (int j = 0; j < assemblyTypes.Length; j++)
                {
                    if (assemblyTypes[j].Name == className)
                    {
                        returnVal.Add(assemblyTypes[j]);
                    }
                }
            }

            return returnVal.ToArray();
        }
    }

    public static class ExtensionMethods
    {
        #region ExecSync
        public static void Exec(this DependencyObject pThis, Action action)
        {
            if (pThis == null || pThis.Dispatcher == null) { return; }
            if (pThis.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                pThis.Dispatcher.Invoke(action);
            }
        }

        public static void Exec(this System.Windows.Application app, Action action)
        {
            if (app == null || app.Dispatcher == null) { return; }
            if (app.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                app.Dispatcher.Invoke(action);
            }
        }
        #endregion
        #region ExecAsync
        public static void ExecAsync(this DependencyObject pThis, Action action)
        {
            if (pThis == null || pThis.Dispatcher == null) { return; }
            if (pThis.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                pThis.Dispatcher.BeginInvoke(action);
            }
        }

        public static void ExecAsync(this System.Windows.Application app, Action action)
        {
            if (app == null || app.Dispatcher == null) { return; }
            if (app.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                app.Dispatcher.BeginInvoke(action);
            }
        }
        #endregion
    }

}
