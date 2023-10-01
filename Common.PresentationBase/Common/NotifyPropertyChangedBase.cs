#region using
using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Application = System.Windows.Application;
#endregion

namespace Common
{

    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        protected bool SetProperty<T>(ref T property, T value, [CallerMemberName] String propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(property, value))
                return false;

            property = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (Thread.CurrentThread.IsBackground == true)
            {
                var dispatcher = Application.Current.Dispatcher;
                dispatcher?.Invoke(() =>
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName))
                );
            }
            else
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
