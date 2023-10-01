#region using
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Binding = System.Windows.Data.Binding;
using DataObject = System.Windows.DataObject;
using DragEventArgs = System.Windows.DragEventArgs;
using DragEventHandler = System.Windows.DragEventHandler;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using KeyEventHandler = System.Windows.Input.KeyEventHandler;
using TextBox = System.Windows.Controls.TextBox;
#endregion

namespace Common
{
    #region IBlocksPropertyChange
    public interface IBlocksPropertyChange
    {
        bool BlocksPropertyChange { get; set; }
    }
    #endregion

    #region ABCBindingGroup
    [Obsolete]
    public class ABCBindingGroup : BindingGroup
    {
        #region IsValid
        public bool IsValid
        {
            get
            {
                foreach (BindingExpression expression in this.BindingExpressions)
                {
                    if (expression.HasError)
                    {
                        SetValue(IsValidProperty, false);
                        return false;
                    }
                }
                SetValue(IsValidProperty, true);
                return true;
            }
            set { SetValue(IsValidProperty, value); }
        }
        public static readonly DependencyProperty IsValidProperty = DependencyProperty.Register("IsValid", typeof(bool), typeof(ABCBindingGroup), new UIPropertyMetadata(true));
        #endregion

        #region .ctor
        public ABCBindingGroup()
            : base()
        {
        }
        #endregion
    }
    #endregion
    /// <summary>A base class for custom markup extension which provides properties
    /// that can be found on regular <see cref="Binding"/> markup extension.</summary>
    [MarkupExtensionReturnType(typeof(object))]
    public abstract class BindingDecoratorBase : MarkupExtension
    {
        /// <summary>
        /// The decorated binding class.
        /// </summary>
        protected Binding _binding;

        public BindingDecoratorBase()
        {
            _binding = new Binding();
        }

        public BindingDecoratorBase(string path)
        {
            _binding = new Binding(path);
        }

        #region properties

        [DefaultValue(null)]
        public object AsyncState
        {
            get { return _binding.AsyncState; }
            set { _binding.AsyncState = value; }
        }

        [DefaultValue(null)]
        public string BindingGroupName
        {
            get { return _binding.BindingGroupName; }
            set { _binding.BindingGroupName = value; }
        }

        [DefaultValue(false)]
        public bool BindsDirectlyToSource
        {
            get { return _binding.BindsDirectlyToSource; }
            set { _binding.BindsDirectlyToSource = value; }
        }

        [DefaultValue(null)]
        public IValueConverter Converter
        {
            get { return _binding.Converter; }
            set { _binding.Converter = value; }
        }

        [TypeConverter(typeof(CultureInfoIetfLanguageTagConverter)), DefaultValue(null)]
        public CultureInfo ConverterCulture
        {
            get { return _binding.ConverterCulture; }
            set { _binding.ConverterCulture = value; }
        }

        [DefaultValue(null)]
        public object ConverterParameter
        {
            get { return _binding.ConverterParameter; }
            set { _binding.ConverterParameter = value; }
        }

        [DefaultValue(null)]
        public string ElementName
        {
            get { return _binding.ElementName; }
            set { _binding.ElementName = value; }
        }

        [DefaultValue(null)]
        public object FallbackValue
        {
            get { return _binding.FallbackValue; }
            set { _binding.FallbackValue = value; }
        }

        [DefaultValue(false)]
        public bool IsAsync
        {
            get { return _binding.IsAsync; }
            set { _binding.IsAsync = value; }
        }

        [DefaultValue(BindingMode.Default)]
        public BindingMode Mode
        {
            get { return _binding.Mode; }
            set { _binding.Mode = value; }
        }

        [DefaultValue(false)]
        public bool NotifyOnSourceUpdated
        {
            get { return _binding.NotifyOnSourceUpdated; }
            set { _binding.NotifyOnSourceUpdated = value; }
        }

        [DefaultValue(false)]
        public bool NotifyOnTargetUpdated
        {
            get { return _binding.NotifyOnTargetUpdated; }
            set { _binding.NotifyOnTargetUpdated = value; }
        }

        [DefaultValue(false)]
        public bool NotifyOnValidationError
        {
            get { return _binding.NotifyOnValidationError; }
            set { _binding.NotifyOnValidationError = value; }
        }

        [DefaultValue(null)]
        public PropertyPath Path
        {
            get { return _binding.Path; }
            set { _binding.Path = value; }
        }

        [DefaultValue(null)]
        public RelativeSource RelativeSource
        {
            get { return _binding.RelativeSource; }
            set { _binding.RelativeSource = value; }
        }

        [DefaultValue(null)]
        public object Source
        {
            get { return _binding.Source; }
            set { _binding.Source = value; }
        }

        [DefaultValue(null)]
        public string StringFormat
        {
            get { return _binding.StringFormat; }
            set { _binding.StringFormat = value; }
        }

        [DefaultValue(null)]
        public object TargetNullValue
        {
            get { return _binding.TargetNullValue; }
            set { _binding.TargetNullValue = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public UpdateSourceExceptionFilterCallback UpdateSourceExceptionFilter
        {
            get { return _binding.UpdateSourceExceptionFilter; }
            set { _binding.UpdateSourceExceptionFilter = value; }
        }

        [DefaultValue(UpdateSourceTrigger.Default)]
        public UpdateSourceTrigger UpdateSourceTrigger
        {
            get { return _binding.UpdateSourceTrigger; }
            set { _binding.UpdateSourceTrigger = value; }
        }

        [DefaultValue(false)]
        public bool ValidatesOnDataErrors
        {
            get { return _binding.ValidatesOnDataErrors; }
            set { _binding.ValidatesOnDataErrors = value; }
        }

        [DefaultValue(false)]
        public bool ValidatesOnExceptions
        {
            get { return _binding.ValidatesOnExceptions; }
            set { _binding.ValidatesOnExceptions = value; }
        }

        [DefaultValue(null)]
        public string XPath
        {
            get { return _binding.XPath; }
            set { _binding.XPath = value; }
        }

        [DefaultValue(null)]
        public Collection<ValidationRule> ValidationRules
        {
            get { return _binding.ValidationRules; }
        }

        #endregion



        /// <summary>
        /// This basic implementation just sets a binding on the targeted
        /// <see cref="DependencyObject"/> and returns the appropriate
        /// <see cref="BindingExpressionBase"/> instance.<br/>
        /// All this work is delegated to the decorated <see cref="Binding"/>
        /// instance.
        /// </summary>
        /// <returns>
        /// The object value to set on the property where the extension is applied. 
        /// In case of a valid binding expression, this is a <see cref="BindingExpressionBase"/>
        /// instance.
        /// </returns>
        /// <param name="provider">Object that can provide services for the markup
        /// extension.</param>
        public override object ProvideValue(IServiceProvider provider)
        {
            //create a binding and associate it with the target
            return _binding.ProvideValue(provider);
        }

        /// <summary>
        /// Validates a service provider that was submitted to the <see cref="ProvideValue"/>
        /// method. This method checks whether the provider is null (happens at design time),
        /// whether it provides an <see cref="IProvideValueTarget"/> service, and whether
        /// the service's <see cref="IProvideValueTarget.TargetObject"/> and
        /// <see cref="IProvideValueTarget.TargetProperty"/> properties are valid
        /// <see cref="DependencyObject"/> and <see cref="DependencyProperty"/>
        /// instances.
        /// </summary>
        /// <param name="provider">The provider to be validated.</param>
        /// <param name="target">The binding target of the binding.</param>
        /// <param name="dp">The target property of the binding.</param>
        /// <returns>True if the provider supports all that's needed.</returns>
        protected virtual bool TryGetTargetItems(IServiceProvider provider, out DependencyObject target, out DependencyProperty dp)
        {
            target = null;
            dp = null;
            if (provider == null) return false;

            //create a binding and assign it to the target
            IProvideValueTarget service = (IProvideValueTarget)provider.GetService(typeof(IProvideValueTarget));
            if (service == null) return false;

            //we need dependency objects / properties
            if (service.TargetObject is DependencyObject) target = service.TargetObject as DependencyObject;
            dp = service.TargetProperty as DependencyProperty;
            return target != null && dp != null;
        }
    }

    #region ABCBinding
    public class ABCBinding : BindingDecoratorBase
    {
        #region internal state

        private WeakReference _targetObject;
        private DependencyObject TargetObject
        {
            get { return _targetObject != null ? _targetObject.Target as DependencyObject : null; }
            set { _targetObject = new WeakReference(value); }
        }
        private DependencyProperty _targetProperty;
        private Collection<ValidationRule> _blockingRules;
        #endregion

        #region .ctor
        public ABCBinding()
            : base()
        {
        }
        public ABCBinding(string path)
            : base(path)
        {
        }
        #endregion

        #region ProvideValue
        /// <summary>This method is being invoked during initialization.</summary>
        /// <param name="provider">Provides access to the bound items.</param>
        /// <returns>The binding expression that is created by
        /// the base class.</returns>
        public override object ProvideValue(IServiceProvider provider)
        {
            if (provider == null) return false;
            //create a binding and assign it to the target
            IProvideValueTarget service = (IProvideValueTarget)provider.GetService(typeof(IProvideValueTarget));
            if (service == null) return null;
            //we need dependency objects / properties
            if (!(service.TargetObject is DependencyObject)) { return this; }

            //delegate binding creation etc. to the base class
            object val = base.ProvideValue(provider);
            //try to get bound items for our custom work
            DependencyObject targetObject = service.TargetObject as DependencyObject;
            this.TargetObject = targetObject;
            _targetProperty = service.TargetProperty as DependencyProperty;

            if (_targetProperty != null && targetObject != null)
            {
                _blockingRules = new Collection<ValidationRule>();
                foreach (ValidationRule rule in ValidationRules)
                {
                    if (rule.ValidationStep == ValidationStep.RawProposedValue)
                    {
                        if (rule is IBlocksPropertyChange && ((IBlocksPropertyChange)rule).BlocksPropertyChange)
                        {
                            _blockingRules.Add(rule);
                        }
                    }
                }

                if (_blockingRules.Count != 0 || _binding.ValidatesOnDataErrors)
                {
                    DataObject.AddPastingHandler(this.TargetObject, new DataObjectPastingEventHandler(OnPasting));

                    if (targetObject is ContentElement)
                    {
                        ((ContentElement)targetObject).PreviewTextInput += new TextCompositionEventHandler(OnPreviewTextInput);
                        ((ContentElement)targetObject).PreviewDrop += new DragEventHandler(OnPreviewDrop);
                        ((ContentElement)targetObject).PreviewKeyDown += new KeyEventHandler(OnPreviewKeyDown);
                    }
                    else if (targetObject is UIElement)
                    {
                        ((UIElement)targetObject).PreviewTextInput += new TextCompositionEventHandler(OnPreviewTextInput);
                        ((UIElement)targetObject).PreviewDrop += new DragEventHandler(OnPreviewDrop);
                        ((UIElement)targetObject).PreviewKeyDown += new KeyEventHandler(OnPreviewKeyDown);
                    }
                    else if (targetObject is UIElement3D)
                    {
                        ((UIElement3D)targetObject).PreviewTextInput += new TextCompositionEventHandler(OnPreviewTextInput);
                        ((UIElement3D)targetObject).PreviewDrop += new DragEventHandler(OnPreviewDrop);
                        ((UIElement3D)targetObject).PreviewKeyDown += new KeyEventHandler(OnPreviewKeyDown);
                    }
                }
            }

            return val;
        }
        #endregion

        #region IsValidValue
        bool IsValidValue(string text)
        {
            foreach (ValidationRule rule in _blockingRules)
            {
                if (rule.Validate(text, CultureInfo.CurrentCulture) != ValidationResult.ValidResult)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region OnPreviewTextInput
        void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            DependencyObject targetObject = this.TargetObject; if (targetObject == null) { return; }

            string previous = targetObject.GetValue(_targetProperty) as string;
            if (previous == null) previous = string.Empty;
            int index = ((TextBox)targetObject).CaretIndex;
            string projected = string.Format("{0}{1}{2}", previous.Substring(0, index), e.Text, previous.Substring(index));
            e.Handled = !IsValidValue(projected);
        }
        #endregion
        #region OnPreviewDrop
        void OnPreviewDrop(object sender, DragEventArgs e)
        {
            DependencyObject targetObject = this.TargetObject; if (targetObject == null) { return; }

            string previous = targetObject.GetValue(_targetProperty) as string;
            if (previous == null) previous = string.Empty;
            int index = ((TextBox)targetObject).CaretIndex;
            string projected = string.Format("{0}{1}{2}", previous.Substring(0, index), e.Data.GetData(typeof(string)), previous.Substring(index));
            e.Handled = !IsValidValue(projected);
        }
        #endregion
        #region OnPreviewKeyDown
        void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            DependencyObject targetObject = this.TargetObject; if (targetObject == null) { return; }

            string previous = targetObject.GetValue(_targetProperty) as string;
            if (previous == null) previous = string.Empty;
            int index = ((TextBox)targetObject).CaretIndex;
            string projected = null;
            switch (e.Key)
            {
                case Key.Space:
                    {
                        projected = string.Format("{0} {1}", previous.Substring(0, index), previous.Substring(index));
                        break;
                    }
                case Key.Back:
                    {
                        if (index != 0)
                        {
                            projected = string.Format("{0}{1}", previous.Substring(0, index - 1), previous.Substring(index));
                        }
                        break;
                    }
                case Key.Delete:
                    {
                        if (index != previous.Length)
                        {
                            projected = string.Format("{0}{1}", previous.Substring(0, index), previous.Substring(index + 1));
                        }
                        break;
                    }
            }
            if (projected != null)
            {
                e.Handled = !IsValidValue(projected);
            }
        }
        #endregion
        #region OnPasting
        void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            DependencyObject targetObject = this.TargetObject; if (targetObject == null) { return; }

            string previous = targetObject.GetValue(_targetProperty) as string;
            if (previous == null) previous = string.Empty;
            int index = ((TextBox)targetObject).CaretIndex;
            string projected = string.Format("{0}{1}{2}", previous.Substring(0, index), e.DataObject.GetData(typeof(string)), previous.Substring(index));
            if (!IsValidValue(projected))
            {
                e.CancelCommand();
            }
        }
        #endregion
    }
    #endregion

    #region BindingHelper
    /// <summary>Classe Binding Helper</summary>
    public class BindingHelper : DependencyObject
    {
        #region Elementi privati
        private BindingBase _binding;
        private Dictionary<object, object> _cache;
        #endregion

        #region Object
        public static readonly DependencyProperty ObjectProperty = DependencyProperty.Register("Object", typeof(object), typeof(BindingHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        #endregion
        #region String
        public static readonly DependencyProperty StringProperty = DependencyProperty.Register("String", typeof(object), typeof(BindingHelper), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        #endregion

        #region .ctor
        public BindingHelper(BindingBase binding)
            : base()
        {
            _binding = binding;
            _cache = new Dictionary<object, object>();
        }
        #endregion

        #region GetBoundValue
        public object GetBoundValue(object source, bool useCache)
        {
            object value = null;
            if (useCache && _cache.ContainsKey(source))
            {
                value = _cache[source];
            }
            else
            {
                //base.DataContext = source;
                BindingBase bindingClone = CloneBinding(_binding, source);
                BindingExpressionBase be = BindingOperations.SetBinding(this, BindingHelper.ObjectProperty, bindingClone);
                //if (bindingClone is MultiBinding) {
                //    MultiBindingExpression b = BindingOperations.GetMultiBindingExpression(this, BindingHelper.ObjectProperty);
                //    b.UpdateTarget();
                //}
                value = this.GetValue(BindingHelper.ObjectProperty);
                if (useCache) { _cache.Add(source, value); }
            }
            return value;
        }
        #endregion

        #region GetBoundMultiValue
        public object GetBoundMultiValue(object source, bool useCache)
        {
            object value = null;
            if (useCache && _cache.ContainsKey(source))
            {
                value = _cache[source];
            }
            else
            {
                BindingBase bindingClone = CloneBinding(_binding, source);
                BindingExpressionBase be = BindingOperations.SetBinding(this, BindingHelper.ObjectProperty, bindingClone);
                if (bindingClone is MultiBinding)
                {
                    MultiBinding mB = bindingClone as MultiBinding;
                    //MultiBindingExpression b = BindingOperations.GetMultiBindingExpression(this, BindingHelper.ObjectProperty);
                    object[] res = new object[mB.Bindings.Count];
                    for (int i = 0; i < mB.Bindings.Count; i++)
                    {
                        BindingBase bClone = CloneBinding(mB.Bindings[i], source);
                        BindingExpressionBase mbe = BindingOperations.SetBinding(this, BindingHelper.ObjectProperty, bClone);
                        res[i] = this.GetValue(BindingHelper.ObjectProperty);
                    }
                    if (string.IsNullOrEmpty(mB.StringFormat))
                    {
                        value = string.Empty;
                        foreach (object o in res)
                        {
                            if (o != null)
                            {
                                value += o.ToString();
                            }
                        }
                    }
                    else
                        value = string.Format(mB.StringFormat, res);
                }
                else
                {
                    value = this.GetValue(BindingHelper.ObjectProperty);
                }
                if (useCache) { _cache.Add(source, value); }
            }
            return value;
        }
        #endregion

        #region CloneBinding
        public static BindingBase CloneBinding(BindingBase bindingBase, object source)
        {

            var binding = bindingBase as Binding;
            if (binding != null)
            {
                var result = new Binding()
                {
                    Source = source,
                    AsyncState = binding.AsyncState,
                    BindingGroupName = binding.BindingGroupName,
                    BindsDirectlyToSource = binding.BindsDirectlyToSource,
                    Converter = binding.Converter,
                    ConverterCulture = binding.ConverterCulture,
                    ConverterParameter = binding.ConverterParameter,
                    FallbackValue = binding.FallbackValue,
                    IsAsync = binding.IsAsync,
                    Mode = binding.Mode,
                    NotifyOnSourceUpdated = binding.NotifyOnSourceUpdated,
                    NotifyOnTargetUpdated = binding.NotifyOnTargetUpdated,
                    NotifyOnValidationError = binding.NotifyOnValidationError,
                    Path = binding.Path,
                    StringFormat = binding.StringFormat,
                    TargetNullValue = binding.TargetNullValue,
                    UpdateSourceExceptionFilter = binding.UpdateSourceExceptionFilter,
                    UpdateSourceTrigger = binding.UpdateSourceTrigger,
                    ValidatesOnDataErrors = binding.ValidatesOnDataErrors,
                    ValidatesOnExceptions = binding.ValidatesOnExceptions,
                    XPath = binding.XPath
                };
                foreach (var validationRule in binding.ValidationRules)
                {
                    result.ValidationRules.Add(validationRule);
                }
                return result;
            }

            var multiBinding = bindingBase as MultiBinding;
            if (multiBinding != null)
            {
                var result = new MultiBinding()
                {
                    BindingGroupName = multiBinding.BindingGroupName,
                    Converter = multiBinding.Converter,
                    ConverterCulture = multiBinding.ConverterCulture,
                    ConverterParameter = multiBinding.ConverterParameter,
                    FallbackValue = multiBinding.FallbackValue,
                    Mode = multiBinding.Mode,
                    NotifyOnSourceUpdated = multiBinding.NotifyOnSourceUpdated,
                    NotifyOnTargetUpdated = multiBinding.NotifyOnTargetUpdated,
                    NotifyOnValidationError = multiBinding.NotifyOnValidationError,
                    StringFormat = multiBinding.StringFormat,
                    TargetNullValue = multiBinding.TargetNullValue,
                    UpdateSourceExceptionFilter = multiBinding.UpdateSourceExceptionFilter,
                    UpdateSourceTrigger = multiBinding.UpdateSourceTrigger,
                    ValidatesOnDataErrors = multiBinding.ValidatesOnDataErrors,
                    ValidatesOnExceptions = multiBinding.ValidatesOnDataErrors
                };
                foreach (var validationRule in multiBinding.ValidationRules)
                {
                    result.ValidationRules.Add(validationRule);
                }
                foreach (var childBinding in multiBinding.Bindings)
                {
                    result.Bindings.Add(CloneBinding(childBinding, source));
                }
                return result;
            }

            var priorityBinding = bindingBase as PriorityBinding;
            if (priorityBinding != null)
            {
                var result = new PriorityBinding
                {
                    BindingGroupName = priorityBinding.BindingGroupName,
                    FallbackValue = priorityBinding.FallbackValue,
                    StringFormat = priorityBinding.StringFormat,
                    TargetNullValue = priorityBinding.TargetNullValue
                };
                foreach (var childBinding in priorityBinding.Bindings)
                {
                    result.Bindings.Add(CloneBinding(childBinding, source));
                }
                return result;
            }

            throw new NotSupportedException("Failed to clone binding");
        }
        #endregion
    }
    #endregion

    #region DelayBindingExtension
    [MarkupExtensionReturnType(typeof(object))]
    public class DelayBindingExtension : MarkupExtension
    {
        #region .ctor
        public DelayBindingExtension()
        {
            Delay = TimeSpan.FromSeconds(0.5);
        }
        public DelayBindingExtension(PropertyPath path)
            : this()
        {
            Path = path;
        }
        #endregion

        #region Converter
        public IValueConverter Converter { get; set; }
        #endregion
        #region ConverterParamter
        public object ConverterParamter { get; set; }
        #endregion
        #region ElementName
        public string ElementName { get; set; }
        #endregion
        #region RelativeSource
        public RelativeSource RelativeSource { get; set; }
        #endregion
        #region Source
        public object Source { get; set; }
        #endregion
        #region ValidatesOnDataErrors
        public bool ValidatesOnDataErrors { get; set; }
        #endregion
        #region ValidatesOnExceptions
        public bool ValidatesOnExceptions { get; set; }
        #endregion
        #region Delay
        public TimeSpan Delay { get; set; }
        #endregion
        #region Path
        [ConstructorArgument("path")]
        public PropertyPath Path { get; set; }
        #endregion
        #region ConverterCulture
        [TypeConverter(typeof(CultureInfoIetfLanguageTagConverter))]
        public CultureInfo ConverterCulture { get; set; }
        #endregion

        #region ProvideValue
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var valueProvider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (valueProvider != null)
            {
                var bindingTarget = valueProvider.TargetObject as DependencyObject;
                var bindingProperty = valueProvider.TargetProperty as DependencyProperty;
                if (bindingProperty == null || bindingTarget == null) { throw new NotSupportedException(string.Format("The property '{0}' on target '{1}' is not valid for a DelayBinding. The DelayBinding target must be a DependencyObject, " + "and the target property must be a DependencyProperty.", valueProvider.TargetProperty, valueProvider.TargetObject)); }

                var binding = new Binding();
                binding.Path = Path;
                binding.Converter = Converter;
                binding.ConverterCulture = ConverterCulture;
                binding.ConverterParameter = ConverterParamter;
                if (ElementName != null) binding.ElementName = ElementName;
                if (RelativeSource != null) binding.RelativeSource = RelativeSource;
                if (Source != null) binding.Source = Source;
                binding.ValidatesOnDataErrors = ValidatesOnDataErrors;
                binding.ValidatesOnExceptions = ValidatesOnExceptions;

                return DelayBinding.SetBinding(bindingTarget, bindingProperty, Delay, binding);
            }
            return null;
        }
        #endregion
    }
    #endregion
    #region DelayBinding
    public class DelayBinding
    {
        #region internal state
        private readonly BindingExpressionBase _bindingExpression;
        private readonly DispatcherTimer _timer;
        #endregion

        #region .ctor
        protected DelayBinding(BindingExpressionBase bindingExpression, DependencyObject bindingTarget, DependencyProperty bindingTargetProperty, TimeSpan delay)
        {
            _bindingExpression = bindingExpression;

            // Subscribe to notifications for when the target property changes. This event handler will be 
            // invoked when the user types, clicks, or anything else which changes the target property 
            var descriptor = DependencyPropertyDescriptor.FromProperty(bindingTargetProperty, bindingTarget.GetType());
            descriptor.AddValueChanged(bindingTarget, BindingTarget_TargetPropertyChanged);

            // Add support so that the Enter key causes an immediate commit        
            var frameworkElement = bindingTarget as FrameworkElement;
            if (frameworkElement != null) { frameworkElement.KeyUp += BindingTarget_KeyUp; }

            // Setup the timer, but it won't be started until changes are detected        
            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
            _timer.Interval = delay;
        }
        #endregion

        #region SetBinding
        public static object SetBinding(DependencyObject bindingTarget, DependencyProperty bindingTargetProperty, TimeSpan delay, Binding binding)
        {
            // Override some specific settings to enable the behavior of delay binding
            binding.Mode = BindingMode.TwoWay;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;

            // Apply and evaluate the binding        
            var bindingExpression = BindingOperations.SetBinding(bindingTarget, bindingTargetProperty, binding);

            // Setup the delay timer around the binding. This object will live as long as the target element lives, since it subscribes to the changing event,         
            // and will be garbage collected as soon as the element isn't required (e.g., when it's Window closes) and the timer has stopped.        
            new DelayBinding(bindingExpression, bindingTarget, bindingTargetProperty, delay);

            // Return the current value of the binding (since it will have been evaluated because of the binding above)        
            return bindingTarget.GetValue(bindingTargetProperty);
        }
        #endregion

        #region BindingTarget_KeyUp
        private void BindingTarget_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            _timer.Stop();
            _bindingExpression.UpdateSource();
        }
        #endregion
        #region BindingTarget_TargetPropertyChanged
        private void BindingTarget_TargetPropertyChanged(object sender, EventArgs e)
        {
            _timer.Stop();
            _timer.Start();
        }
        #endregion
        #region Timer_Tick
        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            _bindingExpression.UpdateSource();
        }
        #endregion
    }
    #endregion

    #region BindingProxy
    public class BindingProxy : Freezable
    {
        #region Overrides of Freezable

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        #endregion

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
    }
    #endregion
}
