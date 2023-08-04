#region using
using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls; 
#endregion

namespace Common
{
    public partial class LogstringHelper
    {
        public static string ToLogStringInternal(System.Windows.Window pthis)
        {
            if (pthis == null) { return null; }
            string logString = $"{{Window:{{Name:{pthis.Name},ActualHeight:{pthis.ActualHeight},ActualWidth:{pthis.ActualWidth},AllowsTransparency:{pthis.AllowsTransparency},Background:{pthis.Background},Width:{pthis.Width},Height:{pthis.Height}}}}}";
            return logString;
        }
        public static string ToLogStringInternal(PropertyChangedEventArgs pthis)
        {
            if (pthis == null) { return null; }
            string logString = $"{{PropertyChangedEventArgs:{{PropertyName:{pthis.PropertyName}}}}}";
            return logString;
        }
        public static string ToLogStringInternal(Button pthis)
        {
            if (pthis == null) { return null; }
            string logString = $"{{Button:{{Name:{pthis.Name},Content:{pthis.Content.GetLogString()}}}}}";
            return logString;
        }
        public static string ToLogStringInternal(Thread pthis)
        {
            if (pthis == null) { return null; }
            string logString = $"{{Button:{{Name:{pthis.Name},ManagedThreadId:{pthis.ManagedThreadId},ApartmentState:{pthis.ApartmentState},ThreadState:{pthis.ThreadState},IsAlive:{pthis.IsAlive},IsBackground:{pthis.IsBackground},IsThreadPoolThread:{pthis.IsThreadPoolThread},Priority:{pthis.Priority},CurrentCulture:{pthis.CurrentCulture},CurrentUICulture:{pthis.CurrentUICulture}}}}}";
            return logString;
        }
        public static string ToLogStringInternal(Microsoft.Graph.Models.Application pthis)
        {
            if (pthis == null) { return null; }
            string logString = $"{{Application:{{Id:{pthis.Id},AppId:{pthis.AppId},DisplayName:{pthis.DisplayName},Description:{pthis.Description},PublisherDomain:{pthis.PublisherDomain},SignInAudience:{pthis.SignInAudience},Owners:{pthis.Owners.GetLogString()}}}}}";
            return logString;
        }
    }
}
