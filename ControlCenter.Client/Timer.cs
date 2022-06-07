using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static ControlCenter.Client.Classes.InstanciaJanelaPrincipal.Janela;

namespace ControlCenter.Client
{
    public class Timer
    {
        public static System.Timers.Timer myTimer = new System.Timers.Timer();


        public static void IniciaTimer()
        {
            Task T = new Task(() =>
            {
                ConfiguraTimer();
            });

            T.Start();

        }

        public static void PararTimer()
        {
            myTimer.Stop();
        }

        private static void ConfiguraTimer()
        {
            myTimer.Elapsed += new ElapsedEventHandler(TempoAtingindo);
            myTimer.Interval = Convert.ToDouble(new ConfiguracaoDosParametros().DadosParametros.Where(x => x.Nome == "INTERVALO DE TEMPO SINCRONIZACAO CONTROL CENTER CLIENT").FirstOrDefault().Valor) *1000 ;
            myTimer.Enabled = true;
        }

        private static void TempoAtingindo(object sender, ElapsedEventArgs e)
        {
            myTimer.Enabled = false;
            Principal.Sincronizar(null, null);
            myTimer.Enabled = true;
        }
    }
}
