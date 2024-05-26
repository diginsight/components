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
using UserControl = System.Windows.Controls.UserControl;
using ComboBox = System.Windows.Controls.ComboBox;
#endregion

namespace Common
{
    /// <summary>Interaction logic for SettingsDownloadControl.xaml</summary>
    public partial class SettingsLanguagesControl : UserControl
    {
        private ILogger<SettingsLanguagesControl> logger;
        #region IsCollapsed
        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }
        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register("IsCollapsed", typeof(bool), typeof(SettingsLanguagesControl), new PropertyMetadata(false));
        #endregion
        #region Title
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(SettingsLanguagesControl), new PropertyMetadata()); 
        #endregion
        //#region Commands
        //public List<CommandInfo> Commands
        //{
        //    get { return (List<CommandInfo>)GetValue(CommandsProperty); }
        //    set { SetValue(CommandsProperty, value); }
        //}
        //public static readonly DependencyProperty CommandsProperty = DependencyProperty.Register("Commands", typeof(List<CommandInfo>), typeof(SettingsLanguagesControl), new PropertyMetadata(null));
        //#endregion

        #region .ctor
        static SettingsLanguagesControl()
        {
        }
        public SettingsLanguagesControl()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }



            cmbLanguages.ItemsSource = LocalizationExtensions.GetAvailableCultures();
            
            cmbLanguages.SelectedValue =  Thread.CurrentThread.CurrentCulture;

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

        private void CmbLanguages_DropDownClosed(object sender, EventArgs e)
        {
            if (sender is ComboBox cb)
            {
                //CultureInfo.CurrentCulture = (CultureInfo)cb.SelectedItem;
                Commands.ChangeCulture.Execute((CultureInfo)cb.SelectedItem, this);
                //Thread.CurrentThread.CurrentCulture = (CultureInfo)cb.SelectedItem;

            }
        }
    }
}

