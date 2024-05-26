#region using
using Common;
using EkipCommon;

using EkipConnect.Models;
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

namespace EkipCommon
{
    /// <summary>Interaction logic for HamburgerMenuControl.xaml</summary>
    public partial class HamburgerMenuControl : UserControl
    {
        ICollectionView _itemsView = null;
        #region .ctor
        static HamburgerMenuControl()
        {
        }
        public HamburgerMenuControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Background
        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(HamburgerMenuControl), new PropertyMetadata(BrushConstants.AbbBlue6));
        #endregion
        //#region Foreground
        //public Brush Foreground
        //{
        //    get { return (Brush)GetValue(ForegroundProperty); }
        //    set { SetValue(ForegroundProperty, value); }
        //}
        //public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Brush), typeof(HamburgerMenuControl), new PropertyMetadata(BrushConstants.AbbBlue6)); // AbbBlue6 AbbGrey38
        //#endregion
        #region Title
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(HamburgerMenuControl), new PropertyMetadata()); 
        #endregion

        #region IsCollapsed
        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }
        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register("IsCollapsed", typeof(bool), typeof(HamburgerMenuControl), new PropertyMetadata(false));
        #endregion
        #region Items
        public List<DeviceSchemaInfo> Items
        {
            get { return (List<DeviceSchemaInfo>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(List<DeviceSchemaInfo>), typeof(HamburgerMenuControl), new PropertyMetadata(null));
        public static void ItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pthis = d as HamburgerMenuControl;
            pthis._itemsView = CollectionViewSource.GetDefaultView(pthis.Items);
        }

        #endregion
        #region SelectedItem
        public DeviceSchemaInfo SelectedItem
        {
            get { return (DeviceSchemaInfo)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(DeviceSchemaInfo), typeof(HamburgerMenuControl), new PropertyMetadata(null));
        #endregion

        #region Filter
        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }
        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(string), typeof(HamburgerMenuControl), new PropertyMetadata(null, new PropertyChangedCallback(FilterChanged)));
        public static void FilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pthis = d as HamburgerMenuControl;
            if (pthis._itemsView == null) { pthis._itemsView = CollectionViewSource.GetDefaultView(pthis.Items); }
            pthis._itemsView.Filter = pthis.CustomerFilter;
        }
        #endregion
        #region ShowAllDevices
        public bool ShowAllDevices
        {
            get { return (bool)GetValue(ShowAllDevicesProperty); }
            set { SetValue(ShowAllDevicesProperty, value); }
        }
        public static readonly DependencyProperty ShowAllDevicesProperty = DependencyProperty.Register("ShowAllDevices", typeof(bool), typeof(HamburgerMenuControl), new PropertyMetadata(false));
        #endregion

        private bool CustomerFilter(object item)
        {
            var deviceSchemaInfo = item as DeviceSchemaInfo;
            var index = deviceSchemaInfo.DeviceName?.IndexOf(this.Filter, StringComparison.CurrentCultureIgnoreCase);
            if (index==null || index < 0) { index = deviceSchemaInfo.DeviceID.ToString()?.IndexOf(this.Filter, StringComparison.CurrentCultureIgnoreCase); }

            if (index >= 0) { return true; }
            return false;
        }


        #region btnToggleIsCollapsed_Click
        private void btnToggleIsCollapsed_Click(object sender, RoutedEventArgs e)
        {
            IsCollapsed = !IsCollapsed;
        }
        #endregion
        #region btnSelectItem_Click
        private void btnSelectItem_Click(object sender, RoutedEventArgs e)
        {
            using (var sec = this.GetCodeSection(new { sender, e }))
            {
                var selectedItem = ((FrameworkElement)sender).DataContext as DeviceSchemaInfo;
                this.SelectedItem = selectedItem;
            }
        }
        #endregion
    }
}

