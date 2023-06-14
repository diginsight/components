using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Common
{
    public class DropDownToggleButton : ToggleButton
    {
        private ILogger<DropDownToggleButton> logger;
        // *** Dependency Properties *** 
        // *** Constructors *** 
        #region .ctor
        static DropDownToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownToggleButton), new FrameworkPropertyMetadata(typeof(DropDownToggleButton)));
        }
        #endregion
        public DropDownToggleButton()
        {
            using (var scope = logger.BeginMethodScope())
            {
                // Bind the ToogleButton.IsChecked property to the drop-down's IsOpen property 
                //Binding binding = new Binding("Menu.IsOpen");
                //binding.Source = this;
                //this.SetBinding(IsCheckedProperty, binding);
                DataContextChanged += (sender, args) =>
                {
                    if (Menu != null) Menu.DataContext = DataContext;
                };
            }
        }

        public override void OnApplyTemplate()
        {
            using (var scope = logger.BeginMethodScope())
            {
                base.OnApplyTemplate();
                VisualStateManager.GoToState(this, "Normal", false);

                //get the different parts
                //var clearFilterPart = this.GetTemplateChild("ClearFilterPart") as Button;
                var toggleButton = this.GetTemplateChild("toggleButton") as ToggleButton;
                toggleButton.Click += (sender, args) =>
                {
                    OnClick();
                };
                var btnDown = this.GetTemplateChild("btnDown") as Button;
                btnDown.Click += (sender, args) =>
                {
                    DropDownClick();
                };
            }
        }

        // *** Properties *** 
        public ContextMenu Menu
        {
            get { return (ContextMenu)GetValue(MenuProperty); }
            set { SetValue(MenuProperty, value); }
        }
        public static readonly DependencyProperty MenuProperty = DependencyProperty.Register("Menu", typeof(ContextMenu), typeof(DropDownToggleButton), new UIPropertyMetadata(null, OnMenuChanged));

        private static void OnMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dropDownButton = (DropDownToggleButton)d;
            var contextMenu = (ContextMenu)e.NewValue;
            contextMenu.DataContext = dropDownButton.DataContext;
        }

        // *** Overridden Methods *** 
        protected void DropDownClick()
        {
            using (var scope = logger.BeginMethodScope())
            {
                if (Menu != null)
                {
                    // If there is a drop-down assigned to this button, then position and display it 
                    Menu.PlacementTarget = this;
                    Menu.Placement = PlacementMode.Bottom;
                    Menu.IsOpen = true;
                }
            }
        }
    }
}