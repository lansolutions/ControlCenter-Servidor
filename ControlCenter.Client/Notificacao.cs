using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ControlCenter.Client.Classes.InstanciaJanelaPrincipal.Janela;
using static ControlCenter.Client.Classes.IP_Socket.Socket;

namespace ControlCenter.Client
{
    public class Notificacao
    {
        public static void ConfigurarMenuNotificacao()
        {
            var contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add("Sincronizar", Principal.Sincronizar);
            contextMenu.MenuItems.Add("Iniciar Serviço", IniciarServidor);
            contextMenu.MenuItems.Add("Parar Serviço", PararServidor);
            contextMenu.MenuItems.Add("Fechar", Fechar);
            Principal.notifyIcon1.ContextMenu = contextMenu;

            Principal.notifyIcon1.ShowBalloonTip(1, "Control Center ", "Serviço Iniciado", ToolTipIcon.Info);

            Principal.Resize += Minimizar;
            Principal.FormClosing += Fechar_Minimizar;
            Principal.notifyIcon1.DoubleClick += DuploCliqueNotificacao;
        }

        public static void Minimizar(object sender, EventArgs e)
        {           
            bool BarraCursos = Screen.GetWorkingArea(Principal).Contains(Cursor.Position);

            if(Principal.WindowState == FormWindowState.Minimized && BarraCursos)
            {
                Principal.ShowInTaskbar = false;
                Principal.Hide();
            }           
        }

        public static void Fechar_Minimizar(object sender, FormClosingEventArgs e)
        {
            if(e.CloseReason == CloseReason.UserClosing)
            {               
                e.Cancel = true; 
                Principal.ShowInTaskbar = false;
                Principal.Hide();                
            }
        }

        public static void Fechar(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public static void DuploCliqueNotificacao(object sender ,EventArgs e)
        {
            Principal.WindowState = FormWindowState.Normal;
            Principal.ShowInTaskbar = true;
            Principal.Visible = true;
        }
    }
}
