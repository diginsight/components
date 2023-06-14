using Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Common
{
    /// <summary>
    /// Interaction logic for TrayIconWindow.xaml
    /// </summary>
    public partial class TrayIconWindow : Window
    {
        private ILogger<TrayIconWindow> logger;

        public const string CONFIGVALUE_DISABLETRAYICON = "DisableTrayIcon"; public const bool DEFAULTVALUE_DISABLETRAYICON = true;
        #region internal state
        System.Windows.Forms.NotifyIcon _notifyIcon;
        ContextMenu _menu;
        Window _applicationWindow;
        #endregion

        #region .ctor
        public TrayIconWindow()
        {
            using (var sec = logger.BeginMethodScope())
            {
                InitializeComponent();
                //this.DataContext = viewModel;
                if (DesignerProperties.GetIsInDesignMode(this)) { return; }


                var disableTrayIcon = ConfigurationHelper.GetClassSetting<TrayIconWindow, bool>(CONFIGVALUE_DISABLETRAYICON, DEFAULTVALUE_DISABLETRAYICON); // , CultureInfo.InvariantCulture

                if (disableTrayIcon == false)
                {
                    // Notification Icon.
                    _notifyIcon = new System.Windows.Forms.NotifyIcon();
                    _notifyIcon.Icon = System.Drawing.Icon.FromHandle(Properties.Resources.ProtectDocumentIcon.Handle);
                    _notifyIcon.Visible = true;
                    _notifyIcon.BalloonTipTitle = "Classifica e proteggi"; // Constants.NOTIFICATION_BALLOONTIP_TITLE;
                    _notifyIcon.BalloonTipText = "Status: RUNNING"; // Constants.NOTIFICATION_BALLOONTIP_TEXT_ENABLED;
                    _notifyIcon.Text = "Tray Icon Window text"; // Constants.NOTIFICATION_TEXT;
                    _notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
                    _notifyIcon.ShowBalloonTip(1000); //Constants.NOTIFICATION_BALLOONTIP_DURATION

                    _notifyIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(Notifier_MouseDown);
                    _menu = (ContextMenu)rootGrid.FindResource("NotifierContextMenu");
                    _menu.StaysOpen = false;
                }
            }
        }
        #endregion
        #region Window_Initialized
        private void Window_Initialized(object sender, EventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }
            using (var sec = logger.BeginMethodScope())
            {
                this.Hide();
                this.WindowState = System.Windows.WindowState.Minimized;
            }
        }
        #endregion
        #region Window_Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (var scope = logger.BeginMethodScope(new { sender, e }))
            {
                this.Hide(); scope.LogDebug($"this.Hide(); completed");
                this.WindowState = System.Windows.WindowState.Minimized;
            }
        }
        #endregion

        #region ShowSimulatorMainWindowCanExecute
        private void ShowSimulatorMainWindowCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
            return;
        }
        #endregion

        #region ExitCanExecute
        private void ExitCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
            return;
        }
        #endregion
        #region ExitCommand
        private async void ExitCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using (var scope = logger.BeginMethodScope())
            {
                if (_applicationWindow != null) { _applicationWindow.Hide(); _applicationWindow.Close(); }
                this.Hide(); this.Close(); scope.LogDebug($"this.Close(); completed");
                await Task.Run(() => { scope.LogDebug("Environment.Exit(0);..."); Environment.Exit(0); scope.LogDebug($"Environment.Exit(0); completed"); });
            }
        }
        #endregion

        #region Notifier_MouseDown
        void Notifier_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            using (var sec = logger.BeginMethodScope())
            {
                try
                {
                    if (e.Button == System.Windows.Forms.MouseButtons.Left || e.Button == System.Windows.Forms.MouseButtons.Right)
                    {
                        _menu.IsOpen = !_menu.IsOpen;
                    }
                }
                catch (Exception ex) { sec.LogException(ex); ExceptionManager.RaiseException(this, ex); }
            }
        }
        #endregion
        #region Exit_Click
        private async void Exit_Click(object sender, RoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope())
            {
                this.Close(); sec.LogDebug($"this.Close(); completed");
                await Task.Run(() => { sec.LogDebug("Environment.Exit(0);..."); Environment.Exit(0); sec.LogDebug($"Environment.Exit(0); completed"); });
            }
        }
        #endregion

        #region Window_StateChanged
        private void Window_StateChanged(object sender, EventArgs e)
        {
            using (var sec = logger.BeginMethodScope())
            {
                if (this.WindowState == WindowState.Minimized)
                {
                    this.Hide();
                }
            }
        }
        #endregion
        #region IncreaseFont_Click
        private void IncreaseFont_Click(object sender, EventArgs e)
        {
            using (var sec = logger.BeginMethodScope())
            {
            }
        }
        #endregion
        #region DecreaseFont_Click
        private void DecreaseFont_Click(object sender, EventArgs e)
        {
            using (var sec = logger.BeginMethodScope())
            {
            }
        }
        #endregion
        #region Minimize_Click
        private void Minimize_Click(object sender, EventArgs e)
        {
            using (var sec = logger.BeginMethodScope())
            {
                this.WindowState = WindowState.Minimized;
            }
        }
        #endregion
        #region SendData_Click
        private void SendData_Click(object sender, EventArgs e)
        {
            using (var sec = logger.BeginMethodScope())
            {
                //var wnd = new SendDataWindow();
                //wnd.Show();
            }
        }
        #endregion
    }
}
