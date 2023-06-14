#region using
using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Resources;
using Common.Properties;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
#endregion

namespace Common
{
    /// <summary>Interaction logic for SettingsDownloadControl.xaml</summary>
    public partial class SettingsAppInsightKeyControl : UserControl
    {
        private ILogger<SettingsAppInsightKeyControl> logger;
        #region IsCollapsed
        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }
        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register("IsCollapsed", typeof(bool), typeof(SettingsAppInsightKeyControl), new PropertyMetadata(false));
        #endregion
        #region Title
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(SettingsAppInsightKeyControl), new PropertyMetadata());
        #endregion

        #region .ctor
        static SettingsAppInsightKeyControl()
        {
        }
        public SettingsAppInsightKeyControl()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }
        }
        #endregion
        private void ctlSettingsAppInsightKeyControl_Initialized(object sender, EventArgs e)
        {
            var app = Application.Current as ApplicationBase;
            this.AIApplicationID = app.Properties["AIApplicationID"] as string;
            this.AIApiKey = app.Properties["AIApiKey"] as string;
        }

        #region ToggleIsCollapsedCanExecute
        private void ToggleIsCollapsedCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //using (var sec = this.GetCodeSection(new { sender, e })) {
            e.CanExecute = true;
            e.Handled = true;
            //}
        }
        #endregion
        #region ToggleIsCollapsedCommand
        private void ToggleIsCollapsedCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                IsCollapsed = !IsCollapsed;
            }
        }
        #endregion

        #region AIApplicationID
        public string AIApplicationID
        {
            get { return (string)GetValue(AIApplicationIDProperty); }
            set { SetValue(AIApplicationIDProperty, value); }
        }
        public static readonly DependencyProperty AIApplicationIDProperty = DependencyProperty.Register("AIApplicationID", typeof(string), typeof(SettingsAppInsightKeyControl), new PropertyMetadata(null, AIApplicationIDChanged));
        public static void AIApplicationIDChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pthis = d as SettingsAppInsightKeyControl;
            var app = Application.Current as ApplicationBase;
            app.SetProperty("AIApplicationID", pthis.AIApplicationID);
        }
        #endregion
        #region AIApiKey
        public string AIApiKey
        {
            get { return (string)GetValue(AIApiKeyProperty); }
            set { SetValue(AIApiKeyProperty, value); }
        }
        public static readonly DependencyProperty AIApiKeyProperty = DependencyProperty.Register("AIApiKey", typeof(string), typeof(SettingsAppInsightKeyControl), new PropertyMetadata(null, AIApiKeyChanged));
        public static void AIApiKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pthis = d as SettingsAppInsightKeyControl;
            var app = Application.Current as ApplicationBase;
            app.SetProperty("AIApiKey", pthis.AIApiKey);
        }
        #endregion
    }
}

