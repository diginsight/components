#region using
using Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
//using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
#endregion

namespace Common
{
    /// <summary>Interaction logic for ExceptionControl.xaml</summary>
    public partial class ExceptionControl : UserControl
    {
        private ILogger<ExceptionControl> logger;
        #region commands
        public static readonly RoutedUICommand Chiudi = new RoutedUICommand("Chiudi", "Chiudi", typeof(ExceptionControl));
        #endregion
        #region internal state
        Brush _informationBackgroundBrush;
        Brush _errorBackgroundBrush;
        Brush _warningBackgroundBrush; 
        #endregion

        #region .ctor
        public ExceptionControl()
        {
            using (var sec = logger.BeginMethodScope())
            {
                InitializeComponent();
                _informationBackgroundBrush = (Brush)this.FindResource("informationBackgroundBrush");
                _warningBackgroundBrush = (Brush)this.FindResource("warningBackgroundBrush");
                _errorBackgroundBrush = (Brush)this.FindResource("errorBackgroundBrush");

                sec.LogDebug(new { _informationBackgroundBrush , _warningBackgroundBrush , _errorBackgroundBrush });
            }
        }
        #endregion

        #region Exception
        public Exception Exception
        {
            get { return (Exception)GetValue(ExceptionProperty); }
            set { SetValue(ExceptionProperty, value); }
        }
        public static readonly DependencyProperty ExceptionProperty = DependencyProperty.Register("Exception", typeof(Exception), typeof(ExceptionControl), new UIPropertyMetadata(null, new PropertyChangedCallback(Exception_Changed)));
        static void Exception_Changed(DependencyObject dep, DependencyPropertyChangedEventArgs args)
        {
            var pthis = dep as ExceptionControl;
            if (pthis.Exceptions == null) { pthis.Exceptions = new ObservableCollection<Exception>(); }
            pthis.Exceptions.Add(pthis.Exception);
        }
        #endregion
        #region Exceptions
        public IList<Exception> Exceptions
        {
            get { return (IList<Exception>)GetValue(ExceptionsProperty); }
            set { SetValue(ExceptionsProperty, value); }
        }
        public static readonly DependencyProperty ExceptionsProperty = DependencyProperty.Register("Exceptions", typeof(IList<Exception>), typeof(ExceptionControl), new PropertyMetadata(null));
        #endregion

        #region AddExceptionCanExecute
        private void AddExceptionCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //using (var sec = this.GetCodeSection(new { sender, e })) {
            e.CanExecute = true;
            e.Handled = true;
            //}
        }
        #endregion
        #region AddExceptionCommand
        private void AddExceptionCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                var ex = e.Parameter as Exception; sec.LogDebug($"received exception {ex?.GetType()?.Name}: '{ex?.Message}'");
                if (ex != null) { this.Exception = ex; }
            }
        }
        #endregion

        #region exception2BackgroundBrush_ConvertEvent
        private object exception2BackgroundBrush_ConvertEvent(DependencyObject source, object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ex = value as ExceptionBase;
            if (ex == null) { return _errorBackgroundBrush; }

            switch (ex.ExceptionLevel)
            {
                case ExceptionLevel.Information:
                case ExceptionLevel.Verbose:
                    return _informationBackgroundBrush;
                case ExceptionLevel.Warning:
                    return _warningBackgroundBrush;
                case ExceptionLevel.Critical:
                case ExceptionLevel.Error:
                case ExceptionLevel.None:
                case ExceptionLevel.Unknown:
                default:
                    return _errorBackgroundBrush;
            }
        }
        #endregion
        #region exceptionIconGlyph_ConvertEvent
        private object exceptionIconGlyph_ConvertEvent(DependencyObject source, object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ex = value as ExceptionBase;
            if (ex == null) { return "!"; }

            switch (ex.ExceptionLevel)
            {
                case ExceptionLevel.Information:
                case ExceptionLevel.Verbose:
                    return "i";
                case ExceptionLevel.Warning:
                    return "?";
                case ExceptionLevel.Critical:
                case ExceptionLevel.Error:
                case ExceptionLevel.None:
                case ExceptionLevel.Unknown:
                default:
                    return "!";
            }
        } 
        #endregion
    }
}
