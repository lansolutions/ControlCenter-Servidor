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

        static List<string> Logs = new List<string>();
        public static void Logger(string Logs)
        {
            try
            {
                AtualizaLogs();
                string LogAnterior = string.Empty;
                string NovoLog = $"{DateTime.Now}: {Logs}{Environment.NewLine}";               
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

            if (Principal.richTextBox2.InvokeRequired)
            {
                Principal.richTextBox2.Invoke(new Action(() => Principal.richTextBox2.Text = Clientes));                
            }
            else
            {
                Principal.richTextBox2.Text = Clientes;
            }            
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

                AtualizaLogs();
            }
            catch
            {

            }
        }

        private static void AtualizaLogs()
        {
            string PathLog = @"C:\LanSolutions\ControlCenter-Servidor\Logs\";

            string Data = DateTime.Now.ToString("dd" + "MM" + "yyyy");          

            string PathLogHoje = PathLog + Data + @"\";

            string[] Logs = File.ReadAllLines(PathLogHoje + "Log.txt");

            if (Principal.richTextBox1.InvokeRequired)
            {
                Principal.richTextBox1.Invoke(new Action(() => Principal.richTextBox1.Text = string.Join(Environment.NewLine, Logs)));
            }
            else
            {
                Principal.richTextBox1.Text = string.Join(Environment.NewLine, Logs);
            }

        }
    }
}
