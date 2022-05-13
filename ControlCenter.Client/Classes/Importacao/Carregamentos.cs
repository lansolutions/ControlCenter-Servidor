using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using System.Data.OleDb;
using System.Data;
using static ControlCenter.Client.Classes.Logs.Log;
using System.Windows;

namespace ControlCenter.Client.Classes.Importacao
{
    public class Carregamentos
    {
        public static void SincronizaCarregamentos()
        {
            InserirCarregamentos.ImportaCarregamento();
            AtualizaCarregamentos.AtualizaCarregamento();
            CargaDeProduto.CargaProduto();
        }

        internal class InserirCarregamentos
        {
            private static DataTable ExportaCarregamento()
            {
                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select numcar as codcarreg, destino from pccarreg where datamon >= trunc(sysdate)-30 and numnotas <> 0";
                DataTable Carregamento = new DataTable();
                OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

                try
                {
                    WinthorLogin.Open();

                    adapter.Fill(Carregamento);

                    WinthorLogin.Dispose();

                    return Carregamento;

                }
                catch (Exception Ex)
                {
                    Logger("Importação de Produtos: Erro ao Exportar: " + Ex.Message);
                    return null;
                }


            }

            private static DataTable FiltraCarregamento()
            {
                DataTable CarregamentosBruto = ExportaCarregamento();

                DataTable CarregamentoFiltrado = CarregamentosBruto;

                NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                string SQL = "select codcarreg from lanexpedicao_carregamento";

                DataTable Carregamentos = new DataTable();
                Carregamentos.Columns.Add("codcarreg");
                Carregamentos.PrimaryKey = new DataColumn[] { Carregamentos.Columns[0] };                

                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(SQL, lanConexão);

                try
                {
                    lanConexão.Open();

                    adapter.Fill(Carregamentos);

                    lanConexão.Dispose();


                    for (int i = CarregamentosBruto.Rows.Count -1; i >= 0; i--)
                    {
                        if (Carregamentos.Rows.Contains(CarregamentosBruto.Rows[i][0]))
                        {
                            DataRow dr = CarregamentoFiltrado.Rows[i];
                            dr.Delete();
                            CarregamentoFiltrado.AcceptChanges();                            
                        }

                    }
                   
                    return CarregamentoFiltrado;

                }
                catch (Exception Ex)
                {
                    Logger("Importação de Produtos: Erro ao importar: " + Ex.Message);
                    MessageBox.Show(Ex.ToString());

                   
                    return null;
                }
            }

            public static void ImportaCarregamento()
            {
                DataTable Carregamento = FiltraCarregamento();

                foreach (DataRow rw in Carregamento.Rows)
                {
                    NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                    string SQL = "insert into lanexpedicao_carregamento(codcarreg, destino, data_importacao) values(@codcarreg, @destino, now())";
                    NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

                    cmd.Parameters.Add(new NpgsqlParameter("@codcarreg", NpgsqlDbType.Integer)).Value = Convert.ToInt32(rw[0]);
                    cmd.Parameters.Add(new NpgsqlParameter("@destino", OleDbType.VarChar)).Value = rw[1];                    

                    try
                    {
                        lanConexão.Open();

                        cmd.ExecuteNonQuery();

                        lanConexão.Dispose();

                    }
                    catch (Exception Ex)
                    {                        
                        Logger("Importação de Carregamentos: Erro ao importar: " + Ex.Message);                       
                       
                    }
                }

                if(Carregamento.Rows.Count != 0)
                {
                    Logger($"Importação de Carregamentos: {Carregamento.Rows.Count} Carregamento(s) Importado(s)");

                }


            }
        }
       
        internal class AtualizaCarregamentos
        {
            private static DataTable ExportaCarregamento()
            {
                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select numcar as codcarreg, dt_cancel from pccarreg where datamon >= trunc(sysdate)-30 and dt_cancel is not null";
                DataTable Carregamento = new DataTable();
                OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

                try
                {
                    WinthorLogin.Open();

                    adapter.Fill(Carregamento);

                    WinthorLogin.Dispose();

                    return Carregamento;

                }
                catch (Exception Ex)
                {
                    Logger("Atualização de Carregamentos: Erro ao Exportar: " + Ex.Message);
                    return null;
                }


            }

            private static DataTable FiltraCarregamento()
            {
                DataTable CarregamentosBruto = ExportaCarregamento();

                DataTable CarregamentoFiltrado = CarregamentosBruto;

                NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                string SQL = "select codcarreg from lanexpedicao_carregamento where data_cancelamento is null";

                DataTable Carregamentos = new DataTable();
                Carregamentos.Columns.Add("codcarreg");
                Carregamentos.PrimaryKey = new DataColumn[] { Carregamentos.Columns[0] };

                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(SQL, lanConexão);

                try
                {
                    lanConexão.Open();

                    adapter.Fill(Carregamentos);

                    lanConexão.Dispose();

                    for (int i = CarregamentosBruto.Rows.Count - 1; i >= 0; i--)
                    {
                        if (Carregamentos.Rows.Contains(CarregamentosBruto.Rows[i][0]))
                        {
                            DataRow dr = CarregamentoFiltrado.Rows[i];
                            dr.Delete();
                            CarregamentoFiltrado.AcceptChanges();                            
                        }

                    }

                    return CarregamentoFiltrado;

                }
                catch (Exception Ex)
                {
                    Logger("Atualização de Carregamentos: Erro ao Filtrar: " + Ex.Message);
                    
                    return null;
                }
            }

            public static void AtualizaCarregamento()
            {
                DataTable Carregamento = FiltraCarregamento();

                foreach (DataRow rw in Carregamento.Rows)
                {
                    NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                    string SQL = "update lanexpedicao_carregamento set data_cancelamento = @data_cancelamento, idusuario_cancelamento = 9999, idusuario_master_cancelamento = 9999 where codcarreg = @codcarreg ";
                    NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);
                   
                    cmd.Parameters.Add(new NpgsqlParameter("@data_cancelamento", OleDbType.Date)).Value = rw[1];
                    cmd.Parameters.Add(new NpgsqlParameter("@codcarreg", NpgsqlDbType.Integer)).Value = Convert.ToInt32(rw[0]);

                    try
                    {
                        lanConexão.Open();

                        cmd.ExecuteNonQuery();

                        lanConexão.Dispose();

                    }
                    catch (Exception Ex)
                    {
                        Logger("Atualização de Produtos: Erro ao Atualizar: " + Ex.Message);
                    }
                }

                if (Carregamento.Rows.Count != 0)
                {
                    Logger($"Atualização de Carregamentos: {Carregamento.Rows.Count} Carregamento(s) Atualizado(s)");
                }


            }
        }

        internal class CargaDeProduto
        {
            private static DataTable Carregamento()
            {    
                NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                //string SQL = "select expe.id, expe.codcarreg from lanexpedicao_carregamento as expe, lanconferencia_carregamento as conf ";

                string SQL = "select expe.id, expe.codcarreg from lanexpedicao_carregamento as expe where expe.id not in(select distinct idcarregamento from lanconferencia_carregamento)";

                DataTable Carregamentos = new DataTable();
                /*Carregamentos.Columns.Add("codcarreg");
                Carregamentos.PrimaryKey = new DataColumn[] { Carregamentos.Columns[0] };*/

                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(SQL, lanConexão);

                try
                {
                    lanConexão.Open();

                    adapter.Fill(Carregamentos);

                    lanConexão.Dispose();

                    return Carregamentos;

                }
                catch (Exception Ex)
                {
                    Logger("Carga de Produtos: Erro ao Exportar Carregamentos: " + Ex.Message);
                    return null;
                }
            }

            private static DataTable ExportaProduto(int numcar)
            {
              
                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select pcpedi.codprod, sum(pcpedi.qt) as qt from pcpedi where numcar = ? group by pcpedi.codprod";
                
                DataTable Carregamento = new DataTable();
                OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

                adapter.SelectCommand.Parameters.Add("@numcar", OleDbType.Integer).Value = numcar;

                try
                {
                    WinthorLogin.Open();

                    adapter.Fill(Carregamento);

                    WinthorLogin.Dispose();

                    return Carregamento;

                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.ToString());
                    Logger("Carga de Produtos: Erro ao Exportar Produtos: " + Ex.Message);
                    return null;
                }


            }          

            public static void CargaProduto()
            {
                DataTable Carregamentos = Carregamento();
                int CargaRealizada = 0;

                foreach (DataRow Row in Carregamentos.Rows)
                {
                    DataTable Produtos = ExportaProduto(Convert.ToInt32(Row[1]));
                   
                    
                    if(Produtos.Rows.Count != 0)
                    {
                        foreach (DataRow row in Produtos.Rows)
                        {
                            NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                            string SQL = "insert into lanconferencia_carregamento(codcarreg, codprod, qt_real, idcarregamento) values(@codcarreg, @codprod, @qt_real, @idcarregamento)";
                            NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

                            cmd.Parameters.Add(new NpgsqlParameter("@codcarreg", NpgsqlDbType.Integer)).Value = Convert.ToInt32(Row[1]);
                            cmd.Parameters.Add(new NpgsqlParameter("@codprod", NpgsqlDbType.Integer)).Value = Convert.ToInt32(row[0]);
                            cmd.Parameters.Add(new NpgsqlParameter("@qt_real", NpgsqlDbType.Numeric)).Value = Convert.ToDecimal(row[1]);
                            cmd.Parameters.Add(new NpgsqlParameter("@idcarregamento", NpgsqlDbType.Numeric)).Value = Convert.ToInt32(Row[0]);

                            try
                            {
                                lanConexão.Open();

                                cmd.ExecuteNonQuery();

                                lanConexão.Dispose();

                            }
                            catch (Exception Ex)
                            {
                                Logger("Carga de Produtos: Erro ao Realizar Carga: " + Ex.Message);
                            }
                        }
                        CargaRealizada += 1;

                    }
                }

                if(CargaRealizada != 0)
                {
                    Logger("Carga de Produtos: Carga Realizada");
                }
               

            }
        }

    }
}
