using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SuperSimpleTcp;
using ControlCenter.Client;
using static ControlCenter.Client.Classes.Logs.Log;
using static ControlCenter.Client.Classes.InstanciaJanelaPrincipal.Janela;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using GenCode128;

namespace ControlCenter.Client.Classes.IP_Socket
{
    public class Socket
    {
        public static SimpleTcpServer server;

        private static void InstanciarServidor()
        {
            string IP = string.Empty;
            try
            {
                string localIP = string.Empty;

                using (System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;

                    localIP = endPoint.Address.ToString();
                    localIP = endPoint.Address.ToString();

                    IP = localIP;

                }
            }
            catch (Exception)
            {
                string nomeMaquina = Dns.GetHostName();
                IPAddress[] ipLocal = Dns.GetHostAddresses(nomeMaquina);
                IP = ipLocal.LastOrDefault().ToString();
            }

            IP += ":3700";
            server = new SimpleTcpServer(IP);
            server.Events.ClientConnected += Events_ClientConnected;
            server.Events.ClientDisconnected += Events_ClientDisconnected;
            server.Events.DataReceived += Events_DataReceived;
            Principal.button1.Click += IniciarServidor;
            Principal.button2.Click += PararServidor;
            
            
        }
      
        private static void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Logger("Dados Recebidos");

            try
            {
                string Dados = Encoding.UTF8.GetString(e.Data);              

                if (Dados.Contains("ForcarSincronizacao"))
                {
                    Principal.Sincronizar(null, null);
                }
            }
            catch(Exception Ex)
            {
                Logger(Ex.ToString());
            }
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
