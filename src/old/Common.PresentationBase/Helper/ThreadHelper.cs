#region using
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
#endregion

namespace Common
{
    public static class EnumHelper
    {
        public static string ToDescription(this Enum value)
        {
            Type type = value.GetType();

            MemberInfo[] memInfo = type.GetMember(value.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }

            return value.ToString();
        }
        //public static string ToDisplay(this Enum value)
        //{
        //    Type type = value.GetType();

        //    MemberInfo[] memInfo = type.GetMember(value.ToString());
        //    if (memInfo != null && memInfo.Length > 0)
        //    {
        //        object[] attrs = memInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);

        //        if (attrs != null && attrs.Length > 0)
        //            return ((DisplayAttribute)attrs[0]).Name;
        //    }

        //    return value.ToString();
        //}


        public static string ToCategory(this Enum value)
        {
            Type type = value.GetType();

            MemberInfo[] memInfo = type.GetMember(value.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(CategoryAttribute), false);

                if (attrs != null && attrs.Length > 0)
                    return ((CategoryAttribute)attrs[0]).Category;
            }

            return value.ToString();
        }
    }
    public static class BitHelper
    {
        public static int FirstOneIndex(this int value)
        {
            for (int i = 0; i < 32; i++)
                if ((value & (1 << i)) != 0)
                    return i;

            return -1;
        }

        public static int OneCount(byte b)
        {
            int count = 0;

            for (int i = 0; i < 8; i++)
                if ((b & (1 << i)) != 0)
                    count++;

            return count;
        }
    }
    public static class ThreadHelper
    {
        public static void SynchronizedInvoke(this Control ctrl, Action action)
        {
            if (ctrl.InvokeRequired)
            {
                ctrl.BeginInvoke(action, new object[] { });
            }
            else
            {
                action();
            }
        }

        public static void SendOrPostToUiThread(Action action)
        {

            if (Thread.CurrentThread.IsBackground == true)
            {
                var dispatcher = System.Windows.Application.Current.Dispatcher;
                dispatcher?.Invoke(action);
            }
            else
                action();
        }
    }

}
