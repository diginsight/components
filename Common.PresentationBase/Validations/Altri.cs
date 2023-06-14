#region using
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

#endregion

namespace Common
{
    #region enum
    public enum CfPivaValidationMode
    {
        All,
        OnlyCf,
        OnlyPiva
    }
    #endregion

    public class CodiceFiscaleRule : ValidationRuleBase
    {
        #region constants
        const string TIPO_NON_STRINGA = "'{Value}' non è convertibile ad un tipo stringa.";
        const string CAMPO_VALUE = "{Value}";
        #endregion

        #region Messaggio
        public string Messaggio { get; set; }
        #endregion
        #region StrongCheck
        public bool StrongCheck { get; set; }
        #endregion

        #region Validate
        public override ValidationResult OnValidate(ValidationArgsBase validationArgs)
        {
            string codiceFiscale = Convert2.ToString(validationArgs.Value);

            if (codiceFiscale != null && (codiceFiscale.Length > 16 || codiceFiscale.Length == 16 && !CodiceFiscale.CheckCodiceFiscale(codiceFiscale))) { Messaggio = "Valore '{string}' non è un codice fiscale valido.".Replace("{string}", Convert2.ToString(codiceFiscale)); return new ValidationResult(false, Messaggio); }
            if (codiceFiscale != null && StrongCheck && codiceFiscale.Length != 16) { Messaggio = "Valore '{string}' non è un codice fiscale valido.".Replace("{string}", Convert2.ToString(codiceFiscale)); return new ValidationResult(false, Messaggio); }

            return ValidationResult.ValidResult;
        }
        #endregion
    }

    public class PartitaIvaRule : ValidationRuleBase
    {
        #region constants
        const string TIPO_NON_STRINGA = "'{Value}' non è convertibile ad un tipo stringa.";
        const string CAMPO_VALUE = "{Value}";
        #endregion

        #region Messaggio
        public string Messaggio { get; set; }
        #endregion
        #region StrongCheck
        public bool StrongCheck { get; set; }
        #endregion

        #region Validate
        public override ValidationResult OnValidate(ValidationArgsBase validationArgs)
        {
            string partitaIva = Convert2.ToString(validationArgs.Value);

            if (partitaIva != null && (partitaIva.Length > 11 || partitaIva.Length == 11 && !PartitaIva.CheckPartitaIva(partitaIva))) { Messaggio = "Valore '{string}' non è un codice fiscale valido.".Replace("{string}", Convert2.ToString(partitaIva)); return new ValidationResult(false, Messaggio); }
            if (partitaIva != null && StrongCheck && partitaIva.Length != 11) { Messaggio = "Valore '{string}' non è un codice fiscale valido.".Replace("{string}", Convert2.ToString(partitaIva)); return new ValidationResult(false, Messaggio); }

            return ValidationResult.ValidResult;
        }
        #endregion
    }

    public class CodiceFiscalePartitaIvaRule : ValidationRuleBase
    {

        #region Public
        public CfPivaValidationMode validationMode;
        #endregion

        #region constants
        const string TIPO_NON_STRINGA = "'{Value}' non è convertibile ad un tipo stringa.";
        const string CAMPO_VALUE = "{Value}";
        #endregion

        #region Messaggio
        public string Messaggio { get; set; }
        #endregion
        #region StrongCheck
        public bool StrongCheck { get; set; }
        #endregion

        #region Validate
        public override ValidationResult OnValidate(ValidationArgsBase validationArgs)
        {
            
            string messaggio = "";
            ValidationResult validationResult = ValidationResult.ValidResult;
            
            switch (validationMode)
            {
                case CfPivaValidationMode.OnlyCf:
                    messaggio = validateCf(validationArgs.Value);
                    break;
                case CfPivaValidationMode.OnlyPiva:
                    messaggio = validatePiva(validationArgs.Value);
                    break;
                default:
                    if (!string.IsNullOrEmpty(validateCf(validationArgs.Value)) && !string.IsNullOrEmpty(validatePiva(validationArgs.Value)))
                    {
                        messaggio = "Codice Fiscale o Partita Iva formalmente errato.";
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(messaggio))
            {
                validationResult = new ValidationResult(false, messaggio);
            }

            return validationResult;
        }
        #endregion

        #region private validator

        #region validazione cf
        private String validateCf(object cf) 
        {
            string codiceFiscale = Convert.ToString(cf);
            if (!string.IsNullOrEmpty(codiceFiscale) && (codiceFiscale.Length > 16) || (codiceFiscale.Length == 16 && !CodiceFiscale.CheckCodiceFiscale(codiceFiscale))) 
            { 
                return "Valore '{string}' non è un codice fiscale valido.".Replace("{string}", Convert2.ToString(codiceFiscale));  
            }
            if (StrongCheck && !string.IsNullOrEmpty(codiceFiscale) && codiceFiscale.Length != 16) 
            { 
                return "Valore '{string}' non è un codice fiscale valido.".Replace("{string}", Convert2.ToString(codiceFiscale)); 
            }
            return "";
        }
        #endregion

        #region validazione piva
        private String validatePiva(object piva)
        {
            string partitaIva = piva==null ? "" : Convert2.ToString(piva);
            if (!string.IsNullOrEmpty(partitaIva) && (partitaIva.Length > 11) || (partitaIva.Length == 11 && !PartitaIva.CheckPartitaIva(partitaIva))) 
            { 
                return "Valore '{string}' non è una partita iva valida.".Replace("{string}", Convert2.ToString(partitaIva)); 
            }
            if (StrongCheck && !string.IsNullOrEmpty(partitaIva) && partitaIva.Length != 11) 
            {
                return "Valore '{string}' non è una partita iva valida.".Replace("{string}", Convert2.ToString(partitaIva)); 
            }

            return "";
        }
        #endregion

        #endregion
    }

    //public class IbanValidationRule : ValidationRuleBase
    //{

    //    private string _errorMessage;

    //    public string ErrorMessage
    //    {
    //        get { return _errorMessage; }
    //        set { _errorMessage = value; }
    //    }

    //    public override ValidationResult OnValidate(ValidationArgsBase validationArgs)
    //    {
    //        ValidationResult result = new ValidationResult(true, null);
    //        string inputString = (validationArgs.Value ?? string.Empty).ToString();
    //        GestioneCodiciBancari gcb = new GestioneCodiciBancari();

    //        if (!string.IsNullOrEmpty(inputString) && !gcb.CheckIBAN(inputString))
    //        {
    //            //result = new ValidationResult(false, new AbcErrorContent(this.ErrorMessage, AbcErrorSeverity.ERROR));
    //            result = new ValidationResult(false, this.ErrorMessage);
    //        }
    //        return result;
    //    }
    //}
    //public class GestioneCodiciBancari
    //{
    //    public GestioneCodiciBancari()
    //    {
    //    }

    //    public GestioneCodiciBancari(GestioneCodiciBancari coordinata)
    //        : this()
    //    {
    //        Abi = coordinata.ABI;
    //        Cab = coordinata.CAB;
    //        ContoCorrente = coordinata.Conto;
    //        Paese = coordinata.CodicePaese;
    //        Cin = coordinata.CIN;
    //        if (Cin == null)
    //            Cin = string.Empty;
    //        else
    //            Cin = Cin.Trim();
    //    }

    //    private string mAbi = String.Empty;
    //    private string mCab = String.Empty;
    //    private string mContoCorrente = String.Empty;
    //    private string mCin = String.Empty;
    //    private const int L_IBAN = 27;
    //    private const int L_CONTO = 12;
    //    private const int L_ABI = 5;
    //    private const int L_CAB = 5;
    //    private bool mNormalizzaConto = true;
    //    private string mIBAN = String.Empty;
    //    private string mBBAN = String.Empty;
    //    private string mCheckDigitIBAN = String.Empty;
    //    private string mPaese = String.Empty;
    //    private int mDivisore = 97;
    //    public string Abi { get { return mAbi; } set { mAbi = NormalizzaDati(value, L_ABI); } }

    //    public string Cab
    //    {
    //        get
    //        {
    //            return mCab;
    //        }
    //        set
    //        {
    //            mCab = NormalizzaDati(value, L_CAB); ;
    //        }
    //    }

    //    public string ContoCorrente
    //    {
    //        get
    //        {
    //            return mContoCorrente;
    //        }
    //        set
    //        {
    //            mContoCorrente = value;
    //        }
    //    }

    //    public string Cin
    //    {
    //        get
    //        {
    //            return mCin;
    //        }
    //        set
    //        {
    //            mCin = value;
    //        }
    //    }

    //    public string BBAN
    //    {
    //        get
    //        {
    //            return mBBAN;
    //        }
    //        set
    //        {
    //            mBBAN = value;
    //        }
    //    }

    //    public string IBAN
    //    {
    //        get
    //        {
    //            return mIBAN;
    //        }
    //        set
    //        {
    //            mIBAN = value;
    //        }
    //    }

    //    public string CheckDigitIBAN
    //    {
    //        get
    //        {
    //            return mCheckDigitIBAN;
    //        }
    //        set
    //        {
    //            mCheckDigitIBAN = value;
    //        }
    //    }

    //    public string Paese
    //    {
    //        get
    //        {
    //            return mPaese;
    //        }
    //        set
    //        {
    //            mPaese = value;
    //        }
    //    }

    //    public bool NormalizzaConto
    //    {
    //        get
    //        {
    //            return mNormalizzaConto;
    //        }
    //        set
    //        {
    //            mNormalizzaConto = value;
    //        }
    //    }

    //    public int Divisore
    //    {
    //        get
    //        {
    //            return mDivisore;
    //        }
    //        set
    //        {
    //            mDivisore = value;
    //        }
    //    }

    //    private string NormalizzaDati(string codice, int lunghezza)
    //    {
    //        codice = codice.Trim();
    //        int k = codice.Length;
    //        if (k < lunghezza)
    //        {
    //            codice = "".PadLeft(lunghezza, '0') + codice;
    //            k += lunghezza;
    //        }
    //        k -= lunghezza;
    //        if (k < 0)
    //            k = 0;
    //        codice = codice.Substring(k);
    //        return codice;
    //    }

    //    public string NormalizzaContoCorrente(string contoCorrenteValue)
    //    {
    //        contoCorrenteValue = contoCorrenteValue.Trim();
    //        int k = contoCorrenteValue.IndexOf(' ');
    //        while (k >= 0)
    //        {
    //            contoCorrenteValue = contoCorrenteValue.Remove(k, 1);
    //            k = contoCorrenteValue.IndexOf(' ');
    //        }
    //        return NormalizzaDati(contoCorrenteValue, L_CONTO);

    //    }

    //    public bool VerificaCin(string cinCode)
    //    {
    //        return (cinCode == CalcolaCin());
    //    }

    //    public string CalcolaIBANdaRapporto(string pPaese, string pABI, string pCAB, string pRapporto)
    //    {

    //        string IBANCalcolato = string.Empty;
    //        if ((pPaese != null && pPaese.Trim() != string.Empty) &&
    //           (pABI != null && pABI.Trim() != string.Empty) &&
    //           (pCAB != null && pCAB.Trim() != string.Empty) &&
    //           (pRapporto != null && pRapporto.Trim() != string.Empty))
    //        {

    //            pABI = NormalizzaDati(pABI, L_ABI);
    //            pCAB = NormalizzaDati(pCAB, L_CAB);
    //            pRapporto = NormalizzaContoCorrente(pRapporto);
    //            mCin = CalcolaCin(pABI, pCAB, pRapporto);
    //            StringBuilder sbBBAN = new StringBuilder();
    //            sbBBAN.Append(mCin);
    //            sbBBAN.Append(pABI);
    //            sbBBAN.Append(pCAB);
    //            sbBBAN.Append(pRapporto);
    //            IBANCalcolato = CalcolaIBAN(pPaese, sbBBAN.ToString());
    //        }
    //        return IBANCalcolato;

    //    }
    //    public string CalcolaCin(string pABI, string pCAB, string pRapporto)
    //    {
    //        // costanti e variabili per calcolo pesi
    //        const string numeri = "0123456789";
    //        const string lettere = "ABCDEFGHIJKLMNOPQRSTUVWXYZ-. ";
    //        const int DIVISORE = 26;
    //        int[] listaPari = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28 };
    //        int[] listaDispari = { 1, 0, 5, 7, 9, 13, 15, 17, 19, 21, 2, 4, 18, 20, 11, 3, 6, 8, 12, 14, 16, 10, 22, 25, 24, 23, 27, 28, 26 };

    //        // normalizzazione dati			
    //        if (pABI.Length != L_ABI)
    //            pABI = NormalizzaDati(pABI, L_ABI);
    //        if (pCAB.Length != L_CAB)
    //            pCAB = NormalizzaDati(pCAB, L_CAB);
    //        //if (this.NormalizzaConto)
    //        //    this.ContoCorrente = NormalizzaContoCorrente(this.ContoCorrente);
    //        if (pRapporto.Length != L_CONTO)
    //            pRapporto = pRapporto.PadRight(L_CONTO);

    //        // codice normalizzato
    //        string codice = pABI + pCAB + pRapporto;

    //        // calcolo valori caratteri
    //        int somma = 0;
    //        char[] c = codice.ToUpper().ToCharArray();
    //        for (int k = 0; k < (L_CONTO + L_ABI + L_CAB); k++)
    //        {
    //            int i = numeri.IndexOf(c[k]);
    //            if (i < 0)
    //                i = lettere.IndexOf(c[k]);

    //            // se ci sono caratteri errati usciamo con un valore 
    //            // impossibile da trovare sul cin
    //            if (i < 0)
    //                return Environment.NewLine;

    //            if ((k % 2) == 0)
    //            {
    //                // valore dispari
    //                somma += listaDispari[i];
    //            }
    //            else
    //            {
    //                // valore pari
    //                somma += listaPari[i];
    //            }
    //        }
    //        return lettere.Substring(somma % DIVISORE, 1);
    //    }

    //    public string CalcolaCin()
    //    {
    //        // costanti e variabili per calcolo pesi
    //        const string numeri = "0123456789";
    //        const string lettere = "ABCDEFGHIJKLMNOPQRSTUVWXYZ-. ";
    //        const int DIVISORE = 26;
    //        int[] listaPari = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28 };
    //        int[] listaDispari = { 1, 0, 5, 7, 9, 13, 15, 17, 19, 21, 2, 4, 18, 20, 11, 3, 6, 8, 12, 14, 16, 10, 22, 25, 24, 23, 27, 28, 26 };

    //        // normalizzazione dati			
    //        if (this.Abi.Length != L_ABI)
    //            mAbi = NormalizzaDati(mAbi, L_ABI);
    //        if (this.Cab.Length != L_CAB)
    //            mCab = NormalizzaDati(mCab, L_CAB);
    //        if (this.NormalizzaConto)
    //            this.ContoCorrente = NormalizzaContoCorrente(this.ContoCorrente);
    //        if (this.ContoCorrente.Length != L_CONTO)
    //            this.ContoCorrente = this.ContoCorrente.PadRight(L_CONTO);

    //        // codice normalizzato
    //        string codice = this.Abi + this.Cab + this.ContoCorrente;

    //        // calcolo valori caratteri
    //        int somma = 0;
    //        char[] c = codice.ToUpper().ToCharArray();
    //        for (int k = 0; k < (L_CONTO + L_ABI + L_CAB); k++)
    //        {
    //            int i = numeri.IndexOf(c[k]);
    //            if (i < 0)
    //                i = lettere.IndexOf(c[k]);

    //            // se ci sono caratteri errati usciamo con un valore 
    //            // impossibile da trovare sul cin
    //            if (i < 0)
    //                return Environment.NewLine;

    //            if ((k % 2) == 0)
    //            {
    //                // valore dispari
    //                somma += listaDispari[i];
    //            }
    //            else
    //            {
    //                // valore pari
    //                somma += listaPari[i];
    //            }
    //        }
    //        return lettere.Substring(somma % DIVISORE, 1);
    //    }

    //    public bool CheckIBAN()
    //    {
    //        string codice;
    //        if (mIBAN != "")
    //            codice = mIBAN;
    //        else
    //        {
    //            string s = mCin;
    //            if (s != "")
    //            {
    //                bool b = VerificaCin(s);
    //                if (!b)
    //                    return false;
    //            }
    //            else
    //            {
    //                s = CalcolaCin();
    //            }
    //            codice = s + NormalizzaDati(mAbi, L_ABI) +
    //                NormalizzaDati(mCab, L_CAB) +
    //                NormalizzaContoCorrente(mContoCorrente);

    //        }
    //        return CheckIBAN(codice);
    //    }

    //    public bool CheckIBAN(string pIBAN)
    //    {
    //        string codice = NormalizzaIBAN(pIBAN);
    //        if (!CheckLength(codice))
    //            return false;
    //        codice = codice.Substring(4) + codice.Substring(0, 4);
    //        string[] r = Funzioni.DivisioneIntera(AlfaToNumber(codice), Divisore.ToString());
    //        int resto = int.Parse(r[1]);
    //        return (resto == 1);
    //    }

    //    public bool CheckDigitCartaCredito(string number)
    //    {
    //        // algoritmo Luhn
    //        return number.Reverse().SelectMany((c, i) => ((c - '0') << (i & 1)).ToString()).Sum(c => (c - '0')) % 10 == 0;
    //    }

    //    public string CalcolaBBAN()
    //    {
    //        string codice;
    //        if (mIBAN != "")
    //            codice = mIBAN;
    //        else
    //        {
    //            string s = mCin;
    //            if (s == "")
    //                s = CalcolaCin();
    //            codice = s + NormalizzaDati(mAbi, L_ABI) +
    //                NormalizzaDati(mCab, L_CAB) +
    //                NormalizzaContoCorrente(mContoCorrente);
    //        }
    //        return codice;
    //    }

    //    public string CalcolaIBAN()
    //    {
    //        string codice;
    //        if (mBBAN != "")
    //            codice = mBBAN;
    //        else
    //        {
    //            codice = CalcolaBBAN();
    //        }
    //        return CalcolaIBAN(mPaese, codice);
    //    }

    //    public string CalcolaCheckIBAN(string pPaese, string pBBAN)
    //    {
    //        return CalcolaIBAN(pPaese, pBBAN).Substring(2, 2);
    //    }

    //    public string CalcolaIBAN(string pPaese, string pBBAN)
    //    {
    //        pBBAN = NormalizzaIBAN(pBBAN);
    //        string codice = pPaese + "00" + pBBAN;
    //        codice = codice.Substring(4) + codice.Substring(0, 4);
    //        string numcode = AlfaToNumber(codice);
    //        string[] r = Funzioni.DivisioneIntera(numcode, Divisore.ToString());
    //        int resto = int.Parse(r[1]);
    //        resto = (Divisore + 1) - resto;
    //        return pPaese + resto.ToString("00") + pBBAN;
    //    }

    //    public string NormalizzaIBAN(string pCodice)
    //    {
    //        const string alfanum = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    //        StringBuilder sb = new StringBuilder();
    //        foreach (char c in pCodice)
    //        {
    //            if (alfanum.IndexOf(c) != -1)
    //                sb.Append(c);
    //        }
    //        return sb.ToString();
    //    }

    //    public string CostruisciIBAN(CoordinataBancaria coordinata)
    //    {
    //        string strIBAN = string.Empty;

    //        if (coordinata != null &&
    //            (coordinata.CodicePaese != null && coordinata.CodicePaese.Trim().Length == 2) &&
    //            (coordinata.CHECK <= 99 && coordinata.CHECK > 0) &&
    //            (coordinata.CIN != null && coordinata.CIN.Trim().Length == 1) &&
    //            (!string.IsNullOrEmpty(coordinata.ABI)) &&
    //            (!string.IsNullOrEmpty(coordinata.CAB)) &&
    //            (!string.IsNullOrEmpty(coordinata.Conto))
    //            )
    //        {
    //            StringBuilder sb = new StringBuilder();
    //            sb.Append(coordinata.CodicePaese);
    //            sb.Append(string.Format("{0:00}", coordinata.CHECK));
    //            sb.Append(coordinata.CIN);
    //            sb.Append(string.Format("{0:00000}", Convert.ToInt32(coordinata.ABI)));
    //            sb.Append(string.Format("{0:00000}", Convert.ToInt32(coordinata.CAB)));
    //            sb.Append(coordinata.Conto.PadLeft(12, '0'));
    //            strIBAN = sb.ToString();
    //        }
    //        return strIBAN;

    //    }

    //    public CoordinataBancaria DecodificaIBAN(string CodiceIBAN)
    //    {
    //        CoordinataBancaria CoordinateBancarie = new CoordinataBancaria();
    //        if (CodiceIBAN.Length == 27)
    //        {
    //            CoordinateBancarie.CodiceIBAN = CodiceIBAN;
    //            CoordinateBancarie.CodicePaese = CodiceIBAN.Substring(0, 2);
    //            CoordinateBancarie.CHECK = Convert.ToInt32(CodiceIBAN.Substring(2, 2));
    //            CoordinateBancarie.CIN = CodiceIBAN.Substring(4, 1);
    //            CoordinateBancarie.ABI = CodiceIBAN.Substring(5, 5);
    //            CoordinateBancarie.CAB = CodiceIBAN.Substring(10, 5);
    //            CoordinateBancarie.Conto = CodiceIBAN.Substring(15);
    //        }
    //        else
    //        {
    //            string messaggioErrore = string.Format("codice IBAN non valido {0}", CodiceIBAN);
    //            throw new Exception(messaggioErrore);
    //        }

    //        return CoordinateBancarie;
    //    }

    //    private bool CheckLength(string pCodice)
    //    {
    //        return true;
    //    }

    //    private string AlfaToNumber(string pCodice)
    //    {
    //        const string alfachars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    //        StringBuilder sb = new StringBuilder();
    //        foreach (char c in pCodice)
    //        {
    //            int k = alfachars.IndexOf(c);
    //            if (k != -1)
    //                sb.Append(k + 10);
    //            else
    //                sb.Append(c);
    //        }
    //        return sb.ToString();
    //    }


    //}

    public sealed class Funzioni
    {
        public Funzioni()
        {
        }

        public static string[] DivisioneIntera(string pDividendo, string pDivisore)
        {
            StringBuilder Intero = new StringBuilder();
            StringBuilder Resto = new StringBuilder();
            double divisore;
            if (!double.TryParse(pDivisore, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out divisore))
                throw new Exception("Divisore errato");
            for (int x = 0; x < pDividendo.Length; x++)
            {
                Resto.Append(pDividendo.Substring(x, 1));
                string s = Resto.ToString();
                double dividendo = 0;
                if (!double.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out dividendo))
                    throw new Exception("Dividendo Errato");
                int volte = 0;
                while (dividendo >= divisore)
                {
                    dividendo -= divisore;
                    volte++;
                }
                Intero.Append(volte);
                string r = dividendo.ToString("0");
                Resto = new StringBuilder();
                Resto.Append(r);
            }
            string[] result = new string[2];
            result[1] = Resto.ToString();
            result[0] = Intero.ToString();
            while (result[0].StartsWith("0"))
                result[0] = result[0].Substring(1);
            if (result[0] == "")
                result[0] = "0";
            return result;
        }
    }


}