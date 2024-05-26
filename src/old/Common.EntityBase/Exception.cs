#region using
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
#endregion

namespace Common
{
    public enum ResultType
    {
        OK = 0,
        KO = 1
    }
    #region Esito
    public class Esito : EntityBase
    {
        #region .ctors
        /// <summary>Ritorna l'istanza di una classe Esito.</summary>
        public Esito() : base()
        {
            this.Risultato = ResultType.OK;
        }

        /// <summary>Ritorna l'istanza di una classe Esito.</summary>
        /// <param name="risultato">Il risultato contenuto nella classe Esito (OK/KO)</param>
        public Esito(ResultType risultato)
        {
            this.Risultato = risultato;
        }

        /// <summary>Ritorna l'istanza di una classe Esito.</summary>
        /// <param name="segnalazione">Un oggetto segnalazione da inserire nella lista in base alla quale verrà determinato il risultato</param>
        public Esito(Segnalazione segnalazione)
        {
            this.Risultato = ResultType.OK;
            this.AggiungiSegnalazione(segnalazione);
        }
        #endregion

        #region public properties
        /// <summary>operation result</summary>
        public ResultType Risultato { get; set; }

        /// <summary>Lista delle segnalazioni</summary>
        public ListaSegnalazioni Segnalazioni { get; set; }

        /// <summary>L'intero che rappresenta la mappa dei 32 possibili semafori</summary>
        public int? DatiSemafori { get; set; }
        #endregion      

        #region public metods
        /// <summary>Aggiunge un oggetto segnalazione alla lista</summary>
        /// <param name="segnalazione"></param>
        public void AggiungiSegnalazione(Segnalazione segnalazione)
        {
            if (Segnalazioni == null)
                Segnalazioni = new ListaSegnalazioni();

            Segnalazioni.Add(segnalazione);

            if (segnalazione.Severity == TipoSeverity.Errore)
                this.Risultato = ResultType.KO;
        }

        /// <summary>Annulla le informazioni temporanee che l'esito trasporta ad ogni chiamata (es. i semafori)
        /// a scopo di caching.</summary>
        public void Clear()
        {
            this.DatiSemafori = null;
        }
        #endregion

        #region Helpers
        public override string ToString()
        {
            return Risultato.ToString();
        }
        #endregion
    } 
    #endregion

    #region Segnalazione
    public class Segnalazione : EntityBase
    {
        #region .ctor
        /// <summary>Crea una nuova istanza della classe Segnalazione</summary>
        public Segnalazione()
        {

        }

        /// <summary>Crea una nuova istanza della classe Segnalazione</summary>
        /// <param name="severity">Tipologia della segnalazione</param>
        /// <param name="codice">Codice della segnalazione</param>
        /// <param name="descrizione">Messaggio utente della segnalazione</param>
        public Segnalazione(TipoSeverity severity, int codice, string descrizione)
            : this(severity, codice, descrizione, false)
        {
        }

        /// <summary>Crea una nuova istanza della classe Segnalazione</summary>
        /// <param name="severity">Tipologia della segnalazione</param>
        /// <param name="codice">Codice della segnalazione</param>
        /// <param name="descrizione">Messaggio utente della segnalazione</param>
        public Segnalazione(TipoSeverity severity, CodiciSegnalazioni codice, string descrizione)
            : this(severity, (int)codice, descrizione, false)
        {
        }

        /// <summary>Crea una nuova istanza della classe Segnalazione</summary>
        /// <param name="severity">Tipologia della segnalazione</param>
        /// <param name="codice">Codice della segnalazione</param>
        /// <param name="descrizione">Messaggio utente della segnalazione</param>
        /// <param name="ignoraInvioHelpHesk">True se ignora invio della segnalazione di errore all'Helpdesk</param>
        public Segnalazione(TipoSeverity severity, CodiciSegnalazioni codice, string descrizione, bool ignoraInvioHelpHesk)
            : this(severity, (int)codice, descrizione, ignoraInvioHelpHesk)
        {
        }

        /// <summary>Crea una nuova istanza della classe Segnalazione</summary>
        /// <param name="severity">Tipologia della segnalazione</param>
        /// <param name="codice">Codice della segnalazione</param>
        /// <param name="descrizione">Messaggio utente della segnalazione</param>
        /// <param name="ignoraInvioHelpHesk">True se ignora invio della segnalazione di errore all'Helpdesk</param>
        public Segnalazione(TipoSeverity severity, int codice, string descrizione, bool ignoraInvioHelpHesk)
        {
            this.Severity = severity;
            this.Codice = codice;
            this.Descrizione = descrizione;
            this.IgnoraInvioHelpHesk = ignoraInvioHelpHesk;
        }

        #endregion

        #region public properties
        /// <summary>Tipologia della segnalazione</summary>
        public TipoSeverity Severity { get; set; }

        /// <summary>
        /// Codice della segnalazione
        /// </summary>
        public int Codice { get; set; }

        /// <summary>
        /// Messaggio utente della segnalazione
        /// </summary>
        public string Descrizione { get; set; }

        /// <summary>
        /// True se non devo inviare la segnalazione di errore all'HelpDesk
        /// </summary>
        public bool IgnoraInvioHelpHesk { get; set; }

        /// <summary>
        /// Categoria della segnalazione
        /// </summary>
        public string Categoria { get; set; }

        #endregion
    }
    #endregion
    #region ListaSegnalazioni
    public class ListaSegnalazioni : EntityCollectionBase<Segnalazione> { }
    #endregion

    #region CodiciSegnalazioni
    public enum CodiciSegnalazioni
    {
        OperazioneOK = 0,
        RispostaHostOK = 1,
        RispostaHostKO = 2,
        TroppeOccorrenze = 3,
        RisultatoNonTrovato = 4
    } 
    #endregion
    public enum TipoSeverity
    {
        [EnumMember]
        Info = 0,
        [EnumMember]
        Warning = 1,
        [EnumMember]
        Errore = 2,
    }

    // Exception level
    #region enum ExceptionType
    [Flags]
    public enum ExceptionType
    {
        Default = 0,
        NonRemovable = 1
    }
    #endregion
    #region enum ExceptionLevel
    public enum ExceptionLevel
    {
        Critical = 1,
        Error = 2,
        Warning = 4,
        Information = 8,
        Verbose = 16,
        None = 32,
        Unknown = 64
    }
    #endregion
    #region ExceptionSourceAttribute
    ///<summary>this attribute is used to define a name for an assembly or class to be used in configuration files.</summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class)]
    public sealed class ExceptionSourceAttribute : Attribute
    {
        #region internal state
        private string _sName;
        #endregion
        #region construction
        ///<summary>constructs the attribute: gets a reference to the name.</summary>
        public ExceptionSourceAttribute(string name) { _sName = name; }
        #endregion

        #region Name 
        ///<summary>returns the name to be used for the assembly or the class.</summary>
        public string Name { get { return _sName; } set { _sName = value; } }
        #endregion
    }
    #endregion

    // Exceptions
    #region ExceptionBase : ApplicationException
    [Serializable]
    public class ExceptionBase : ApplicationException
    {
        #region .ctor
        ///<summary>Initializes an Exception instance.</summary>
        public ExceptionBase() : base() { }
        ///<summary>Initializes an Exception instance with a message.</summary>
        public ExceptionBase(string message) : base(message) { }
        ///<summary>Initializes an Exception instance with a messag and an inner exception.</summary>
        public ExceptionBase(string message, Exception innerException) : base(message, innerException) { }
        #endregion
        #region internal state
        private string _className;
        private string _metodName;
        MethodBase _metodo;
        //string _source;
        #endregion

        #region ExceptionLevel
        public ExceptionLevel ExceptionLevel = ExceptionLevel.Unknown;
        #endregion
        #region Code
        public long Code { get; set; }
        #endregion
        #region ClassName
        public string ClassName
        {
            get { return _className != null ? _className : Method?.DeclaringType?.Name; }
            set { _className = value; }
        }
        #endregion
        #region MetodName
        public string MetodName
        {
            get { return _metodName != null ? _metodName : Method?.Name; }
            set { _metodName = value; }
        }
        #endregion
        #region Method
        public MethodBase Method
        {
            get
            {
                if (_metodo == null && TargetSite != null) { return TargetSite; }
                return _metodo;
            }
            set { _metodo = value; }
        }
        #endregion
        //#region Source
        //public string Source
        //{
        //    get { return _source; }
        //    set { _source = value; }
        //}
        //#endregion

        //#region MergeExceptionsConverter
        //public static ConvertDelegate2 MergeExceptionsConverterDelegate = MergeExceptionsConverter;
        //public static object MergeExceptionsConverter(DependencyObject d, object[] values, Type targetType, object parameter, CultureInfo culture)
        //{
        //    Exception ex = null;
        //    if (values != null)
        //    {
        //        foreach (object value in values)
        //        {
        //            var e = value as Exception;
        //            if (e == null) { continue; }
        //            ex = ex == null ? new ClientException(e.Message, e) : new ClientException(ex.Message + "\r\n" + e.Message, e);
        //        }
        //    }

        //    return ex;
        //}
        //#endregion
    }
    #endregion
    #region ClientException : ExceptionBase
    [Serializable]
    public class ClientException : ExceptionBase
    {
        #region .ctor
        ///<summary>Inizializza una eccezione ABCClientException.</summary>
        public ClientException() : base() { }
        ///<summary>Inizializza una eccezione ABCClientException con un messaggio.</summary>
        public ClientException(string message) : base(message) { }
        ///<summary>Inizializza una eccezione ABCClientException con un messaggio ed una inner exception.</summary>
        public ClientException(string message, Exception innerException) : base(message, innerException) { }
        #endregion

        #region ExceptionLevel
        public ExceptionType ExceptionType = ExceptionType.Default;
        #endregion
    }
    #endregion
    #region BackgroundProcessingException : ClientException
    /// <summary>Used for background processing exceptions (see. ExceptionManager.ThrowException)</summary>
    [Serializable]
    public class BackgroundProcessingException : ClientException
    {
        ///<summary>Initializes a BackgroundProcessingException.</summary>
        public BackgroundProcessingException() { }
        ///<summary>Initializes a BackgroundProcessingException with a message.</summary>
        public BackgroundProcessingException(string message) : base(message) { }
        ///<summary>Initializes a BackgroundProcessingException with a message and an inner exception.</summary>
        public BackgroundProcessingException(string message, Exception innerException) : base(message, innerException) { }
    }
    #endregion
    #region IgnoreException : ClientException
    /// <summary>Incapsulates exceptions that should be ignored.</summary>
    [Serializable]
    public class IgnoreException : ClientException
    {
        ///<summary>Initializes an IgnoreException.</summary>
        public IgnoreException() { }
        ///<summary>Initializes an IgnoreException with a message.</summary>
        public IgnoreException(string message) : base(message) { }
        ///<summary>Initializes an IgnoreException with a message and an inner exception.</summary>
        public IgnoreException(string message, Exception innerException) : base(message, innerException) { }
    }
    #endregion
    #region ValidationFailureException : ClientException
    /// <summary>Incapsulates exceptions generated by validations.</summary>
    [Serializable]
    public class ValidationFailureException : ClientException
    {
        ///<summary>Initializes a ValidationFailureException.</summary>
        public ValidationFailureException() { }
        ///<summary>Initializes a ValidationFailureException with a message.</summary>
        public ValidationFailureException(string message) : base(message) { }
        ///<summary>Initializes a ValidationFailureException with a message and an inner exception.</summary>
        public ValidationFailureException(string message, Exception innerException) : base(message, innerException) { }
    }
    #endregion
    #region ServerException : ExceptionBase
    [Serializable]
    public class ServerException : ExceptionBase
    {
        #region .ctor
        ///<summary>Initializes a ServerException.</summary>
        public ServerException() : base() { }
        ///<summary>Initializes a ServerException with a message.</summary>
        public ServerException(string message) : base(message) { }
        ///<summary>Initializes a ServerException with a message and an inner exception.</summary>
        public ServerException(string message, Exception innerException) : base(message, innerException) { }
        #endregion

        #region internal state
        object _response;
        #endregion

        #region Request
        public object Request { get; set; }
        #endregion
        #region Response
        public object Response
        {
            get { return _response; }
            set { _response = value; }
        }
        #endregion
    }
    #endregion

}
