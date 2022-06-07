using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ControlCenter.Client.Classes.InstanciaJanelaPrincipal.Janela;

namespace ControlCenter.Client.Classes.Logs
{
    
    public class Log 
    {            
        public static void Logger(string Logs)
        {

            Thread T = new Thread(new ThreadStart(() =>
            {
               
            }));
            
            Task.Run(() =>
            {
                
            });

            try
            {
                string LogAnterior = string.Empty;
                string NovoLog = $"{DateTime.Now}: {Logs}{Environment.NewLine}";
                Principal.richTextBox1.Invoke(new Action(() => LogAnterior = Principal.richTextBox1.Text));
                Principal.richTextBox1.Invoke(new Action(() => Principal.richTextBox1.Text = NovoLog));
                Principal.richTextBox1.Invoke(new Action(() => Principal.richTextBox1.Text += LogAnterior));
                /*LogAnterior = Principal.richTextBox1.Text;
                Principal.richTextBox1.Text = NovoLog;
                Principal.richTextBox1.Text += LogAnterior;*/
                LogTxt(NovoLog);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            


        }

        public static void LogCliente(string Movimentacao, string IP)
        {
            string Clientes = string.Empty;
           
            if(Movimentacao == "Conectado")
            {
                Clientes += IP;
            }

            else
            {
                Clientes.Replace(IP, "");
            }

            Principal.richTextBox2.Invoke(new Action(() => Principal.richTextBox2.Text = Clientes));
        }


        private static void LogTxt(string Log)
        {
            if (!Directory.Exists(@"C:\LanSolutions\ControlCenter-Servidor\Logs\"))
            {
                Directory.CreateDirectory(@"C:\LanSolutions\ControlCenter-Servidor\Logs\");
            }

            string PathLog = @"C:\LanSolutions\ControlCenter-Servidor\Logs\";

            string Data = DateTime.Now.ToString("dd" + "MM" + "yyyy");

            if (!Directory.Exists($@"{PathLog}{Data}\"))
            {
                Directory.CreateDirectory($@"{PathLog}{Data}\");
            }

            string PathLogHoje = PathLog + Data + @"\";

            if (!File.Exists(PathLogHoje + "Log.txt"))
            {
                File.Create(PathLogHoje + "Log.txt").Dispose();                
            }

            try
            {
                using (StreamWriter outputFile = new StreamWriter(PathLogHoje + "Log.txt", append: true))
                {
                    outputFile.Write(Log);
                }
            }
            catch
            {

            }



        }
    }
}
