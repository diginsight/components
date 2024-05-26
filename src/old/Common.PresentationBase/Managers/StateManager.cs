#region using
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Shapes;
#endregion

namespace Common
{
    #region IClassStateProvider
    public interface IClassStateProvider<T, S>
    {
        #region Initialize
        void Attch(T t, S s);
        #endregion
        #region GetState
        S GetState();
        #endregion
        #region RestoreState
        void RestoreState();
        #endregion
    }
    #endregion
    #region ClassStateProvider<T, S>
    public abstract class ClassStateProvider<T, S> : IClassStateProvider<T, S>
    {
        #region delegaes
        public delegate S t2s(T t);
        public delegate T s2t(T t, S s);
        #endregion

        #region T2S
        public t2s True2Surrogate { get; set; }
        #endregion
        #region S2T
        public s2t Surrogate2True { get; set; }
        #endregion

        #region S
        protected S State { get; set; }
        #endregion
        #region T
        protected T Instance { get; set; }
        #endregion

        #region Initialize
        public void Attch(T t, S s) { this.Instance = t; this.State = s; }
        #endregion
        #region GetState
        public S GetState() { this.State = True2Surrogate(this.Instance); return this.State; }
        #endregion
        #region RestoreState
        public void RestoreState() { Surrogate2True(this.Instance, this.State); return; }
        #endregion
    }
    #endregion

    #region PositionState
    public class PositionState
    {
        #region Top
        public double Top { get; set; }
        #endregion
        #region Left
        public double Left { get; set; }
        #endregion
        #region Width
        public double Width { get; set; }
        #endregion
        #region Height
        public double Height { get; set; }
        #endregion
        #region WindowState
        public WindowState WindowState { get; set; }
        #endregion
    }
    #endregion
    #region PositionStateProvider
    [DataContract]
    public class PositionStateProvider : ClassStateProvider<Window, PositionState>
    {
        #region .ctor
        public PositionStateProvider() {
            this.True2Surrogate = delegate (Window t)
            {
                PositionState state = new PositionState() { Top = t.Top, Left = t.Left, Width = t.Width, Height = t.Height, WindowState = t.WindowState };
                return state;
            };

            this.Surrogate2True = delegate (Window t, PositionState s)
            {
                if (t == null) { t = new Window(); }
                // check for existing monitors 
                // check for topleft existence 
                // if !Existing translate into primary monitor 

                var position = new System.Drawing.Rectangle((int)s.Left, (int)s.Top, (int)s.Width, (int)s.Height);
                if (!IsVisibleOnAnyScreen(position))
                {
                    //var startPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation;
                    var primaryScreen = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(scr => scr.Primary == true);
                    position = new System.Drawing.Rectangle(primaryScreen.WorkingArea.X + 200, primaryScreen.WorkingArea.Y + 100, (int)s.Width, (int)s.Height);
                }

                if (IsVisibleOnAnyScreen(position))
                {
                    t.Top = position.Top;
                    t.Left = position.Left;
                    t.Width = position.Width;
                    t.Height = position.Height;
                    t.WindowState = s.WindowState;
                }

                return t;
            };
        }
        #endregion

        private bool IsVisibleOnAnyScreen(System.Drawing.Rectangle rect)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.IntersectsWith(rect))
                {
                    return true;
                }
            }

            return false;
        }
    }
    #endregion

    #region ZoomState
    public class ZoomState
    {
        #region Zoom
        public double Zoom { get; set; }
        public double ZoomWebBrowser { get; set; }
        #endregion
    }
    #endregion
    #region ZoomStateProvider
    public class ZoomStateProvider : ClassStateProvider<ApplicationBase, ZoomState>
    {
        #region .ctor
        public ZoomStateProvider()
        {
            this.True2Surrogate = delegate (ApplicationBase t)
            {
                ZoomState state = new ZoomState()
                {
                    Zoom = t.Zoom,
                };
                return state;
            };
            this.Surrogate2True = delegate (ApplicationBase t, ZoomState s)
            {
                if (t == null) { return t; }
                t.Zoom = s.Zoom;
                return t;
            };
        }
        #endregion
    }
    #endregion

    #region ObservableDictionary
    [Serializable]
    public class ObservableDictionary<TKey, TValue> :
        IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>,
        IDictionary, ICollection, IEnumerable,
        ISerializable, IDeserializationCallback, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region KeyedDictionaryEntryCollection
        protected class KeyedDictionaryEntryCollection : KeyedCollection<TKey, DictionaryEntry>
        {
            #region .ctors
            public KeyedDictionaryEntryCollection() : base() { }
            public KeyedDictionaryEntryCollection(IEqualityComparer<TKey> comparer) : base(comparer) { }
            #endregion
            #region GetKeyForItem
            protected override TKey GetKeyForItem(DictionaryEntry entry)
            {
                return (TKey)entry.Key;
            }
            #endregion
        }
        #endregion
        #region Enumerator
        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IDictionaryEnumerator, IEnumerator
        {
            #region .ctor
            internal Enumerator(ObservableDictionary<TKey, TValue> dictionary, bool isDictionaryEntryEnumerator)
            {
                _dictionary = dictionary;
                _version = dictionary._version;
                _index = -1;
                _isDictionaryEntryEnumerator = isDictionaryEntryEnumerator;
                _current = new KeyValuePair<TKey, TValue>();
            }
            #endregion

            #region fields
            private ObservableDictionary<TKey, TValue> _dictionary;
            private int _version;
            private int _index;
            private KeyValuePair<TKey, TValue> _current;
            private bool _isDictionaryEntryEnumerator;
            #endregion fields

            #region Current
            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    ValidateCurrent();
                    return _current;
                }
            }
            #endregion public

            #region Dispose
            public void Dispose() { }
            #endregion
            #region MoveNext
            public bool MoveNext()
            {
                ValidateVersion();
                _index++;
                if (_index < _dictionary._keyedEntryCollection.Count)
                {
                    _current = new KeyValuePair<TKey, TValue>((TKey)_dictionary._keyedEntryCollection[_index].Key, (TValue)_dictionary._keyedEntryCollection[_index].Value);
                    return true;
                }
                _index = -2;
                _current = new KeyValuePair<TKey, TValue>();
                return false;
            }
            #endregion
            #region ValidateCurrent
            private void ValidateCurrent()
            {
                if (_index == -1)
                {
                    throw new InvalidOperationException("The enumerator has not been started.");
                }
                else if (_index == -2)
                {
                    throw new InvalidOperationException("The enumerator has reached the end of the collection.");
                }
            }
            #endregion
            #region ValidateVersion
            private void ValidateVersion()
            {
                if (_version != _dictionary._version)
                {
                    throw new InvalidOperationException("The enumerator is not valid because the dictionary changed.");
                }
            }
            #endregion

            #region IEnumerator
            object IEnumerator.Current
            {
                get
                {
                    ValidateCurrent();
                    if (_isDictionaryEntryEnumerator)
                    {
                        return new DictionaryEntry(_current.Key, _current.Value);
                    }
                    return new KeyValuePair<TKey, TValue>(_current.Key, _current.Value);
                }
            }
            void IEnumerator.Reset()
            {
                ValidateVersion();
                _index = -1;
                _current = new KeyValuePair<TKey, TValue>();
            }
            #endregion IEnumerator implemenation
            #region IDictionaryEnumerator
            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    ValidateCurrent();
                    return new DictionaryEntry(_current.Key, _current.Value);
                }
            }
            object IDictionaryEnumerator.Key
            {
                get
                {
                    ValidateCurrent();
                    return _current.Key;
                }
            }
            object IDictionaryEnumerator.Value
            {
                get
                {
                    ValidateCurrent();
                    return _current.Value;
                }
            }
            #endregion
        }
        #endregion

        #region internal state
        protected KeyedDictionaryEntryCollection _keyedEntryCollection;
        private Dictionary<TKey, TValue> _dictionaryCache = new Dictionary<TKey, TValue>();
        private int _countCache = 0;
        private int _dictionaryCacheVersion = 0;
        private int _version = 0;
        [NonSerialized]
        private SerializationInfo _siInfo = null;
        #endregion
        #region .ctor
        #region .ctor
        public ObservableDictionary()
        {
            _keyedEntryCollection = new KeyedDictionaryEntryCollection();
        }
        #endregion
        #region .ctor IDictionary
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _keyedEntryCollection = new KeyedDictionaryEntryCollection();

            foreach (KeyValuePair<TKey, TValue> entry in dictionary)
            {
                DoAddEntry((TKey)entry.Key, (TValue)entry.Value);
            }
        }
        #endregion
        #region .ctor IEqualityComparer
        public ObservableDictionary(IEqualityComparer<TKey> comparer)
        {
            _keyedEntryCollection = new KeyedDictionaryEntryCollection(comparer);
        }
        #endregion
        #region .ctor ObservableDictionary, IEqualityComparer
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            _keyedEntryCollection = new KeyedDictionaryEntryCollection(comparer);

            foreach (KeyValuePair<TKey, TValue> entry in dictionary)
            {
                DoAddEntry((TKey)entry.Key, (TValue)entry.Value);
            }
        }
        #endregion
        #region .ctor SerializationInfo, StreamingContext
        protected ObservableDictionary(SerializationInfo info, StreamingContext context)
        {
            _siInfo = info;
        }
        #endregion
        #endregion

        #region TrueDictionary
        private Dictionary<TKey, TValue> TrueDictionary
        {
            get
            {
                if (_dictionaryCacheVersion != _version)
                {
                    _dictionaryCache.Clear();
                    foreach (DictionaryEntry entry in _keyedEntryCollection)
                    {
                        _dictionaryCache.Add((TKey)entry.Key, (TValue)entry.Value);
                    }
                    _dictionaryCacheVersion = _version;
                }
                return _dictionaryCache;
            }
        }
        #endregion

        #region properties
        #region Comparer
        public IEqualityComparer<TKey> Comparer
        {
            get { return _keyedEntryCollection.Comparer; }
        }
        #endregion
        #region Count
        public int Count
        {
            get { return _keyedEntryCollection.Count; }
        }
        #endregion
        #region Keys
        public Dictionary<TKey, TValue>.KeyCollection Keys
        {
            get { return TrueDictionary.Keys; }
        }
        #endregion
        #region this
        public TValue this[TKey key]
        {
            get { return (TValue)_keyedEntryCollection[key].Value; }
            set { DoSetEntry(key, value); }
        }
        #endregion
        #region Values
        public Dictionary<TKey, TValue>.ValueCollection Values
        {
            get { return TrueDictionary.Values; }
        }
        #endregion
        #endregion

        #region private methods
        #region DoAddEntry
        private void DoAddEntry(TKey key, TValue value)
        {
            if (AddEntry(key, value))
            {
                _version++;
                DictionaryEntry entry;
                int index = GetIndexAndEntryForKey(key, out entry);
                FireEntryAddedNotifications(entry, index);
            }
        }
        #endregion
        #region DoClearEntries
        private void DoClearEntries()
        {
            if (ClearEntries())
            {
                _version++;
                FireResetNotifications();
            }
        }
        #endregion
        #region DoRemoveEntry
        private bool DoRemoveEntry(TKey key)
        {
            DictionaryEntry entry;
            int index = GetIndexAndEntryForKey(key, out entry);
            bool result = RemoveEntry(key);
            if (result)
            {
                _version++;
                if (index > -1)
                {
                    FireEntryRemovedNotifications(entry, index);
                }
            }
            return result;
        }
        #endregion
        #region DoSetEntry
        private void DoSetEntry(TKey key, TValue value)
        {
            DictionaryEntry entry;
            int index = GetIndexAndEntryForKey(key, out entry);
            if (SetEntry(key, value))
            {
                _version++;
                // if prior entry existed for this key, fire the removed notifications
                if (index > -1)
                {
                    FireEntryRemovedNotifications(entry, index);
                }
                // then fire the added notifications
                index = GetIndexAndEntryForKey(key, out entry);
                FireEntryAddedNotifications(entry, index);
            }
        }
        #endregion
        #region FireEntryAddedNotifications
        private void FireEntryAddedNotifications(DictionaryEntry entry, int index)
        {
            // fire the relevant PropertyChanged notifications
            FirePropertyChangedNotifications();
            // fire CollectionChanged notification
            if (index > -1)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, entry, index));
            }
            else
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        #endregion
        #region FireEntryRemovedNotifications
        private void FireEntryRemovedNotifications(DictionaryEntry entry, int index)
        {
            // fire the relevant PropertyChanged notifications
            FirePropertyChangedNotifications();
            // fire CollectionChanged notification
            if (index > -1)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, entry, index));
            }
            else
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        #endregion
        #region FirePropertyChangedNotifications
        private void FirePropertyChangedNotifications()
        {
            if (Count != _countCache)
            {
                _countCache = Count;
                OnPropertyChanged("Count");
                OnPropertyChanged("Keys");
                OnPropertyChanged("Values");
            }
        }
        #endregion
        #region FireResetNotifications
        private void FireResetNotifications()
        {
            // fire the relevant PropertyChanged notifications
            FirePropertyChangedNotifications();
            // fire CollectionChanged notification
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        #endregion
        #endregion

        #region methods
        #region Add
        public void Add(TKey key, TValue value)
        {
            DoAddEntry(key, value);
        }
        #endregion
        #region Clear
        public void Clear()
        {
            DoClearEntries();
        }
        #endregion
        #region ContainsKey
        public bool ContainsKey(TKey key)
        {
            return _keyedEntryCollection.Contains(key);
        }
        #endregion
        #region ContainsValue
        public bool ContainsValue(TValue value)
        {
            return TrueDictionary.ContainsValue(value);
        }
        #endregion
        #region GetEnumerator
        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this, false);
        }
        #endregion
        #region Remove
        public bool Remove(TKey key)
        {
            return DoRemoveEntry(key);
        }
        #endregion
        #region TryGetValue
        public bool TryGetValue(TKey key, out TValue value)
        {
            bool result = _keyedEntryCollection.Contains(key);
            value = result ? (TValue)_keyedEntryCollection[key].Value : default(TValue);
            return result;
        }
        #endregion

        #region AddEntry
        protected virtual bool AddEntry(TKey key, TValue value)
        {
            _keyedEntryCollection.Add(new DictionaryEntry(key, value));
            return true;
        }
        #endregion
        #region ClearEntries
        protected virtual bool ClearEntries()
        {
            // check whether there are entries to clear
            bool result = (Count > 0);
            if (result)
            {
                // if so, clear the dictionary
                _keyedEntryCollection.Clear();
            }
            return result;
        }
        #endregion
        #region GetIndexAndEntryForKey
        protected int GetIndexAndEntryForKey(TKey key, out DictionaryEntry entry)
        {
            entry = default(DictionaryEntry);
            int index = -1;
            if (_keyedEntryCollection.Contains(key))
            {
                entry = _keyedEntryCollection[key];
                index = _keyedEntryCollection.IndexOf(entry);
            }
            return index;
        }
        #endregion
        #region OnCollectionChanged
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }
        #endregion
        #region OnPropertyChanged
        protected virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
        #endregion
        #region RemoveEntry
        protected virtual bool RemoveEntry(TKey key)
        {
            // remove the entry
            return _keyedEntryCollection.Remove(key);
        }
        #endregion
        #region SetEntry
        protected virtual bool SetEntry(TKey key, TValue value)
        {
            bool keyExists = _keyedEntryCollection.Contains(key);

            // if identical key/value pair already exists, nothing to do
            if (keyExists && value.Equals((TValue)_keyedEntryCollection[key].Value))
                return false;

            // otherwise, remove the existing entry
            if (keyExists)
                _keyedEntryCollection.Remove(key);

            // add the new entry
            _keyedEntryCollection.Add(new DictionaryEntry(key, value));

            return true;
        }
        #endregion
        #endregion

        #region interfaces
        #region IDictionary<TKey, TValue>
        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            DoAddEntry(key, value);
        }
        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            return DoRemoveEntry(key);
        }
        bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
        {
            return _keyedEntryCollection.Contains(key);
        }
        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
        {
            return TryGetValue(key, out value);
        }
        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get { return Keys; }
        }
        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get { return Values; }
        }
        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get { return (TValue)_keyedEntryCollection[key].Value; }
            set { DoSetEntry(key, value); }
        }
        #endregion IDictionary<TKey, TValue>
        #region IDictionary
        void IDictionary.Add(object key, object value)
        {
            DoAddEntry((TKey)key, (TValue)value);
        }
        void IDictionary.Clear()
        {
            DoClearEntries();
        }
        bool IDictionary.Contains(object key)
        {
            return _keyedEntryCollection.Contains((TKey)key);
        }
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new Enumerator(this, true);
        }
        bool IDictionary.IsFixedSize
        {
            get { return false; }
        }
        bool IDictionary.IsReadOnly
        {
            get { return false; }
        }
        object IDictionary.this[object key]
        {
            get { return _keyedEntryCollection[(TKey)key].Value; }
            set { DoSetEntry((TKey)key, (TValue)value); }
        }
        ICollection IDictionary.Keys
        {
            get { return Keys; }
        }
        void IDictionary.Remove(object key)
        {
            DoRemoveEntry((TKey)key);
        }
        ICollection IDictionary.Values
        {
            get { return Values; }
        }
        #endregion
        #region ICollection<KeyValuePair<TKey, TValue>>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> kvp)
        {
            DoAddEntry(kvp.Key, kvp.Value);
        }
        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            DoClearEntries();
        }
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> kvp)
        {
            return _keyedEntryCollection.Contains(kvp.Key);
        }
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("CopyTo() failed:  array parameter was null");
            }
            if ((index < 0) || (index > array.Length))
            {
                throw new ArgumentOutOfRangeException("CopyTo() failed:  index parameter was outside the bounds of the supplied array");
            }
            if ((array.Length - index) < _keyedEntryCollection.Count)
            {
                throw new ArgumentException("CopyTo() failed:  supplied array was too small");
            }

            foreach (DictionaryEntry entry in _keyedEntryCollection)
                array[index++] = new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);
        }
        int ICollection<KeyValuePair<TKey, TValue>>.Count
        {
            get { return _keyedEntryCollection.Count; }
        }
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> kvp)
        {
            return DoRemoveEntry(kvp.Key);
        }
        #endregion
        #region ICollection
        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_keyedEntryCollection).CopyTo(array, index);
        }
        int ICollection.Count
        {
            get { return _keyedEntryCollection.Count; }
        }
        bool ICollection.IsSynchronized
        {
            get { return ((ICollection)_keyedEntryCollection).IsSynchronized; }
        }
        object ICollection.SyncRoot
        {
            get { return ((ICollection)_keyedEntryCollection).SyncRoot; }
        }
        #endregion
        #region IEnumerable<KeyValuePair<TKey, TValue>>
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator(this, false);
        }
        #endregion
        #region IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
        #region ISerializable
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            Collection<DictionaryEntry> entries = new Collection<DictionaryEntry>();
            foreach (DictionaryEntry entry in _keyedEntryCollection)
                entries.Add(entry);
            info.AddValue("entries", entries);
        }
        #endregion ISerializable
        #region IDeserializationCallback
        public virtual void OnDeserialization(object sender)
        {
            if (_siInfo != null)
            {
                Collection<DictionaryEntry> entries = (Collection<DictionaryEntry>)
                    _siInfo.GetValue("entries", typeof(Collection<DictionaryEntry>));
                foreach (DictionaryEntry entry in entries)
                    AddEntry((TKey)entry.Key, (TValue)entry.Value);
            }
        }
        #endregion IDeserializationCallback
        #region INotifyCollectionChanged
        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add { CollectionChanged += value; }
            remove { CollectionChanged -= value; }
        }
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion INotifyCollectionChanged
        #region INotifyPropertyChanged
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { PropertyChanged += value; }
            remove { PropertyChanged -= value; }
        }
        public virtual event PropertyChangedEventHandler PropertyChanged;
        #endregion INotifyPropertyChanged
        #endregion interfaces
    }
    #endregion

    #region StreamReader2
    /// <summary>StreamReader2 provides a StreamReader with
    /// . Position property to track the real reading position within the file
    /// . a Seek() method for absolute or relative positioning
    /// . a ReadLine() method with a limit on the number of characters to be read</summary>
    public class StreamReader2 : StreamReader
    {
        #region .ctor
        static StreamReader2() { }
        public StreamReader2(Stream stream) : base(stream) { }
        public StreamReader2(string path) : base(path) { }
        public StreamReader2(Stream stream, bool detectEncodingFromByteOrderMarks) : base(stream, detectEncodingFromByteOrderMarks) { }
        public StreamReader2(Stream stream, Encoding encoding) : base(stream, encoding) { }
        public StreamReader2(string path, bool detectEncodingFromByteOrderMarks) : base(path, detectEncodingFromByteOrderMarks) { }
        public StreamReader2(string path, Encoding encoding) : base(path, encoding) { }
        public StreamReader2(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks) : base(stream, encoding, detectEncodingFromByteOrderMarks) { }
        public StreamReader2(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks) : base(path, encoding, detectEncodingFromByteOrderMarks) { }
        public StreamReader2(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize) : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize) { }
        public StreamReader2(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize) : base(path, encoding, detectEncodingFromByteOrderMarks, bufferSize) { }
        #endregion

        #region Position
        public long Position { get; private set; }
        #endregion

        #region Peek
        public override int Peek() { return base.Peek(); }
        #endregion
        #region Read
        public override int Read()
        {
            int code = base.Read();
            if (code != -1) { this.Position++; }
            return code;
        }
        #endregion
        #region Read
        public override int Read(char[] buffer, int index, int count)
        {
            int read = base.Read(buffer, index, count);
            if (read > 0) { this.Position += read; }
            return read;
        }
        #endregion
        #region ReadBlock
        public override int ReadBlock(char[] buffer, int index, int count)
        {
            int read = base.ReadBlock(buffer, index, count);
            if (read > 0) { this.Position += read; }
            return read;
        }
        #endregion
        #region ReadLine
        public override string ReadLine()
        {
            string line = base.ReadLine();
            if (!string.IsNullOrEmpty(line)) { this.Position += line.Length; }
            return line;
        }
        #endregion
        #region ReadToEnd
        public override string ReadToEnd()
        {
            string content = base.ReadToEnd();
            if (!string.IsNullOrEmpty(content)) { this.Position += content.Length; }
            return content;
        }
        #endregion
        #region Seek
        public virtual long Seek(long offset, SeekOrigin origin)
        {
            this.DiscardBufferedData();
            this.BaseStream.Seek(this.Position, SeekOrigin.Begin);
            return this.Position = this.BaseStream.Seek(offset, origin);
        }
        #endregion

        #region ReadLine
        public virtual string ReadLine(int maxchars)
        {
            bool isCRLFLine = false;
            return ReadLine(maxchars, out isCRLFLine);
        }
        #endregion
        #region ReadLine
        public virtual string ReadLine(int maxchars, out bool isCRLFLine)
        {
            isCRLFLine = false;
            int len = Math.Min(maxchars, (int)(this.BaseStream.Length - this.Position));
            if (len <= 0) { return null; }

            char[] buffer = new char[len];
            int read = this.Read(buffer, 0, len);
            if (buffer == null) { return null; }

            IEnumerable<char> enumBuffer = read > 0 && buffer != null ? buffer.Take(read).TakeWhile(c => c != '\r' && c != '\n') : null;
            string line = enumBuffer != null && enumBuffer.Count() > 0 ? new string(enumBuffer.ToArray()) : null;
            //if (line == null) { return null; }

            int lineLenght = line != null ? line.Length : 0;
            int remainder = buffer.Length - lineLenght;                       // se la linea era + corta di 120... il resto deve essere ancora letto
            if (remainder > 0) { this.Seek(-remainder, SeekOrigin.Current); } // riporto indietro la position dello stream

            int code = -1;
            code = this.Read();
            if (code == 13) { code = this.Read(); isCRLFLine = true; }
            if (code == 10) { code = this.Read(); isCRLFLine = true; }
            if (code != -1) { this.Seek(-1, SeekOrigin.Current); }

            return line;
        }
        #endregion
        #region SkipBlanks
        public virtual void SkipBlanks(int keepCount)
        {
            int count = 0;

            for (int code = this.Peek(); code != -1 && (code == 32 || code == 26); code = this.Peek())
            { // char.IsWhiteSpace(Convert.ToChar())
                code = this.Read(); count++;
            }
            //for (code = this.Read(), count++; char.IsWhiteSpace(Convert.ToChar(code)); code = this.Read(), count++) { } // salta i whitespaces
            //if (code != -1) { this.Seek(-1, SeekOrigin.Current); count--; } else { count--; } // l'ultimo read

            int backCount = Math.Min(keepCount, count);
            if (backCount > 0) { this.Seek(-backCount, SeekOrigin.Current); }
        }
        #endregion
    }
    #endregion
}
