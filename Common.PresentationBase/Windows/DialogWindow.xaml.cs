#region using
using Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
#endregion

namespace Common
{
    /// <summary>Interaction logic for DialogWindow.xaml</summary>
    public partial class DialogWindow : Window
    {
        private ILogger<DialogWindow> logger;
        #region internal state
        #endregion

        #region .ctor
        public DialogWindow()
        {
            using (var sec = logger.BeginMethodScope())
            {
                InitializeComponent(); sec.LogDebug("InitializeComponent(); completed");
            }
        }
        public DialogWindow(FrameworkElement content) : this()
        {
            using (var sec = logger.BeginMethodScope(new { content }))
            {
                if (this.Items==null) { this.Items = new ObservableCollection<FrameworkElement>();  }
                this.Items.Add(content); sec.LogDebug("this.Items.Add(content); completed");
            }
        }
        #endregion
        #region Window_Initialized
        private void Window_Initialized(object sender, EventArgs e)
        {
            using (var sec = logger.BeginMethodScope())
            {
                //this.DialogResult = false;
            }
        }
        #endregion
        #region OnMouseLeftButtonDown
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        } 
        #endregion

        #region Items
        public IList<FrameworkElement> Items
        {
            get { return (IList<FrameworkElement>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(IList<FrameworkElement>), typeof(DialogWindow), new PropertyMetadata());
        #endregion

        #region CancelCanExecute
        private void CancelCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion
        #region CancelExecuted
        private void CancelExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                this.DialogResult = false;
                this.Close();
            }
        }
        #endregion
        #region CloseCanExecute
        private void CloseCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        } 
        #endregion
        #region CloseExecuted
        private void CloseExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                this.DialogResult = false;
                this.Close();
            }
        }
        #endregion
        #region ExitCanExecute
        private void ExitCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion
        #region ExitExecuted
        private void ExitExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                this.DialogResult = true;
                //Environment.Exit(0);
            }
        }
        #endregion
        #region HideCanExecute
        private void HideCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion
        #region HideExecuted
        private void HideExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                this.Hide();
            }
        }
        #endregion
        #region SetWindowStateCanExecute
        private void SetWindowStateCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion
        #region SetWindowStateExecuted
        private void SetWindowStateExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                if (e.Parameter == null) { return; }
                var windowState = (System.Windows.WindowState)e.Parameter;
                this.WindowState = windowState;
            }
        }
        #endregion
    }
}
