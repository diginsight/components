using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using ValidationResult = System.Windows.Controls.ValidationResult;

namespace Common
{
    #region ValidationRuleBase
    public abstract class ValidationRuleBase : ValidationRule
    {
        Reference<bool> _lockGetBoundValue = new Reference<bool>();
        #region Properties
        // Tipologia errore
        public AbcErrorSeverity Severity { get; set; }
        #endregion

        #region Validate
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (this._lockGetBoundValue.Value) { return ValidationResult.ValidResult; }
            ValidationArgsBase validationArgs = new ValidationArgsBase();
            validationArgs.CultureInfo = cultureInfo;

            if (value != null && value is BindingExpression)
            {
                BindingExpression bExp = value as BindingExpression;

                if (bExp.ParentBinding != null && bExp.DataItem != null)
                {
                    BindingHelper be = new BindingHelper(bExp.ParentBinding);
                    using (new SwitchOnDispose(this._lockGetBoundValue, true))
                    {
                        validationArgs.Value = be.GetBoundValue(bExp.DataItem, false);
                    }
                }
                // TODO: else RETURN
            }
            else
            {
                validationArgs.Value = value;
            }

            ValidationResult tmpResult = OnValidate(validationArgs);
            if (!tmpResult.IsValid && Severity > AbcErrorSeverity.INFO &&
                tmpResult.ErrorContent != null && !(tmpResult.ErrorContent is ErrorContentBase))
            {
                // Se flag Severity Settato da binding allora trasformo i risultati in un AbcErrorContent
                tmpResult = new ValidationResult(false, new ErrorContentBase(tmpResult.ErrorContent, Severity));
            }

            return tmpResult;

        }
        #endregion


        #region Abstract methods
        // Metodo validazione per le AbcRule
        public abstract ValidationResult OnValidate(ValidationArgsBase validationArgs);
        #endregion

    }
    #endregion

    #region ValidationArgsBase
    // Parametro per validazione (AbcRule) AbcValidate
    public class ValidationArgsBase : Object
    {
        public ValidationArgsBase()
        {
        }

        public object Value { get; set; }
        public object CultureInfo { get; set; }
    }
    #endregion

    #region AbcErrorContent
    // Parametro tornato nella validazione
    public class ErrorContentBase : Object
    {
        public object MessageContent { get; set; }
        public AbcErrorSeverity Severity { get; set; }

        public ErrorContentBase(object messageContent, AbcErrorSeverity severity)
        {
            MessageContent = messageContent;
            Severity = severity;
        }
    }

    #region AbcErrorSeverity
    public enum AbcErrorSeverity
    {
        INFO = 0,
        WARNING = 1,
        ERROR = 2
    }
    #endregion
    #endregion
}
