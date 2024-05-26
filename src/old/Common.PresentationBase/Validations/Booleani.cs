#region using
using System;
using System.Windows.Controls;

#endregion

namespace Common
{
    #region BooleanRule
    public class BooleanRule : ValidationRuleBase
    {
        public string FailMessage { get; set; }
        public bool FailValue { get; set; }

        #region Validate
        public override ValidationResult OnValidate(ValidationArgsBase validationArgs)
        {
            bool bValue = false;
            try
            {
                bValue = Convert.ToBoolean(validationArgs.Value);
            }
            catch (FormatException)
            {
                return new ValidationResult(false, string.Format("il valore '{0}' non è un booleano valido", bValue));
            }

            if (bValue == FailValue) { return new ValidationResult(false, FailMessage != null ? FailMessage : ""); }

            return ValidationResult.ValidResult;
        }
        #endregion
    }
    #endregion


    //
    // Summary:
    //     Represents the result returned by the System.Windows.Controls.ValidationRule.System.Windows.Controls.ValidationRule.Validate(System.Object,System.Globalization.CultureInfo)
    //     method that indicates whether the checked value passed the System.Windows.Controls.ValidationRule.
    public class ExceptionValidationResult : ValidationResult
    {
        public ExceptionValidationResult(bool isValid, Exception ex) : base(isValid, ex?.Message) { this.Exception = ex; }

        public Exception Exception { get; set; }
    }

    #region ExceptionRule
    public class ExceptionRule : ValidationRuleBase
    {
        public string DefaultMessage { get; set; }
        public Exception Exception { get; set; }

        #region Validate
        public override ValidationResult OnValidate(ValidationArgsBase validationArgs)
        {
            Exception ex = validationArgs.Value as Exception;
            this.Exception = ex;
            if (ex != null) { return new ExceptionValidationResult(false, ex); }
            return ValidationResult.ValidResult;
        }
        #endregion
    }
    #endregion
}
