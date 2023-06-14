using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common
{
    public class PartitaIva
    {
        #region .ctor
        public PartitaIva()
        {
        }
        #endregion

        #region CheckPartitaIva
        public static bool CheckPartitaIva(string partitaIva)
        {
            bool result = false;
            const int caratteri = 11;
            const int codfisc = 16;
            string partitaIvaCopy = partitaIva;
            if (partitaIvaCopy == null) { return result; }

            Regex pregex = new Regex("^\\d{" + caratteri.ToString() + "}$");
            // per questa chiamata vedi la funzione del codice fiscale 
            if (partitaIvaCopy.Length == codfisc) { return CodiceFiscale.CheckCodiceFiscale(partitaIva); }

            if (partitaIvaCopy.Length != caratteri)
            {
                if (partitaIvaCopy.Length < caratteri) { partitaIvaCopy.PadLeft(caratteri, '0'); } else { partitaIvaCopy = partitaIvaCopy.Substring(2); }
            }

            Match m = pregex.Match(partitaIvaCopy);
            result = m.Success;

            if (result)
            {
                result = ((int.Parse(partitaIvaCopy.Substring(0, 7)) != 0) &&
                          (int.Parse(partitaIvaCopy.Substring(7, 3)) >= 0));
                // &&(int.Parse(partitaIvaCopy.Substring(7, 3)) < 201)
            }

            if (result)
            {
                int somma = 0;
                for (int i = 0; i < caratteri - 1; i++)
                {
                    int j = int.Parse(partitaIvaCopy.Substring(i, 1));
                    if ((i + 1) % 2 == 0)
                    {
                        j *= 2;
                        char[] c = j.ToString("00").ToCharArray();
                        somma += int.Parse(c[0].ToString());
                        somma += int.Parse(c[1].ToString());
                    }
                    else
                        somma += j;
                }
                if ((somma.ToString("00").Substring(1, 1) == "0") && (partitaIvaCopy.Substring(10, 1) != "0"))
                {
                    result = false;
                }
                somma = int.Parse(partitaIvaCopy.Substring(10, 1)) + int.Parse(somma.ToString("00").Substring(1, 1));
                if (result)
                {
                    result = (somma.ToString("00").Substring(1, 1) == "0");
                }
            }
            return result;
        }
        #endregion
    }

    public class CodiceFiscale
    {
        #region .ctor
        public CodiceFiscale() { }
        #endregion

        #region CheckCodiceFiscale
        public static bool CheckCodiceFiscale(string CodiceFiscale)
        {
            bool result = false;
            const int caratteri = 16;
            if (CodiceFiscale == null) { return result; }
            if (CodiceFiscale.Length < caratteri) { return PartitaIva.CheckPartitaIva(CodiceFiscale); }
            if (CodiceFiscale.Length != caratteri) { return result; }

            // stringa per controllo e calcolo omocodia 
            const string omocodici = "LMNPQRSTUV";
            // per il calcolo del check digit e la conversione in numero
            const string listaControllo = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int[] listaPari = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };
            int[] listaDispari = { 1, 0, 5, 7, 9, 13, 15, 17, 19, 21, 2, 4, 18, 20, 11, 3, 6, 8, 12, 14, 16, 10, 22, 25, 24, 23 };


            CodiceFiscale = CodiceFiscale.ToUpper();
            char[] codice = CodiceFiscale.ToCharArray();

            // check della correttezza formale del codice fiscale elimino dalla stringa gli eventuali caratteri utilizzati negli 
            // spazi riservati ai 7 che sono diventati carattere in caso di omocodia
            for (int k = 6; k < 15; k++)
            {
                if ((k == 8) || (k == 11)) { continue; }

                int x = (omocodici.IndexOf(codice[k]));
                if (x != -1) { codice[k] = x.ToString().ToCharArray()[0]; }
            }

            //  Regex rgx = new Regex(@"^[A-Z]{6}[]{2}[A-Z][]{2}[A-Z][]{3}[A-Z]$");
            //  Match m = rgx.Match(new string(cCodice)); result = m.Success;
            result = true;

            // normalizzato il codice fiscale se la regular non ha buon fine è inutile continuare
            if (result)
            {
                int somma = 0;
                codice = CodiceFiscale.ToCharArray();
                for (int i = 0; i < 15; i++)
                {
                    char c = codice[i];
                    int x = "0123456789".IndexOf(c);
                    if (x != -1)
                        c = listaControllo.Substring(x, 1).ToCharArray()[0];
                    x = listaControllo.IndexOf(c);
                    if (x != -1)
                    {
                        // i modulo 2 = 0 è dispari perchè iniziamo da 0
                        if ((i % 2) == 0)
                            x = listaDispari[x];
                        else
                            x = listaPari[x];
                        somma += x;
                    }
                }

                result = (listaControllo.Substring(somma % 26, 1) == CodiceFiscale.Substring(15, 1));
            }
            return result;
        }
        #endregion
    }
}
