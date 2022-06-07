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
        static DataTable Produto = new DataTable();
        static DataTable ProdutosExistentes = new DataTable();
        static DataTable ProdutosInexistentes = new DataTable();          

        public static void SincronizaProdutos()
        {
            ExportaProdutos();
            FiltraProduto();
        }

        private static void FiltraProduto()
        {
            NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
            string SQL = "select idproduto_parceiro from lanprodutos";

            /*string SQL = "insert into lanprodutos (idproduto_parceiro, descricao, embalagem, codbarras1, codbarras2, codbarras3, codbarras4, qtunit1, qtunit2, qtunit3, qtunit4, permite_multiplicar, iduserinseriu) values (?,?,?,?,?,?,?,?,?,?,?,'N',0)";*/
                       
            NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(SQL, lanConexão);            

            try
            {
                lanConexão.Open();

                adapter.Fill(ProdutosExistentes);

                lanConexão.Dispose();

            }
            catch(Exception Ex)
            {
                Logger("Importação de Produtos: Erro ao importar: " + Ex.Message);
                return;
            }

            if (ProdutosInexistentes.Columns.Count == 0)
            {
                ProdutosInexistentes.Columns.Add("codprod");
                ProdutosInexistentes.Columns.Add("descricao");
                ProdutosInexistentes.Columns.Add("embalagem");

                ProdutosExistentes.PrimaryKey = new DataColumn[] { ProdutosExistentes.Columns[0] };
            }


            foreach (DataRow dr in Produto.Rows)
            {
                if (!ProdutosExistentes.Rows.Contains(dr[0].ToString()))
                {
                    ProdutosInexistentes.Rows.Add(dr[0], dr[1], dr[2]);
                }
            }

          

            if (ProdutosInexistentes.Rows.Count != 0)
            {
               
                foreach (DataRow rw in ProdutosInexistentes.Rows)
                {
                    ImportaProdutos(rw[0].ToString(), rw[1].ToString(), rw[2].ToString(), 1);
                }

                try
                {
                    Task.Run(() =>
                    {
                        Logger($"Importação de Produtos: {ProdutosInexistentes.Rows.Count} Produto(s) Importado(s)");
                    });

                   

                }

                catch (Exception Ex)
                {
                    Logger(Ex.ToString());
                }

            }

            try
            {
                Produto.Clear();
                ProdutosExistentes.Clear();
                ProdutosInexistentes.Clear();
              
            }
            catch(Exception Ex)
            {
                Logger(Ex.ToString()); 
            }

           
        }

        private static void ExportaProdutos()
        {
            OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
            string SQL = "select pcprodut.codprod, pcprodut.descricao, pcprodut.embalagem from pcprodut where revenda = 'S' and codepto <> 3";

            OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

            try
            {
                WinthorLogin.Open();

                adapter.Fill(Produto);

                WinthorLogin.Dispose();               

            }
            catch(Exception Ex)
            {
                Logger("Importação de Produtos: Erro ao importar: " + Ex.Message);
                return;
            }
        }

        private static void ImportaProdutos(string idproduto_parceiro, string descricao, string embalagem, int idparceiro)
        {
            NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
            string SQL = "insert into lanprodutos(idproduto_parceiro, descricao, embalagem, permite_multiplicar, idusuario_insercao, data_insercao, idparceiro, codbarras1, qtunit1) values(@idproduto_parceiro, @descricao, @embalagem, 'N', 9999, now(), @idparceiro, 0, 0) ";


            NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

            cmd.Parameters.Add(new NpgsqlParameter("@idproduto_parceiro", NpgsqlDbType.Integer)).Value = Convert.ToInt32(idproduto_parceiro);
            cmd.Parameters.Add(new NpgsqlParameter("@descricao", OleDbType.VarChar)).Value = descricao;
            cmd.Parameters.Add(new NpgsqlParameter("@embalagem", OleDbType.VarChar)).Value = embalagem;
            cmd.Parameters.Add(new NpgsqlParameter("@idparceiro", OleDbType.Integer)).Value = idparceiro;

            try
            {
                lanConexão.Open();

                cmd.ExecuteNonQuery();

                lanConexão.Dispose();

            }
            catch(Exception Ex)
            {
                Logs.Log.Logger("Importação de Produtos: Erro ao importar: " + Ex.Message);
                return;
            }

            
        }
    }
}
