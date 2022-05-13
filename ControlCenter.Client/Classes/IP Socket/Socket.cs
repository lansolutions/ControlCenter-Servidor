using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SuperSimpleTcp;
using static ControlCenter.Client.Classes.Logs.Log;
using static ControlCenter.Client.Classes.InstanciaJanelaPrincipal.Janela;


namespace ControlCenter.Client.Classes.IP_Socket
{
    public class Socket
    {
        public static SimpleTcpServer server;

        private static void InstanciarServidor()
        {
            string IP = "10.40.100.65";
            IP += ":9000";
            server = new SimpleTcpServer(IP);
            server.Events.ClientConnected += Events_ClientConnected;
            server.Events.ClientDisconnected += Events_ClientDisconnected;
            server.Events.DataReceived += Events_DataReceived;
            Principal.button1.Click += IniciarServidor;
            Principal.button2.Click += PararServidor;
            
            
        }

        public static string Teste()
        {
            string Clientes = string.Empty;

            foreach(string Cliente in server.GetClients())
            {
                Clientes += $"{Cliente} {Environment.NewLine}"; 
            }
            return Clientes;
        }

        private static void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Logger("Dados Recebidos");
        }

        private static void Events_ClientDisconnected(object sender, ConnectionEventArgs e)
        {
            Logger($"Cliente {e.IpPort} Desconectado");
            LogCliente("Desconectado", e.IpPort);
        }

        private static void Events_ClientConnected(object sender, ConnectionEventArgs e)
        {
            Logger($"Cliente {e.IpPort} Conectado");
            LogCliente("Conectado", e.IpPort);
        }

        public static void IniciarServidor(object sender, EventArgs e)
        {
            if (server == null)
            {
                InstanciarServidor();               
            }

            if (server.IsListening)
            {
                MessageBox.Show("Serviço já está Iniciado");
                return;
            }

            server.Start();
            Logger("Serviço Iniciado");
            Timer.IniciaTimer();
            
            Principal.pictureBox2.Visible = false; Principal.pictureBox1.Visible = true;
            Principal.label5.Visible = false; Principal.label4.Visible = true;
        }

        public static void PararServidor(object sender, EventArgs e)
        {
            if (!server.IsListening)
            {
                MessageBox.Show("Serviço já está Parado");
                return;
            }

            server.Stop();
            Logger("Serviço Parado");
            Timer.PararTimer();
            
            Principal.pictureBox2.Visible = true; Principal.pictureBox1.Visible = false;
            Principal.label5.Visible = true; Principal.label4.Visible = false;
        }

     

    }

    
}
