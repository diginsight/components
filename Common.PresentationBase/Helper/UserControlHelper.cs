#region using
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
#endregion

namespace Common
{
    #region UserControlHelper
    public class UserControlHelper
    {
        #region internal state
        FrameworkElement Source { get; set; }
        //Segnalazioni SegnalazioniSeverity { get; set; }
        #endregion
        #region .ctor
        public UserControlHelper() { }
        public UserControlHelper(FrameworkElement source) // , ABCServerHelper.Segnalazioni segnalazioniSeverity
        {
            this.Source = source;
            //this.SegnalazioniSeverity = segnalazioniSeverity;
        }
        #endregion

        #region FindOwningWindow
        public static Window FindOwningWindow(FrameworkElement element)
        {
            FrameworkElement owningWindow = element;
            while (owningWindow != null)
            {
                if (owningWindow is Window) { return owningWindow as Window; }
                owningWindow = (owningWindow.Parent ?? VisualTreeHelper.GetParent(owningWindow)) as FrameworkElement;
            }
            return owningWindow as Window;
        }
        #endregion

        #region FindAncestor
        public static T FindAncestor<T>(FrameworkElement element) where T : class
        {
            return FindAncestor<T>(element, false);
        }
        public static T FindAncestor<T>(FrameworkElement element, bool skipFirst) where T : class
        {
            FrameworkElement fe = element;
            bool first = true;
            while (fe != null)
            {
                if (!first || skipFirst) { fe = (fe.Parent ?? VisualTreeHelper.GetParent(fe)) as FrameworkElement; }
                first = false;
                if (fe is T) { return fe as T; }
            }
            return fe as T;
        }
        public static T FindAncestor<T>(FrameworkElement element, Func<T, bool> selector) where T : FrameworkElement
        {
            return FindAncestor<T>(element, selector, true);
        }
        public static T FindAncestor<T>(FrameworkElement element, Func<T, bool> selector, bool skipFirst) where T : FrameworkElement
        {
            FrameworkElement fe = element;
            bool first = true;
            while (fe != null)
            {
                if (!first || skipFirst) { fe = (fe.Parent ?? VisualTreeHelper.GetParent(fe)) as FrameworkElement; }
                first = false;
                if (fe is T && selector(fe as T)) { return fe as T; }
            }
            return fe as T;
        }
        #endregion
        #region FindVisualAncestor
        public static T FindVisualAncestor<T>(FrameworkElement element) where T : class { return FindVisualAncestor<T>(element, false); }
        public static T FindVisualAncestor<T>(FrameworkElement element, bool skipFirst) where T : class
        {
            FrameworkElement fe = element;
            bool first = true;
            while (fe != null)
            {
                if (!first || skipFirst) { fe = VisualTreeHelper.GetParent(fe) as FrameworkElement; }
                first = false;
                if (fe is T) { return fe as T; }
            }
            return fe as T;
        }
        public static T FindVisualAncestor<T>(FrameworkElement element, Func<T, bool> selector) where T : FrameworkElement
        {
            FrameworkElement fe = element;
            while (fe != null)
            {
                fe = FindVisualAncestor<T>(fe, true);
                if (fe is T && selector(fe as T)) { return fe as T; }
            }
            return fe as T;
        }
        #endregion

        #region FindDescendant
        public static T FindDescendant<T>(FrameworkElement element) where T : class
        {
            return FindDescendant<T>(element, 100);
        }
        public static T FindDescendant<T>(FrameworkElement element, int depth) where T : class
        {
            return FindDescendant<T>(element, depth, null);
        }
        public static T FindDescendant<T>(FrameworkElement element, int depth, string name) where T : class
        {
            FrameworkElement fe = element;
            FrameworkElement feChild;
            T childDescendant;
            if (depth == 0) return null;
            int nChilds = 0;
            nChilds = VisualTreeHelper.GetChildrenCount(fe);
            if (nChilds > 0)
            {
                for (int i = 0; i < nChilds; i++)
                {
                    feChild = VisualTreeHelper.GetChild(fe, i) as FrameworkElement;
                    if (feChild is T)
                    {
                        if (name == null || feChild.Name == name)
                            return feChild as T;
                    }
                }
                for (int i = 0; i < nChilds; i++)
                {
                    feChild = VisualTreeHelper.GetChild(fe, i) as FrameworkElement;
                    if (feChild != null)
                    {
                        childDescendant = FindDescendant<T>(feChild, depth - 1);
                        if (childDescendant != null)
                        {
                            return childDescendant;
                        }
                    }
                }
            }
            return null;
        }
        #endregion
        #region FindDescendants
        public static List<T> FindDescendants<T>(FrameworkElement element) where T : class
        {
            return FindDescendants<T>(element, 100);
        }
        public static List<T> FindDescendants<T>(FrameworkElement element, int depth) where T : class
        {
            return FindDescendants<T>(element, depth, null);
        }
        public static List<T> FindDescendants<T>(FrameworkElement element, int depth, string name) where T : class
        {
            List<T> ret = new List<T>();
            FrameworkElement fe = element;
            FrameworkElement feChild;
            if (depth == 0) return null;
            int nChilds = 0;
            nChilds = VisualTreeHelper.GetChildrenCount(fe);
            if (nChilds > 0)
            {
                for (int i = 0; i < nChilds; i++)
                {
                    feChild = VisualTreeHelper.GetChild(fe, i) as FrameworkElement;
                    if (feChild is T)
                    {
                        if (name == null || feChild.Name == name)
                            ret.Add(feChild as T);
                    }
                }
                for (int i = 0; i < nChilds; i++)
                {
                    feChild = VisualTreeHelper.GetChild(fe, i) as FrameworkElement;
                    if (feChild != null)
                    {
                        List<T> results = FindDescendants<T>(feChild, depth - 1);
                        ret.AddRange(results);
                    }
                }
            }
            return ret;
        }
        #endregion
        #region UpdateTargets
        public static void UpdateTargets(BindingGroup bg)
        {
            if (bg.BindingExpressions != null)
            {
                Collection<BindingExpressionBase> beColl = CollectionHelper.Clone<Collection<BindingExpressionBase>>(bg.BindingExpressions);
                foreach (BindingExpressionBase be in beColl) { try { be.UpdateTarget(); } catch (NullReferenceException) { } }
            }
        }
        public static void UpdateTargets(BindingGroup bg, BindingExpression excludeExpression)
        {
            if (bg.BindingExpressions != null)
            {
                Collection<BindingExpressionBase> beColl = CollectionHelper.Clone<Collection<BindingExpressionBase>>(bg.BindingExpressions);
                foreach (BindingExpressionBase be in beColl)
                {
                    if (be == excludeExpression) { continue; }// be.HasError || 
                    try { be.UpdateTarget(); } catch (NullReferenceException) { }
                }
            }
        }
        #endregion
        #region XamlSave
        public static void XamlSave(object control, string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine(XamlWriter.Save(control));
            }
        }
        #endregion

        #region GetIsInDesignMode
        public static bool GetIsInDesignMode(DependencyObject source)
        {
            if (source == null) { return false; }
            if (Thread.CurrentThread.IsBackground == false && Thread.CurrentThread.IsThreadPoolThread == false) { return DesignerProperties.GetIsInDesignMode(source); }

            bool ret = false;
            Dispatcher dispatcher = source != null ? source.Dispatcher : ApplicationBase.Current.Dispatcher;
            dispatcher.Invoke(new Action(delegate () {
                ret = DesignerProperties.GetIsInDesignMode(source);
            }), DispatcherPriority.ApplicationIdle, null);

            return ret;
        }
        #endregion

        #region Clone
        public static T Clone<T>(T control) where T : class
        {
            object clone = null;
            string xaml = XamlWriter.Save(control);
            StringReader stringReader = new StringReader(xaml);
            XmlReader xamlReader = XmlReader.Create(stringReader);
            clone = System.Windows.Markup.XamlReader.Load(xamlReader);

            return clone as T;
        }
        #endregion

        //#region GESTIONE DELEGATE
        //#region TipoDelegateCall
        //public DelegateCallEnum TipoDelegateCall { get; set; }
        //#endregion
        //#endregion

        #region SetAndNotifyChange
        public static void SetAndNotifyChange<T>(INotifyPropertyChanged pthis, PropertyChangedEventHandler del, string prop, ref T field, T value)
        {
            field = value;
            if (del != null) { del(pthis, new PropertyChangedEventArgs(prop)); }
        }
        #endregion
    }
    #endregion
}
