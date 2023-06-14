#region using
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
#endregion

namespace Common
{
    /// <summary>Interaction logic for SettingsThemesControl.xaml</summary>
    public partial class SettingsThemesControl : UserControl
    {
        private ILogger<SettingsThemesControl> logger;
        #region IsCollapsed
        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }
        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register("IsCollapsed", typeof(bool), typeof(SettingsThemesControl), new PropertyMetadata(false));
        #endregion
        #region Title
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(SettingsThemesControl), new PropertyMetadata()); 
        #endregion
        #region Commands
        public List<CommandInfo> Commands
        {
            get { return (List<CommandInfo>)GetValue(CommandsProperty); }
            set { SetValue(CommandsProperty, value); }
        }
        public static readonly DependencyProperty CommandsProperty = DependencyProperty.Register("Commands", typeof(List<CommandInfo>), typeof(SettingsThemesControl), new PropertyMetadata(null));
        #endregion

        #region .ctor
        static SettingsThemesControl()
        {
        }
        public SettingsThemesControl()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }
        }
        #endregion

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
    }
}

