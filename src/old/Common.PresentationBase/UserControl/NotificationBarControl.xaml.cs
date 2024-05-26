#region using
using Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
//using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
#endregion

namespace EkipCommon
{
    /// <summary>Interaction logic for ExceptionControl.xaml</summary>
    public partial class NotificationBarControl : UserControl
    {
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        int counter = 0;

        #region Message
        public string DialogMessage
        {
            get { return (string)GetValue(DialogMessageProperty); }
            set { SetValue(DialogMessageProperty, value); }
        }
        public static readonly DependencyProperty DialogMessageProperty = DependencyProperty.Register("DialogMessage", typeof(string), typeof(NotificationBarControl), new PropertyMetadata(null));
        #endregion


        #region .ctor
        public NotificationBarControl()
        {
            using (var sec = this.GetCodeSection())
            {
                InitializeComponent();
                //  DispatcherTimer setup
                dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            }
        }
        #endregion

        private void BtnCloseNotification_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = 1;
            animation.To = 0;
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath(UserControl.OpacityProperty));
            animation.Completed += (ss, ee) => { ((Panel)Parent).Children.Remove(this); };
            Storyboard sb = new Storyboard();
            sb.Children.Add(animation);
            sb.Begin();
            dispatcherTimer.Stop();

        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (counter == 5)
            {
                DoubleAnimation animation = new DoubleAnimation();
                animation.From = 1;
                animation.To = 0;
                Storyboard.SetTarget(animation, this);
                Storyboard.SetTargetProperty(animation, new PropertyPath(UserControl.OpacityProperty));
                animation.Completed += (ss, ee) => { ((Panel)Parent).Children.Remove(this); };
                Storyboard sb = new Storyboard();
                sb.Children.Add(animation);
                sb.Begin();
                dispatcherTimer.Stop();
            }
            else
            {
                counter++;
            }
        }

        private void GdMessageDialog_Loaded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Start();
        }
    }
}
