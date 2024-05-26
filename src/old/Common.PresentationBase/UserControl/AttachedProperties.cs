using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;

namespace Common
{
    public class AttachedProperties
    {
        #region IsVisible0
        public static bool GetIsVisible0(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsVisible0Property);
        }
        public static void SetIsVisible0(DependencyObject obj, bool value)
        {
            obj.SetValue(IsVisible0Property, value);
        }
        public static readonly DependencyProperty IsVisible0Property = DependencyProperty.RegisterAttached("IsVisible0", typeof(bool), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region IsVisible1
        public static bool GetIsVisible1(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsVisible1Property);
        }
        public static void SetIsVisible1(DependencyObject obj, bool value)
        {
            obj.SetValue(IsVisible1Property, value);
        }
        public static readonly DependencyProperty IsVisible1Property = DependencyProperty.RegisterAttached("IsVisible1", typeof(bool), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region IsVisible2
        public static bool GetIsVisible2(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsVisible2Property);
        }
        public static void SetIsVisible2(DependencyObject obj, bool value)
        {
            obj.SetValue(IsVisible2Property, value);
        }
        public static readonly DependencyProperty IsVisible2Property = DependencyProperty.RegisterAttached("IsVisible2", typeof(bool), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region IsVisible3
        public static bool GetIsVisible3(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsVisible3Property);
        }
        public static void SetIsVisible3(DependencyObject obj, bool value)
        {
            obj.SetValue(IsVisible3Property, value);
        }
        public static readonly DependencyProperty IsVisible3Property = DependencyProperty.RegisterAttached("IsVisible3", typeof(bool), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region IsVisible4
        public static bool GetIsVisible4(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsVisible4Property);
        }
        public static void SetIsVisible4(DependencyObject obj, bool value)
        {
            obj.SetValue(IsVisible4Property, value);
        }
        public static readonly DependencyProperty IsVisible4Property = DependencyProperty.RegisterAttached("IsVisible4", typeof(bool), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region IsVisible5
        public static bool GetIsVisible5(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsVisible5Property);
        }
        public static void SetIsVisible5(DependencyObject obj, bool value)
        {
            obj.SetValue(IsVisible5Property, value);
        }
        public static readonly DependencyProperty IsVisible5Property = DependencyProperty.RegisterAttached("IsVisible5", typeof(bool), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion

        #region Boolean0
        public static bool GetBoolean0(DependencyObject obj)
        {
            return (bool)obj.GetValue(Boolean0Property);
        }
        public static void SetBoolean0(DependencyObject obj, bool value)
        {
            obj.SetValue(Boolean0Property, value);
        }
        public static readonly DependencyProperty Boolean0Property = DependencyProperty.RegisterAttached("Boolean0", typeof(bool), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Boolean1
        public static bool GetBoolean1(DependencyObject obj)
        {
            return (bool)obj.GetValue(Boolean1Property);
        }
        public static void SetBoolean1(DependencyObject obj, bool value)
        {
            obj.SetValue(Boolean1Property, value);
        }
        public static readonly DependencyProperty Boolean1Property = DependencyProperty.RegisterAttached("Boolean1", typeof(bool), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Boolean2
        public static bool GetBoolean2(DependencyObject obj)
        {
            return (bool)obj.GetValue(Boolean2Property);
        }
        public static void SetBoolean2(DependencyObject obj, bool value)
        {
            obj.SetValue(Boolean2Property, value);
        }
        public static readonly DependencyProperty Boolean2Property = DependencyProperty.RegisterAttached("Boolean2", typeof(bool), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Boolean3
        public static bool GetBoolean3(DependencyObject obj)
        {
            return (bool)obj.GetValue(Boolean3Property);
        }
        public static void SetBoolean3(DependencyObject obj, bool value)
        {
            obj.SetValue(Boolean3Property, value);
        }
        public static readonly DependencyProperty Boolean3Property = DependencyProperty.RegisterAttached("Boolean3", typeof(bool), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Boolean4
        public static bool GetBoolean4(DependencyObject obj)
        {
            return (bool)obj.GetValue(Boolean4Property);
        }
        public static void SetBoolean4(DependencyObject obj, bool value)
        {
            obj.SetValue(Boolean4Property, value);
        }
        public static readonly DependencyProperty Boolean4Property = DependencyProperty.RegisterAttached("Boolean4", typeof(bool), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Boolean5
        public static bool GetBoolean5(DependencyObject obj)
        {
            return (bool)obj.GetValue(Boolean5Property);
        }
        public static void SetBoolean5(DependencyObject obj, bool value)
        {
            obj.SetValue(Boolean5Property, value);
        }
        public static readonly DependencyProperty Boolean5Property = DependencyProperty.RegisterAttached("Boolean5", typeof(bool), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion

        #region Integer0
        public static int GetInteger0(DependencyObject obj)
        {
            return (int)obj.GetValue(Integer0Property);
        }
        public static void SetInteger0(DependencyObject obj, int value)
        {
            obj.SetValue(Integer0Property, value);
        }
        public static readonly DependencyProperty Integer0Property = DependencyProperty.RegisterAttached("Integer0", typeof(int), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Integer1
        public static int GetInteger1(DependencyObject obj)
        {
            return (int)obj.GetValue(Integer1Property);
        }
        public static void SetInteger1(DependencyObject obj, int value)
        {
            obj.SetValue(Integer1Property, value);
        }
        public static readonly DependencyProperty Integer1Property = DependencyProperty.RegisterAttached("Integer1", typeof(int), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Integer2
        public static int GetInteger2(DependencyObject obj)
        {
            return (int)obj.GetValue(Integer2Property);
        }
        public static void SetInteger2(DependencyObject obj, int value)
        {
            obj.SetValue(Integer2Property, value);
        }
        public static readonly DependencyProperty Integer2Property = DependencyProperty.RegisterAttached("Integer2", typeof(int), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Integer3
        public static int GetInteger3(DependencyObject obj)
        {
            return (int)obj.GetValue(Integer3Property);
        }
        public static void SetInteger3(DependencyObject obj, int value)
        {
            obj.SetValue(Integer3Property, value);
        }
        public static readonly DependencyProperty Integer3Property = DependencyProperty.RegisterAttached("Integer3", typeof(int), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Integer4
        public static int GetInteger4(DependencyObject obj)
        {
            return (int)obj.GetValue(Integer4Property);
        }
        public static void SetInteger4(DependencyObject obj, int value)
        {
            obj.SetValue(Integer4Property, value);
        }
        public static readonly DependencyProperty Integer4Property = DependencyProperty.RegisterAttached("Integer4", typeof(int), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Integer5
        public static int GetInteger5(DependencyObject obj)
        {
            return (int)obj.GetValue(Integer5Property);
        }
        public static void SetInteger5(DependencyObject obj, int value)
        {
            obj.SetValue(Integer5Property, value);
        }
        public static readonly DependencyProperty Integer5Property = DependencyProperty.RegisterAttached("Integer5", typeof(int), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion


        #region String0
        public static string GetString0(DependencyObject obj)
        {
            return (string)obj.GetValue(String0Property);
        }
        public static void SetString0(DependencyObject obj, string value)
        {
            obj.SetValue(String0Property, value);
        }
        public static readonly DependencyProperty String0Property = DependencyProperty.RegisterAttached("String0", typeof(string), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region String1
        public static string GetString1(DependencyObject obj)
        {
            return (string)obj.GetValue(String1Property);
        }
        public static void SetString1(DependencyObject obj, string value)
        {
            obj.SetValue(String1Property, value);
        }
        public static readonly DependencyProperty String1Property = DependencyProperty.RegisterAttached("String1", typeof(string), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region String2
        public static string GetString2(DependencyObject obj)
        {
            return (string)obj.GetValue(String2Property);
        }
        public static void SetString2(DependencyObject obj, string value)
        {
            obj.SetValue(String2Property, value);
        }
        public static readonly DependencyProperty String2Property = DependencyProperty.RegisterAttached("String2", typeof(string), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region String3
        public static string GetString3(DependencyObject obj)
        {
            return (string)obj.GetValue(String3Property);
        }
        public static void SetString3(DependencyObject obj, string value)
        {
            obj.SetValue(String3Property, value);
        }
        public static readonly DependencyProperty String3Property = DependencyProperty.RegisterAttached("String3", typeof(string), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region String4
        public static string GetString4(DependencyObject obj)
        {
            return (string)obj.GetValue(String4Property);
        }
        public static void SetString4(DependencyObject obj, string value)
        {
            obj.SetValue(String4Property, value);
        }
        public static readonly DependencyProperty String4Property = DependencyProperty.RegisterAttached("String4", typeof(string), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region String5
        public static string GetString5(DependencyObject obj)
        {
            return (string)obj.GetValue(String5Property);
        }
        public static void SetString5(DependencyObject obj, string value)
        {
            obj.SetValue(String5Property, value);
        }
        public static readonly DependencyProperty String5Property = DependencyProperty.RegisterAttached("String5", typeof(string), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion

        #region DateTime0
        public static DateTime? GetDateTime0(DependencyObject obj)
        {
            return (DateTime?)obj.GetValue(DateTime0Property);
        }
        public static void SetDateTime0(DependencyObject obj, DateTime? value)
        {
            obj.SetValue(DateTime0Property, value);
        }
        public static readonly DependencyProperty DateTime0Property = DependencyProperty.RegisterAttached("DateTime0", typeof(DateTime?), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region DateTime1
        public static DateTime? GetDateTime1(DependencyObject obj)
        {
            return (DateTime?)obj.GetValue(DateTime1Property);
        }
        public static void SetDateTime1(DependencyObject obj, DateTime? value)
        {
            obj.SetValue(DateTime1Property, value);
        }
        public static readonly DependencyProperty DateTime1Property = DependencyProperty.RegisterAttached("DateTime1", typeof(DateTime?), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region DateTime2
        public static DateTime? GetDateTime2(DependencyObject obj)
        {
            return (DateTime?)obj.GetValue(DateTime2Property);
        }
        public static void SetDateTime2(DependencyObject obj, DateTime? value)
        {
            obj.SetValue(DateTime2Property, value);
        }
        public static readonly DependencyProperty DateTime2Property = DependencyProperty.RegisterAttached("DateTime2", typeof(DateTime?), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region DateTime3
        public static DateTime? GetDateTime3(DependencyObject obj)
        {
            return (DateTime?)obj.GetValue(DateTime3Property);
        }
        public static void SetDateTime3(DependencyObject obj, DateTime? value)
        {
            obj.SetValue(DateTime3Property, value);
        }
        public static readonly DependencyProperty DateTime3Property = DependencyProperty.RegisterAttached("DateTime3", typeof(DateTime?), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region DateTime4
        public static DateTime? GetDateTime4(DependencyObject obj)
        {
            return (DateTime?)obj.GetValue(DateTime4Property);
        }
        public static void SetDateTime4(DependencyObject obj, DateTime? value)
        {
            obj.SetValue(DateTime4Property, value);
        }
        public static readonly DependencyProperty DateTime4Property = DependencyProperty.RegisterAttached("DateTime4", typeof(DateTime?), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region DateTime5
        public static DateTime? GetDateTime5(DependencyObject obj)
        {
            return (DateTime?)obj.GetValue(DateTime5Property);
        }
        public static void SetDateTime5(DependencyObject obj, DateTime? value)
        {
            obj.SetValue(DateTime5Property, value);
        }
        public static readonly DependencyProperty DateTime5Property = DependencyProperty.RegisterAttached("DateTime5", typeof(DateTime?), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion

        #region Exception0
        public static Exception GetException0(DependencyObject obj)
        {
            return (Exception)obj.GetValue(Exception0Property);
        }
        public static void SetException0(DependencyObject obj, Exception value)
        {
            obj.SetValue(Exception0Property, value);
        }
        public static readonly DependencyProperty Exception0Property = DependencyProperty.RegisterAttached("Exception0", typeof(Exception), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Exception1
        public static Exception GetException1(DependencyObject obj)
        {
            return (Exception)obj.GetValue(Exception1Property);
        }
        public static void SetException1(DependencyObject obj, Exception value)
        {
            obj.SetValue(Exception1Property, value);
        }
        public static readonly DependencyProperty Exception1Property = DependencyProperty.RegisterAttached("Exception1", typeof(Exception), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Exception2
        public static Exception GetException2(DependencyObject obj)
        {
            return (Exception)obj.GetValue(Exception2Property);
        }
        public static void SetException2(DependencyObject obj, Exception value)
        {
            obj.SetValue(Exception2Property, value);
        }
        public static readonly DependencyProperty Exception2Property = DependencyProperty.RegisterAttached("Exception2", typeof(Exception), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Exception3
        public static Exception GetException3(DependencyObject obj)
        {
            return (Exception)obj.GetValue(Exception3Property);
        }
        public static void SetException3(DependencyObject obj, Exception value)
        {
            obj.SetValue(Exception3Property, value);
        }
        public static readonly DependencyProperty Exception3Property = DependencyProperty.RegisterAttached("Exception3", typeof(Exception), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Exception4
        public static Exception GetException4(DependencyObject obj)
        {
            return (Exception)obj.GetValue(Exception4Property);
        }
        public static void SetException4(DependencyObject obj, Exception value)
        {
            obj.SetValue(Exception4Property, value);
        }
        public static readonly DependencyProperty Exception4Property = DependencyProperty.RegisterAttached("Exception4", typeof(Exception), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Exception5
        public static Exception GetException5(DependencyObject obj)
        {
            return (Exception)obj.GetValue(Exception5Property);
        }
        public static void SetException5(DependencyObject obj, Exception value)
        {
            obj.SetValue(Exception5Property, value);
        }
        public static readonly DependencyProperty Exception5Property = DependencyProperty.RegisterAttached("Exception5", typeof(Exception), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion

        #region Object0
        public static object GetObject0(DependencyObject obj)
        {
            return (object)obj.GetValue(Object0Property);
        }
        public static void SetObject0(DependencyObject obj, object value)
        {
            obj.SetValue(Object0Property, value);
        }
        public static readonly DependencyProperty Object0Property = DependencyProperty.RegisterAttached("Object0", typeof(object), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Object1
        public static object GetObject1(DependencyObject obj)
        {
            return (object)obj.GetValue(Object1Property);
        }
        public static void SetObject1(DependencyObject obj, object value)
        {
            obj.SetValue(Object1Property, value);
        }
        public static readonly DependencyProperty Object1Property = DependencyProperty.RegisterAttached("Object1", typeof(object), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Object2
        public static object GetObject2(DependencyObject obj)
        {
            return (object)obj.GetValue(Object2Property);
        }
        public static void SetObject2(DependencyObject obj, object value)
        {
            obj.SetValue(Object2Property, value);
        }
        public static readonly DependencyProperty Object2Property = DependencyProperty.RegisterAttached("Object2", typeof(object), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Object3
        public static object GetObject3(DependencyObject obj)
        {
            return (object)obj.GetValue(Object3Property);
        }
        public static void SetObject3(DependencyObject obj, object value)
        {
            obj.SetValue(Object3Property, value);
        }
        public static readonly DependencyProperty Object3Property = DependencyProperty.RegisterAttached("Object3", typeof(object), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Object4
        public static object GetObject4(DependencyObject obj)
        {
            return (object)obj.GetValue(Object4Property);
        }
        public static void SetObject4(DependencyObject obj, object value)
        {
            obj.SetValue(Object4Property, value);
        }
        public static readonly DependencyProperty Object4Property = DependencyProperty.RegisterAttached("Object4", typeof(object), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion
        #region Object5
        public static object GetObject5(DependencyObject obj)
        {
            return (object)obj.GetValue(Object5Property);
        }
        public static void SetObject5(DependencyObject obj, object value)
        {
            obj.SetValue(Object5Property, value);
        }
        public static readonly DependencyProperty Object5Property = DependencyProperty.RegisterAttached("Object5", typeof(object), typeof(AttachedProperties), new UIPropertyMetadata());
        #endregion

        #region Color
        public static Color GetColor(DependencyObject obj)
        {
            return (Color)obj.GetValue(ColorProperty);
        }
        public static void SetColor(DependencyObject obj, Color value)
        {
            obj.SetValue(ColorProperty, value);
        }
        public static readonly DependencyProperty ColorProperty = DependencyProperty.RegisterAttached("Color", typeof(Color), typeof(AttachedProperties), new UIPropertyMetadata(default(Color)));
        #endregion
        #region Color0
        public static Color GetColor0(DependencyObject obj)
        {
            return (Color)obj.GetValue(Color0Property);
        }
        public static void SetColor0(DependencyObject obj, Color value)
        {
            obj.SetValue(Color0Property, value);
        }
        public static readonly DependencyProperty Color0Property = DependencyProperty.RegisterAttached("Color0", typeof(Color), typeof(AttachedProperties), new UIPropertyMetadata(default(Color)));
        #endregion
        #region Brush0
        public static Brush GetBrush0(DependencyObject obj)
        {
            return (Brush)obj.GetValue(Brush0Property);
        }
        public static void SetBrush0(DependencyObject obj, Brush value)
        {
            obj.SetValue(Brush0Property, value);
        }
        public static readonly DependencyProperty Brush0Property = DependencyProperty.RegisterAttached("Brush0", typeof(Brush), typeof(AttachedProperties), new UIPropertyMetadata(default(Brush)));
        #endregion
    }
}
