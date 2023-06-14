#region using
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
#endregion

namespace Common
{
    #region DateLimitRule
    [Obsolete("Classe sostituita dalla classe RangeDateRule")]
    public class DateLimitRule : ValidationRuleBase
    {
        #region costanti
        const string TIPO_NON_DATA = "'{Value}' non è un tipo data.";
        const string CAMPO_VALUE = "{Value}";
        #endregion

        public DateTime Limit { get; set; }
        public string Op { get; set; }
        public string Messaggio { get; set; }

        #region Validate
        public override ValidationResult OnValidate(ValidationArgsBase validationArgs)
        {
            DateTime date;
            try {
                date = Convert.ToDateTime(validationArgs.Value);
            } catch (FormatException) {
                string sValue = validationArgs.Value != null ? Convert.ToString(validationArgs.Value) : "null";
                return new ValidationResult(false, TIPO_NON_DATA.Replace(CAMPO_VALUE, sValue));
            }

            if (Op == "<" && date >= Limit) { Messaggio = "'{date}' è maggiore o uguale al valore massimo consentito '{Limit}'.".Replace("{date}", date.ToShortDateString()).Replace("{Limit}", Limit.ToShortDateString()); return new ValidationResult(false, Messaggio); }
            if (Op == "<=" && date > Limit) { Messaggio = "Valore max consentito '{Limit}'.".Replace("{date}", date.ToShortDateString()).Replace("{Limit}", Limit.ToShortDateString()); return new ValidationResult(false, Messaggio); }
            if (Op == ">=" && date < Limit) { Messaggio = "'{date}' è minore o uguale al valore minimo consentito '{Limit}'.".Replace("{date}", date.ToShortDateString()).Replace("{Limit}", Limit.ToShortDateString()); return new ValidationResult(false, Messaggio); }
            if (Op == ">" && date <= Limit) { Messaggio = "Valore minimo consentito '{Limit}'.".Replace("{date}", date.ToShortDateString()).Replace("{Limit}", Limit.ToShortDateString()); return new ValidationResult(false, Messaggio); }

            return ValidationResult.ValidResult;
        }
        #endregion
    }
    #endregion

    #region RangeDateRule
    /// <summary>Verifica la validità della data e se è compresa in un intervallo definito.</summary>
    public class RangeDateRule : ValidationRuleBase
    {
        #region costanti
        const string TIPO_NON_DATA = "'{Value}' non è un tipo data.";
        const string CAMPO_VALUE = "{Value}";
        #endregion

        #region property

        public DateTime MinDate { get; set; }
        public DateTime MinDateNotEqual { get; set; }
        public DateTime MaxDate { get; set; }
        public DateTime MaxDateNotEqual { get; set; }

        public bool ConsentiSoloGiorniLavorativi { get; set; }

        /// <summary>
        /// Giorno (incluso) minimo consentito.
        /// Codifica giorni:
        /// oggi : data odierna
        /// </summary>
        public string MinDay { get; set; }
        /// <summary>
        /// Giorno (escluso) minimo consentito.
        /// Codifica giorni:
        /// oggi : data odierna
        /// </summary>
        public string MinDayNotEqual { get; set; }
        /// <summary>
        /// Giorno (incluso) massimo consentito.
        /// Codifica giorni:
        /// oggi : data odierna
        /// </summary>
        public string MaxDay { get; set; }
        /// <summary>
        /// Giorno (escluso) massimo consentito.
        /// Codifica giorni:
        /// oggi : data odierna
        /// </summary>
        public string MaxDayNotEqual { get; set; }

        public string Messaggio { get; set; }

        #endregion

        #region .ctor
        public RangeDateRule() {
            MinDate = DateTime.MinValue;
            MinDateNotEqual = DateTime.MinValue;
            MaxDate = DateTime.MaxValue;
            MaxDateNotEqual = DateTime.MaxValue;

            MinDay = "";
            MinDayNotEqual = "";
            MaxDay = "";
            MaxDayNotEqual = "";

        }
        #endregion

        #region Validate
        public override ValidationResult OnValidate(ValidationArgsBase validationArgs)
        {
            if (Application.Current.MainWindow==null || DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow)) { return ValidationResult.ValidResult; }

            DateTime date;

            if ((validationArgs.Value == null) || (String.IsNullOrEmpty(validationArgs.Value.ToString())) )
            {
                return ValidationResult.ValidResult;
            }

            if (!DateTime.TryParse(validationArgs.Value.ToString(), out date))
            {
                // String data non valida
                string sValue = validationArgs.Value != null ? Convert.ToString(validationArgs.Value) : "null";
                return new ValidationResult(false, TIPO_NON_DATA.Replace(CAMPO_VALUE, sValue));
            }

            // Controllo date
            if (!MaxDate.Equals(DateTime.MaxValue) && date.CompareTo(MaxDate) >= 0) { Messaggio = "'{date}' è maggiore o uguale del valore massimo consentito '{Limit}'.".Replace("{date}", date.ToShortDateString()).Replace("{Limit}", MaxDate.ToShortDateString()); return new ValidationResult(false, Messaggio); }
            if (date.CompareTo(MaxDateNotEqual) > 0) { Messaggio = "Valore massimo consentito '{Limit}'.".Replace("{date}", date.ToShortDateString()).Replace("{Limit}", MaxDateNotEqual.ToShortDateString()); return new ValidationResult(false, Messaggio); }
            if (!MinDate.Equals(DateTime.MinValue) && date.CompareTo(MinDate) <= 0) { Messaggio = "'{date}' è minore o uguale al valore minimo consentito '{Limit}'.".Replace("{date}", date.ToShortDateString()).Replace("{Limit}", MinDate.ToShortDateString()); return new ValidationResult(false, Messaggio); }
            if (date.CompareTo(MinDateNotEqual) < 0) { Messaggio = "Valore minimo consentito '{Limit}'.".Replace("{date}", date.ToShortDateString()).Replace("{Limit}", MinDateNotEqual.ToShortDateString()); return new ValidationResult(false, Messaggio); }

            // Controllo date codificate
            // OGGI
            DateTime dateDay;
            if (MaxDay.ToLower().Equals("oggi")) {
                dateDay = DateTime.Today;
                if (date.CompareTo(dateDay) >= 0) { Messaggio = "'{date}' è minore o uguale al valore minimo consentito '{Limit}'.".Replace("{date}", date.ToShortDateString()).Replace("{Limit}", dateDay.ToString()); return new ValidationResult(false, Messaggio); }
            }
            if (MaxDayNotEqual.ToLower().Equals("oggi")) {
                dateDay = DateTime.Today;
                if (date.CompareTo(dateDay) > 0) { Messaggio = "Valore min. consentito '{Limit}'.".Replace("{date}", date.ToShortDateString()).Replace("{Limit}", dateDay.ToShortDateString()); return new ValidationResult(false, Messaggio); }
            }
            if (MinDay.ToLower().Equals("oggi")) {
                dateDay = DateTime.Today;
                if (date.CompareTo(dateDay) <= 0) { Messaggio = "'{date}' è minore o uguale al valore minimo consentito '{Limit}'.".Replace("{date}", date.ToShortDateString()).Replace("{Limit}", dateDay.ToString()); return new ValidationResult(false, Messaggio); }
            }
            if (MinDayNotEqual.ToLower().Equals("oggi")) {
                dateDay = DateTime.Today;
                if (date.CompareTo(dateDay) < 0) { Messaggio = "Valore min. consentito '{Limit}'.".Replace("{date}", date.ToShortDateString()).Replace("{Limit}", dateDay.ToShortDateString()); return new ValidationResult(false, Messaggio); }
            }
            //Controlo giorni lavorativi
            if (ConsentiSoloGiorniLavorativi)
            {
                if (!CalendarHelper.IsWorkingDay(date)) { Messaggio = "'{date}' non è un giorno lavorativo.".Replace("{date}", date.ToShortDateString()); return new ValidationResult(false, Messaggio); }               
            }

            return ValidationResult.ValidResult;
        }
        #endregion
    }
    #endregion
}
