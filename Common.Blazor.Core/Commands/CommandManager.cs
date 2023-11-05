#region using
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
#endregion

namespace Common.Core.Blazor
{
    public class CommandManager
    {
        private List<CommandBinding> _commandBindings = new List<CommandBinding>();
        //private static HybridDictionary _classInputBindings = new HybridDictionary();
        public List<CommandBinding> CommandBindings
        {
            get { return _commandBindings; }
            set { _commandBindings = value; }
        }
    }

    public sealed class RoutedEvent
    {
        private string _name;

        //private RoutingStrategy _routingStrategy;

        private Type _handlerType;

        private Type _ownerType;

        private int _globalIndex;

        /// <summary>Gets the identifying name of the routed event.</summary>
        /// <returns>The name of the routed event.</returns>
        public string Name => _name;

        /// <summary>Gets the handler type of the routed event.</summary>
        /// <returns>The handler type of the routed event.</returns>
        public Type HandlerType => _handlerType;

        /// <summary>Gets the registered owner type of the routed event.</summary>
        /// <returns>The owner type of the routed event.</returns>
        public Type OwnerType => _ownerType;

        internal int GlobalIndex => _globalIndex;

        /// <summary>Associates another owner type with the routed event represented by a <see cref="T:System.Windows.RoutedEvent" /> instance, and enables routing of the event and its handling.</summary>
        /// <param name="ownerType">The type where the routed event is added.</param>
        /// <returns>The identifier field for the event. This return value should be used to set a public static read-only field that will store the identifier for the representation of the routed event on the owning type. This field is typically defined with public access, because user code must reference the field in order to attach any instance handlers for the routed event when using the <see cref="M:System.Windows.UIElement.AddHandler(System.Windows.RoutedEvent,System.Delegate,System.Boolean)" /> utility method.</returns>
        public RoutedEvent AddOwner(Type ownerType)
        {
            //GlobalEventManager.AddOwner(this, ownerType);
            return this;
        }

        internal bool IsLegalHandler(Delegate handler)
        {
            Type type = handler.GetType();
            if (!(type == HandlerType))
            {
                return type == typeof(RoutedEventHandler);
            }
            return true;
        }

        /// <summary>Returns the string representation of this <see cref="T:System.Windows.RoutedEvent" />.</summary>
        /// <returns>A string representation for this object, which is identical to the value returned by <see cref="P:System.Windows.RoutedEvent.Name" />.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[2]
            {
                _ownerType.Name,
                _name
            });
        }

        internal RoutedEvent(string name, Type handlerType, Type ownerType)
        {
            _name = name;
            _handlerType = handlerType;
            _ownerType = ownerType;
            //_globalIndex = GlobalEventManager.GetNextAvailableGlobalIndex(this);
        }
    }
    public class RoutedEventArgs : EventArgs
    {
        private RoutedEvent _routedEvent;

        private object _source;

        private object _originalSource;

        private BitVector32 _flags;

        private const int HandledIndex = 1;

        private const int UserInitiatedIndex = 2;

        private const int InvokingHandlerIndex = 4;

        /// <summary>Gets or sets the <see cref="P:System.Windows.RoutedEventArgs.RoutedEvent" /> associated with this <see cref="T:System.Windows.RoutedEventArgs" /> instance.</summary>
        /// <returns>The identifier for the event that has been invoked.</returns>
        /// <exception cref="T:System.InvalidOperationException">Attempted to change the <see cref="P:System.Windows.RoutedEventArgs.RoutedEvent" /> value while the event is being routed.</exception>
        public RoutedEvent RoutedEvent
        {
            get
            {
                return _routedEvent;
            }
            set
            {
                if (UserInitiated && InvokingHandler)
                {
                    throw new InvalidOperationException("RoutedEventCannotChangeWhileRouting");
                }
                _routedEvent = value;
            }
        }

        /// <summary>Gets or sets a value that indicates the present state of the event handling for a routed event as it travels the route.</summary>
        /// <returns>If setting, set to <see langword="true" /> if the event is to be marked handled; otherwise <see langword="false" />. If reading this value, <see langword="true" /> indicates that either a class handler, or some instance handler along the route, has already marked this event handled. <see langword="false" />.indicates that no such handler has marked the event handled.  
        ///  The default value is <see langword="false" />.</returns>
        public bool Handled
        {
            get
            {
                return _flags[1];
            }
            set
            {
                //if (_routedEvent == null)
                //{
                //    throw new InvalidOperationException("RoutedEventArgsMustHaveRoutedEvent");
                //}
                //if (TraceRoutedEvent.IsEnabled)
                //{
                //    TraceRoutedEvent.TraceActivityItem(TraceRoutedEvent.HandleEvent, value, RoutedEvent.OwnerType.Name, RoutedEvent.Name, this);
                //}
                _flags[1] = value;
            }
        }

        /// <summary>Gets or sets a reference to the object that raised the event.</summary>
        /// <returns>The object that raised the event.</returns>
        public object Source
        {
            get
            {
                return _source;
            }
            set
            {
                if (InvokingHandler && UserInitiated)
                {
                    throw new InvalidOperationException("RoutedEventCannotChangeWhileRouting");
                }
                if (_routedEvent == null)
                {
                    throw new InvalidOperationException("RoutedEventArgsMustHaveRoutedEvent");
                }
                if (_source == null && _originalSource == null)
                {
                    _source = (_originalSource = value);
                    OnSetSource(value);
                }
                else if (_source != value)
                {
                    _source = value;
                    OnSetSource(value);
                }
            }
        }

        /// <summary>Gets the original reporting source as determined by pure hit testing, before any possible <see cref="P:System.Windows.RoutedEventArgs.Source" /> adjustment by a parent class.</summary>
        /// <returns>The original reporting source, before any possible <see cref="P:System.Windows.RoutedEventArgs.Source" /> adjustment made by class handling, which may have been done to flatten composited element trees.</returns>
        public object OriginalSource => _originalSource;

        internal bool UserInitiated
        {
            get
            {
                if (_flags[2])
                {
                    return true;
                }
                return false;
            }
        }

        private bool InvokingHandler
        {
            get { return _flags[4]; }
            set { _flags[4] = value; }
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.RoutedEventArgs" /> class.</summary>
        public RoutedEventArgs() { }

        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.RoutedEventArgs" /> class, using the supplied routed event identifier.</summary>
        /// <param name="routedEvent">The routed event identifier for this instance of the <see cref="T:System.Windows.RoutedEventArgs" /> class.</param>
        public RoutedEventArgs(RoutedEvent routedEvent)
            : this(routedEvent, null) { }

        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.RoutedEventArgs" /> class, using the supplied routed event identifier, and providing the opportunity to declare a different source for the event.</summary>
        /// <param name="routedEvent">The routed event identifier for this instance of the <see cref="T:System.Windows.RoutedEventArgs" /> class.</param>
        /// <param name="source">An alternate source that will be reported when the event is handled. This pre-populates the <see cref="P:System.Windows.RoutedEventArgs.Source" /> property.</param>
        public RoutedEventArgs(RoutedEvent routedEvent, object source)
        {
            _routedEvent = routedEvent;
            _source = (_originalSource = source);
        }

        internal void OverrideRoutedEvent(RoutedEvent newRoutedEvent)
        {
            _routedEvent = newRoutedEvent;
        }

        internal void OverrideSource(object source)
        {
            _source = source;
        }

        /// <summary>When overridden in a derived class, provides a notification callback entry point whenever the value of the <see cref="P:System.Windows.RoutedEventArgs.Source" /> property of an instance changes.</summary>
        /// <param name="source">The new value that <see cref="P:System.Windows.RoutedEventArgs.Source" /> is being set to.</param>
        protected virtual void OnSetSource(object source)
        {
        }

        /// <summary>When overridden in a derived class, provides a way to invoke event handlers in a type-specific way, which can increase efficiency over the base implementation.</summary>
        /// <param name="genericHandler">The generic handler / delegate implementation to be invoked.</param>
        /// <param name="genericTarget">The target on which the provided handler should be invoked.</param>
        protected virtual void InvokeEventHandler(Delegate genericHandler, object genericTarget)
        {
            if ((object)genericHandler == null) { throw new ArgumentNullException("genericHandler"); }
            if (genericTarget == null) { throw new ArgumentNullException("genericTarget"); }
            if (_routedEvent == null) { throw new InvalidOperationException("RoutedEventArgsMustHaveRoutedEvent"); }
            InvokingHandler = true;
            try
            {
                if (genericHandler is RoutedEventHandler)
                {
                    ((RoutedEventHandler)genericHandler)(genericTarget, this);
                    return;
                }
                genericHandler.DynamicInvoke(genericTarget, this);
            }
            finally
            {
                InvokingHandler = false;
            }
        }

        internal void InvokeHandler(Delegate handler, object target)
        {
            InvokingHandler = true;
            try
            {
                InvokeEventHandler(handler, target);
            }
            finally
            {
                InvokingHandler = false;
            }
        }

        internal void MarkAsUserInitiated()
        {
            _flags[2] = true;
        }

        internal void ClearUserInitiated()
        {
            _flags[2] = false;
        }
    }
    public sealed class CanExecuteRoutedEventArgs : RoutedEventArgs
    {
        private ICommand _command;
        private object _parameter;
        private bool _canExecute;
        private bool _continueRouting;

        /// <summary>Gets the command associated with this event.</summary>
        /// <returns>The command. Unless the command is a custom command, this is generally a <see cref="T:System.Windows.Input.RoutedCommand" />. There is no default value.</returns>
        public ICommand Command => _command;
        /// <summary>Gets the command specific data.</summary>
        /// <returns>The command data.  The default value is <see langword="null" />.</returns>
        public object Parameter => _parameter;
        /// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Input.RoutedCommand" /> associated with this event can be executed on the command target.</summary>
        /// <returns>
        ///   <see langword="true" /> if the event can be executed on the command target; otherwise, <see langword="false" />.  The default value is <see langword="false" />.</returns>
        public bool CanExecute
        {
            get { return _canExecute; }
            set { _canExecute = value; }
        }
        /// <summary>Determines whether the input routed event that invoked the command should continue to route through the element tree.</summary>
        /// <returns>
        ///   <see langword="true" /> if the routed event should continue to route through element tree; otherwise, <see langword="false" />.   The default value is <see langword="false" />.</returns>
        public bool ContinueRouting
        {
            get { return _continueRouting; }
            set { _continueRouting = value; }
        }

        internal CanExecuteRoutedEventArgs(ICommand command, object parameter)
        {
            if (command == null) { throw new ArgumentNullException("command"); }
            _command = command;
            _parameter = parameter;
        }

        internal void InvokeEventHandler(Delegate genericHandler, object target)
        {
            CanExecuteRoutedEventHandler canExecuteRoutedEventHandler = (CanExecuteRoutedEventHandler)genericHandler;
            canExecuteRoutedEventHandler(target as object, this);
        }
    }
    public sealed class ExecutedRoutedEventArgs : RoutedEventArgs
    {
        private ICommand _command;
        private object _parameter;

        /// <summary>Gets the command that was invoked.</summary>
        /// <returns>The command associated with this event.</returns>
        public ICommand Command => _command;

        /// <summary>Gets data parameter of the command.</summary>
        /// <returns>The command-specific data. The default value is <see langword="null" />.</returns>
        public object Parameter => _parameter;

        internal ExecutedRoutedEventArgs(ICommand command, object parameter)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            _command = command;
            _parameter = parameter;
        }

        internal void InvokeEventHandler(Delegate genericHandler, object target)
        {
            ExecutedRoutedEventHandler executedRoutedEventHandler = (ExecutedRoutedEventHandler)genericHandler;
            executedRoutedEventHandler(target as object, this);
        }
    }

    public delegate void ExecutedRoutedEventHandler(object sender, ExecutedRoutedEventArgs e);
    public delegate void CanExecuteRoutedEventHandler(object sender, CanExecuteRoutedEventArgs e);
    public delegate void RoutedEventHandler(object sender, RoutedEventArgs e);

    /// <summary>Binds a <see cref="T:System.Windows.Input.RoutedCommand" /> to the event handlers that implement the command.</summary>
    public class CommandBinding
    {
        private ICommand _command;
        /// <summary>Gets or sets the <see cref="T:System.Windows.Input.ICommand" /> associated with this <see cref="T:System.Windows.Input.CommandBinding" />.</summary>
        /// <returns>The command associated with this binding.</returns>
        public ICommand Command
        {
            get { return _command; }
            set
            {
                if (value == null) { throw new ArgumentNullException("value"); }
                _command = value;
            }
        }
        /// <summary>Occurs when the command associated with this <see cref="T:System.Windows.Input.CommandBinding" /> executes.</summary>
        public event ExecutedRoutedEventHandler PreviewExecuted;
        /// <summary>Occurs when the command associated with this <see cref="T:System.Windows.Input.CommandBinding" /> executes.</summary>
        public event ExecutedRoutedEventHandler Executed;
        /// <summary>Occurs when the command associated with this <see cref="T:System.Windows.Input.CommandBinding" /> initiates a check to determine whether the command can be executed on the current command target.</summary>
        public event CanExecuteRoutedEventHandler PreviewCanExecute;
        /// <summary>Occurs when the command associated with this <see cref="T:System.Windows.Input.CommandBinding" /> initiates a check to determine whether the command can be executed on the command target.</summary>
        public event CanExecuteRoutedEventHandler CanExecute;
        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.CommandBinding" /> class.</summary>
        public CommandBinding()
        {
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.CommandBinding" /> class by using the specified <see cref="T:System.Windows.Input.ICommand" />.</summary>
        /// <param name="command">The command to base the new <see cref="T:System.Windows.Input.RoutedCommand" /> on.</param>
        public CommandBinding(ICommand command)
            : this(command, null, null)
        {
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.CommandBinding" /> class by using the specified <see cref="T:System.Windows.Input.ICommand" /> and the specified <see cref="E:System.Windows.Input.CommandBinding.Executed" /> event handler.</summary>
        /// <param name="command">The command to base the new <see cref="T:System.Windows.Input.RoutedCommand" /> on.</param>
        /// <param name="executed">The handler for the <see cref="E:System.Windows.Input.CommandBinding.Executed" /> event on the new <see cref="T:System.Windows.Input.RoutedCommand" />.</param>
        public CommandBinding(ICommand command, ExecutedRoutedEventHandler executed)
            : this(command, executed, null)
        {
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.CommandBinding" /> class by using the specified <see cref="T:System.Windows.Input.ICommand" /> and the specified <see cref="E:System.Windows.Input.CommandBinding.Executed" /> and <see cref="E:System.Windows.Input.CommandBinding.CanExecute" /> event handlers.</summary>
        /// <param name="command">The command to base the new <see cref="T:System.Windows.Input.RoutedCommand" /> on.</param>
        /// <param name="executed">The handler for the <see cref="E:System.Windows.Input.CommandBinding.Executed" /> event on the new <see cref="T:System.Windows.Input.RoutedCommand" />.</param>
        /// <param name="canExecute">The handler for the <see cref="E:System.Windows.Input.CommandBinding.CanExecute" /> event on the new <see cref="T:System.Windows.Input.RoutedCommand" />.</param>
        public CommandBinding(ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute)
        {
            if (command == null) { throw new ArgumentNullException("command"); }
            _command = command;
            if (executed != null) { Executed += executed; }
            if (canExecute != null) { CanExecute += canExecute; }
        }
        internal void OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            if (this.CanExecute != null)
            {
                this.CanExecute(sender, e);
                if (e.CanExecute)
                {
                    e.Handled = true;
                }
            }
        }
        private bool CheckCanExecute(object sender, ExecutedRoutedEventArgs e)
        {
            CanExecuteRoutedEventArgs canExecuteRoutedEventArgs = new CanExecuteRoutedEventArgs(e.Command, e.Parameter);
            //canExecuteRoutedEventArgs.RoutedEvent = CommandManager.CanExecuteEvent;
            canExecuteRoutedEventArgs.Source = e.OriginalSource;
            canExecuteRoutedEventArgs.OverrideSource(e.Source);
            OnCanExecute(sender, canExecuteRoutedEventArgs);
            return canExecuteRoutedEventArgs.CanExecute;
        }
        internal void OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Handled) { return; }

            //CanExecuteRoutedEventArgs ceArg = new CanExecuteRoutedEventArgs(this.Command, e.Parameter);
            //OnCanExecute(sender, ceArg);
            //if (ceArg.CanExecute)
            //{
            this.Executed(sender, e);
            e.Handled = true;
            //}
        }
    }
}
