#region using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Common
{
    public static class CalendarHelper
    {
        private static List<DateTime> _listaFestivita;
        private static bool caricaListaFestivita;

        static CalendarHelper()
        {
            CaricaListaFestivita();
        }

        /// <summary>
        /// Controlla se la data passata in input é un giorno lavorativo
        /// </summary>
        /// <param name="data">Data da controllare</param>
        /// <returns>true se la data é un giorno lavorativo</returns>
        public static bool IsWorkingDay(DateTime data)
        {
            if (!caricaListaFestivita) { CaricaListaFestivita(); }

            data = new DateTime(data.Year, data.Month, data.Day);

            //if (_listaFestivita == null) throw new ArgumentNullException("Lista festività non valorizzata");

            bool b = false;
            if (!(data.DayOfWeek.Equals(DayOfWeek.Saturday) || data.DayOfWeek.Equals(DayOfWeek.Sunday) || _listaFestivita != null && _listaFestivita.Contains(data))) b = true;
            return b;
        }

        /// <summary>
        /// Controlla se la data passata in input é un giorno lavorativo
        /// </summary>
        /// <param name="data">Data da controllare</param>
        /// <param name="listaFestivita">Elenco delle festività da includere nel controllo</param>
        /// <returns>true se la data é un giorno lavorativo</returns>
        //public static bool IsGiornoLavorativo(DateTime data, List<DateTime> listaFestivita)
        //{
        //    _listaFestivita = listaFestivita;
        //    return IsGiornoLavorativo(data);
        //}

        private static void CaricaListaFestivita()
        {
            // ABCServerHelper server = new ABCServerHelper();
            // StrutturaServiceProxy proxy = ABCServerHelper.GetProxy<StrutturaServiceProxy>();
            // RichiestaEstraiListaFestivita richiesta = new RichiestaEstraiListaFestivita();

            // RispostaEstraiListaFestivita risposta =
            //     server.Execute<RichiestaEstraiListaFestivita, RispostaEstraiListaFestivita>(richiesta, proxy.EstraiListaFestivita, hlpListaFestivitaOffline);

            // if (risposta != null && risposta.ListaFestivita != null)
            // {
            //     _listaFestivita = risposta.ListaFestivita;
            // }

            caricaListaFestivita = true;
        }
    }
}
