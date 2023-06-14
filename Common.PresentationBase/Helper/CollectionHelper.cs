#region using
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data; 
#endregion

namespace Common
{
    #region CollectionHelper
    public static class CollectionHelper
    {
        #region Clone
        public static R Clone<R>(IEnumerable list) where R : IList, new()
        {
            R newList = new R();
            if (list != null)
            {
                foreach (var item in list)
                {
                    newList.Add(item);
                }
            }
            return newList;
        }
        #endregion
        #region Clone
        public static R Clone<R>(IEnumerable list, bool preserveNull) where R : IList, new()
        {
            R retList = default(R);
            if (list != null)
            {
                retList = new R();
                foreach (var item in list)
                {
                    retList.Add(item);
                }
            }
            return retList;
        }
        #endregion
        #region Copy
        public static IList Copy(IList listT, IEnumerable listS)
        {
            if (listT == null) return null;
            listT.Clear();
            if (listS != null)
            {
                foreach (var item in listS)
                {
                    listT.Add(item);
                }
            }
            return listT;
        }
        #endregion

        #region ReplaceItem
        public static bool ReplaceItem<T>(IList<T> list, T item, T replace)
        {
            if (list == null) { return false; }
            var index = list.IndexOf(item);
            if (index >= 0)
            {
                list.RemoveAt(index);
                list.Insert(index, replace);
                return true;
            }
            return false;
        }
        #endregion
        #region ReplaceItem
        public static bool ReplaceItem(IList list, object item, object replace)
        {
            if (list == null) { return false; }
            var index = list.IndexOf(item);
            if (index >= 0)
            {
                list.RemoveAt(index);
                list.Insert(index, replace);
                return true;
            }
            return false;
        }
        #endregion

        #region FilterItems
        public static IList<T> FilterItems<T>(IList<T> all, IList<T> include, IList<T> exclude, IList<T> coll)
        {
            if (all != null)
            {
                if (coll != null)
                {
                    List<T> removeList = new List<T>();
                    foreach (var item in coll)
                    {
                        if (!all.Contains(item)) { removeList.Add(item); }
                    }
                    if (removeList != null)
                    {
                        foreach (var item in removeList)
                        {
                            coll.Remove(item);
                        }
                    }
                }
                foreach (var item in all)
                {
                    if (include == null || include.Contains(item))
                    {
                        if (exclude == null || !exclude.Contains(item))
                        {
                            if (coll == null) { coll = new ObservableCollection<T>(); }
                            if (!coll.Contains(item)) { coll.Add(item); }
                        }
                        else { if (coll != null && coll.Contains(item)) { coll.Remove(item); } }
                    }
                    else { if (coll != null && coll.Contains(item)) { coll.Remove(item); } }
                }
            }
            else { if (coll != null) { coll.Clear(); } }
            return coll;
        }
        #endregion

        #region SetDefaultViewCurrent
        public static void SetDefaultViewCurrent(IEnumerable coll, object current)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(coll); if (view != null) { view.MoveCurrentTo(current); }
        }
        #endregion

        #region PropertyRegisterCollectionChanged
        public static void PropertyRegisterCollectionChanged(DependencyPropertyChangedEventArgs e, NotifyCollectionChangedEventHandler del)
        {
            INotifyCollectionChanged oldColl = e.OldValue as INotifyCollectionChanged;
            if (oldColl != null) { oldColl.CollectionChanged -= new NotifyCollectionChangedEventHandler(del); }
            INotifyCollectionChanged newColl = e.NewValue as INotifyCollectionChanged;
            if (newColl != null) { newColl.CollectionChanged += new NotifyCollectionChangedEventHandler(del); }
            return;
        }
        #endregion
        #region PropertyRegisterCollectionChanged
        public static void PropertyRegisterCollectionChanged(IEnumerable oldValue, IEnumerable newValue, NotifyCollectionChangedEventHandler del)
        {
            INotifyCollectionChanged oldColl = oldValue as INotifyCollectionChanged;
            if (oldColl != null) { oldColl.CollectionChanged -= new NotifyCollectionChangedEventHandler(del); }
            INotifyCollectionChanged newColl = newValue as INotifyCollectionChanged;
            if (newColl != null) { newColl.CollectionChanged += new NotifyCollectionChangedEventHandler(del); }
            return;
        }
        #endregion
    }
    #endregion
}
