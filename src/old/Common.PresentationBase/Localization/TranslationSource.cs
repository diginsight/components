#region using
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data; 
#endregion

namespace Common
{
    public class TranslationSource : INotifyPropertyChanged
    {
        private static readonly TranslationSource instance = new TranslationSource();

        public static TranslationSource Instance
        {
            get { return instance; }
        }

        //private readonly ResourceManager resManager = ResourcesHelper.Resources.First().Value;
        private CultureInfo currentCulture = null;

        public object this[string key]
        {
            get {
                return this.GetResourceValue<string>(key) ?? DependencyProperty.UnsetValue; 
            }
        }

        public CultureInfo CurrentCulture
        {
            get { return this.currentCulture; }
            set
            {
                if (this.currentCulture != value)
                {
                    this.currentCulture = value;
                    var @event = this.PropertyChanged;
                    if (@event != null) {
                        @event.Invoke(this, new PropertyChangedEventArgs(string.Empty));
                        @event.Invoke(this, new PropertyChangedEventArgs("CurrentCulture"));
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
