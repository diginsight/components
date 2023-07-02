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
    /// <summary>Interaction logic for InputBoxControl.xaml</summary>
    public partial class InputBoxControl : UserControl
    {
        private static readonly Type T = typeof(InputBoxControl);
        private ILogger<InputBoxControl> logger;
        public event EventHandler<InputBoxControl> OnOK;
        public event EventHandler<InputBoxControl> OnCancel;
        //public event EventHandler<InputBoxControl> OnClose;

        #region IsCollapsed
        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }
        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register("IsCollapsed", typeof(bool), T, new PropertyMetadata(false));
        #endregion
        #region Title
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), T, new PropertyMetadata());
        #endregion
        #region Label
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), T, new PropertyMetadata());
        #endregion
        #region Text
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), T, new PropertyMetadata());
        #endregion

        #region Commands
        public List<CommandInfo> Commands
        {
            get { return (List<CommandInfo>)GetValue(CommandsProperty); }
            set { SetValue(CommandsProperty, value); }
        }
        public static readonly DependencyProperty CommandsProperty = DependencyProperty.Register("Commands", typeof(List<CommandInfo>), T, new PropertyMetadata(null));
        #endregion

        #region .ctor
        static InputBoxControl()
        {
        }
        public InputBoxControl()
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
        #region CancelCanExecute
        private void CancelCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //using (var sec = this.GetCodeSection(new { sender, e })) {
            e.CanExecute = true;
            e.Handled = true;
            //}
        }
        #endregion
        #region CancelCommand
        private void CancelCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using var sec = logger.BeginMethodScope(new { sender, e });
            if (OnCancel != null) { OnCancel(this, this); }
            Common.Commands.Close.Execute(this, this);
        }
        #endregion
        #region OKCanExecute
        private void OKCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //using (var sec = this.GetCodeSection(new { sender, e })) {
            e.CanExecute = true;
            e.Handled = true;
            //}
        }
        #endregion
        #region OKCommand
        private void OKCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using var sec = logger.BeginMethodScope(new { sender, e });

            if (OnOK != null) { OnOK(this, this); }

            Common.Commands.Close.Execute(this, this);
        }
        #endregion
    }
}

