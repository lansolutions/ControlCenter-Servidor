using Npgsql;
using NpgsqlTypes;
using System;
using System.Windows.Forms;

namespace ControlCenter.Client
{
    public class Autenticador
    {
        public static bool ParceiroAutenticado;
        
        public Autenticador()
        {
            Parceiro();
            ParceiroAutenticado = AutenticarParceiro();
        }
        
        public class ParceiroCredenciado
        {
            public static string RazaoSocial { get; set; }
            public static string Fantasia { get; set; }
            public static string CNPJ { get; set; }
            public static string IE { get; set; }

        }

        public class SituacaoParceiro
        {
            public static string IdParceiro { get; set; }
            public static string IdSistema { get; set; }
            public static string Situacao { get; set; }
            public static string Observacao { get; set; }
        }

        private void Parceiro()
        {
            ParceiroCredenciado.RazaoSocial = "SF DISTRIBUIDORA DE ALIMENTOS LTDA";
            ParceiroCredenciado.Fantasia = "SULFRIOS";
            ParceiroCredenciado.CNPJ = "07244026000124";
            ParceiroCredenciado.IE = "066513840";
        }

        private void ConsultarParceiro()
        {
            NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
            string SQL = "select id, situacao, observacao, idsistema from lanparceiro where cnpj = @cnpj";

            NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

            cmd.Parameters.Add(new NpgsqlParameter("@cnpj", NpgsqlDbType.Varchar)).Value = ParceiroCredenciado.CNPJ;

            try
            {
                lanConexão.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        if (rd.HasRows)
                        {
                            SituacaoParceiro.IdParceiro = rd[0].ToString();
                            SituacaoParceiro.Situacao = rd[1].ToString();
                            SituacaoParceiro.Observacao = rd[2].ToString();
                            SituacaoParceiro.IdSistema = rd[3].ToString();
                        }
                        else
                        {
                            throw new Exception("Parceiro Não foi Localizado.");
                        }
                    }


                }

                lanConexão.Dispose();


            }
            catch (Exception Ex)
            {
                lanConexão.Dispose();
                MessageBox.Show("Erro: " + Ex.Message);
            }
        }

        private bool AutenticarParceiro()
        {
            bool Autenticado = false;

            try
            {
                ConsultarParceiro();

                if (SituacaoParceiro.Situacao == "I")
                {
                    MessageBox.Show($"O Parceiro { ParceiroCredenciado.RazaoSocial} Está Inativo e não poderá Iniciar o sistema.\nMotivo:{  SituacaoParceiro.Observacao}");
                    System.Windows.Forms.Application.Exit();
                }

                else
                {
                    Autenticado = true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return Autenticado;
        }
    }
}
