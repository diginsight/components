#region using
using Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
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
    /// <summary>Interaction logic for SettingsControl.xaml</summary>
    public partial class SettingsControl : UserControl
    {
        private static readonly Type T = typeof(SettingsControl);
        private readonly ResourceManager resManager = Properties.Resources.ResourceManager;
        private ILogger<SettingsControl> logger;

        #region .ctor
        static SettingsControl() { }
        public SettingsControl()
        {
            using var sec = logger.BeginMethodScope();
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }

            Commands = new List<CommandInfo>() {
                new CommandInfo() { Name=this.GetResourceValue<string>("Settings.Themes.Title", "Themes"), Description="Themes", Command = Common.Commands.Themes },
                new CommandInfo() { Name=this.GetResourceValue<string>("Settings.Lang.Title", "Language"), Description=this.GetResourceValue<string>("Settings.Lang.Title"), Command = Common.Commands.Languages },
                new CommandInfo() { Name=this.GetResourceValue<string>("Settings.About.Title", "About"), Description="About", Command = Common.Commands.About }
            };

            Common.Commands.Reset.Execute(null, this);
        }
        public SettingsControl(FrameworkElement content) : this()
        {
            using var sec = logger.BeginMethodScope(new { content });

            Common.Commands.AddItem.Execute(content, this);
        }
        #endregion

        #region IsMouseOverOuterElement
        public bool IsMouseOverOuterElement
        {
            get { return (bool)GetValue(IsMouseOverOuterElementProperty); }
            set { SetValue(IsMouseOverOuterElementProperty, value); }
        }
        public static readonly DependencyProperty IsMouseOverOuterElementProperty = DependencyProperty.Register("IsMouseOverOuterElement", typeof(bool), T, new PropertyMetadata());
        #endregion
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

        #region Commands
        public List<CommandInfo> Commands
        {
            get { return (List<CommandInfo>)GetValue(CommandsProperty); }
            set { SetValue(CommandsProperty, value); }
        }
        public static readonly DependencyProperty CommandsProperty = DependencyProperty.Register("Commands", typeof(List<CommandInfo>), T, new PropertyMetadata(null));
        #endregion
        #region Items
        public IList<FrameworkElement> Items
        {
            get { return (IList<FrameworkElement>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(IList<FrameworkElement>), T, new PropertyMetadata());
        #endregion

        #region ResetCanExecute
        private void ResetCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //using (var sec = this.GetCodeSection(new { sender, e })) {
            e.CanExecute = true;
            e.Handled = true;
            //}
        }
        #endregion
        #region ResetCommand
        private void ResetCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                // var fe = e.Parameter as FrameworkElement; sec.Debug($"fe: {fe.GetLogString()}");
                // if (fe == null) { return; }

                var settingsMenu = new SettingsMenuControl();
                settingsMenu.DataContext = this;
                settingsMenu.SetBinding(SettingsMenuControl.CommandsProperty, new Binding("Commands") { Mode = BindingMode.OneWay });

                Common.Commands.Clear.Execute(settingsMenu, this);
                Common.Commands.AddItem.Execute(settingsMenu, this);
            }
        }
        #endregion
        #region RegisterPanelCanExecute
        private void RegisterPanelCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //using (var sec = this.GetCodeSection(new { sender, e })) {
            e.CanExecute = true;
            e.Handled = true;
            //}
        }
        #endregion
        #region RegisterPanelCommand
        private void RegisterPanelCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                var panelInfo = e.Parameter as SettingsPanelInfo;

                var commandInfo = new CommandInfo() { Name = panelInfo.Name, Description = panelInfo.Description, Command = Common.Commands.AddSettingsPanel, CommandParameter = panelInfo };
                this.Commands.Insert(panelInfo.Position, commandInfo);
            }
        }
        #endregion
        #region AddSettingsPanelCanExecute
        private void AddSettingsPanelCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //using (var sec = this.GetCodeSection(new { sender, e })) {
            e.CanExecute = true;
            e.Handled = true;
            //}
        }
        #endregion
        #region AddSettingsPanelCommand
        private void AddSettingsPanelCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                var panelInfo = e.Parameter as SettingsPanelInfo;
                var panel = panelInfo.GetPanelInstance();
                Common.Commands.AddItem.Execute(panel, this);
            }
        }
        #endregion

        #region ClearCanExecute
        private void ClearCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //using (var sec = this.GetCodeSection(new { sender, e })) {
            e.CanExecute = true;
            e.Handled = true;
            //}
        }
        #endregion
        #region ClearCommand
        private void ClearCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                var fe = e.Parameter as FrameworkElement; sec.LogDebug($"fe: {fe.GetLogString()}");
                if (fe == null) { return; }
                if (this.Items == null) { this.Items = new ObservableCollection<FrameworkElement>(); }
                this.Items.Clear(); sec.LogDebug("this.Items.Add(fe); completed");
            }
        }
        #endregion
        #region AddItemCanExecute
        private void AddItemCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //using (var sec = this.GetCodeSection(new { sender, e })) {
            e.CanExecute = true;
            e.Handled = true;
            //}
        }
        #endregion
        #region AddItemCommand
        private void AddItemCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                var fe = e.Parameter as FrameworkElement; sec.LogDebug($"fe: {fe.GetLogString()}");
                if (fe == null) { return; }
                if (this.Items == null) { this.Items = new ObservableCollection<FrameworkElement>(); }
                this.Items.Add(fe); sec.LogDebug("this.Items.Add(fe); completed");
            }
        }
        #endregion
        #region RemoveItemCanExecute
        private void RemoveItemCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //using (var sec = this.GetCodeSection(new { sender, e })) {
            e.CanExecute = true;
            e.Handled = true;
            //}
        }
        #endregion
        #region RemoveItemCommand
        private void RemoveItemCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                var fe = e.Parameter as FrameworkElement; sec.LogDebug($"fe: {fe.GetLogString()}");
                if (fe == null) { return; }
                if (this.Items == null) { this.Items = new ObservableCollection<FrameworkElement>(); }

                if (this.Items.Contains(fe))
                {
                    this.Items.Remove(fe); sec.LogDebug("this.Remove.Add(fe); completed");
                }
            }
        }
        #endregion

        #region ThemesCanExecute
        private void ThemesCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //using (var sec = this.GetCodeSection(new { sender, e })) {
            e.CanExecute = true;
            e.Handled = true;
            //}
        }
        #endregion
        #region ThemesCommand
        private void ThemesCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                var themes = new SettingsThemesControl();
                Common.Commands.AddItem.Execute(themes, this);
            }
        }
        #endregion
        #region LanguagesCanExecute
        private void LanguagesCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //using (var sec = this.GetCodeSection(new { sender, e })) {
            e.CanExecute = true;
            e.Handled = true;
            //}
        }
        #endregion
        #region LanguagesCommand
        private void LanguagesCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                var lang = new SettingsLanguagesControl();
                Common.Commands.AddItem.Execute(lang, this);
            }
        }
        #endregion
        #region AboutCanExecute
        private void AboutCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //using (var sec = this.GetCodeSection(new { sender, e })) {
            e.CanExecute = true;
            e.Handled = true;
            //}
        }
        #endregion
        #region AboutCommand
        private void AboutCommand(object sender, ExecutedRoutedEventArgs e)
        {
            using (var sec = logger.BeginMethodScope(new { sender, e }))
            {
                var themes = new SettingsAboutControl();
                Common.Commands.AddItem.Execute(themes, this);
            }
        }
        #endregion

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            using var sec = logger.BeginMethodScope(new { sender, e });
            this.IsMouseOverOuterElement = true;
        }
        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            using var sec = logger.BeginMethodScope(new { sender, e });
            this.IsMouseOverOuterElement = false;
        }
    }
    public class CommandInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string InternalName { get; set; }
        public Type Type { get; set; }
        public RoutedCommand Command { get; set; }
        public object CommandParameter { get; set; }
    }
    public abstract class SettingsPanelInfo
    {
        public int Position { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string InternalName { get; set; }
        public Type Type { get; set; }
        public abstract FrameworkElement GetPanelInstance();
    }
    public class SettingsPanelInfo<T> : SettingsPanelInfo
        where T : FrameworkElement, new()
    {
        public override FrameworkElement GetPanelInstance() { return new T(); }
    }
}

