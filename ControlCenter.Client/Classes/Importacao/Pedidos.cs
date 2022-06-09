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
        public Pedido()
        {
            try
            {
                new InserirPedido();
                new AtualizaPedido();
                new CargaDeProduto();
            }
            catch(Exception Ex)
            {
                Logger(Ex.ToString());
            }
        }

        internal class InserirPedido
        {
            public InserirPedido()
            {
                ImportaPedido();
            }
            private DataTable ExportaPedidoSistemaParceiro()
            {
                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select to_char(numped), cliente as descricao from pcpedc, pcclient where pcclient.codcli = pcpedc.CODCLI and pcpedc.DATA >= trunc(sysdate) - 30 and pcpedc.DTCANCEL is null and pcpedc.NUMCAR = 0 and posicao in('L','M','F')";
                DataTable dt = new DataTable();
                OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

                try
                {
                    WinthorLogin.Open();

                    adapter.Fill(dt);

                    WinthorLogin.Dispose();
                }
                catch (Exception Ex)
                {
                    Logger("Importação de Produtos: Erro ao Exportar: " + Ex.Message); throw new Exception(Ex.Message);
                }

                return dt;
            }

            private DataTable ExportaPedidoControlConter()
            {               
                NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
                string SQL = "select codpedido::varchar from lanexpedicao_pedido";
                
                DataTable dt = new DataTable();
               
                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(SQL, lanConexão);

                try
                {
                    lanConexão.Open();

                    adapter.Fill(dt);

                    lanConexão.Dispose();
                }
                catch (Exception Ex)
                {
                    Logger("Importação de Pedido: Erro ao importar: " + Ex.Message); throw new Exception(Ex.Message);
                }

                return dt;
            }

            public void ImportaPedido()
            {
                DataTable dt = ExportaPedidoControlConter().Copy();
                DataTable dt2 = ExportaPedidoSistemaParceiro().Copy();

                int PedidosAdicionados = 0;

                dt2.AsEnumerable().Where(x => !dt.AsEnumerable().Any(y => y.Field<string>(0) == x.Field<string>(0))).ToList().ForEach(x =>
                {
                    NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
                    string SQL = "insert into lanexpedicao_pedido(codpedido, descricao, data_importacao, idparceiro) values(@codpedido, @descricao, now(), @idparceiro)";
                    NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

                    cmd.Parameters.Add(new NpgsqlParameter("@codpedido", NpgsqlDbType.Bigint)).Value = Convert.ToInt64(x[0]);
                    cmd.Parameters.Add(new NpgsqlParameter("@descricao", OleDbType.VarChar)).Value = x[1];
                    cmd.Parameters.Add(new NpgsqlParameter("@idparceiro", NpgsqlDbType.Integer)).Value = 2;

                    try
                    {
                        lanConexão.Open();

                        cmd.ExecuteNonQuery(); ++PedidosAdicionados;

                        lanConexão.Dispose();

                    }
                    catch (Exception Ex)
                    {
                        Logger("Importação de Pedidos: Erro ao importar: " + Ex.Message); throw new Exception(Ex.Message);

                    }
                });
                                   

                if(PedidosAdicionados != 0)
                {
                    Logger($"Importação de Pedidos: {PedidosAdicionados} Pedido(s) Importado(s)");
                }


            }
        }

        internal class AtualizaPedido
        {
            public AtualizaPedido()
            {
                AtualizarPedido();
            }
            private DataTable ExportaPedidoSistemaParceiro()
            {
                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select to_char(numped) as numped, dtcancel from pcpedc where data >= trunc(sysdate)-30 and dtcancel is not null";
                DataTable dt = new DataTable();
                OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

                try
                {
                    WinthorLogin.Open();

                    adapter.Fill(dt);

                    WinthorLogin.Dispose();
                }
                catch (Exception Ex)
                {
                    Logger("Atualização de Pedido: Erro ao Exportar: " + Ex.Message); throw new Exception(Ex.Message);
                }

                return dt;
            }

            private DataTable ExportaPedidoControlConter()
            {
                NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
                string SQL = "select codpedido::varchar from lanexpedicao_pedido where data_cancelamento is null";
                DataTable dt = new DataTable();               

                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(SQL, lanConexão);

                try
                {
                    lanConexão.Open();

                    adapter.Fill(dt);

                    lanConexão.Dispose();                 
                }
                catch (Exception Ex)
                {
                    Logger("Atualização de Pedido: Erro ao Filtrar: " + Ex.Message); throw new Exception(Ex.Message);
                }

                return dt;
            }

            public void AtualizarPedido()
            {
                DataTable dt = ExportaPedidoControlConter().Copy();
                DataTable dt2 = ExportaPedidoSistemaParceiro().Copy();

                int PedidosAtualizados = 0;

                dt2.AsEnumerable().Where(x => !dt.AsEnumerable().Any(y => y.Field<string>("codpedido") == x.Field<string>("numped"))).ToList().ForEach(x =>
                {
                    NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
                    string SQL = "update lanexpedicao_pedido set data_cancelamento = @data_cancelamento, idusuario_cancelamento = 9999, idusuario_master_cancelamento = 9999 where codpedido = @codpedido ";
                    NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

                    cmd.Parameters.Add(new NpgsqlParameter("@data_cancelamento", OleDbType.Date)).Value = x[1];
                    cmd.Parameters.Add(new NpgsqlParameter("@codpedido", NpgsqlDbType.Bigint)).Value = Convert.ToInt64(x[0]);

                    try
                    {
                        lanConexão.Open();

                        cmd.ExecuteNonQuery(); ++PedidosAtualizados;

                        lanConexão.Dispose();

                    }
                    catch (Exception Ex)
                    {
                        Logger("Atualização de Pedidos: Erro ao Atualizar: " + Ex.Message); throw new Exception(Ex.Message);
                    }

                });
                    

                if (PedidosAtualizados != 0)
                {
                    Logger($"Atualização de Pedido: {PedidosAtualizados} Pedido(s) Atualizado(s)");
                }


            }
        }
        
        internal class CargaDeProduto
        {
            public CargaDeProduto()
            {
                CargaProduto();
            }
            private DataTable Pedido()
            {
                NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);

                string SQL = "select expe.id, expe.codpedido from lanexpedicao_pedido as expe where expe.id not in(select distinct idpedido from lanconferencia_pedido)";

                DataTable Pedido = new DataTable();                

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
                    Logger("Carga de Produtos: Erro ao Exportar Pedido: " + Ex.Message); throw new Exception(Ex.Message);
                    return null;
                }
            }

            private DataTable ExportaProduto(Int64 numPedido)
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
                    Logger("Carga de Produtos: Erro ao Exportar Produtos: " + Ex.Message); throw new Exception(Ex.Message);
                    return null;
                }


            }

            public void CargaProduto()
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
                            NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
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
                                Logger("Carga de Produtos: Erro ao Realizar Carga: " + Ex.Message); throw new Exception(Ex.Message);
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
