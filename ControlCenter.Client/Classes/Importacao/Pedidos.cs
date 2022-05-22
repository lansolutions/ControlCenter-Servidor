using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ControlCenter.Client.Classes.Logs.Log;
using System.Data.OleDb;

namespace ControlCenter.Client.Classes.Importacao
{
    public class Pedido
    {

        public static void SincronizaPedido()
        {
            InserirPedido.ImportaPedido();
            AtualizaPedido._AtualizaPedido();
            CargaDeProduto.CargaProduto();
        }

        internal class InserirPedido
        {
            private static DataTable ExportaPedido()
            {
                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select numped, cliente as descricao from pcpedc, pcclient where pcclient.codcli = pcpedc.CODCLI and pcpedc.DATA >= trunc(sysdate) - 30 and pcpedc.DTCANCEL is null and pcpedc.DTFAT is null and pcpedc.NUMCAR = 0 and posicao in('L','M','F')";
                DataTable Pedido = new DataTable();
                OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

                try
                {
                    WinthorLogin.Open();

                    adapter.Fill(Pedido);

                    WinthorLogin.Dispose();

                    return Pedido;

                }
                catch (Exception Ex)
                {
                    Logger("Importação de Produtos: Erro ao Exportar: " + Ex.Message);
                    return null;
                }


            }

            private static DataTable FiltraPedido()
            {
                DataTable PedidoBruto = ExportaPedido();

                DataTable PedidoFiltrado = PedidoBruto;

                NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                string SQL = "select codpedido from lanexpedicao_pedido";
                
                DataTable Pedido = new DataTable();
                Pedido.Columns.Add("codpedido");
                Pedido.PrimaryKey = new DataColumn[] { Pedido.Columns[0] };

                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(SQL, lanConexão);

                try
                {
                    lanConexão.Open();

                    adapter.Fill(Pedido);

                    lanConexão.Dispose();


                    for (int i = PedidoBruto.Rows.Count - 1; i >= 0; i--)
                    {
                        if (Pedido.Rows.Contains(PedidoBruto.Rows[i][0]))
                        {
                            DataRow dr = PedidoFiltrado.Rows[i];
                            dr.Delete();
                            PedidoFiltrado.AcceptChanges();
                        }

                    }

                    return PedidoFiltrado;

                }
                catch (Exception Ex)
                {
                    Logger("Importação de Pedido: Erro ao importar: " + Ex.Message);
                    MessageBox.Show(Ex.ToString());
                    return null;
                }
            }

            public static void ImportaPedido()
            {
                DataTable Pedido = FiltraPedido();

                foreach (DataRow rw in Pedido.Rows)
                {
                    NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                    string SQL = "insert into lanexpedicao_pedido(codpedido, descricao, data_importacao) values(@codpedido, @descricao, now())";
                    NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

                    cmd.Parameters.Add(new NpgsqlParameter("@codpedido", NpgsqlDbType.Bigint)).Value = Convert.ToInt64(rw[0]);
                    cmd.Parameters.Add(new NpgsqlParameter("@descricao", OleDbType.VarChar)).Value = rw[1];

                    try
                    {
                        lanConexão.Open();

                        cmd.ExecuteNonQuery();

                        lanConexão.Dispose();

                    }
                    catch (Exception Ex)
                    {
                        Logger("Importação de Pedidos: Erro ao importar: " + Ex.Message);

                    }
                }

                if (Pedido.Rows.Count != 0)
                {
                    Logger($"Importação de Pedidos: {Pedido.Rows.Count} Pedido(s) Importado(s)");

                }


            }
        }

        internal class AtualizaPedido
        {
            private static DataTable ExportaPedido()
            {
                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select numped as codpedido, dtcancel from pcpedc where data >= trunc(sysdate)-30 and dtcancel is not null";
                DataTable Pedido = new DataTable();
                OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

                try
                {
                    WinthorLogin.Open();

                    adapter.Fill(Pedido);

                    WinthorLogin.Dispose();

                    return Pedido;

                }
                catch (Exception Ex)
                {
                    Logger("Atualização de Pedido: Erro ao Exportar: " + Ex.Message);
                    return null;
                }


            }

            private static DataTable FiltraPedido()
            {
                DataTable PedidoBruto = ExportaPedido();

                DataTable PedidoFiltrado = PedidoBruto;

                NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                string SQL = "select codpedido from lanexpedicao_pedido where data_cancelamento is null";

                DataTable Pedido = new DataTable();
                Pedido.Columns.Add("codpedido");
                Pedido.PrimaryKey = new DataColumn[] { Pedido.Columns[0] };

                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(SQL, lanConexão);

                try
                {
                    lanConexão.Open();

                    adapter.Fill(Pedido);

                    lanConexão.Dispose();

                    for (int i = PedidoBruto.Rows.Count - 1; i >= 0; i--)
                    {
                        if (Pedido.Rows.Contains(PedidoBruto.Rows[i][0]))
                        {
                            DataRow dr = PedidoFiltrado.Rows[i];
                            dr.Delete();
                            PedidoFiltrado.AcceptChanges();
                        }

                    }

                    return PedidoFiltrado;

                }
                catch (Exception Ex)
                {
                    Logger("Atualização de Pedido: Erro ao Filtrar: " + Ex.Message);

                    return null;
                }
            }

            public static void _AtualizaPedido()
            {
                DataTable Pedido = FiltraPedido();

                foreach (DataRow rw in Pedido.Rows)
                {
                    NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                    string SQL = "update lanexpedicao_pedido set data_cancelamento = @data_cancelamento, idusuario_cancelamento = 9999, idusuario_master_cancelamento = 9999 where codpedido = @codpedido ";
                    NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

                    cmd.Parameters.Add(new NpgsqlParameter("@data_cancelamento", OleDbType.Date)).Value = rw[1];
                    cmd.Parameters.Add(new NpgsqlParameter("@codpedido", NpgsqlDbType.Bigint)).Value = Convert.ToInt64(rw[0]);

                    try
                    {
                        lanConexão.Open();

                        cmd.ExecuteNonQuery();

                        lanConexão.Dispose();

                    }
                    catch (Exception Ex)
                    {
                        Logger("Atualização de Pedidos: Erro ao Atualizar: " + Ex.Message);
                    }
                }

                if (Pedido.Rows.Count != 0)
                {
                    Logger($"Atualização de Pedido: {Pedido.Rows.Count} Pedido(s) Atualizado(s)");
                }


            }
        }
        
        internal class CargaDeProduto
        {
            private static DataTable Pedido()
            {
                NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                //string SQL = "select expe.id, expe.codcarreg from lanexpedicao_carregamento as expe, lanconferencia_carregamento as conf ";

                string SQL = "select expe.id, expe.codpedido from lanexpedicao_pedido as expe where expe.id not in(select distinct idpedido from lanconferencia_pedido)";

                DataTable Pedido = new DataTable();
                /*Carregamentos.Columns.Add("codcarreg");
                Carregamentos.PrimaryKey = new DataColumn[] { Carregamentos.Columns[0] };*/

                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(SQL, lanConexão);

                try
                {
                    lanConexão.Open();

                    adapter.Fill(Pedido);

                    lanConexão.Dispose();

                    return Pedido;

                }
                catch (Exception Ex)
                {
                    Logger("Carga de Produtos: Erro ao Exportar Pedido: " + Ex.Message);
                    return null;
                }
            }

            private static DataTable ExportaProduto(Int64 numPedido)
            {

                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select codprod, sum(qt) as qt from pcpedi where numped = ? group by codprod";

                DataTable Pedido = new DataTable();
                OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

                adapter.SelectCommand.Parameters.Add("@numped", OleDbType.BigInt).Value = numPedido;

                try
                {
                    WinthorLogin.Open();

                    adapter.Fill(Pedido);

                    WinthorLogin.Dispose();

                    return Pedido;

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
                DataTable _Pedido = Pedido();
                int CargaRealizada = 0;

                foreach (DataRow Row in _Pedido.Rows)
                {
                    DataTable Produtos = ExportaProduto(Convert.ToInt64(Row[1]));


                    if (Produtos.Rows.Count != 0)
                    {
                        foreach (DataRow row in Produtos.Rows)
                        {
                            NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                            string SQL = "insert into lanconferencia_pedido(codPedido, codprod, qt_real, idPedido) values(@codpedido, @codprod, @qt_real, @idpedido)";
                            NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

                            cmd.Parameters.Add(new NpgsqlParameter("@codpedido", NpgsqlDbType.Bigint)).Value = Row[1];
                            cmd.Parameters.Add(new NpgsqlParameter("@codprod", NpgsqlDbType.Integer)).Value = Convert.ToInt32(row[0]);
                            cmd.Parameters.Add(new NpgsqlParameter("@qt_real", NpgsqlDbType.Numeric)).Value = Convert.ToDecimal(row[1]);
                            cmd.Parameters.Add(new NpgsqlParameter("@idpedido", NpgsqlDbType.Numeric)).Value = Convert.ToInt32(Row[0]);

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

                if (CargaRealizada != 0)
                {
                    Logger("Carga de Produtos: Carga Realizada");
                }


            }
        }

    }
}
