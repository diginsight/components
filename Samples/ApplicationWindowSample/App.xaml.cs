using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ApplicationWindowSample
{
    /// <summary>Interaction logic for App.xaml</summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainControl = new MainControl();
            var applicationWindow = new ApplicationWindow(mainControl)
            {
                Width = 800, Height = 450,
                //ResizeMode = ResizeMode.CanResize,
                //WindowState = WindowState.Normal,
                //WindowStyle = WindowStyle.None,
                DragMode = WindowBase.WindowDragMode.Full,
                WindowChrome = new System.Windows.Shell.WindowChrome()
                {
                    CaptionHeight = 0,
                    ResizeBorderThickness = new Thickness(5)
                }

            };
            applicationWindow.Name = mainControl.Name;

            this.MainWindow = applicationWindow;
            applicationWindow.Show();
        }
    }
}
