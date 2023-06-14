#region using
using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
#endregion

namespace EventHubSample
{
    public partial class LogstringHelper
    {
        public static string ToLogStringInternal(System.Windows.Window pthis)
        {
            string logString = $"{{Window:{{Name:{pthis.Name},ActualHeight:{pthis.ActualHeight},ActualWidth:{pthis.ActualWidth},AllowsTransparency:{pthis.AllowsTransparency},Background:{pthis.Background},Width:{pthis.Width},Height:{pthis.Height}}}}}";
            return logString;
        }
        public static string ToLogStringInternal(PropertyChangedEventArgs pthis)
        {
            string logString = $"{{PropertyChangedEventArgs:{{PropertyName:{pthis.PropertyName}}}}}";
            return logString;
        }
        public static string ToLogStringInternal(Button pthis)
        {
            string logString = $"{{Button:{{Name:{pthis.Name},Content:{pthis.Content.GetLogString()}}}}}";
            return logString;
        }
        public static string ToLogStringInternal(Thread pthis)
        {
            var currentUICulture = default(CultureInfo); try { currentUICulture = pthis.CurrentUICulture; } catch (Exception ex)  { TraceLogger.LogError($"{ex.Message}"); }
            var currentCulture = default(CultureInfo); try { currentCulture = pthis.CurrentCulture; } catch (Exception ex) { TraceLogger.LogError($"{ex.Message}"); }
            var priority = default(ThreadPriority); try { priority = pthis.Priority; } catch (Exception ex) { TraceLogger.LogError($"{ex.Message}"); }
            var isThreadPoolThread = default(bool); try { isThreadPoolThread = pthis.IsThreadPoolThread; } catch (Exception ex) { TraceLogger.LogError($"{ex.Message}"); }
            var isBackground = default(bool); try { isBackground = pthis.IsBackground; } catch (Exception ex) { TraceLogger.LogError($"{ex.Message}"); }
            var isAlive = default(bool); try { isAlive = pthis.IsAlive; } catch (Exception ex) { TraceLogger.LogError($"{ex.Message}"); }
            var threadState = default(ThreadState); try { threadState = pthis.ThreadState; } catch (Exception ex) { TraceLogger.LogError($"{ex.Message}"); }
            var apartmentState = default(ApartmentState); try { apartmentState = pthis.ApartmentState; } catch (Exception ex) { TraceLogger.LogError($"{ex.Message}"); }
            var managedThreadId = default(int); try { managedThreadId = pthis.ManagedThreadId; } catch (Exception ex) { TraceLogger.LogError($"{ex.Message}"); }
            var name = default(string); try { name = pthis.Name; } catch (Exception ex) { TraceLogger.LogError($"{ex.Message}"); }

            string logString = $"{{Button:{{Name:{name},ManagedThreadId:{managedThreadId},ApartmentState:{apartmentState},ThreadState:{threadState},IsAlive:{isAlive},IsBackground:{isBackground},IsThreadPoolThread:{isThreadPoolThread},Priority:{priority},CurrentCulture:{currentCulture},CurrentUICulture:{currentUICulture}}}}}";
            return logString;
        }
        //_Document
        //_Application
    }
}
