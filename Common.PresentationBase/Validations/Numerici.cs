#region using
using System;
using System.Globalization;
using System.Windows.Controls;

#endregion

namespace Common
{
    #region RangeNumberRule
    public class RangeNumberRule : ValidationRuleBase, IBlocksPropertyChange
    {
        #region costanti
        const string TIPO_NON_NUMERICO_INTERO = "'{Value}' non è un valore numerico ammissibile.";
        const string CAMPO_VALUE = "{Value}";
        #endregion

        #region property
        public bool CancelEdit { get; set; }
        public decimal MinValue { get; set; }
        public decimal MinValueNotEqual { get; set; }
        public decimal MaxValue { get; set; }
        public decimal MaxValueNotEqual { get; set; }
        public int NumberOfDecimal { get; set; }
        public string Messaggio { get; set; }
        #endregion

        #region .ctor
        public RangeNumberRule() {
            MinValue = Decimal.MinValue;
            MinValueNotEqual = Decimal.MinValue;
            MaxValue = Decimal.MaxValue;
            MaxValueNotEqual = Decimal.MaxValue;
            NumberOfDecimal = -1;
        }
        #endregion

        #region Validate
        public override ValidationResult OnValidate(ValidationArgsBase validationArgs)
        {
            string strValue = validationArgs.Value as string;
            // Se il valore è vuoto, lo considero valido
            if (StringHelper.IsEmpty(strValue)) {
                return ValidationResult.ValidResult;
            }

            decimal number = 0;
            if (!Decimal.TryParse(strValue, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out number)){
//            if (!Decimal.TryParse(strValue, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out number)) {
                // Numero non intero
                string sValue = validationArgs.Value != null ? strValue : "null";
                return new ValidationResult(false, TIPO_NON_NUMERICO_INTERO.Replace(CAMPO_VALUE, sValue));
            }

            // Contatore cifre decimali dopo virgola
            if (NumberOfDecimal > -1) {
                int numDecimalDigits = (number % 1).ToString().Length - 2;
                if (numDecimalDigits > NumberOfDecimal) {
                    Messaggio = "value '{number}' contiene più di {Limit} cifre decimali".Replace("{number}", number.ToString()).Replace("{Limit}", NumberOfDecimal.ToString());
                    return new ValidationResult(false, Messaggio);
                }
            }

            if (number < MinValue) { Messaggio = "'{number}' è inferiore al valore minimo '{Limit}'.".Replace("{number}", number.ToString()).Replace("{Limit}", MinValue.ToString()); return new ValidationResult(false, Messaggio); }
            if (number <= MinValueNotEqual) { Messaggio = "'{number}' è inferiore o uguale al valore minimo '{Limit}'.".Replace("{number}", number.ToString()).Replace("{Limit}", MinValueNotEqual.ToString()); return new ValidationResult(false, Messaggio); }
            if (number > MaxValue) { Messaggio = "'{number}' è superiore al valore massimo '{Limit}'.".Replace("{number}", number.ToString()).Replace("{Limit}", MaxValue.ToString()); return new ValidationResult(false, Messaggio); }
            if (number >= MaxValueNotEqual) { Messaggio = "'{number}' è superiore o uguale al valore massimo '{Limit}'.".Replace("{number}", number.ToString()).Replace("{Limit}", MaxValueNotEqual.ToString()); return new ValidationResult(false, Messaggio); }

            return ValidationResult.ValidResult;
        }
        #endregion

        #region IBlocksPropertyChange Members
        public bool BlocksPropertyChange { get; set; }
        #endregion
    }
    #endregion

    #region RangeIntegerNumberRule
    public class RangeIntegerNumberRule : ValidationRuleBase, IBlocksPropertyChange
    {
        #region costanti
        const string TIPO_NON_NUMERICO_INTERO = "'{Value}' non è un tipo numerico intero.";
        const string CAMPO_VALUE = "{Value}";
        #endregion

        #region property
        public int MinValue { get; set; }
        public int MinValueNotEqual { get; set; }
        public int MaxValue { get; set; }
        public int MaxValueNotEqual { get; set; }

        public string Messaggio { get; set; }
        #endregion

        #region .ctor
        public RangeIntegerNumberRule() {
            MinValue = int.MinValue;
            MinValueNotEqual = int.MinValue;
            MaxValue = int.MaxValue;
            MaxValueNotEqual = int.MaxValue;
        }
        #endregion

        #region Validate
        public override ValidationResult OnValidate(ValidationArgsBase validationArgs)
        {

            // Se il valore è vuoto, lo considero valido
            if (StringHelper.IsEmpty(validationArgs.Value as string))
            {
                return ValidationResult.ValidResult;
            }

            int number = 0;

            if (!Int32.TryParse(validationArgs.Value.ToString(), out number))
            {
                // Numero non intero
                string sValue = validationArgs.Value != null ? Convert.ToString(validationArgs.Value) : "null";
                return new ValidationResult(false, TIPO_NON_NUMERICO_INTERO.Replace(CAMPO_VALUE, sValue));
            }

            if (number < MinValue) { Messaggio = "'{number}' è inferiore al valore minimo consentito '{Limit}'.".Replace("{number}", number.ToString()).Replace("{Limit}", MinValue.ToString()); return new ValidationResult(false, Messaggio); }
            if (number <= MinValueNotEqual) { Messaggio = "'{number}' è inferiore o uguale al limite minimo '{Limit}'.".Replace("{number}", number.ToString()).Replace("{Limit}", MinValueNotEqual.ToString()); return new ValidationResult(false, Messaggio); }
            if (number > MaxValue) { Messaggio = "'{number}' è superiore al valore massimo consentito '{Limit}'.".Replace("{number}", number.ToString()).Replace("{Limit}", MaxValue.ToString()); return new ValidationResult(false, Messaggio); }
            if (number >= MaxValueNotEqual) { Messaggio = "'{number}' è superiore o uguale al limite massimo '{Limit}'.".Replace("{number}", number.ToString()).Replace("{Limit}", MaxValueNotEqual.ToString()); return new ValidationResult(false, Messaggio); }

            return ValidationResult.ValidResult;
        }
        #endregion

        #region IBlocksPropertyChange Members
        public bool BlocksPropertyChange { get; set; }
        #endregion
    }
    #endregion

    #region NumberLimitRule
    //[Obsolete("Classe sostituita dalla classe RangeNumberRule")]
    public class NumberLimitRule<T> : ValidationRuleBase
        where T : struct, IComparable {
        #region costanti
        const string TIPO_NON_NUMERICO = "'{Value}' non è un tipo numerico.";
        const string CAMPO_VALUE = "{Value}";
        #endregion

        public T? Limit { get; set; }
        public string Op { get; set; }
        public string Messaggio { get; set; }

        #region Validate
        public override ValidationResult OnValidate(ValidationArgsBase validationArgs)
        {
            IMath<T> math = Math<T>.Default;
            T number = default(T);
            bool ok = false;
            string tempMessaggio;

            if (validationArgs.Value is string && !string.IsNullOrEmpty(validationArgs.Value as string)) {
                ok = math.TryParse(validationArgs.Value as string, NumberStyles.Any, validationArgs.CultureInfo as IFormatProvider, out number);
            } else if (validationArgs.Value is T) {
                number = (T)validationArgs.Value;
                ok = true;
            } else {
                try {
                    IConvertible conv = number as IConvertible;
                    if (conv == null) {
                        string sValue = validationArgs.Value != null ? Convert.ToString(validationArgs.Value) : "null";
                        return new ValidationResult(false, TIPO_NON_NUMERICO.Replace(CAMPO_VALUE, sValue));
                    }

                    number = (T)conv.ToType(typeof(T), validationArgs.CultureInfo as IFormatProvider);
                } catch (FormatException) {
                    string sValue = validationArgs.Value != null ? Convert.ToString(validationArgs.Value) : "null";
                    return new ValidationResult(false, TIPO_NON_NUMERICO.Replace(CAMPO_VALUE, sValue));
                }
            }

            if (Op == "<" && this.Limit != null && number.CompareTo(this.Limit.Value)>=0) { tempMessaggio = String.IsNullOrEmpty(Messaggio) ? "'{number}' è maggiore o uguale al valore massimo '{Limit}'.".Replace("{number}", number.ToString()).Replace("{Limit}", Limit.ToString()) : Messaggio; return new ValidationResult(false, tempMessaggio); }
            if (Op == "<=" && this.Limit != null && number.CompareTo(this.Limit.Value) > 0) { tempMessaggio = String.IsNullOrEmpty(Messaggio) ? "'{number}' è superiore al valore massimo '{Limit}'.".Replace("{number}", number.ToString()).Replace("{Limit}", Limit.ToString()) : Messaggio; return new ValidationResult(false, tempMessaggio); }
            if (Op == ">=" && this.Limit != null && number.CompareTo(this.Limit.Value) < 0) { tempMessaggio = String.IsNullOrEmpty(Messaggio) ? "'{number}' è minore o uguale al valore minimo '{Limit}'.".Replace("{number}", number.ToString()).Replace("{Limit}", Limit.ToString()) : Messaggio; return new ValidationResult(false, tempMessaggio); }
            if (Op == ">" && this.Limit != null && number.CompareTo(this.Limit.Value) <= 0) { tempMessaggio = String.IsNullOrEmpty(Messaggio) ? "'{number}' è inferiore al valore minimo '{Limit}'.".Replace("{number}", number.ToString()).Replace("{Limit}", Limit.ToString()) : Messaggio; return new ValidationResult(false, tempMessaggio); }
            return ValidationResult.ValidResult;
        }
        #endregion
    }
    public sealed class NumberLimitRuleInt32 : NumberLimitRule<int> { }
    public sealed class NumberLimitRuleInt64 : NumberLimitRule<long> { }
    public sealed class NumberLimitRuleDecimal : NumberLimitRule<decimal> { }
    public sealed class NumberLimitRuleSingle : NumberLimitRule<Single> { }
    public sealed class NumberLimitRuleDouble : NumberLimitRule<Double> { }
    public sealed class NumberLimitRule : NumberLimitRule<decimal> { }
    #endregion
}
