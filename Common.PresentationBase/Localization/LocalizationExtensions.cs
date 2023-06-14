#region using
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
//using System.Management.Instrumentation;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
#endregion

namespace Common
{
    public class Localization
    {
        public static Localize GetUid(DependencyObject obj)
        {
            return (Localize)obj.GetValue(UidProperty);
        }
        public static void SetUid(DependencyObject obj, Localize value)
        {
            obj.SetValue(UidProperty, value);
        }
        public static readonly DependencyProperty UidProperty = DependencyProperty.RegisterAttached("Uid", typeof(Localize), typeof(Localization), new PropertyMetadata(null, UidChanged));
        #region UidChanged
        static void UidChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pthis = d as FrameworkElement;
            var loc = e.NewValue as Localize;
            if (loc == null) { return; }

            var fallback = pthis.GetValue(loc.Property);
            var binding = new Binding($"[{loc.Key}]") { Mode = BindingMode.OneWay, Source = TranslationSource.Instance, FallbackValue = fallback };
            pthis.SetBinding(loc.Property, binding);
        }
        #endregion
    }

    public class Localize : MarkupExtension
    {
        #region .ctor
        public Localize() { }
        public Localize(string key) { this.Key = key; }
        #endregion

        public DependencyProperty Property { get; set; }
        public string Key { get; set; }
        public string FallbackValue { get; set; }

        #region ProvideValue
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
        #endregion
    }


    public static class LocalizationExtensions // : Binding
    {
        //#region .ctor
        //public LocalizationExtensions(string name) : base("[" + name + "]")
        //{
        //    this.Mode = BindingMode.OneWay;
        //    this.Source = TranslationSource.Instance;
        //} 
        //#endregion

        public static IEnumerable<CultureInfo> GetAvailableCultures()
        {
            List<CultureInfo> result = new List<CultureInfo>();

            //ResourceManager rm = new ResourceManager(typeof(Resources));
            var resourceManagers = ResourcesHelper.Resources;

            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            foreach (CultureInfo culture in cultures)
            {
                try
                {
                    if (culture.Equals(CultureInfo.InvariantCulture)) continue; //do not use "==", won't work

                    var ok = resourceManagers.Any(rm => rm.Value.GetResourceSet(culture, true, false) != null);
                    if (ok)
                        result.Add(culture);
                }
                catch (CultureNotFoundException)
                {
                    //NOP
                }
            }
            return result;
        }

        public static ObservableCollection<CultureInfo> GetAvailableLanguages()
        {
            var languages = new ObservableCollection<CultureInfo>();
            var cultures = GetAvailableCultures();
            foreach (CultureInfo culture in cultures)
                languages.Add(culture);
            return languages;
        }

        public static string ToLocalize(this string pthis, string defaultValue = null)
        {
            if (string.IsNullOrEmpty(pthis)) { return pthis; }
            var res = ResourcesHelper.GetResourceValue(pthis, pthis, defaultValue ?? pthis);
            return res;
        }
    }
}
