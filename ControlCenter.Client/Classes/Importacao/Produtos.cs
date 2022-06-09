using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using System.Data.OleDb;
using System.Data;
using System.Windows;
using System.Threading;
using System.Windows.Documents;
using static ControlCenter.Client.Classes.Logs.Log;

namespace ControlCenter.Client.Classes.Importacao
{
    public class Produtos
    {
        public Produtos()
        {
            try
            {
                ImportaProdutos();
            }
            catch (Exception Ex)
            {
                Logger("Importação de Produtos: Erro ao importar: " + Ex.Message);
            }
        }

        ~Produtos()
        {

        }
        private DataTable BuscarProdutosControlCenter()
        {
            NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
            string SQL = "select idproduto_parceiro from lanprodutos";
            DataTable dt = new DataTable();
                       
            NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(SQL, lanConexão);

            dt.Columns.Add("idproduto_parceiro");
          
            dt.PrimaryKey = new DataColumn[] { dt.Columns["idproduto_parceiro"] };

            try
            {
                lanConexão.Open();

                adapter.Fill(dt);

                lanConexão.Dispose();

            }
            catch(Exception Ex)
            {
                Logger("Importação de Produtos: Erro ao importar: " + Ex.Message); throw new Exception(Ex.Message);
            }

            return dt;
        }

        private DataTable BuscarProdutosSistemaParceiro()
        {
            OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
            string SQL = "select pcprodut.codprod, pcprodut.descricao, pcprodut.embalagem from pcprodut where revenda = 'S'";
            DataTable dt = new DataTable();

            OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

            dt.Columns.Add("codprod");
            dt.Columns.Add("descricao");
            dt.Columns.Add("embalagem");
            dt.PrimaryKey = new DataColumn[] { dt.Columns["codprod"] };

            try
            {
                WinthorLogin.Open();

                adapter.Fill(dt);

                WinthorLogin.Dispose();               

            }
            catch(Exception Ex)
            {
                Logger("Importação de Produtos: Erro ao importar: " + Ex.Message); throw new Exception(Ex.Message);
            }

            return dt;
        }

        private void ImportaProdutos()
        {
            DataTable dt = BuscarProdutosControlCenter().Copy();
            DataTable dt2 = BuscarProdutosSistemaParceiro().Copy();

            int ProdutosAdicionados = 0;

            dt2.AsEnumerable().Where(x => !dt.AsEnumerable().Any(y => y.Field<string>("idproduto_parceiro") == x.Field<string>("codprod"))).ToList().ForEach(x =>           
            {

                NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
                string SQL = "insert into lanprodutos(idproduto_parceiro, descricao, embalagem, permite_multiplicar, idusuario_insercao, data_insercao, idparceiro, codbarras1, qtunit1) values(@idproduto_parceiro, @descricao, @embalagem, 'N', 9999, now(), @idparceiro, 0, 0) ";
                NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

                cmd.Parameters.Add(new NpgsqlParameter("@idproduto_parceiro", NpgsqlDbType.Integer)).Value = Convert.ToInt32(x[0]);
                cmd.Parameters.Add(new NpgsqlParameter("@descricao", OleDbType.VarChar)).Value = x[1];
                cmd.Parameters.Add(new NpgsqlParameter("@embalagem", OleDbType.VarChar)).Value = x[2];
                cmd.Parameters.Add(new NpgsqlParameter("@idparceiro", OleDbType.Integer)).Value = 2;

                try
                {
                    lanConexão.Open();

                    cmd.ExecuteNonQuery(); ++ProdutosAdicionados;

                    lanConexão.Dispose();

                }
                catch (Exception Ex)
                {
                   Logger("Importação de Produtos: Erro ao importar: " + Ex.Message); throw new Exception(Ex.Message);                    
                }
            });

            if(ProdutosAdicionados != 0)
            {
                Logger($"Produtos: {ProdutosAdicionados} Novo(s) Produto(s) Adicionado(s)");
            }            
        }
    }
}
