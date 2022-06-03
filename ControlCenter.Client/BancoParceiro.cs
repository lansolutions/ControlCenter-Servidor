using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ControlCenter.Client
{
    class BancoParceiro
    {
        public static string StringConexao = string.Empty;
        
        private string Diretorio = @"C:\ControlCenter\bin\ControlCenter.ini";

        private static string ENCRYPTKEY = "AlLg09*2017";

        private class String
        {
            public static string Host { get; set; }
            public static string Usuario { get; set; }
            public static string Senha { get; set; }
            public static string Schema { get; set; }
            public static string Sgbd { get; set; }
            
        }

        public BancoParceiro()
        {
            Autenticador autenticador = new Autenticador();
            
           if(Autenticador.ParceiroAutenticado == true)
           {
                ConsultarDados();
                DescriptografarString();
                MontarString();
                TestarConexao();
           }
        }

        
        private void ConsultarDados()
        {
            NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
            string SQL = "select host, usuario_banco, senha_banco, schema_banco, sgbd from lansistema_parceiro where id = @id";

            NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

            cmd.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Integer)).Value = Convert.ToInt32(Autenticador.SituacaoParceiro.IdSistema);
                        
            try
            {
                lanConexão.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    rd.Read();

                    if (rd.HasRows)
                    {
                        String.Host = rd[0].ToString();
                        String.Usuario = rd[1].ToString();
                        String.Senha = rd[2].ToString();
                        String.Schema = rd[3].ToString();
                        String.Sgbd = rd[4].ToString();

                    }

                    else
                    {
                        throw new Exception();
                    }
                }

                lanConexão.Dispose();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.ToString());
            }
        }

        private void DescriptografarString()
        {
            String.Host = Cript.Decrypt(String.Host);
            String.Usuario = Cript.Decrypt(String.Usuario);
            String.Senha = Cript.Decrypt(String.Senha);
            String.Schema = Cript.Decrypt(String.Schema);
            String.Sgbd = Cript.Decrypt(String.Sgbd);

        }
        
        private void MontarString()
        {
            //Provider = MSDAORA;
            if (String.Sgbd.Contains("oracle"))
            {
                StringConexao = $"Provider = MSDAORA; Data Source=(DESCRIPTION=(CID=GTU_APP)(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={String.Host})(PORT=1521)))(CONNECT_DATA=(SID={String.Schema})(SERVER=DEDICATED))); User Id={String.Usuario}; Password={String.Senha};";
            }
            
            else if (String.Sgbd.Contains("MySQL"))
            {
                StringConexao = $"Provider=MSDAORA; Data Source={String.Host}/{String.Schema}; User Id={String.Usuario}; Password={String.Senha};";
            }
            
            else if (String.Sgbd.Contains("PostGres"))
            {
                StringConexao = $"Provider=MSDAORA; Data Source={String.Host}/{String.Schema}; User Id={String.Usuario}; Password={String.Senha};";
            }

        }
    

        private void TestarConexao()
        {          

            OleDbConnection WinthorLogin = new OleDbConnection(StringConexao);
            string SQL = "select sysdate from dual";
            OleDbCommand cmd = new OleDbCommand(SQL, WinthorLogin);

            try
            {
                WinthorLogin.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        if (!rd.HasRows)
                        {
                            MessageBox.Show("Não foi possível conectar ao banco do Sistema Parceiro, entre em contato com o suporte", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            MessageBox.Show("O Programa será finalizado", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            Application.Exit();

                        }
                    }
                }


                WinthorLogin.Dispose();
            }

            catch (Exception Ex)
            {
                MessageBox.Show("Não foi possível conectar ao banco, entre em contato com o suporte. " + Ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox.Show("O Programa será finalizado", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();

            }
        }
    }

  
}
