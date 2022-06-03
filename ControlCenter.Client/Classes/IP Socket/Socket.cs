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

        public static object Code128Rendering { get; private set; }

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

            try
            {
                string Dados = Encoding.UTF8.GetString(e.Data);

               

                if (Dados.Contains("ForcarSincronizacao"))
                {
                    Principal.Sincronizar(null, null);
                }
                
                else if(Dados.Contains("ImagemEtiqueta"))
                {
                    List<string> Impressoras = PrinterSettings.InstalledPrinters.OfType<string>().ToList();

                    string Impressora = string.Empty;

                    foreach (var item in Impressoras)
                    {
                        if (item.Contains("4BARCODE"))
                        {
                            Impressora = item;
                            break;
                        }
                    }

                    if(Impressora == string.Empty)
                    {
                        Logger("Impressora não encontrada");
                        return;
                    }

                    PrintDocument pd = new PrintDocument();

                    pd.PrinterSettings.PrinterName = Impressora;

                    StringFormat AlinhadoEsquerda = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Near
                    };
                    StringFormat AlinhadoCentro = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Near
                    };

                    //string Dado = $"ImagemEtiqueta;{Id};{Descricao};{Peso};{CodBarra}";

                    string Id = Dados.Split(';')[1];
                    string CodBarra = Dados.Split(';')[3] + "*" + Dados.Split(';')[4];
                    string Descricao = Dados.Split(';')[2];
                    string Peso = Dados.Split(';')[3];

                    Image CodBarras = GenCode128.Code128Rendering.MakeBarcodeImage(CodBarra, 2, true);

                    /*Pen blackPen = new Pen(Color.FromArgb(255, 0, 0, 0), 5);
                    Rectangle rect = new Rectangle(11, 4, 215, 100);*/


                    pd.PrintPage += (s, f) =>
                    {
                        //f.Graphics.DrawRectangle(blackPen, rect);
                        
                        if (Descricao.Length >= 25)
                        {
                            f.Graphics.DrawString(Descricao, new Font("Arial", 11, FontStyle.Bold), Brushes.Black, new Rectangle(11, 4, 215, 100), AlinhadoCentro);
                        }

                        else
                        {
                            f.Graphics.DrawString(Descricao, new Font("Arial", 15, FontStyle.Bold), Brushes.Black, new Rectangle(11, 4, 215, 100), AlinhadoCentro);
                        }

                        f.Graphics.DrawString($"ID: {Id}", new Font("Arial", 12, FontStyle.Regular), Brushes.Black, new Rectangle(11, 50, 215, 100), AlinhadoEsquerda);

                        f.Graphics.DrawString($"Peso: {Peso}", new Font("Arial", 12, FontStyle.Regular), Brushes.Black, new Rectangle(125, 50, 215, 100), AlinhadoEsquerda);

                        f.Graphics.DrawImage(CodBarras, new Rectangle(11, 80, 210, 25));
                    };

                    pd.Print();
                    pd.Dispose();
                }

                else if (Dados.Contains("RequisicaoDePeso"))
                {
                    string IpClient = new ConfiguracaoDosParametros().DadosParametros.Where(x => x.Nome == "ENDEREÇO IPV4 PORTA COM > PORTA LAN").FirstOrDefault().Valor;
                    using (SimpleTcpClient client = new SimpleTcpClient($"{IpClient}:3700"))
                    {
                        client.Connect();

                        client.Send("RequisicaoDePeso");

                        client.Events.DataSent += (s, f) => { client.Disconnect(); };
                    }
                    
                }

                else if (Dados.Contains("Peso;"))
                {
                    string IpClient = e.IpPort.Split(':')[0];
                    using (SimpleTcpClient client = new SimpleTcpClient($"{IpClient}:3700"))
                    {
                        Dados = Dados.Replace("Peso;", "");
                        client.Connect();

                        client.Send(Dados);

                        client.Events.DataSent += (s, f) => { client.Disconnect(); };
                    }
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
