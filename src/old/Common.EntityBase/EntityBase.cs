#region using
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks; 
#endregion

namespace Common
{
    #region enum RequestScope
    public enum RequestScope
    {
        Dipendenza,
        Banca,
        Gruppo,
        Globale,
    }
    #endregion

    #region ISupportsKey
    public interface ISupportsKey
    {
        object Key { get; }
    }
    #endregion
    #region ISupportsKey<T>
    public interface ISupportsKey<T>
    {
        T Key { get; }
    }
    #endregion
    #region ISupportsCacheKey
    public interface ISupportsCacheKey
    {
        object CacheKey { get; }
    }
    #endregion
    #region ISupportsCacheKey<T>
    public interface ISupportsCacheKey<T>
    {
        T CacheKey { get; }
    }
    #endregion
    #region IMessageBase
    public interface IMessageBase { }
    #endregion

    #region class EntityBase
    public abstract class EntityBase : INotifyPropertyChanged, ICloneable, IDisposable //, IExtensibleDataObject, ICopiable, IComponent
    {
        // Events
        #region Disposed
        public event EventHandler Disposed;
        #endregion

        #region .ctor
        protected EntityBase() { }
        #endregion

        #region Clone
        public object Clone() { return SerializationHelper.CloneBySerializing(this); }
        #endregion
        #region Dispose
        public void Dispose() { this.Dispose(true); GC.SuppressFinalize(this); }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                EventHandler disposed = this.Disposed;
                if (disposed != null) { disposed(this, EventArgs.Empty); }
            }
        }
        #endregion

        #region CopyTo
        public virtual void CopyTo(object pcopy) { return; }
        #endregion

        #region PropertyChanged
        public IDictionary Properties { get; } = new Dictionary<string,object>();
        #region SetValue
        protected void SetValue<T>(Expression<Func<T>> propertySelector, T value)
        {
            string propertyName = GetPropertyName(propertySelector);
            var oldValue = Properties[propertyName];
            Properties[propertyName] = value;
            if (!object.Equals(oldValue, value)) { NotifyPropertyChanged(propertySelector); }
        }
        #endregion
        #region SetValue
        protected void SetValue<T>(string propertyName, T value)
        {
            Properties[propertyName] = value;
            NotifyPropertyChanged(propertyName);
        }
        #endregion
        #region GetValue
        protected T GetValue<T>(Expression<Func<T>> propertySelector)
        {
            string propertyName = GetPropertyName(propertySelector);
            return GetValue<T>(propertyName);
        }
        #endregion
        #region GetValue
        protected T GetValue<T>(string propertyName)
        {
            object value;
            if (Properties.Contains(propertyName))
            {
                value = Properties[propertyName];
                return (T)value;
            }
            return default(T);
        }
        #endregion
        #region NotifyPropertyChanged
        protected void NotifyPropertyChanged<T>(Expression<Func<T>> propertySelector)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                string propertyName = GetPropertyName(propertySelector);
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
        #region INotifyPropertyChanged Members
        /// <summary>Raised when a property on this object has a new value.</summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>Raises this object's PropertyChanged event.</summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
        #endregion // INotifyPropertyChanged Members
        #region GetPropertyName
        private string GetPropertyName(LambdaExpression expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null) { throw new InvalidOperationException(); }
            return memberExpression.Member.Name;
        }
        #endregion

        #region Debugging
        /// <summary>Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.</summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real, public, instance property on this object.
            if (this.GetType().GetProperty(propertyName) == null)
            {
                string msg = "Invalid property name: " + propertyName;
                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Assert(false, msg);
            }
        }
        /// <summary>Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.</summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }
        #endregion
        #endregion
    }
    #endregion
    #region class EntityBase<T>
    public abstract class EntityBase<T> : EntityBase, ISupportsKey<T>, ISupportsKey
    {
        #region GetHashCode
        public override int GetHashCode()
        {
            return (Key == null) ? base.GetHashCode() : Key.GetHashCode();
        }
        #endregion
        #region Equals
        public override bool Equals(object obj)
        {
            EntityBase<T> entita = obj as EntityBase<T>; // null keys --> fallback su ReferenceEquals
            if (this.Key == null || entita == null || entita.Key == null) { return object.ReferenceEquals(this, obj); }

            bool result = false; // potenziamente non simmetrica !!! (con inheritance strane)
            if (this.Key.Equals(entita.Key) && (this.GetType().IsInstanceOfType(obj) || obj.GetType().IsInstanceOfType(this)))
            {
                result = true;
            }

            return result;
        }
        #endregion

        #region Key
        protected abstract T Key { get; }
        T ISupportsKey<T>.Key
        {
            get { return this.Key; }
        }
        #endregion
        #region ISupportsKey.Key
        object ISupportsKey.Key { get { return this.Key; } }
        #endregion
    }
    #endregion
    #region class EntityCollectionBase<T>
    public class EntityCollectionBase<T> : List<T>, IList, ICollection, IEnumerable, IDisposable
        where T : EntityBase, new()
    {
        #region .ctor
        public EntityCollectionBase()
        {
        }
        public EntityCollectionBase(T[] entities)
            : base((entities != null) ? new List<T>(entities) : new List<T>())
        {
        }
        public EntityCollectionBase(List<T> entities)
            : base((entities != null) ? new List<T>(entities) : new List<T>())
        {
        }
        #endregion

        #region Disposed
        public event EventHandler Disposed;
        #endregion

        #region Clone
        public object Clone()
        {
            return SerializationHelper.CloneBySerializing(this);
        }
        #endregion
        #region Dispose
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                EventHandler disposed = this.Disposed;
                if (disposed != null)
                {
                    disposed(this, EventArgs.Empty);
                }
            }
        }
        #endregion
    }
    #endregion

    #region MessageBase
    public class MessageBase : EntityBase, IMessageBase { } 
    #endregion

    #region InMessageBase
    public class InMessageBase : MessageBase
    {
        #region .ctors
        /// <summary>Ritorna un'istanza della classe RichiestaBase.</summary>
        public InMessageBase() : base()
        {
        }
        #endregion

        /// <summary>Se valorizzato a true viene skippata la chiamata al task e l'operazione torna immediatamente.</summary>
        [DataMember]
        public bool FlagLoopback { get; set; }
    }
    #endregion
    #region InMessageBase<T>
    public abstract class InMessageBase<T> : InMessageBase, ISupportsKey<T>, ISupportsKey
    {
        #region GetHashCode
        public override int GetHashCode()
        {
            return (Key == null) ? base.GetHashCode() : Key.GetHashCode();
        }
        #endregion
        #region Equals
        public override bool Equals(object obj)
        {
            InMessageBase<T> entita = obj as InMessageBase<T>; // null keys --> fallback su ReferenceEquals
            if (this.Key == null || entita == null || entita.Key == null) { return object.ReferenceEquals(this, obj); }

            bool result = false; // potenziamente non simmetrica !!! (con inheritance strane)
            if (this.Key.Equals(entita.Key) && (this.GetType().IsInstanceOfType(obj) || obj.GetType().IsInstanceOfType(this)))
            {
                result = true;
            }

            return result;
        }
        #endregion

        #region Scope
        //[DataMember]
        public virtual RequestScope Scope { get { return RequestScope.Dipendenza; } }
        #endregion
        #region Key
        public abstract T Key { get; }
        T ISupportsKey<T>.Key
        {
            get { return this.Key; }
        }
        #endregion
        #region ISupportsKey.Key
        object ISupportsKey.Key { get { return this.Key; } }
        #endregion

        #region GetCacheKey
        public Tuple<RequestScope, T> GetCacheKey()
        {
            return new Tuple<RequestScope, T>(Scope, Key);
        }
        public string GetCacheStringKey()
        {
            //    var contesto = ContestoABC.Current;
            var scope = "";

            return string.Format("{0}#{1}", scope, Key);
        }
        #endregion
    }
    #endregion

    #region OutMessageBase
    public class OutMessageBase : MessageBase
    {
        #region Costruttori
        /// <summary>Ritorna l'istanza di una classe RispostaBase.</summary>
        public OutMessageBase() : base()
        {
            this.Esito = null;
        }
        public OutMessageBase(Segnalazione risultato)
        {
            this.Esito = risultato;
        }
        #endregion

        #region Proprietà Pubbliche
        /// <summary>L'esito del servizio invocato.</summary>
        [DataMember]
        public Segnalazione Esito { get; set; }
        #endregion     

        #region Metodi Pubblici
        /// <summary>Definisce se questa risposta può essere cachata oppure è necessario invalidare la cache.
        /// La decisione viene presa sulla base dell'esito.</summary>
        /// <returns></returns>
        public bool CanBeCached()
        {
            if (this.Esito == null || !this.Esito.Severity.HasFlag(TipoSeverity.Errore)) //  && !this.Esito.Severity.HasFlag(TipoSeverity.Warning)
            {
                return true;
            }

            return false;
        }
        #endregion
    }
    #endregion
}
