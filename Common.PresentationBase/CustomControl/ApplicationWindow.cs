#region using
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shapes;
using System.Windows.Shell;
#endregion

namespace Common
{
    public class ApplicationWindow : WindowBase
    {
        private ILogger<ApplicationWindow> logger;
        private static Type T = typeof(ApplicationWindow);
        //private HwndSource _hwndSource;

        #region .ctor
        static ApplicationWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ApplicationWindow), new FrameworkPropertyMetadata(typeof(ApplicationWindow)));
        }
        public ApplicationWindow()
            : base()
        {
            using (var scope = logger.BeginMethodScope())
            {
                //PreviewMouseMove += OnPreviewMouseMove;
            }
        }
        public ApplicationWindow(FrameworkElement content) : this()
        {
            using (var sec = logger.BeginMethodScope(new { content }))
            {
                if (this.Items == null) { this.Items = new ObservableCollection<FrameworkElement>(); }
                this.Items.Add(content); sec.LogDebug("this.Items.Add(content); completed");
            }
        }
        #endregion

        #region Items
        public IList<FrameworkElement> Items
        {
            get { return (IList<FrameworkElement>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(IList<FrameworkElement>), typeof(ApplicationWindow), new PropertyMetadata(null));
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
        #region AddItemCanExecute
        private void AddItemCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion
        #region AddItemExecuted
        private void AddItemExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                if (e.Parameter == null) { return; }
                var userControl = (UserControl)e.Parameter;
                this.Items.Add(userControl); sec.LogDebug("this.Items.Add(userControl); comleted");
            }
        }
        #endregion
        #region RemoveItemCanExecute
        private void RemoveItemCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion
        #region RemoveItemExecuted
        private void RemoveItemExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                if (e.Parameter == null) { return; }
                var userControl = (UserControl)e.Parameter;
                this.Items.Remove(userControl);
            }
        }
        #endregion


        #region OnInitialized
        protected override void OnInitialized(EventArgs e)
        {
            using (var scope = logger.BeginMethodScope())
            {
                //SourceInitialized += OnSourceInitialized;
                base.OnInitialized(e);

                this.CommandBindings.Add(new CommandBinding(Commands.Close, CloseExecuted, CloseCanExecute));
                this.CommandBindings.Add(new CommandBinding(Commands.AddItem, AddItemExecuted, AddItemCanExecute));
                this.CommandBindings.Add(new CommandBinding(Commands.RemoveItem, RemoveItemExecuted, RemoveItemCanExecute));
            }
        } 
        #endregion
        #region OnApplyTemplate
        public override void OnApplyTemplate()
        {
            using (var scope = logger.BeginMethodScope())
            {
                base.OnApplyTemplate();
            }
        }
        #endregion
    }
}
