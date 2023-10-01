#region using
using Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Application = System.Windows.Application;
using UserControl = System.Windows.Controls.UserControl;
#endregion

namespace Common
{
    // PreviewExceptionEventArgs
    #region PreviewExceptionEventArgs : RoutedEventArgs
    public class PreviewExceptionEventArgs : RoutedEventArgs
    {
        #region .ctor
        public PreviewExceptionEventArgs(Exception exception)
        {
            base.RoutedEvent = ExceptionManager.PreviewExceptionEvent;
            this._exception = exception;
        }
        #endregion

        #region InvokeEventHandler
        protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
        {
            var handler = (EventHandler<PreviewExceptionEventArgs>)genericHandler;
            handler(genericTarget, this);
        }
        #endregion

        // Properties
        #region Exception
        private Exception _exception;
        public Exception Exception
        {
            get { return this._exception; }
        }
        #endregion
        #region Cancel
        private bool _cancel;
        public bool Cancel
        {
            get { return this._cancel; }
            set { this._cancel = value; }
        }
        #endregion
    }
    #endregion
    #region ExceptionEventArgs : RoutedEventArgs
    public class ExceptionEventArgs : RoutedEventArgs
    {
        #region .ctor
        public ExceptionEventArgs(Exception exception)
        {
            base.RoutedEvent = ExceptionManager.ExceptionEvent;
            this._exception = exception;
        }
        #endregion

        #region InvokeEventHandler
        protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
        {
            EventHandler<ExceptionEventArgs> handler = (EventHandler<ExceptionEventArgs>)genericHandler;
            handler(genericTarget, this);
        }
        #endregion

        // Properties
        #region Exception
        private Exception _exception;
        public Exception Exception
        {
            get { return this._exception; }
        }
        #endregion
        #region Caught
        private bool _caught;
        public bool Caught
        {
            get { return this._caught; }
            set { this._caught = value; }
        }
        #endregion
    }
    #endregion

    #region ExceptionTranslationRuleElement
    public class ExceptionTranslationRuleElement : ConfigurationElement
    {
        #region Name
        [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return base["name"] as string; }
            set { base["name"] = value; }
        }
        #endregion
        #region Type
        [ConfigurationProperty("type", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string Type
        {
            get { return base["type"] as string; }
            set { base["type"] = value; }
        }
        #endregion

        #region IsEnabledPattern
        [ConfigurationProperty("isEnabledPattern", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string IsEnabledPattern
        {
            get { return base["isEnabledPattern"] as string; }
            set { base["isEnabledPattern"] = value; }
        }
        #endregion
        #region MessageInformationPattern
        [ConfigurationProperty("messageInformationPattern", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string MessageInformationPattern
        {
            get { return base["messageInformationPattern"] as string; }
            set { base["messageInformationPattern"] = value; }
        }
        #endregion

        #region TranslatedLevel
        [ConfigurationProperty("translatedLevel", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string TranslatedLevel
        {
            get { return base["translatedLevel"] as string; }
            set { base["translatedLevel"] = value; }
        }
        #endregion
        #region TranslatedMessage
        [ConfigurationProperty("translatedMessage", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string TranslatedMessage
        {
            get { return base["translatedMessage"] as string; }
            set { base["translatedMessage"] = value; }
        }
        #endregion
    }
    #endregion
    #region NamedTypeElementCollection
    [ConfigurationCollection(typeof(ExceptionTranslationRuleElement))]
    public class ExceptionTranslationRuleCollection : ConfigurationElementCollection
    {
        #region CreateNewElement
        protected override ConfigurationElement CreateNewElement()
        {
            return new ExceptionTranslationRuleElement();
        }
        #endregion
        #region GetElementKey
        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as ExceptionTranslationRuleElement).Name;
        }
        #endregion

        #region IsLoadCompleted
        public bool IsLoadCompleted { get; set; }
        #endregion

        #region this[int idx]
        public ExceptionTranslationRuleElement this[int idx]
        {
            get { return BaseGet(idx) as ExceptionTranslationRuleElement; }
        }
        #endregion
    }
    #endregion
    #region ExceptionTranslationsSection 
    public sealed class ExceptionTranslationsSection : ConfigurationSection
    {
        #region .ctor
        public ExceptionTranslationsSection() { }
        #endregion

        #region Rules 
        [ConfigurationProperty("Rules", IsDefaultCollection = true)]
        public ExceptionTranslationRuleCollection Rules
        {
            get { return base["Rules"] as ExceptionTranslationRuleCollection; }
        }
        #endregion
    }
    #endregion

    #region ExceptionManager
    ///<summary>Exceptions manager class.</summary>
    public sealed class ExceptionManager
    {
        #region constants
        public const int MAXMESSAGECOUNT = 20;
        public const int MAXMESSAGECOUNTDELAY = 1000;
        public const string CONFIGVALUE_ROUTEUNHANDLEDEXCEPTIONSTOFOCUSEDELEMENT = "RouteUnhandledExceptionsToFocusedElement"; const bool DEFAULTVALUE_ROUTEUNHANDLEDEXCEPTIONSTOFOCUSEDELEMENT = false;
        public const string CONFIGVALUE_UNHANDLEDEXCEPTIONHANDLERENABLED = "UnhandledExceptionHandlerEnabled"; const bool DEFAULTVALUE_UNHANDLEDEXCEPTIONHANDLERENABLED = true;
        public const string CONFIGVALUE_UNHANDLEDEXCEPTIONTHREADHANDLERENABLED = "UnhandledExceptionThreadHandlerEnabled"; const bool DEFAULTVALUE_UNHANDLEDEXCEPTIONTHREADHANDLERENABLED = true;

        public const string CONFIGVALUE_UNWINDEXCEPTIONSPATTERN = "UnwindExceptionsPattern"; const string DEFAULTVALUE_UNWINDEXCEPTIONSPATTERN = "AggregateException";
        public const string CONFIGVALUE_IGNOREEXCEPTIONSPATTERN = "IgnoreExceptionsPattern"; const string DEFAULTVALUE_IGNOREEXCEPTIONSPATTERN = "";
        public const string CONFIGVALUE_INFORMATIONEXCEPTIONSPATTERN = "InformationExceptionsPattern"; const string DEFAULTVALUE_INFORMATIONEXCEPTIONSPATTERN = "";
        public const string CONFIGVALUE_WARNINGEXCEPTIONSPATTERN = "WarningExceptionsPattern"; const string DEFAULTVALUE_WARNINGEXCEPTIONSPATTERN = "";
        public const string CONFIGVALUE_ERROREXCEPTIONSPATTERN = "ErrorExceptionsPattern"; const string DEFAULTVALUE_ERROREXCEPTIONSPATTERN = "";
        public const string CONFIGVALUE_DISABLEEXCEPTIONS = "DisableExceptions"; const bool DEFAULTVALUE_DISABLEEXCEPTIONS = false;
        ///<summary>Codice di eccezione per la fine event log.</summary>
        public const int ERROR_LOG_FILE_FULL = 0x5de;
        ///<summary>Codice di categoria per le eccezioni.</summary>
        private const string S_CATEGORY_EXCEPTION = "except";
        #endregion
        #region Internal state
        private ILogger<ExceptionManager> logger;
        //static EccezioniConfigurationSection configSection = null;
        static int _exceptionsCount = 0;
        static bool _routeUnhandledExceptionsToFocusedElement = false;
        static bool _unhandledExceptionHandlerEnabled = false, _unhandledExceptionThreadHandlerEnabled = true, _disableExceptions;
        static string _unwindExceptionsPattern;
        static string _ignoreExceptionsPattern, _informationExceptionsPattern, _warningExceptionsPattern, _errorExceptionsPattern;
        static Regex _unwindExceptionsRegularExpression;
        static Regex _ignoreExceptionsRegularExpression, _informationExceptionsRegularExpression, _warningExceptionsRegularExpression, _errorExceptionsRegularExpression;
        static DialogWindow _currentDialog;
        #endregion
        #region commands
        public static readonly RoutedUICommand AddException = new RoutedUICommand("AddException", "AddException", typeof(ApplicationBase));
        public static readonly RoutedUICommand AddHistoryExceptions = new RoutedUICommand("AddHistoryExceptions", "AddHistoryExceptions", typeof(ApplicationBase));
        #endregion
        #region events
        public static readonly RoutedEvent PreviewExceptionEvent = EventManager.RegisterRoutedEvent("PreviewException", RoutingStrategy.Tunnel, typeof(EventHandler<PreviewExceptionEventArgs>), typeof(ExceptionManager));
        public static readonly RoutedEvent ExceptionEvent = EventManager.RegisterRoutedEvent("Exception", RoutingStrategy.Bubble, typeof(EventHandler<ExceptionEventArgs>), typeof(ExceptionManager));
        #endregion

        #region .ctor
        static ExceptionManager()
        {
            _routeUnhandledExceptionsToFocusedElement = ConfigurationHelper.GetClassSetting<ExceptionManager, bool>(CONFIGVALUE_ROUTEUNHANDLEDEXCEPTIONSTOFOCUSEDELEMENT, DEFAULTVALUE_ROUTEUNHANDLEDEXCEPTIONSTOFOCUSEDELEMENT); // , CultureInfo.InvariantCulture
            _unhandledExceptionHandlerEnabled = ConfigurationHelper.GetClassSetting<ExceptionManager, bool>(CONFIGVALUE_UNHANDLEDEXCEPTIONHANDLERENABLED, DEFAULTVALUE_UNHANDLEDEXCEPTIONHANDLERENABLED); // , CultureInfo.InvariantCulture
            _unhandledExceptionThreadHandlerEnabled = ConfigurationHelper.GetClassSetting<ExceptionManager, bool>(CONFIGVALUE_UNHANDLEDEXCEPTIONTHREADHANDLERENABLED, DEFAULTVALUE_UNHANDLEDEXCEPTIONTHREADHANDLERENABLED); // , CultureInfo.InvariantCulture
            _unwindExceptionsPattern = ConfigurationHelper.GetClassSetting<ExceptionManager, string>(CONFIGVALUE_UNWINDEXCEPTIONSPATTERN, DEFAULTVALUE_UNWINDEXCEPTIONSPATTERN); // , CultureInfo.InvariantCulture
            _ignoreExceptionsPattern = ConfigurationHelper.GetClassSetting<ExceptionManager, string>(CONFIGVALUE_IGNOREEXCEPTIONSPATTERN, DEFAULTVALUE_IGNOREEXCEPTIONSPATTERN); // , CultureInfo.InvariantCulture
            _informationExceptionsPattern = ConfigurationHelper.GetClassSetting<ExceptionManager, string>(CONFIGVALUE_INFORMATIONEXCEPTIONSPATTERN, DEFAULTVALUE_INFORMATIONEXCEPTIONSPATTERN); // , CultureInfo.InvariantCulture
            _warningExceptionsPattern = ConfigurationHelper.GetClassSetting<ExceptionManager, string>(CONFIGVALUE_WARNINGEXCEPTIONSPATTERN, DEFAULTVALUE_WARNINGEXCEPTIONSPATTERN); // , CultureInfo.InvariantCulture
            _errorExceptionsPattern = ConfigurationHelper.GetClassSetting<ExceptionManager, string>(CONFIGVALUE_ERROREXCEPTIONSPATTERN, DEFAULTVALUE_ERROREXCEPTIONSPATTERN); // , CultureInfo.InvariantCulture

            _disableExceptions = ConfigurationHelper.GetClassSetting<ExceptionManager, bool>(CONFIGVALUE_DISABLEEXCEPTIONS, DEFAULTVALUE_DISABLEEXCEPTIONS); // , CultureInfo.InvariantCulture
        }
        #endregion
        #region Initialize
        /// <summary>Initialize exceptions management configuration</summary>
        public static void Initialize()
        {
            using (var sec = TraceLogger.BeginMethodScope(typeof(ExceptionManager)))
            {
                if (!StringHelper.IsEmpty(_ignoreExceptionsPattern)) { _ignoreExceptionsRegularExpression = new Regex(_ignoreExceptionsPattern); } // try { } catch { }
                if (!StringHelper.IsEmpty(_informationExceptionsPattern)) { _informationExceptionsRegularExpression = new Regex(_informationExceptionsPattern); } // try { } catch { }
                if (!StringHelper.IsEmpty(_warningExceptionsPattern)) { _warningExceptionsRegularExpression = new Regex(_warningExceptionsPattern); } // try { } catch { }
                if (!StringHelper.IsEmpty(_errorExceptionsPattern)) { _errorExceptionsRegularExpression = new Regex(_errorExceptionsPattern); } // try { } catch { }
                if (!StringHelper.IsEmpty(_unwindExceptionsPattern)) { _unwindExceptionsRegularExpression = new Regex(_unwindExceptionsPattern); } // try { } catch { }

                // ConfigurationManager.GetSection("ConfigurazioneRegisterBindingClasses");

                if (_unhandledExceptionHandlerEnabled)
                {
                    AppDomain.CurrentDomain.UnhandledException += DomainUnhandledException;
                    // Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(UnhandleExceptionHandler);
                }
                if (_unhandledExceptionThreadHandlerEnabled)
                {
                    Application.Current.Dispatcher.UnhandledException += new DispatcherUnhandledExceptionEventHandler(DispatcherUnhandleExceptionHandler);
                }
                //if (_unhandledExceptionThreadHandlerEnabled)
                //{
                //    Application.Current.MainWindow.Dispatcher.UnhandledException += new DispatcherUnhandledExceptionEventHandler(UnhandleExceptionHandler);
                //}
            }
        }
        #endregion

        #region Check
        ///<summary>Converte un return value in ina eccezione.</summary>
        public static object Check(object obj, object value, Exception e)
        {
            if (obj == value) throw e;
            return obj;
        }
        #endregion
        #region OnException
        public static void BeginOnException(DependencyObject source, Exception ex)
        {
            var app = Application.Current;
            Application.Current.ExecAsync(() => { RaiseExceptionForeground(source, ex); });
        }
        public static bool RaiseException(DependencyObject source, Exception ex)
        {
            bool ret = true;
            Application.Current.Exec(() => { ret = RaiseExceptionForeground(source, ex); });
            return ret;
        }
        public static bool RaiseExceptionForeground(DependencyObject source, Exception ex)
        {
            // Check for exception severity and translations.
            var exceptionType = default(Type); if (ex != null) { exceptionType = ex.GetType(); }
            Exception unwoundException = hlpUnwindException(ex);
            Type unwoundExceptionType = null; if (ex != null) { unwoundExceptionType = unwoundException.GetType(); }
            var abcException = unwoundException as ExceptionBase;

            // Level
            var level = ExceptionLevel.Unknown;
            if (abcException != null) { level = abcException.ExceptionLevel; }

            var win32Exception = unwoundException as Win32Exception;
            if (win32Exception != null)
            {
                switch (win32Exception.NativeErrorCode)
                {
                    case ExceptionManager.ERROR_LOG_FILE_FULL: { level = ExceptionLevel.Verbose; break; }
                    default: { level = ExceptionLevel.Error; break; }
                }
            }
            var invalidOperationException = unwoundException as InvalidOperationException;
            if (invalidOperationException != null && !string.IsNullOrEmpty(invalidOperationException.Message) && invalidOperationException.Message.IndexOf("IsFrozen") >= 0) { level = ExceptionLevel.None; return true; }
            var xamlParseException = unwoundException as XamlParseException;
            if (xamlParseException != null && !string.IsNullOrEmpty(xamlParseException.Message) && xamlParseException.Message.IndexOf("IsFrozen") >= 0) { level = ExceptionLevel.None; return true; }
            if (_ignoreExceptionsRegularExpression != null) { if (_ignoreExceptionsRegularExpression.IsMatch(unwoundExceptionType.FullName)) { level = ExceptionLevel.None; return true; } }

            DependencyObject fe = null;
            if (source != null)
            {
                fe = source;
            }
            else
            {
                // Look for the active window
                Window activeWindow = null;
                for (int i = Application.Current.Windows.Count - 1; i >= 0; i--)
                {
                    if (Application.Current.Windows[i].IsActive || Application.Current.Windows[i].Content != null
                                                                   && Application.Current.Windows[i].Content is FrameworkElement
                                                                   && ((FrameworkElement)Application.Current.Windows[i].Content).IsLoaded)
                    {
                        activeWindow = Application.Current.Windows[i];
                        break;
                    }
                }
                if (activeWindow == null)
                {
                    if (Application.Current.Windows.Count > 0)
                    {
                        activeWindow = Application.Current.Windows[0];
                    }
                }

                // Look for the focused element
                if (_routeUnhandledExceptionsToFocusedElement)
                {
                    fe = Keyboard.FocusedElement as FrameworkElement;
                    if (fe == null) { fe = FocusManager.GetFocusedElement(activeWindow) as FrameworkElement; }
                    if (fe == null) { fe = activeWindow; }
                }
                if (fe != null && fe == activeWindow)
                {
                    fe = activeWindow.Content as DependencyObject;
                }
            }

            bool cancel = ExceptionManager.OnPreviewExceptionInternalExecute(fe, ex);
            if (cancel == true) { return true; }

            bool caught = false;
            if (fe != null) { caught = ExceptionManager.OnExceptionInternalExecute(fe, ex); }

            if (caught == false)
            {
                var shellControl = ApplicationBase.Current != null ? ApplicationBase.Current.Properties[GlobalConstants.GLOBAL_CONTROLLOROOT] as UserControl : null;
                if (shellControl != null) { caught = ExceptionManager.OnExceptionInternalExecute(shellControl, ex); }
            }

            if (!caught)
            {
                try
                {
                    if (_disableExceptions == false)
                    {
                        ShowException(fe as FrameworkElement, ex); caught = true;
                    }
                }
                catch (Exception) { }
            }

            // Logger.Exception(ex);
            return caught;
        }
        
        #endregion

        #region OnPreviewExceptionInternalExecute
        private static bool OnPreviewExceptionInternalExecute(DependencyObject source, Exception ex)
        {
            //using (var sec = TraceManager.GetCodeSection(typeof(ExceptionManager), new { source, ex }))
            //{
            PreviewExceptionEventArgs e = new PreviewExceptionEventArgs(ex);

            if (source is ContentElement)
            {
                ((ContentElement)source).RaiseEvent(e);
            }
            else if (source is UIElement)
            {
                ((UIElement)source).RaiseEvent(e);
            }
            else if (source is UIElement3D)
            {
                ((UIElement3D)source).RaiseEvent(e);
            }

            return e.Cancel;
            //}
        }
        #endregion
        #region OnExceptionInternalExecute
        private static bool OnExceptionInternalExecute(DependencyObject source, Exception ex)
        {
            using (var sec = TraceLogger.BeginMethodScope(typeof(ExceptionManager), new { source, ex }))
            {
                ExceptionEventArgs e = new ExceptionEventArgs(ex);

                if (source is ContentElement)
                {
                    ((ContentElement)source).RaiseEvent(e);
                }
                else if (source is UIElement)
                {
                    ((UIElement)source).RaiseEvent(e);
                }
                else if (source is UIElement3D)
                {
                    ((UIElement3D)source).RaiseEvent(e);
                }

                return e.Caught;
            }
        }
        #endregion

        #region DumpStackTrace
        ///<summary>Esegue il dump di uno stack trace.</summary>
        public static void DumpStackTrace(StackTrace stackTrace)
        {
            return;
        }
        #endregion
        #region DumpStackTrace
        ///<summary>Esegue il dump di uno stack trace.</summary>
        public static void DumpStackTrace(string stackTrace)
        {
            string[] aCallStack = stackTrace != null ? stackTrace.Replace("\r", String.Empty).Split(new char[] { '\n' }) : null;
            if (aCallStack != null && aCallStack.Length > 0)
            {
                int nLen = aCallStack.Length;
                foreach (string sFrame in aCallStack)
                {
                    nLen--;
                    // cc.TraceValue("sFrame", new System.String('\t', (int)nLen) + sFrame, S_CATEGORY_EXCEPTION, MessageLevel.llWarning);
                }
            }
        }
        #endregion

        #region ThrowException
        ///<summary>Solleva una eccezione verso il chiamante o, verso il thread di foreground.</summary>
        public static void ThrowException(Exception ex)
        {
            using (var sec = TraceLogger.BeginMethodScope(typeof(ExceptionManager), new { ex }))
            {
                throw ex;
            }
        }
        #endregion
        #region ThrowExceptionForeground
        public static void ThrowExceptionForeground(Exception ex)
        {
            using (var sec = TraceLogger.BeginMethodScope(typeof(ExceptionManager), new { ex }))
            {
                Application.Current.Exec(() => ThrowException(ex));
            }
        }
        #endregion

        #region IgnoreExceptionsRegularExpression
        public static Regex IgnoreExceptionsRegularExpression
        {
            get { return _ignoreExceptionsRegularExpression; }
            //set { _ignoreExceptionsRegularExpression = value; }
        }
        #endregion

        #region GetExceptionDialog
        public static DialogWindow GetExceptionDialog(out bool isNewDialogInstance)
        {
            using (var sec = TraceManager.GetCodeSection<ExceptionManager>())
            {
                isNewDialogInstance = false;
                //_currentDialog = null;
                if (_currentDialog == null)
                {
                    var exceptionsControl = new ExceptionControl();
                    var dialogWindow = new DialogWindow(exceptionsControl)
                    {
                        ShowInTaskbar = false,
                        WindowStyle = WindowStyle.None,
                        ResizeMode = ResizeMode.CanResize,
                        Title = "Exception occurred",
                        Height = 450,
                        Width = 800,
                        AllowsTransparency = false,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };

                    _currentDialog = dialogWindow ?? throw new InvalidOperationException("Exception dialog creation Failed.");

                    isNewDialogInstance = true;
                }

                sec.Debug($"isNewDialogInstance: {isNewDialogInstance.GetLogString()}, _currentDialog: {_currentDialog.GetLogString()}");
                return _currentDialog;
            }
        }
        #endregion
        #region ShowException
        public static void ShowException(FrameworkElement fe, Exception ex)
        {
            using (var sec = TraceManager.GetCodeSection<ExceptionManager>(new { fe, ex }))
            {
                bool isNewDialogInstance = false;
                var dialog = GetExceptionDialog(out isNewDialogInstance);
                if (dialog == null) { throw new ClientException($"No dialog registered for unhandled exception '{ex?.GetType()?.Name}'.", ex); }
                ExceptionManager.AddException.Execute(ex, dialog?.Items.LastOrDefault());

                if (isNewDialogInstance || dialog.Visibility != Visibility.Visible)
                {
                    bool? dialogResult = null;
                    try
                    {
                        _exceptionsCount = 0;
                        if (fe != null)
                        {
                            var ownerwindow = UserControlHelper.FindOwningWindow(fe);
                            dialog.Owner = ownerwindow;
                        }
                        else
                        {
                            var activeWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
                            if (activeWindow == null && Application.Current.MainWindow != null && Application.Current.MainWindow.IsInitialized) { activeWindow = Application.Current.MainWindow; }
                            if (dialog == activeWindow) { return; }
                            dialog.Owner = activeWindow;
                            //Mouse.OverrideCursor = Cursors.Arrow;
                        }
                        dialog.ShowInTaskbar = false;
                        dialog.WindowStyle = WindowStyle.None;
                        dialog.ResizeMode = ResizeMode.NoResize;
                        dialog.Title = "Exception occurred";
                        //dialog.Height = 120 * ((Common.ApplicationBase)Application.Current).Zoom;
                        //dialog.Width = 500 * ((Common.ApplicationBase)Application.Current).Zoom;
                        dialog.AllowsTransparency = true;
                        dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        dialogResult = dialog.ShowDialog();
                    }
                    finally { }
                    if (dialogResult == true) { Application.Current.Shutdown(); sec.Debug("Environment.Exit(0);..."); Environment.Exit(0); } // || dialog.Owner == null
                    _currentDialog = null;
                }
                else
                {
                    if (_exceptionsCount < MAXMESSAGECOUNT) { _exceptionsCount++; } else { Thread.Sleep(MAXMESSAGECOUNTDELAY); }
                }
            }
        }
        #endregion
        #region ShowMessage
        public void ShowMessage(Segnalazione s) { }
        #endregion
        #region HideMessage
        public static void HideMessage(object sender)
        {
            try
            {
                Exception ex = null;
                FrameworkElement fe;
                UserControl control = null;
                bool bSkipFirst = true;

                // Find active exception container
                if (!(sender is FrameworkElement)) { return; }
                if (sender is UserControl) { bSkipFirst = false; }

                fe = sender as FrameworkElement;
                while (ex == null)
                {
                    control = UserControlHelper.FindAncestor<UserControl>(fe, bSkipFirst);
                    if (control == null) break;
                    fe = control;
                    bSkipFirst = true;
                    if ((bool)control.GetValue(ExceptionProperties.BreakExceptionRoutingProperty))
                    {
                        //ex = control.Exception;
                        break;
                    }
                }

                //if (ex != null)
                //    exContainer.Exception = null;

                //if (DesktopManager.VistaCorrente != null && DesktopManager.VistaCorrente.QuadroCorrente != null)
                //    DesktopManager.VistaCorrente.QuadroCorrente.ExceptionContainer = null;
            }
            catch { }
        }
        #endregion

        #region UnhandleExceptionHandler
        private static void DomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            using (var sec = TraceLogger.BeginMethodScope(typeof(ExceptionManager), new { sender, e }))
            {
                var currentThread = Thread.CurrentThread;
                sec.LogDebug(new { CurrentThread = currentThread.GetLogString() });
                if (!currentThread.ThreadState.HasFlag(System.Threading.ThreadState.Running))
                {
                    sec.LogDebug($"The current thread is not running {currentThread.ThreadState}");
                    return;
                }
                //if (!Application.Current.Dispatcher.CheckAccess())
                //{
                //    // rethrow exception to main UI thread
                //    //Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action<Exception>(ExceptionManager.ThrowExceptionForeground), new BackgroundProcessingException("Eccezione non gestita nel processing di background", e.ExceptionObject as Exception));
                //    ExceptionManager.ThrowExceptionForeground(new BackgroundProcessingException("Eccezione non gestita nel processing di background", e.ExceptionObject as Exception));
                //    return;
                //}
                if (!ExceptionManager.RaiseException(null, e.ExceptionObject as Exception))
                {
                    // e.Handled = false;
                }
            }
        }
        static void DispatcherUnhandleExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            using (var sec = TraceLogger.BeginMethodScope(typeof(ExceptionManager), new { sender, e }))
            {
                var currentThread = Thread.CurrentThread;
                sec.LogDebug(new { CurrentThread = currentThread.GetLogString() });
                if (!currentThread.ThreadState.HasFlag(System.Threading.ThreadState.Running))
                {
                    sec.LogDebug($"The current thread is not running {currentThread.ThreadState}");
                    return;
                }

                UnhandleExceptionHandler(sender, e);
            }
        }
        static void UnhandleExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            using (var sec = TraceLogger.BeginMethodScope(typeof(ExceptionManager), new { sender, e }))
            {
                var currentThread = Thread.CurrentThread;
                sec.LogDebug(new { CurrentThread = currentThread.GetLogString() });
                if (!currentThread.ThreadState.HasFlag(System.Threading.ThreadState.Running))
                {
                    sec.LogDebug($"The current thread is not running {currentThread.ThreadState}");
                    return;
                }

                if (ExceptionManager.RaiseException(null, e.Exception as Exception))
                {
                    e.Handled = true;
                }
                //if (!Application.Current.Dispatcher.CheckAccess())
                //{
                //    sec.Warning($"rethrowing exception '{e.Exception.GetLogString()}' to main UI thread");
                //    ExceptionManager.ThrowExceptionForeground(new BackgroundProcessingException("Eccezione non gestita nel processing di background", e.Exception));
                //    return;
                //}
                if (e.Handled) return;
                //e.Handled = true;
                //if (!ExceptionManager.OnException(null, e.Exception))
                //{
                //    e.Handled = false;
                //}
            }
        }
        #endregion
        #region GetExceptionLevel
        public static ExceptionLevel GetExceptionLevel(Exception ex)
        {
            ExceptionLevel level = ExceptionLevel.Warning;
            if (ex == null) { return level; }
            Type exceptionType = ex.GetType();

            ExceptionBase abcException = ex as ExceptionBase;
            if (ex is ExceptionBase && abcException.ExceptionLevel != ExceptionLevel.Unknown) { level = abcException.ExceptionLevel; }

            Win32Exception win32Exception = ex as Win32Exception;
            if (win32Exception != null)
            {
                switch (win32Exception.NativeErrorCode)
                {
                    case ExceptionManager.ERROR_LOG_FILE_FULL: { level = ExceptionLevel.Verbose; break; }
                    default: { level = ExceptionLevel.Error; break; }
                }
            }
            if (_ignoreExceptionsRegularExpression != null) { if (_ignoreExceptionsRegularExpression.IsMatch(exceptionType.FullName)) { level = ExceptionLevel.None; } }
            if (_informationExceptionsRegularExpression != null) { if (_informationExceptionsRegularExpression.IsMatch(exceptionType.FullName)) { level = ExceptionLevel.Information; } }
            if (_warningExceptionsRegularExpression != null) { if (_warningExceptionsRegularExpression.IsMatch(exceptionType.FullName)) { level = ExceptionLevel.Warning; } }
            if (_errorExceptionsRegularExpression != null) { if (_errorExceptionsRegularExpression.IsMatch(exceptionType.FullName)) { level = ExceptionLevel.Error; } }

            return level;
        }
        #endregion
        #region hlpUnwindException
        internal static Exception hlpUnwindException(Exception ex)
        {
            Exception unwoundException = ex;
            while (unwoundException.InnerException != null && _unwindExceptionsRegularExpression != null && _unwindExceptionsRegularExpression.IsMatch(unwoundException.GetType().Name))
            {
                unwoundException = unwoundException.InnerException;
            }
            return unwoundException;
        }
        #endregion
        #region hlpGetExceptionMessage
        internal static string hlpGetExceptionMessage(Exception currentException)
        {
            string translatedMessage = null;
            translatedMessage = currentException.Message;
            return translatedMessage == null ? currentException.Message : translatedMessage;
        }
        #endregion
    }
    #endregion

    #region ExceptionProperties
    public class ExceptionProperties
    {
        #region Exception
        public static Exception GetException(DependencyObject obj)
        {
            return (Exception)obj.GetValue(ExceptionProperty);
        }
        public static void SetException(DependencyObject obj, Exception value)
        {
            obj.SetValue(ExceptionProperty, value);
        }
        public static readonly DependencyProperty ExceptionProperty = DependencyProperty.RegisterAttached("Exception", typeof(Exception), typeof(ExceptionProperties), new UIPropertyMetadata());
        #endregion
        #region Exceptions
        public static ObservableCollection<Exception> GetExceptions(DependencyObject obj)
        {
            return (ObservableCollection<Exception>)obj.GetValue(ExceptionsProperty);
        }
        public static void SetExceptions(DependencyObject obj, ObservableCollection<Exception> value)
        {
            obj.SetValue(ExceptionsProperty, value);
        }
        public static readonly DependencyProperty ExceptionsProperty = DependencyProperty.RegisterAttached("Exceptions", typeof(ObservableCollection<Exception>), typeof(ExceptionProperties), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region BreakExceptionRouting
        public static bool GetBreakExceptionRouting(DependencyObject obj)
        {
            return (bool)obj.GetValue(BreakExceptionRoutingProperty);
        }
        public static void SetBreakExceptionRouting(DependencyObject obj, bool value)
        {
            obj.SetValue(BreakExceptionRoutingProperty, value);
        }
        public static readonly DependencyProperty BreakExceptionRoutingProperty = DependencyProperty.RegisterAttached("BreakExceptionRouting", typeof(bool), typeof(ExceptionProperties), new UIPropertyMetadata(false));
        #endregion
        #region ShowExceptionExceptions
        public static bool GetShowExceptionExceptions(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowExceptionExceptionsProperty);
        }
        public static void SetShowExceptionExceptions(DependencyObject obj, bool value)
        {
            obj.SetValue(ShowExceptionExceptionsProperty, value);
        }
        public static readonly DependencyProperty ShowExceptionExceptionsProperty = DependencyProperty.RegisterAttached("ShowExceptionExceptions", typeof(bool), typeof(ExceptionProperties), new UIPropertyMetadata(false));
        #endregion
        //#region CollectABCExceptions
        //public static bool GetCollectABCExceptions(DependencyObject obj)
        //{
        //    return (bool)obj.GetValue(CollectABCExceptionsProperty);
        //}
        //public static void SetCollectABCExceptions(DependencyObject obj, bool value)
        //{
        //    obj.SetValue(CollectABCExceptionsProperty, value);
        //}
        //public static readonly DependencyProperty CollectABCExceptionsProperty = DependencyProperty.RegisterAttached("CollectABCExceptions", typeof(bool), typeof(ExceptionProperties), new UIPropertyMetadata(false));
        //#endregion
    }
    #endregion
}
