#region using
using Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
#endregion

namespace Common
{
    #region ExecutionMode
    //[Flags]
    public enum ExecutionMode
    {
        Normal = 0,
        Safe = 2,
        Debug = 4,
        Test = 8
    }
    #endregion

    public class ApplicationBase : Application, INotifyPropertyChanged
    {
        private ILogger<ApplicationBase> logger;
        #region OnIdleProcessing
        ///<summary>Esegue processing di bakcground periodicamente, in accordo con la configurazione.</summary>
        public event EventHandler OnIdleProcessing;
        #endregion
        #region internal state
        static string _environment;
        ExecutionMode _executionMode;
        bool _isStartupComplete;
        Dispatcher _dispatcher;
        bool _useIdleProcessing, _idleProcessingBThreadExit;
        int _idleProcessingInterval;
        Thread _idleProcessingBThread;
        #endregion

        #region .ctor
        public ApplicationBase()
            : base()
        {
            using (var scope = logger.BeginMethodScope())
            {
                if (TraceLogger.Configuration == null) { TraceLogger.InitConfiguration(TraceLogger.Configuration); }
                ConfigurationHelper.Init(TraceLogger.Configuration);

                _executionMode = ConfigurationHelper.GetClassSetting<ApplicationBase, ExecutionMode>(GlobalConstants.CONFIGVALUE_EXECUTIONMODE, GlobalConstants.DEFAULTVALUE_EXECUTIONMODE); // , CultureInfo.InvariantCulture
                _environment = ConfigurationHelper.GetClassSetting<ApplicationBase, string>(GlobalConstants.CONFIGVALUE_ENVIRONMENT, GlobalConstants.DEFAULTVALUE_ENVIRONMENT); // , CultureInfo.InvariantCulture
                _useIdleProcessing = ConfigurationHelper.GetClassSetting<ApplicationBase, bool>(GlobalConstants.CONFIGVALUE_USEIDLEPROCESSING, GlobalConstants.DEFAULTVALUE_USEIDLEPROCESSING); // , CultureInfo.InvariantCulture
                _idleProcessingInterval = ConfigurationHelper.GetClassSetting<ApplicationBase, int>(GlobalConstants.CONFIGVALUE_IDLEPROCESSINGINTERVAL, GlobalConstants.DEFAULTVALUE_IDLEPROCESSINGINTERVAL); // , CultureInfo.InvariantCulture
                if (_useIdleProcessing)
                {
                    _idleProcessingBThread = new Thread(new ThreadStart(IdleProcessing));
                    _idleProcessingBThread.IsBackground = true;
                    _idleProcessingBThread.Priority = ThreadPriority.Lowest;
                    _idleProcessingBThread.Start();
                }
                //_gcCollectInterval = ConfigurationHelper.GetAppSetting<int>(CONFIGVALUE_GCCOLLECTINTERVAL, DEFAULTVALUE_GCCOLLECTINTERVAL, CultureInfo.InvariantCulture);
                //_gcCollectGeneration = ConfigurationHelper.GetAppSetting<int>(CONFIGVALUE_GCCOLLECTGENERATION, DEFAULTVALUE_GCCOLLECTGENERATION, CultureInfo.InvariantCulture);
                //_gcCollectMode = ConfigurationHelper.GetAppSetting<GCCollectionMode>(CONFIGVALUE_GCCOLLECTMODE, DEFAULTVALUE_GCCOLLECTMODE, CultureInfo.InvariantCulture);

                //_dynamicProperties.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_dynamicProperties_PropertyChanged);
                ExceptionManager.Initialize();
            }
        }
        #endregion

        public IHost Host { get; set; }

        #region Zoom
        public double Zoom
        {
            get { return GetValue(() => Zoom); }
            set
            {
                if (value < this.ZoomMin) { value = this.ZoomMin; }
                if (value > this.ZoomMax) { value = this.ZoomMax; }
                SetValue(() => Zoom, value);
            }
        }
        #endregion
        #region ZoomMin
        public double ZoomMin
        {
            get { return GetValue(() => ZoomMin); }
            set { SetValue(() => ZoomMin, value); }
        }
        #endregion
        #region ZoomMax
        public double ZoomMax
        {
            get { return GetValue(() => ZoomMax); }
            set { SetValue(() => ZoomMax, value); }
        }
        #endregion
        #region ShowDeveloperTools
        public bool ShowDeveloperTools
        {
            get { return GetValue(() => ShowDeveloperTools); }
            set { SetValue(() => ShowDeveloperTools, value); }
        }
        #endregion

        #region ExecutionMode
        public ExecutionMode ExecutionMode
        {
            get { return _executionMode; }
        }
        #endregion
        #region IsStartupComplete
        /// <summary>True, se il processing di startup ed il processing di background sono completati</summary>
        public bool IsStartupComplete
        {
            get { return _isStartupComplete; }
        }
        #endregion
        #region IsPersistentStateInvalid
        public bool IsPersistentStateDirty { get; set; }
        #endregion

        // idle processing
        #region IdleProcessing support
        ///<summary>realizza l'idle Processing; se IdleProcessing è abilitato questa funzione esegue su un thread di background ed esegue periodicamente l'evento OnIdleProcessing.</summary>
        private void IdleProcessing()
        {
            for (; _idleProcessingBThreadExit == false;)
            {
                try
                {
                    if (OnIdleProcessing != null) { OnIdleProcessing(this, new EventArgs()); }

                    if (_idleProcessingBThreadExit == false) { Thread.Sleep(_idleProcessingInterval); }
                }
                catch (Exception) { } // don't block idle processing thread on exceptions
            }
        }
        #endregion
        #region OnStartup
        /// <summary>invoca l'evento di startup per l'oggetto application e scatena il processing di background.
        /// seguendo il pattern "Event-based Asynchronous Pattern".</summary>
        protected override async void OnStartup(StartupEventArgs e)
        {
            using var scope = logger.BeginMethodScope(null);

            //Microsoft.Win32.SystemEvents.DisplaySettingsChanged += new EventHandler((sender, eventArg) => this.CalcolaFattoriZoom());
            //this.CalcolaFattoriZoom();
            base.OnStartup(e);

            string username = Environment.UserName;
            var environment = TraceManager.EnvironmentName;
            var instanceName = string.Format(GlobalConstants.ENVIRONMENTUSER_FORMAT, environment, username, "");

            this.ZoomMin = 0.7;
            this.ZoomMax = 1.7;
            Exception exRestore = await ABCActivator.RestoreState<ApplicationBase, ZoomState, ZoomStateProvider>(this, instanceName, null);
            if (exRestore != null)
            {
                double defZoom = 1.0;
                if (defZoom < this.ZoomMin) { defZoom = this.ZoomMin; }
                if (defZoom > this.ZoomMax) { defZoom = this.ZoomMax; }
                this.Zoom = defZoom;
            }

            this.OnIdleProcessing += new EventHandler(ApplicationBase_OnIdleProcessing);
            this.PropertyChanged += new PropertyChangedEventHandler(ApplicationBase_PropertyChanged);

            _dispatcher = this.Dispatcher;
            //  LoadCommonStyles();

            //  StartupAsyncTaskEventArgs backgroundArgs = new StartupAsyncTaskEventArgs() { Args = e.Args };
            //  _backgroundThread = new Thread(new ParameterizedThreadStart(OnStartupAsync));
            //  _backgroundThread.Start(backgroundArgs);
            _isStartupComplete = true;
        }
        #endregion
        #region ApplicationBase_PropertyChanged
        void ApplicationBase_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Zoom":
                    this.IsPersistentStateDirty = true;
                    break;
                default:
                    break;
            }
        }
        #endregion
        #region ApplicationBase_OnIdleProcessing
        void ApplicationBase_OnIdleProcessing(object sender, EventArgs e)
        {
            try
            {
                var dispatcher = Application.Current?.Dispatcher;
                if (dispatcher == null) { return; }
                if (dispatcher.HasShutdownStarted || dispatcher.Thread == null || !dispatcher.Thread.IsAlive || dispatcher.Thread.ThreadState != System.Threading.ThreadState.Running) { return; }

                dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action<Exception>(async (ex) => await PersistApplicationState(ex)), null);
                dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action<Exception>(async (ex) => await PersistWindowsState(ex)), null);
            }
            catch (Exception ex) { TraceManager.Exception(ex); }
        }
        #endregion
        #region PersistApplicationState
        public async Task PersistApplicationState(Exception ex)
        {
            ApplicationBase application = this;
            if (application == null || application.IsStartupComplete == false) { return; }

            if (application != null && application.IsPersistentStateDirty)
            {
                string username = Environment.UserName;
                var environment = TraceManager.EnvironmentName;
                string instanceName = string.Format(GlobalConstants.ENVIRONMENTUSER_FORMAT, environment, username, "");

                //ClassStateProvider<ApplicationBase, ZoomState, ZoomSurrogate> sur2 = new ClassStateProvider<ApplicationBase, ZoomState, ZoomSurrogate>();
                var sur2 = new ZoomStateProvider();
                await ABCActivator.Persist<ApplicationBase, ZoomState>(application, instanceName, null, sur2);

                application.IsPersistentStateDirty = false;
            }
        }
        #endregion
        #region PersistWindowsState
        public async Task PersistWindowsState(Exception ex)
        {
            string username = Environment.UserName;
            var environment = TraceManager.EnvironmentName;

            var windows = Application.Current?.Windows;
            if (windows != null)
            {
                foreach (Window window in windows)
                {
                    if (window == null || window.IsLoaded == false) { continue; }

                    if (window != null) //  && window.IsPositionStateDirty
                    {
                        var instanceName = string.Format(GlobalConstants.ENVIRONMENTUSER_FORMAT, environment, username, window.Name);
                        if (instanceName == null) { return; }

                        var sur2 = new PositionStateProvider();
                        //var positionState = sur2.GetState(window);

                        await ABCActivator.Persist<Window, PositionState>((Window)window, instanceName + ".Location", null, sur2);
                        // window.IsPositionStateDirty = false;
                    }
                }
            }
        }
        #endregion

        #region OnActivated
        protected override void OnActivated(EventArgs e)
        {
            //using (var sec = this.GetCodeSection(SourceLevels.Verbose, new { e }))
            //{
            base.OnActivated(e);
            //}
        }
        #endregion
        #region OnDeactivated
        /// <summary>Invoca il processing di deattivazione della applicazione.</summary>
        protected override void OnDeactivated(EventArgs e)
        {
            //using (var sec = this.GetCodeSection(SourceLevels.Verbose, new { e }))
            //{
            base.OnDeactivated(e);
            //}
        }
        #endregion
        #region OnExit
        /// <summary>Invoca il processing di terminazione della applicazione.</summary>
        protected override void OnExit(ExitEventArgs e)
        {
            using var scope = logger.BeginMethodScope(new { e });
            //{
            _idleProcessingBThreadExit = true;

            scope.LogDebug(new { _idleProcessingBThread = _idleProcessingBThread.GetLogString() });
            if (_idleProcessingBThread != null)
            {
                if (_idleProcessingBThread != null && _idleProcessingBThread.IsAlive)
                {
                    _idleProcessingBThread.Join(GlobalConstants.DEFAULTVALUE_JOINIDLEPROCESSINGTIMEOUT); scope.LogDebug($"_idleProcessingBThread.Join({GlobalConstants.DEFAULTVALUE_JOINIDLEPROCESSINGTIMEOUT}); completed");
                }
            }

            base.OnExit(e);
            //}
        }
        #endregion

        #region SetProperty
        public void SetProperty<T>(string propertyName, T value)
        {
            var oldValue = Properties[propertyName];
            Properties[propertyName] = value;
            if (!object.Equals(oldValue, value)) { NotifyPropertyChanged(propertyName); }
        }
        #endregion
        #region GetProperty
        protected T GetProperty<T>(string propertyName)
        {
            object value;
            if (Properties.Contains(propertyName))
            {
                value = Properties[propertyName];
                return (T)value;
            }
            return default(T);
        }
        #endregion
        // INotifyPropertyChanged 
        #region SetValue
        protected void SetValue<T>(Expression<Func<T>> propertySelector, T value)
        {
            string propertyName = GetPropertyName(propertySelector);
            var oldValue = Properties[propertyName];
            Properties[propertyName] = value;
            if (!object.Equals(oldValue, value)) { NotifyPropertyChanged(propertySelector); }
        }
        #endregion
        #region SetValue
        protected void SetValue<T>(string propertyName, T value)
        {
            var oldValue = Properties[propertyName];
            Properties[propertyName] = value;
            if (!object.Equals(oldValue, value)) { NotifyPropertyChanged(propertyName); }
        }
        #endregion
        #region GetValue
        protected T GetValue<T>(Expression<Func<T>> propertySelector)
        {
            string propertyName = GetPropertyName(propertySelector);
            return GetValue<T>(propertyName);
        }
        #endregion
        #region GetValue
        protected T GetValue<T>(string propertyName)
        {
            object value;
            if (Properties.Contains(propertyName))
            {
                value = Properties[propertyName];
                return (T)value;
            }
            return default(T);
        }
        #endregion
        #region NotifyPropertyChanged
        protected void NotifyPropertyChanged<T>(Expression<Func<T>> propertySelector)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                string propertyName = GetPropertyName(propertySelector);
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
        #region INotifyPropertyChanged Members
        /// <summary>Raised when a property on this object has a new value.</summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>Raises this object's PropertyChanged event.</summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
        #endregion // INotifyPropertyChanged Members
        #region GetPropertyName
        private string GetPropertyName(LambdaExpression expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null) { throw new InvalidOperationException(); }
            return memberExpression.Member.Name;
        }
        #endregion

        #region Debugging
        /// <summary>Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.</summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            //Verify that the property name matches a real, public, instance property on this object.
            //if (this.GetType().GetProperty(propertyName) == null)
            //{
            //    string msg = "Invalid property name: " + propertyName;
            //    if (this.ThrowOnInvalidPropertyName)
            //        throw new Exception(msg);
            //    else
            //        Debug.Assert(false, msg);
            //}
        }
        /// <summary>Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.</summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }
        #endregion
    }
}
