#region using
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Binding = System.Windows.Data.Binding;
using Point = System.Windows.Point;
#endregion

namespace Common
{
    #region BackgroundWorkerContext
    public class BackgroundWorkerContext<T>
        where T : EventArgs
    {
        #region .ctor
        public BackgroundWorkerContext(BackgroundWorker worker, T args) { this.Worker = worker; this.Args = args; }
        #endregion

        #region Worker
        public BackgroundWorker Worker { get; private set; }
        #endregion
        #region Sender
        public object Sender { get; set; }
        #endregion

        #region Args
        public T Args { get; set; }
        #endregion
        #region Attributes
        private IDictionary<string, object> _attributes;
        public IDictionary<string, object> Attributes
        {
            get
            {
                if (_attributes == null) { _attributes = new Dictionary<string, object>(); }
                return _attributes;
            }
            set { _attributes = value; }
        }
        #endregion
    }
    #endregion
    #region DoWorkContext
    public class DoWorkContext : BackgroundWorkerContext<DoWorkEventArgs>
    {
        #region .ctor
        public DoWorkContext(BackgroundWorker worker, DoWorkEventArgs args) : base(worker, args) { }
        #endregion
        #region Result
        public Reference Result
        {
            get { return this.Args.Result as Reference; }
        }
        #endregion
    }
    #endregion
    #region ProgressChangedContext
    public class ProgressChangedContext : BackgroundWorkerContext<ProgressChangedEventArgs>
    {
        #region .ctor
        public ProgressChangedContext(BackgroundWorker worker, ProgressChangedEventArgs args) : base(worker, args) { }
        #endregion
    }
    #endregion
    #region RunWorkerCompletedContext
    public class RunWorkerCompletedContext : BackgroundWorkerContext<RunWorkerCompletedEventArgs>
    {
        #region .ctor
        public RunWorkerCompletedContext(BackgroundWorker worker, RunWorkerCompletedEventArgs args) : base(worker, args) { }
        #endregion
        #region Result
        public Reference Result
        {
            get { return this.Args.Result as Reference; }
        }
        #endregion
        #region Exception
        Exception _exception;
        public Exception Exception
        {
            get { return _exception; }
            set { _exception = value; }
        }
        #endregion
    }
    #endregion

    #region ValueConverterGroup
    /// <summary>A value converter which contains a list of IValueConverters and invokes their Convert or ConvertBack methods
    /// in the order that they exist in the list.  The output of one converter is piped into the next converter
    /// allowing for modular value converters to be chained together.  If the ConvertBack method is invoked, the
    /// value converters are executed in reverse order (highest to lowest index).  Do not leave an element in the
    /// Converters property collection null, every element must reference a valid IValueConverter instance. If a
    /// value converter's type is not decorated with the ValueConversionAttribute, an InvalidOperationException will be
    /// thrown when the converter is added to the Converters collection.</summary>
    [System.Windows.Markup.ContentProperty("Converters")]
    public class ValueConverterGroup : IValueConverter
    {
        #region internal state
        private readonly ObservableCollection<IValueConverter> converters = new ObservableCollection<IValueConverter>();
        private readonly Dictionary<IValueConverter, ValueConversionAttribute> cachedAttributes = new Dictionary<IValueConverter, ValueConversionAttribute>();
        #endregion // Data

        #region Constructor
        public ValueConverterGroup()
        {
            this.converters.CollectionChanged += this.OnConvertersCollectionChanged;
        }
        #endregion // Constructor

        #region Converters
        /// <summary>ritorna la lista degli IValueConverters contenuti in questo converter.</summary>
        public ObservableCollection<IValueConverter> Converters
        {
            get { return this.converters; }
        }
        #endregion // Converters

        #region IValueConverter Members
        /// <summary>esegue la conversione concatenando i converter.</summary>
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object output = value;

            for (int i = 0; i < this.Converters.Count; ++i)
            {
                IValueConverter converter = this.Converters[i];
                Type currentTargetType = this.GetTargetType(i, targetType, true);
                output = converter.Convert(output, currentTargetType, parameter, culture);

                // If the converter returns 'DoNothing' then the binding operation should terminate.
                if (output == Binding.DoNothing) { break; }
            }

            return output;
        }
        /// <summary>esegue la conversione inversa.</summary>
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object output = value;

            for (int i = this.Converters.Count - 1; i > -1; --i)
            {
                IValueConverter converter = this.Converters[i];
                Type currentTargetType = this.GetTargetType(i, targetType, false);
                output = converter.ConvertBack(output, currentTargetType, parameter, culture);

                // When a converter returns 'DoNothing' the binding operation should terminate.
                if (output == Binding.DoNothing)
                    break;
            }

            return output;
        }
        #endregion // IValueConverter Members

        #region Private Helpers
        #region GetTargetType
        /// <summary>Ritorna il target type per una operazione di conversione.</summary>
        /// <param name="converterIndex">l'indice del converter che deve essere eseguito.</param>
        /// <param name="finalTargetType">l'argomento 'targetType' passato al metodo di conversione.</param>
        /// <param name="convert">true se invocato da convert, false se invocato da ConvertBack.</param>
        protected virtual Type GetTargetType(int converterIndex, Type finalTargetType, bool convert)
        {
            // If the current converter is not the last/first in the list, get a reference to the next/previous converter.
            IValueConverter nextConverter = null;
            if (convert)
            {
                if (converterIndex < this.Converters.Count - 1)
                {
                    nextConverter = this.Converters[converterIndex + 1];
                    if (nextConverter == null)
                    {
                        throw new InvalidOperationException("The Converters collection of the ValueConverterGroup contains a null reference at index: " + (converterIndex + 1));
                    }
                }
            }
            else
            {
                if (converterIndex > 0)
                {
                    nextConverter = this.Converters[converterIndex - 1];
                    if (nextConverter == null)
                    {
                        throw new InvalidOperationException("The Converters collection of the ValueConverterGroup contains a null reference at index: " + (converterIndex - 1));
                    }
                }
            }

            if (nextConverter != null)
            {
                ValueConversionAttribute conversionAttribute = cachedAttributes[nextConverter];
                // If the Convert method is going to be called, we need to use the SourceType of the next converter in the list.  If ConvertBack is called, use the TargetType.
                return convert ? conversionAttribute.SourceType : conversionAttribute.TargetType;
            }

            // If the current converter is the last one to be executed return the target type passed into the conversion method.
            return finalTargetType;
        }
        #endregion // GetTargetType
        #region OnConvertersCollectionChanged
        /// <summary>al variare della collection dei converters rilegge i ValueConversionAttribute .</summary>
        void OnConvertersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // The 'Converters' collection has been modified, so validate that each value converter it now
            // contains is decorated with ValueConversionAttribute and then cache the attribute value.
            IList convertersToProcess = null;
            if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                convertersToProcess = e.NewItems;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (IValueConverter converter in e.OldItems)
                    this.cachedAttributes.Remove(converter);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.cachedAttributes.Clear();
                convertersToProcess = this.converters;
            }

            if (convertersToProcess != null && convertersToProcess.Count > 0)
            {
                foreach (IValueConverter converter in convertersToProcess)
                {
                    object[] attributes = converter.GetType().GetCustomAttributes(typeof(ValueConversionAttribute), false);
                    if (attributes.Length != 1) { throw new InvalidOperationException("All value converters added to a ValueConverterGroup must be decorated with the ValueConversionAttribute attribute exactly once."); }

                    //this.cachedAttributes.Add(converter, attributes[0] as ValueConversionAttribute);
                }
            }
        }
        #endregion // OnConvertersCollectionChanged
        #endregion // Private Helpers
    }
    #endregion

    #region DoubleToDurationConverter
    [ValueConversion(typeof(double), typeof(Duration))]
    public class DoubleToDurationConverter : IValueConverter
    {
        public DoubleToDurationConverter() { }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Duration duration = new Duration(new TimeSpan(0, 0, System.Convert.ToInt16(value)));
            return duration;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    #endregion

    #region HourToAngle
    [ValueConversion(typeof(double), typeof(Duration))]
    public class HourToAngle : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime time = System.Convert.ToDateTime(value);
            double Angle = time.Hour * 30;
            Angle += 12 * time.Minute / 60;
            return Angle;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not required.
            return null;
        }
        #endregion
    }
    #endregion

    #region MinuteToAngle
    [ValueConversion(typeof(double), typeof(Duration))]
    public class MinuteToAngle : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime time = System.Convert.ToDateTime(value);
            double Angle = time.Minute * 6;
            return Angle;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not required;
            return null;
        }
        #endregion
    }
    #endregion

    #region ConverterEventArgs
    public sealed class ConverterEventArgs : RoutedEventArgs
    {
        object Value { get; set; }
        Type TargetType { get; set; }
        object Parameter { get; set; }
        CultureInfo Culture { get; set; }
    }
    #endregion

    #region DelegateConverter
    public delegate object ConvertDelegate(DependencyObject source, object value, Type targetType, object parameter, CultureInfo culture);
    public delegate void DoWorkDelegate(DoWorkContext worker, DependencyObject source, object value, Type targetType, object parameter, CultureInfo culture);
    public delegate void ProgressChangedDelegate(ProgressChangedContext worker, DependencyObject source, object value, Type targetType, object parameter, CultureInfo culture);
    public delegate void RunWorkerCompletedDelegate(RunWorkerCompletedContext worker, DependencyObject source, object value, Type targetType, object parameter, CultureInfo culture);
    public delegate object ConvertDelegate2(DependencyObject source, object[] values, Type targetType, object parameter, CultureInfo culture);
    public delegate void DoWorkDelegate2(DoWorkContext worker, DependencyObject source, object[] values, Type targetType, object parameter, CultureInfo culture);
    public delegate void ProgressChangedDelegate2(ProgressChangedContext worker, DependencyObject source, object[] values, Type targetType, object parameter, CultureInfo culture);
    public delegate void RunWorkerCompletedDelegate2(RunWorkerCompletedContext worker, DependencyObject source, object[] values, Type targetType, object parameter, CultureInfo culture);

    public delegate object ConvertBackDelegate(DependencyObject source, object value, Type targetType, object parameter, CultureInfo culture);
    public delegate object[] ConvertBackDelegate2(DependencyObject source, object value, Type[] targetTypes, object parameter, CultureInfo culture);

    [ValueConversion(typeof(Object), typeof(Object))]
    public class DelegateConverter : IValueConverter, IMultiValueConverter
    {
        private const string CONFIGVALUE_DISABLEASYNCUI = "DisableAsyncUI"; private const bool DEFAULTVALUE_DISABLEASYNCUI = false;
        private static readonly bool _disableAsyncUI = ConfigurationHelper.GetClassSetting<DelegateConverter, bool>(CONFIGVALUE_DISABLEASYNCUI, DEFAULTVALUE_DISABLEASYNCUI); // , CultureInfo.InvariantCulture
        public event Func<FrameworkElement> GetSource;

        #region delegates
        public ConvertDelegate ConvertDelegate { get; set; }
        public DoWorkDelegate ConvertDelegateAsync { get; set; }
        public ProgressChangedDelegate ConvertDelegateProgressChanged { get; set; }
        public RunWorkerCompletedDelegate ConvertDelegateAsyncCompleted { get; set; }
        public ConvertDelegate2 ConvertDelegate2 { get; set; }
        public DoWorkDelegate2 ConvertDelegate2Async { get; set; }
        public ProgressChangedDelegate2 ConvertDelegate2ProgressChanged { get; set; }
        public RunWorkerCompletedDelegate2 ConvertDelegate2AsyncCompleted { get; set; }
        public ConvertBackDelegate ConvertBackDelegate { get; set; }
        public ConvertBackDelegate2 ConvertBackDelegate2 { get; set; }
        #endregion

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FrameworkElement source = GetSource != null ? GetSource() : null;
            object ret = value;
            if (this.ConvertDelegate != null)
            {
                try
                {
                    ret = this.ConvertDelegate(source, value, targetType, parameter, culture);
                }
                catch (Exception ex) {
                    //Logger.Exception(ex);
                    ExceptionManager.RaiseException(source, ex); return ret;
                }
            }
            if (ret == Binding.DoNothing) { return ret; }

            if (this.ConvertDelegateAsync != null)
            {
                if (Application.Current == null || Application.Current.MainWindow == null || DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow)) { return null; }

                Exception exception = null;
                //ContestoABC contesto = SerializationHelper.CloneBySerializing(ContestoABC.Current, SerializationMode.DataContract);
                var retReference = new Reference() { Value = ret }; ret = retReference;
                var retReferenceCopy = new Reference() { Value = ret };
                IDictionary<string, object> attributes = null;

                var worker = new BackgroundWorker() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
                DoWorkEventHandler doWork = ((sender, e) => {
                    try
                    {
                        //Context.Current = contesto; e.Result = retReferenceCopy;
                        if (ConvertDelegateAsync != null)
                        {
                            var context = new DoWorkContext(worker, e) { Attributes = attributes };
                            ConvertDelegateAsync(context, source, value, targetType, parameter, culture);
                            attributes = context.Attributes;
                        }
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        Dispatcher dispatcher = source != null ? source.Dispatcher : Application.Current.Dispatcher;
                        dispatcher.BeginInvoke(new Action(delegate () {
                            //Context.Current = contesto; Logger.Exception(ex);
                            ExceptionManager.RaiseException(source, ex);
                        }), DispatcherPriority.ApplicationIdle, null);
                    }
                });
                worker.DoWork += doWork;

                ProgressChangedEventHandler progressChanged = ((sender, e) => {
                    Dispatcher dispatcher = source != null ? source.Dispatcher : ApplicationBase.Current.Dispatcher;
                    dispatcher.BeginInvoke(new Action(delegate () {
                        //Context.Current = contesto;
                        if (ConvertDelegateProgressChanged != null)
                        {
                            try
                            {
                                var context = new ProgressChangedContext(worker, e) { Attributes = attributes };
                                ConvertDelegateProgressChanged(context, source, value, targetType, parameter, culture);
                                attributes = context.Attributes;
                            }
                            catch (Exception ex) { exception = ex; ExceptionManager.RaiseException(source, ex); }
                        }
                    }), DispatcherPriority.ApplicationIdle, null);
                });
                worker.ProgressChanged += progressChanged;

                RunWorkerCompletedEventHandler runWorkerCompleted = ((sender, e) => {
                    Dispatcher dispatcher = source != null ? source.Dispatcher : ApplicationBase.Current.Dispatcher;
                    dispatcher.BeginInvoke(new Action(delegate () {
                        //Context.Current = contesto;
                        if (ConvertDelegateAsyncCompleted != null)
                        {
                            try
                            {
                                var context = new RunWorkerCompletedContext(worker, e) { Attributes = attributes, Exception = exception };
                                ConvertDelegateAsyncCompleted(context, source, value, targetType, parameter, culture);
                                attributes = context.Attributes;
                            }
                            catch (Exception ex) { exception = ex; ExceptionManager.RaiseException(source, ex); }
                        }
                        if (e.Cancelled == false) { retReference.Value = retReferenceCopy.Value; }
                    }), DispatcherPriority.ApplicationIdle, null);
                });
                worker.RunWorkerCompleted += runWorkerCompleted;

                if (_disableAsyncUI)
                {
                    Exception ex = null;
                    DoWorkEventArgs e = new DoWorkEventArgs(value);
                    try { doWork(worker, e); } catch (Exception exc) { ex = exc; } finally { runWorkerCompleted(worker, new RunWorkerCompletedEventArgs(value, ex, e.Cancel)); }
                }
                else { worker.RunWorkerAsync(value); }
            }
            return ret;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FrameworkElement source = GetSource != null ? GetSource() : null;
            return this.ConvertBackDelegate != null ? this.ConvertBackDelegate(source, value, targetType, parameter, culture) : value;
        }
        #endregion

        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            FrameworkElement source = GetSource != null ? GetSource() : null;
            object ret = DependencyProperty.UnsetValue;
            if (this.ConvertDelegate2 != null)
            {
                try
                {
                    ret = this.ConvertDelegate2(source, values, targetType, parameter, culture);
                }
                catch (Exception ex) { ExceptionManager.RaiseException(source, ex); return ret; }
            }
            if (ret == Binding.DoNothing) { return ret; }

            if (this.ConvertDelegate2Async != null)
            {
                if (Application.Current == null || Application.Current.MainWindow == null || DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow)) { return null; }

                Exception exception = null;
                Reference retReference = new Reference() { Value = ret }; ret = retReference;
                Reference retReferenceCopy = new Reference() { Value = ret };
                object[] valuesCopy = values.Clone() as object[];
                IDictionary<string, object> attributes = null;

                var worker = new BackgroundWorker() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
                DoWorkEventHandler doWork = ((sender, e) => {
                    try
                    {
                        e.Result = retReferenceCopy;
                        var context = new DoWorkContext(worker, e) { Attributes = attributes };
                        ConvertDelegate2Async(context, source, valuesCopy, targetType, parameter, culture);
                        attributes = context.Attributes;
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        Dispatcher dispatcher = source != null ? source.Dispatcher : Application.Current.Dispatcher;
                        dispatcher.BeginInvoke(new Action(delegate () {
                            ExceptionManager.RaiseException(source, ex);
                        }), DispatcherPriority.ApplicationIdle, null);
                    }
                });
                worker.DoWork += doWork;

                ProgressChangedEventHandler progressChanged = ((sender, e) => {
                    Dispatcher dispatcher = source != null ? source.Dispatcher : Application.Current.Dispatcher;
                    dispatcher.BeginInvoke(new Action(delegate () {
                        if (ConvertDelegate2ProgressChanged != null)
                        {
                            try
                            {
                                var context = new ProgressChangedContext(worker, e) { Attributes = attributes };
                                ConvertDelegate2ProgressChanged(context, source, values, targetType, parameter, culture);
                                attributes = context.Attributes;
                            }
                            catch (Exception ex) { exception = ex; ExceptionManager.RaiseException(source, ex); }
                        }
                    }), DispatcherPriority.ApplicationIdle, null);
                });
                worker.ProgressChanged += progressChanged;

                RunWorkerCompletedEventHandler runWorkerCompleted = ((sender, e) => {
                    Dispatcher dispatcher = source != null ? source.Dispatcher : Application.Current.Dispatcher;
                    dispatcher.BeginInvoke(new Action(delegate () {
                        if (ConvertDelegate2AsyncCompleted != null)
                        {
                            try
                            {
                                var context = new RunWorkerCompletedContext(worker, e) { Attributes = attributes, Exception = exception };
                                ConvertDelegate2AsyncCompleted(context, source, valuesCopy, targetType, parameter, culture);
                                attributes = context.Attributes;
                            }
                            catch (Exception ex) { exception = ex; ExceptionManager.RaiseException(source, ex); }
                        }
                        if (e.Cancelled == false) { retReference.Value = retReferenceCopy.Value; }
                    }), DispatcherPriority.ApplicationIdle, null);
                });
                worker.RunWorkerCompleted += runWorkerCompleted;

                if (_disableAsyncUI)
                {
                    Exception ex = null;
                    DoWorkEventArgs e = new DoWorkEventArgs(values);
                    try { doWork(worker, e); } catch (Exception exc) { ex = exc; } finally { runWorkerCompleted(worker, new RunWorkerCompletedEventArgs(values, ex, e.Cancel)); }
                }
                else { worker.RunWorkerAsync(values); }
            }
            return ret;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            FrameworkElement source = GetSource != null ? GetSource() : null;
            return this.ConvertBackDelegate2 != null ? this.ConvertBackDelegate2(source, value, targetTypes, parameter, culture) : null;
        }
        #endregion

        #region IsInString
        public static ConvertDelegate IsNotInStringDelegate = IsNotInString;
        public static object IsNotInString(DependencyObject source, object value, Type targetType, object parameter, CultureInfo culture) { return !(bool)IsInString(source, value, targetType, parameter, culture); }
        public static ConvertDelegate IsInStringDelegate = IsInString;
        public static object IsInString(DependencyObject source, object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool found = false;
            string s = value as string; if (string.IsNullOrEmpty(s)) { found = true; return found; }
            string findstr = parameter as string; if (string.IsNullOrEmpty(findstr)) { found = false; return found; }
            found = findstr.Contains(s);
            return found;
        }
        #endregion
        #region IsInString2
        public static ConvertDelegate2 IsNotInString2Delegate = IsNotInString2;
        public static object IsNotInString2(DependencyObject source, object[] values, Type targetType, object parameter, CultureInfo culture) { return !(bool)IsInString2(source, values, targetType, parameter, culture); }
        public static ConvertDelegate2 IsInString2Delegate = IsInString2;
        public static object IsInString2(DependencyObject source, object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int i = 0; bool found = false;
            string s = values != null && values.Length > i ? values[i] as string : null; i++;
            if (string.IsNullOrEmpty(s)) { found = true; return found; }
            string findstr = values != null && values.Length > i ? values[i] as string : null; i++;
            if (string.IsNullOrEmpty(findstr)) { found = false; return found; }
            found = findstr.Contains(s);
            return found;
        }
        #endregion
    }
    #endregion

    #region EventConverter
    [ValueConversion(typeof(object), typeof(object))]
    public class EventConverter : IValueConverter, IMultiValueConverter
    {
        #region constants
        private const string CONFIGVALUE_DISABLEASYNCUI = "DisableAsyncUI"; private const bool DEFAULTVALUE_DISABLEASYNCUI = false;
        #endregion
        #region internal state
        private static readonly bool _disableAsyncUI = ConfigurationHelper.GetClassSetting<EventConverter, bool>(CONFIGVALUE_DISABLEASYNCUI, DEFAULTVALUE_DISABLEASYNCUI);
        #endregion

        #region events
        public event Func<FrameworkElement> GetSource;

        public event ConvertDelegate ConvertEvent;
        public event DoWorkDelegate ConvertEventAsync;
        public event ProgressChangedDelegate ConvertEventProgressChanged;
        public event RunWorkerCompletedDelegate ConvertEventAsyncCompleted;
        public event ConvertDelegate2 ConvertEvent2;
        public event DoWorkDelegate2 ConvertEvent2Async;
        public event ProgressChangedDelegate2 ConvertEvent2ProgressChanged;
        public event RunWorkerCompletedDelegate2 ConvertEvent2AsyncCompleted;

        public event ConvertBackDelegate ConvertBackEvent;
        public event ConvertBackDelegate2 ConvertBackEvent2;
        #endregion

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FrameworkElement source = GetSource != null ? GetSource() : null;
            object ret = value;
            if (this.ConvertEvent != null)
            {
                try
                {
                    ret = this.ConvertEvent(source, value, targetType, parameter, culture);
                }
                catch (Exception ex) { ExceptionManager.RaiseException(source, ex); }
            }
            if (ret == Binding.DoNothing) { return ret; }

            if (this.ConvertEventAsync != null)
            {
                if (Application.Current == null || Application.Current.MainWindow == null || DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow)) { return null; }

                Exception exception = null;
                Reference retReference = new Reference() { Value = ret }; ret = retReference;
                Reference retReferenceCopy = new Reference() { Value = ret };
                IDictionary<string, object> attributes = null;

                var worker = new BackgroundWorker() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
                DoWorkEventHandler asyncWorkDelegate = ((sender, arg) =>
                {
                    try
                    {
                        arg.Result = retReferenceCopy;
                        if (ConvertEventAsync != null)
                        {
                            var context = new DoWorkContext(worker, arg) { Attributes = attributes };
                            ConvertEventAsync(context, source, value, targetType, parameter, culture);
                            attributes = context.Attributes;
                        }
                        arg.Result = retReferenceCopy;
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        Dispatcher dispatcher = source != null ? source.Dispatcher : Application.Current.Dispatcher;
                        dispatcher.BeginInvoke(new Action(delegate ()
                        {
                            ExceptionManager.RaiseException(source, ex);
                        }), DispatcherPriority.ApplicationIdle, null);
                    }
                });
                worker.DoWork += asyncWorkDelegate;

                ProgressChangedEventHandler progressChanged = ((sender, e) =>
                {
                    Dispatcher dispatcher = source != null ? source.Dispatcher : Application.Current.Dispatcher;
                    dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        if (ConvertEventProgressChanged != null)
                        {
                            try
                            {
                                var context = new ProgressChangedContext(worker, e) { Attributes = attributes };
                                ConvertEventProgressChanged(context, source, value, targetType, parameter, culture);
                                attributes = context.Attributes;
                            }
                            catch (Exception ex) { exception = ex; ExceptionManager.RaiseException(source, ex); }
                        }
                    }), DispatcherPriority.ApplicationIdle, null);
                });
                worker.ProgressChanged += progressChanged;

                RunWorkerCompletedEventHandler workCompletedDelegate = ((sender, arg) =>
                {
                    Dispatcher dispatcher = source != null ? source.Dispatcher : Application.Current.Dispatcher;
                    dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        if (ConvertEventAsyncCompleted != null)
                        {
                            try
                            {
                                var context = new RunWorkerCompletedContext(worker, arg) { Attributes = attributes, Exception = exception };
                                ConvertEventAsyncCompleted(context, source, value, targetType, parameter, culture);
                                attributes = context.Attributes;
                            }
                            catch (Exception ex) { exception = ex; ExceptionManager.RaiseException(source, ex); }
                        }
                        if (arg.Cancelled == false) { retReference.Value = retReferenceCopy.Value; }
                    }), DispatcherPriority.ApplicationIdle, null);
                });
                worker.RunWorkerCompleted += workCompletedDelegate;

                if (_disableAsyncUI)
                {
                    Exception ex = null;
                    DoWorkEventArgs e = new DoWorkEventArgs(value);
                    try { asyncWorkDelegate(worker, e); }
                    catch (Exception exc) { ex = exc; }
                    finally { workCompletedDelegate(worker, new RunWorkerCompletedEventArgs(value, ex, e.Cancel)); }
                }
                else { worker.RunWorkerAsync(value); }
            }
            return ret;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FrameworkElement source = GetSource != null ? GetSource() : null;
            return this.ConvertBackEvent != null ? this.ConvertBackEvent(source, value, targetType, parameter, culture) : value;
        }
        #endregion

        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            FrameworkElement source = GetSource != null ? GetSource() : null;
            object ret = DependencyProperty.UnsetValue;
            if (this.ConvertEvent2 != null)
            {
                try
                {
                    ret = this.ConvertEvent2(source, values, targetType, parameter, culture);
                }
                catch (Exception ex) { ExceptionManager.RaiseException(source, ex); return ret; }
            }
            if (ret == Binding.DoNothing) { return ret; }

            if (this.ConvertEvent2Async != null)
            {
                if (Application.Current == null || Application.Current.MainWindow == null || DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow)) { return null; }

                Exception exception = null;
                Reference retReference = new Reference() { Value = ret }; ret = retReference;
                Reference retReferenceCopy = new Reference() { Value = ret };
                object[] valuesCopy = values.Clone() as object[];
                IDictionary<string, object> attributes = null;

                var worker = new BackgroundWorker() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
                DoWorkEventHandler doWork = ((sender, arg) =>
                {
                    try
                    {
                        arg.Result = retReferenceCopy;
                        var context = new DoWorkContext(worker, arg) { Attributes = attributes };
                        ConvertEvent2Async(context, source, valuesCopy, targetType, parameter, culture);
                        attributes = context.Attributes;
                        arg.Result = retReferenceCopy;
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        Dispatcher dispatcher = source != null ? source.Dispatcher : Application.Current.Dispatcher;
                        dispatcher.BeginInvoke(new Action(delegate () { ExceptionManager.RaiseException(source, ex); }), DispatcherPriority.ApplicationIdle, null);
                    }
                });
                worker.DoWork += doWork;

                ProgressChangedEventHandler progressChanged = ((sender, e) =>
                {
                    Dispatcher dispatcher = source != null ? source.Dispatcher : Application.Current.Dispatcher;
                    dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        if (ConvertEvent2ProgressChanged != null)
                        {
                            try
                            {
                                var context = new ProgressChangedContext(worker, e) { Attributes = attributes };
                                ConvertEvent2ProgressChanged(context, source, values, targetType, parameter, culture);
                                attributes = context.Attributes;
                            }
                            catch (Exception ex) { exception = ex; ExceptionManager.RaiseException(source, ex); }
                        }
                    }), DispatcherPriority.ApplicationIdle, null);
                });
                worker.ProgressChanged += progressChanged;

                RunWorkerCompletedEventHandler runWorkerCompleted = ((sender, arg) =>
                {
                    Dispatcher dispatcher = source != null ? source.Dispatcher : Application.Current.Dispatcher;
                    dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        if (ConvertEvent2AsyncCompleted != null)
                        {
                            try
                            {
                                var context = new RunWorkerCompletedContext(worker, arg) { Attributes = attributes, Exception = exception };
                                ConvertEvent2AsyncCompleted(context, source, valuesCopy, targetType, parameter, culture);
                                attributes = context.Attributes;
                            }
                            catch (Exception ex) { exception = ex; ExceptionManager.RaiseException(source, ex); }
                        }
                        if (arg.Cancelled == false) { retReference.Value = retReferenceCopy.Value; }
                    }), DispatcherPriority.ApplicationIdle, null);
                });
                worker.RunWorkerCompleted += runWorkerCompleted;

                if (_disableAsyncUI)
                {
                    Exception ex = null;
                    DoWorkEventArgs e = new DoWorkEventArgs(values);
                    try { doWork(worker, e); }
                    catch (Exception exc) { ex = exc; }
                    finally { runWorkerCompleted(worker, new RunWorkerCompletedEventArgs(values, ex, e.Cancel)); }
                }
                else { worker.RunWorkerAsync(values); }
            }
            return ret;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            FrameworkElement source = GetSource != null ? GetSource() : null;
            return this.ConvertBackEvent2 != null ? this.ConvertBackEvent2(source, value, targetTypes, parameter, culture) : null;
        }
        #endregion
    }
    #endregion

    #region TransformConverter<T, R>
    [ValueConversion(typeof(object), typeof(object))]
    public class TransformConverter<T, R> : IMultiValueConverter, IValueConverter
    {
        #region internal state
        DelegateConverter _conv = null;
        #endregion
        #region Transformations
        public Func<T, R> Trasformation;
        public Func<T[], R> Trasformation2;
        public Func<R, T> BackTrasformation;
        public Func<R, T[]> BackTrasformation2;
        public event Func<FrameworkElement> GetSource;
        #endregion

        #region .ctor
        public TransformConverter()
        {
            _conv = new DelegateConverter()
            {
                ConvertDelegate = Transform,
                ConvertDelegate2 = Transform2,
                ConvertBackDelegate = BackTransform,
                ConvertBackDelegate2 = BackTransform2
            };
            _conv.GetSource += delegate () { return GetSource != null ? GetSource() : null; };
        }
        #endregion

        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return _conv.Convert(values, targetType, parameter, culture);
        }
        #endregion
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _conv.Convert(value, targetType, parameter, culture);
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return _conv.ConvertBack(value, targetTypes, parameter, culture);
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _conv.ConvertBack(value, targetType, parameter, culture);
        }
        #endregion

        #region Transform
        private object Transform(DependencyObject source, object value, Type targetType, object parameter, CultureInfo culture)
        {
            T t = value is T ? (T)value : default(T);
            return this.Trasformation(t);
        }
        #endregion
        #region Transform2
        private object Transform2(DependencyObject source, object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            T[] at = values != null && values.All(v => v != null && v is T) ? CollectionHelper.Clone<List<T>>(values, true).ToArray() : null;
            if (at == null) { return DependencyProperty.UnsetValue; }
            return this.Trasformation2(at);
        }
        #endregion
        #region BackTransform
        private object BackTransform(DependencyObject source, object value, Type targetType, object parameter, CultureInfo culture)
        {
            R r = value is R ? (R)value : default(R);
            return this.BackTrasformation(r);
        }
        #endregion
        #region BackTransform2
        private object[] BackTransform2(DependencyObject source, object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
        #endregion
    }
    public class TransformConverter<T> : TransformConverter<T, T> { };
    #endregion

    #region FilterListItems
    [ValueConversion(typeof(object), typeof(object))]
    public class FilterListItems : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object selectedItem = values != null && values.Length > 0 ? values[0] : null;
            IList ietms = values != null && values.Length > 1 ? values[1] as IList : null;
            object ret = null;
            if (selectedItem != null && ietms != null && ietms.Contains(selectedItem)) { ret = selectedItem; }
            return ret;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { value, null };
        }
    }
    #endregion

    #region DistributeConverter
    [ValueConversion(typeof(object[]), typeof(object))]
    public sealed class DistributeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object value = values != null && values.Length > 0 ? values[0] : DependencyProperty.UnsetValue;
            return value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            List<object> list = new List<object>();
            for (int i = 0; i < targetTypes.Length; i++)
            {
                list.Add(value);
            }
            return list.ToArray();
        }
    }
    #endregion

    #region PositionConverter
    [ValueConversion(typeof(Object), typeof(Object))]
    public class PositionConverter : IValueConverter, IMultiValueConverter
    {

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visual visual = value as Visual;
            return null;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        #endregion

        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int i = 0;
            Visual visualSource = values != null && values.Length > i && values[i] is Visual ? values[i] as Visual : null;
            Visual visualTarget = values != null && values.Length > i && values[i] is Visual ? values[i] as Visual : null;
            Point? point = values != null && values.Length > i && values[i] is Point ? (Point?)values[i] : null;

            Point? ret = null;
            if (point != null)
            {
                GeneralTransform transform = visualSource.TransformToVisual(visualTarget);
                ret = transform.Transform(point.Value);
            }
            return ret;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
        #endregion
    }
    #endregion

    #region NullCoalesceConverter
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class NullCoalesceConverter : IValueConverter, IMultiValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object ret = value ?? DependencyProperty.UnsetValue;
            return ret;
        }
        #endregion
        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int i = 0;
            object value = values != null && values.Length >= i ? values[i] : null; i++;
            object value1 = values != null && values.Length >= i ? values[i] : null; i++;

            object ret = value ?? value1;
            return ret;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object objRet = value;
            return objRet;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            object objRet = value;
            return new object[] { objRet };
        }
        #endregion
    }
    #endregion

    #region EmptyCoalesceConverter
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class EmptyCoalesceConverter : IValueConverter, IMultiValueConverter
    {
        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object ret = value ?? DependencyProperty.UnsetValue;
            return ret;
        }
        #endregion
        #region Convert
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int i = 0;
            object value = values != null && values.Length >= i ? values[i] : null; i++;
            object value1 = values != null && values.Length >= i ? values[i] : null; i++;

            if (value is string && string.IsNullOrEmpty(value as string)) { value = null; }
            if (value is IList && (value as IList).Count == 0) { value = null; }
            if (value1 is string && string.IsNullOrEmpty(value1 as string)) { value1 = null; }
            if (value1 is IList && (value1 as IList).Count == 0) { value = null; }

            if (value == DependencyProperty.UnsetValue) { return value1; }
            if (value1 == DependencyProperty.UnsetValue) { return value; }

            object ret = value ?? value1;
            return ret;
        }
        #endregion
        #region ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object objRet = value;
            return objRet;
        }
        #endregion
        #region ConvertBack
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            object objRet = value;
            return new object[] { objRet };
        }
        #endregion
    }
    #endregion

    #region ObjectToImageConverter
    // [Localizability(LocalizationCategory.NeverLocalize)]
    [ValueConversion(typeof(object), typeof(Image))]
    public class ObjectToImageConverter : IValueConverter
    {
        public string Format { get; set; }

        public ObjectToImageConverter()
        {
            Format = string.Empty;
        }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string uri = string.Format(Format, value);
                BitmapImage img = new BitmapImage(new Uri(uri));
                return img;
            }
            catch
            {
                return new BitmapImage();
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
            //return value.Equals(Vero);
        }
    }
    #endregion

    #region ConverterChain
    [System.Windows.Markup.ContentProperty("Converters")]
    [System.Windows.Markup.ContentWrapper(typeof(ValueConverterCollection))]
    public class ConverterChain : IValueConverter
    {
        ValueConverterCollection _converters;
        /// <summary>Gets the converters to execute.</summary>
        public ValueConverterCollection Converters
        {
            get { return _converters ?? (_converters = new ValueConverterCollection()); }
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var valueConverter in Converters)
            {
                value = valueConverter.Convert(value, targetType, parameter, culture);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    #endregion

    #region ValueConverterCollection
    /// <summary>Represents a collection of <see cref="IValueConverter"/>s.</summary>
    public sealed class ValueConverterCollection : Collection<IValueConverter> { }
    #endregion

    #region StringToXamlConverter
    // [Localizability(LocalizationCategory.NeverLocalize)]
    [ValueConversion(typeof(string), typeof(UIElement))]
    public class StringToXamlConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string xaml = value as string;
            if (string.IsNullOrEmpty(xaml))
                return null;
            return System.Windows.Markup.XamlReader.Parse(xaml);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
            //return value.Equals(Vero);
        }
    }
    #endregion
}
