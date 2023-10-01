#region using
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
using Application = System.Windows.Application;
using Cursors = System.Windows.Input.Cursors;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Rectangle = System.Windows.Shapes.Rectangle;
#endregion

namespace Common
{
    public class WindowBase : Window
    {
        private ILogger<WindowBase> logger;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        private static Type T = typeof(WindowBase);
        private HwndSource _hwndSource;

        #region .ctor
        static WindowBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowBase), new FrameworkPropertyMetadata(typeof(WindowBase)));
        }
        public WindowBase()
            : base()
        {
            using (var scope = logger.BeginMethodScope())
            {
                PreviewMouseMove += OnPreviewMouseMove;
            }
        }
        #endregion

        #region Click events
        protected void MinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        protected void RestoreClick(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        }

        protected void CloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        protected override void OnInitialized(EventArgs e)
        {
            using (var scope = logger.BeginMethodScope())
            {
                SourceInitialized += OnSourceInitialized;
                base.OnInitialized(e);

                this.CommandBindings.Add(new CommandBinding(Commands.Close, CloseExecuted, CloseCanExecute));
                this.CommandBindings.Add(new CommandBinding(Commands.Cancel, CancelExecuted, CancelCanExecute));
                this.CommandBindings.Add(new CommandBinding(Commands.Hide, HideExecuted, HideCanExecute));
                this.CommandBindings.Add(new CommandBinding(Commands.Exit, ExitExecuted, ExitCanExecute));
                this.CommandBindings.Add(new CommandBinding(Commands.Minimize, MinimizeExecuted, MinimizeCanExecute));
                this.CommandBindings.Add(new CommandBinding(Commands.Maximize, MaximizeExecuted, MaximizeCanExecute));
                this.CommandBindings.Add(new CommandBinding(Commands.Restore, RestoreExecuted, RestoreCanExecute));
                this.CommandBindings.Add(new CommandBinding(Commands.ToggleWindowState, ToggleWindowStateExecuted, ToggleWindowStateCanExecute));
                this.CommandBindings.Add(new CommandBinding(Commands.SetWindowState, SetWindowStateExecuted, SetWindowStateCanExecute));
                //this.CommandBindings.Add(new CommandBinding(Commands.AddItem, AddItemExecuted, AddItemCanExecute));
                //this.CommandBindings.Add(new CommandBinding(Commands.RemoveItem, RemoveItemExecuted, RemoveItemCanExecute));
            }
        }

        #region WindowChrome
        public WindowChrome WindowChrome
        {
            get { return this.GetValue(WindowChrome.WindowChromeProperty) as WindowChrome; }
            set { this.SetValue(WindowChrome.WindowChromeProperty, value); }
        }
        #endregion
        #region DragMode
        public WindowDragMode DragMode
        {
            get { return (WindowDragMode)GetValue(DragModeProperty); }
            set { SetValue(DragModeProperty, value); }
        }
        public static readonly DependencyProperty DragModeProperty = DependencyProperty.Register("DragMode", typeof(WindowDragMode), typeof(WindowBase), new PropertyMetadata(WindowDragMode.Standard));
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
                this.Hide();
                try { this.DialogResult = false; } catch (Exception ex) { sec.LogException(ex); }
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
                this.Hide();
                try { this.DialogResult = false; } catch (Exception ex) { sec.LogException(ex); }
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
        private async void ExitExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                // Environment.Exit(0);
                this.Hide(); sec.LogDebug("this.Hide();");
                this.Close(); sec.LogDebug("this.Close();");

                var application = Application.Current;
                var isAddIn = application.Properties != null && application.Properties.Contains("IsAddIn") ? (bool)application.Properties["IsAddIn"] : false;
                if (!isAddIn)
                {
                    await Task.Run(() =>
                    {
                        sec.LogDebug("Environment.Exit(0);..."); Environment.Exit(0); sec.LogDebug($"Environment.Exit(0); completed");
                    });
                }

            }
        }
        #endregion
        #region MinimizeCanExecute
        private void MinimizeCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion
        #region MinimizeExecuted
        private void MinimizeExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                this.WindowState = WindowState.Minimized;
            }
        }
        #endregion
        #region MaximizeCanExecute
        private void MaximizeCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion
        #region MaximizeExecuted
        private void MaximizeExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                this.WindowState = WindowState.Maximized;
            }
        }
        #endregion
        #region RestoreCanExecute
        private void RestoreCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion
        #region RestoreExecuted
        private void RestoreExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                this.WindowState = WindowState.Normal;
            }
        }
        #endregion
        #region ToggleWindowStateCanExecute
        private void ToggleWindowStateCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion
        #region ToggleWindowStateExecuted
        private void ToggleWindowStateExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                if (this.WindowState == WindowState.Normal)
                {
                    Commands.Maximize.Execute(null, this);
                }
                else
                {
                    Commands.Restore.Execute(null, this);
                }
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

        #region OnSourceInitialized
        private void OnSourceInitialized(object sender, EventArgs e)
        {
            using (var scope = logger.BeginMethodScope())
            {
                _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
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
        #region OnMouseLeftButtonDown
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (this.DragMode == WindowDragMode.Full && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        #endregion
        protected void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!this.ResizeMode.HasFlag(ResizeMode.CanResize)) { return; }

            //using (var sec = this.GetCodeSection(null, System.Diagnostics.SourceLevels.Verbose, "UI"))
            //{
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                Cursor = Cursors.Arrow;
            //}
        }
        protected void ResizeRectangle_MouseMove(Object sender, MouseEventArgs e)
        {
            if (!this.ResizeMode.HasFlag(ResizeMode.CanResize)) { return; }

            Rectangle rectangle = sender as Rectangle;
            switch (rectangle.Name)
            {
                case "top":
                    Cursor = Cursors.SizeNS;
                    break;
                case "bottom":
                    Cursor = Cursors.SizeNS;
                    break;
                case "left":
                    Cursor = Cursors.SizeWE;
                    break;
                case "right":
                    Cursor = Cursors.SizeWE;
                    break;
                case "topLeft":
                    Cursor = Cursors.SizeNWSE;
                    break;
                case "topRight":
                    Cursor = Cursors.SizeNESW;
                    break;
                case "bottomLeft":
                    Cursor = Cursors.SizeNESW;
                    break;
                case "bottomRight":
                    Cursor = Cursors.SizeNWSE;
                    break;
                default:
                    break;
            }
        }
        protected void ResizeRectangle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.ResizeMode.HasFlag(ResizeMode.CanResize)) { return; }

            Rectangle rectangle = sender as Rectangle;
            switch (rectangle.Name)
            {
                case "top":
                    Cursor = Cursors.SizeNS;
                    ResizeWindow(ResizeDirection.Top);
                    break;
                case "bottom":
                    Cursor = Cursors.SizeNS;
                    ResizeWindow(ResizeDirection.Bottom);
                    break;
                case "left":
                    Cursor = Cursors.SizeWE;
                    ResizeWindow(ResizeDirection.Left);
                    break;
                case "right":
                    Cursor = Cursors.SizeWE;
                    ResizeWindow(ResizeDirection.Right);
                    break;
                case "topLeft":
                    Cursor = Cursors.SizeNWSE;
                    ResizeWindow(ResizeDirection.TopLeft);
                    break;
                case "topRight":
                    Cursor = Cursors.SizeNESW;
                    ResizeWindow(ResizeDirection.TopRight);
                    break;
                case "bottomLeft":
                    Cursor = Cursors.SizeNESW;
                    ResizeWindow(ResizeDirection.BottomLeft);
                    break;
                case "bottomRight":
                    Cursor = Cursors.SizeNWSE;
                    ResizeWindow(ResizeDirection.BottomRight);
                    break;
                default:
                    break;
            }
        }
        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(_hwndSource.Handle, 0x112, (IntPtr)(61440 + direction), IntPtr.Zero);
        }
        private enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }
        public enum WindowDragMode
        {
            Standard = 1,
            Full = 2,
        }
    }
}
