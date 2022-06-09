using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using ControlCenter.Client.Classes.IP_Socket;
using ControlCenter.Client.Classes.Logs;
using ControlCenter.Client.Classes.InstanciaJanelaPrincipal;
using ControlCenter.Client.Classes.Importacao;
using System.Threading;
using System.Security.Cryptography;

namespace ControlCenter.Client
{
    public partial class JanelaPrincipal : Form
    {
        public static Task T;
        public JanelaPrincipal()
        {
            InitializeComponent();
            Janela.Principal = this;
            new BancoPostGres();
            new BancoParceiro();
            
        }
        private void JanelaPrincipal_Load(object sender, EventArgs e)
        {
            Socket.IniciarServidor(null, null);
            Notificacao.ConfigurarMenuNotificacao();
        }

        public void Sincronizar(object sender, EventArgs e) 
        {
            if(Socket.server.IsListening == false)
            {
                MessageBox.Show("Serviço Está Parado");
                return;
            }

            if(T != null)
            {                
                if (T.IsCompleted == false && sender != null)
                {
                    MessageBox.Show("Sincronização não Realizada! Existe uma Sincronização em Andamento");
                    return;
                }
                else if(T.IsCompleted == false)
                {
                    Log.Logger("Sincronização não Realizada! Existe uma Sincronização em Andamento");
                    return;
                }
            }

            if (sender != null)
            {
                Log.Logger("Sincronização Manual Iniciada");
            }
            
            else
            {
                Log.Logger("Sincronização Automática Iniciada");
            }
            
            new Produtos();
            new Carregamentos();
            new ControleValidadeProdutos();
            new Bonus();            
            new Pedido();            
            new Usuarios();

            /*T =  Task.Run(() =>
            {
                
            });
             

            Task.WhenAll(T).Wait(); */

            Log.Logger("Sincronização Finalizada");

        }
    }
}
