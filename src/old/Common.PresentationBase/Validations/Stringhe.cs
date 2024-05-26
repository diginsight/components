#region using
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;


#endregion

namespace Common
{
    #region RegExRule
    public class RegExRule : ValidationRuleBase, IBlocksPropertyChange
    {
        #region internal state
        Regex _expr;
        #endregion

        #region .ctor
        public RegExRule() {
            this.RegexOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline;
        }
        #endregion

        #region Pattern
        public string Pattern { get; set; }
        #endregion
        #region RegexOptions
        public RegexOptions RegexOptions { get; set; }
        #endregion
        #region BlocksPropertyChange
        public bool BlocksPropertyChange { get; set; }
        #endregion
        #region NomePattern
        public string NomePattern { get; set; }
        #endregion

        #region Validate
        public override ValidationResult OnValidate(ValidationArgsBase validationArgs)
        {
            ValidationResult res = ValidationResult.ValidResult;
            if (Pattern == null) { return res; }
            if (validationArgs.Value == null || validationArgs.Value == DependencyProperty.UnsetValue) { return res; }
            if (_expr == null || _expr.Options != this.RegexOptions || _expr.ToString() != this.Pattern) { _expr = new Regex(this.Pattern, this.RegexOptions); }

            string sValue = null;
            try {
                sValue = Convert2.ToString(validationArgs.Value);
                if (_expr.IsMatch(sValue)) {
                    return res;
                }

                return new ValidationResult(false, "il valore '{value}' non corrisponde al formato '{nomepattern}'".Replace("{value}", sValue).Replace("{nomepattern}", this.NomePattern != null ? this.NomePattern : "")); //  ('{pattern}') .Replace("{pattern}", Convert2.ToString(this.Pattern))
            } catch (Exception ex) {
                var exc = new ClientException("errore nella validazione del valore '{value}' rispetto al formato  '{nomepattern}'".Replace("{value}", sValue).Replace("{nomepattern}", this.NomePattern != null ? this.NomePattern : ""), ex);
                return new ValidationResult(false, exc);
            }
        }
        #endregion
    }
    #endregion

    #region StringLenghtRule
    public class StringLenghtRule : ValidationRuleBase
    {
        #region .ctor

        public StringLenghtRule() {
            MinLenght = 0;
            MaxLenght = int.MaxValue;
        }

        #endregion

        #region MinLenght
        public int MinLenght { get; set; }
        #endregion
        #region MaxLenght
        public int MaxLenght { get; set; }
        #endregion
        #region Messaggio
        public string Messaggio { get; set; }
        #endregion

        #region Validate
        public override ValidationResult OnValidate(ValidationArgsBase validationArgs)
        {
            if (validationArgs.Value != null)
            {
                if (validationArgs.Value.ToString().Length > MaxLenght) { Messaggio = "La stringa inserita non può essere superiore a {MaxLenght} caratteri.".Replace("{MaxLenght}", MaxLenght.ToString()); return new ValidationResult(false, Messaggio); }
                if (validationArgs.Value.ToString().Length < MinLenght) { Messaggio = "La stringa inserita non può essere inferiore di {MinLenght} caratteri.".Replace("{MinLenght}", MinLenght.ToString()); return new ValidationResult(false, Messaggio); } 
            }
            return ValidationResult.ValidResult;
        }
        #endregion
    }
    #endregion
}
