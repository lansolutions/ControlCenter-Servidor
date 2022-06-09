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
        public Carregamentos()
        {
            try
            {
                
                new InserirCarregamentos();               
                new AtualizaCarregamentos();               
                new CargaDeProduto();
                new AtualizarCargaDeProduto();
            }
            catch (Exception Ex)
            {
                Logger(Ex.ToString());
            }
            
        }

        ~Carregamentos()
        {

        }

        internal class InserirCarregamentos
        {
            public InserirCarregamentos()
            {
                ImportaCarregamento();
            }
            private DataTable ExportaCarregamentoSistemaParceiro()
            {
                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select to_char(numcar) as numcar, destino from pccarreg where datamon >= trunc(sysdate)-30 and numnotas <> 0";
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
                    Logger("Importação de Carregamentos: Erro ao Exportar: " + Ex.Message);
                    return null;
                }
            }

            private DataTable ExportaCarregamentoControlCenter()
            {

                NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
                string SQL = "select codcarreg::varchar from lanexpedicao_carregamento";

                DataTable dt = new DataTable();
                /*dt.Columns.Add("codcarreg");
                dt.PrimaryKey = new DataColumn[] { dt.Columns[0] };*/

                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(SQL, lanConexão);

                try
                {
                    lanConexão.Open();

                    adapter.Fill(dt);

                    lanConexão.Dispose();
                }
                catch (Exception Ex)
                {
                    Logger("Importação de Produtos: Erro ao importar: " + Ex.Message); throw new Exception(Ex.ToString());
                }

                return dt;
            }

            public void ImportaCarregamento()
            {
                DataTable dt = ExportaCarregamentoControlCenter().Copy();
                DataTable dt2 = ExportaCarregamentoSistemaParceiro().Copy();

                
                int CarregamentosAdicionados = 0;

                dt2.AsEnumerable().Where(x => !dt.AsEnumerable().Any(y => y.Field<string>(0) == x.Field<string>(0))).ToList().ForEach(x =>
                {
                    NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
                    string SQL = "insert into lanexpedicao_carregamento(codcarreg, destino, data_importacao, idparceiro) values(@codcarreg, @destino, now(), @idparceiro)";
                    NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);
                    
                    cmd.Parameters.Add(new NpgsqlParameter("@codcarreg", NpgsqlDbType.Integer)).Value = Convert.ToInt32(x[0].ToString());
                    cmd.Parameters.Add(new NpgsqlParameter("@destino", OleDbType.VarChar)).Value = x[1].ToString();
                    cmd.Parameters.Add(new NpgsqlParameter("@idparceiro", NpgsqlDbType.Integer)).Value = 2;

                    try
                    {
                        lanConexão.Open();

                        cmd.ExecuteNonQuery(); ++CarregamentosAdicionados;

                        lanConexão.Dispose();

                    }
                    catch (Exception Ex)
                    {
                        Logger("Importação de Carregamentos: Erro ao importar: " + Ex.Message); throw new Exception(Ex.ToString());
                    }
                });

                if (CarregamentosAdicionados != 0)
                {
                    Logger($"Importação de Carregamentos: {CarregamentosAdicionados} Carregamento(s) Importado(s)");
                }

            }
        }

        internal class CargaDeProduto
        {
            public CargaDeProduto()
            {
                CargaProduto();
            }

            ~CargaDeProduto()
            {

            }
            private DataTable Carregamento()
            {
                NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
                string SQL = "select expe.id, expe.codcarreg from lanexpedicao_carregamento as expe where expe.id not in(select distinct idcarregamento from lanconferencia_carregamento)";

                DataTable Carregamentos = new DataTable();
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
                    Logger("Carga de Produtos: Erro ao Exportar Carregamentos: " + Ex.Message); throw new Exception(Ex.ToString());                    
                }
            }

            private DataTable ExportaProduto(int numcar)
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
                    Logger("Carga de Produtos: Erro ao Exportar Produtos: " + Ex.Message); throw new Exception(Ex.ToString());
                    return null;
                }


            }

            public void CargaProduto()
            {
                DataTable Carregamentos = Carregamento();
                int CargaRealizada = 0;

                foreach (DataRow Row in Carregamentos.Rows)
                {
                    DataTable Produtos = ExportaProduto(Convert.ToInt32(Row[1]));


                    if (Produtos.Rows.Count != 0)
                    {
                        foreach (DataRow row in Produtos.Rows)
                        {
                            NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
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

                if (CargaRealizada != 0)
                {
                    Logger("Carga de Produtos: Carga Realizada");
                }


            }
        }

        internal class AtualizaCarregamentos
        {
            public AtualizaCarregamentos()
            {
                AtualizaCarregamento();
            }

            ~AtualizaCarregamentos()
            {

            }
            private DataTable ExportaCarregamentoSistemaParceiro()
            {
                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select to_char(numcar) as numcar, to_char(dt_cancel) as dt_cancel from pccarreg where datamon >= trunc(sysdate)-30 and dt_cancel is not null";
                
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
                    Logger("Atualização de Carregamentos: Erro ao Exportar: " + Ex.Message); throw new Exception(Ex.ToString());
                }

                return dt;
            }

            private DataTable ExportaCarregamentoControlCenter()
            {
                NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
                string SQL = "select codcarreg::varchar, data_cancelamento::varchar from lanexpedicao_carregamento where data_cancelamento is null";

                DataTable dt = new DataTable();
                dt.Columns.Add("codcarreg");
                dt.PrimaryKey = new DataColumn[] { dt.Columns[0] };

                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(SQL, lanConexão);

                try
                {
                    lanConexão.Open();

                    adapter.Fill(dt);

                    lanConexão.Dispose();                  

                }
                catch (Exception Ex)
                {
                    Logger("Atualização de Carregamentos: Erro ao Filtrar: " + Ex.Message); throw new Exception(Ex.ToString());                    
                }

                return dt;
            }

            public void AtualizaCarregamento()
            {
                DataTable dt = ExportaCarregamentoControlCenter().Copy();
                DataTable dt2 = ExportaCarregamentoSistemaParceiro().Copy();

                int CarregamentosAtualizados = 0;

                dt2.AsEnumerable().Where(x => !dt.AsEnumerable().Any(y => y.Field<string>("codcarreg") == x.Field<string>("numcar"))).ToList().ForEach(x =>
                {
                    NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
                    string SQL = "update lanexpedicao_carregamento set data_cancelamento = @data_cancelamento, idusuario_cancelamento = 9999, idusuario_master_cancelamento = 9999 where codcarreg = @codcarreg ";
                    NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

                    cmd.Parameters.Add(new NpgsqlParameter("@data_cancelamento", OleDbType.Date)).Value = Convert.ToDateTime(x[1]);
                    cmd.Parameters.Add(new NpgsqlParameter("@codcarreg", NpgsqlDbType.Integer)).Value = Convert.ToInt32(x[0]);

                    try
                    {
                        lanConexão.Open();

                        cmd.ExecuteNonQuery(); ++CarregamentosAtualizados;

                        lanConexão.Dispose();

                    }
                    catch (Exception Ex)
                    {
                        Logger("Atualização de Produtos: Erro ao Atualizar: " + Ex.Message); throw new Exception(Ex.ToString());
                    }

                });

                if (CarregamentosAtualizados != 0)
                {
                    Logger($"Atualização de Carregamentos: {CarregamentosAtualizados} Carregamento(s) Atualizado(s)");
                }


            }
        }

        internal class AtualizarCargaDeProduto
        {
            public AtualizarCargaDeProduto()
            {
                AtualizarCarga();
            }

            ~AtualizarCargaDeProduto()
            {

            }

            private DataTable ConferenciaCarregamentoControlCenter() 
            {
                NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
                string SQL = "select codcarreg::varchar, round(qt_real, 6) as qt_real, codprod from sulfrios.lanconferencia_carregamento where idcarregamento in(select id from sulfrios.lanexpedicao_carregamento where data_cancelamento is null and DATE_TRUNC('day', data_importacao) >= current_date - interval '4 days') order by codcarreg desc";
                DataTable Carregamentos = new DataTable();               

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

            private DataTable ConferenciaCarregamentoSistemaParceiro()
            {
                var dt = ConferenciaCarregamentoControlCenter().AsEnumerable().Select(x => new { codcarreg = x.Field<string>("codcarreg") }).Distinct().ToList();

                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select pcpedi.qt, to_char(numcar) as numcar, to_char(codprod) as codprod from pcpedi where numcar = ?";

                DataTable dt2 = new DataTable();
                OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

                adapter.SelectCommand.Parameters.Add("@numcar", OleDbType.Integer);

                try
                {
                    WinthorLogin.Open();

                    foreach (var row in dt)
                    {
                        adapter.SelectCommand.Parameters["@numcar"].Value = Convert.ToInt32(row.codcarreg);
                        adapter.Fill(dt2);
                    }                   

                    WinthorLogin.Dispose();
                }
                catch (Exception Ex)
                {
                    Logger("Carga de Produtos: Erro ao Exportar Produtos: " + Ex.Message); throw new Exception(Ex.ToString());
                }

                return dt2;
            }           
            
            public void AtualizarCarga()
            {         
                int ProdutosAlterados = 0;
                
                var CarregamentoSistemaParceiroAgrupado = from temp in ConferenciaCarregamentoSistemaParceiro().AsEnumerable()
                            group temp by new
                            {
                                NumCar = temp.Field<string>("numcar"),
                                CodProd = temp.Field<string>("codprod")
                            } into xGroup
                            select new
                            {
                                NumCar = xGroup.Key.NumCar,
                                CodProd = xGroup.Key.CodProd,
                                Qt = xGroup.Sum(temp => temp.Field<decimal>("qt"))
                            };


                var CarregamentoControlCenterAgrupado = from temp in ConferenciaCarregamentoControlCenter().AsEnumerable()
                                                          group temp by new
                                                          {
                                                              CodCarreg = temp.Field<string>("codcarreg"),
                                                              CodProd = temp.Field<int>("codprod").ToString()
                                                          } into xGroup
                                                          select new
                                                          {
                                                              CodCarreg = xGroup.Key.CodCarreg,
                                                              CodProd = xGroup.Key.CodProd,
                                                              Qt = xGroup.Sum(temp => temp.Field<decimal>("qt_real"))
                                                          };


                CarregamentoSistemaParceiroAgrupado.Where(x => CarregamentoControlCenterAgrupado.Any(y => y.CodCarreg == x.NumCar && y.CodProd == x.CodProd && y.Qt != x.Qt)).ToList().ForEach(x =>
                {
                    NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
                    string SQL = "update sulfrios.lanconferencia_carregamento set qt_real = @qt_real where codcarreg = @codcarreg and codprod = @codprod";
                    NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

                    cmd.Parameters.Add("@qt_real", NpgsqlDbType.Numeric);
                    cmd.Parameters.Add("@codcarreg", NpgsqlDbType.Integer);
                    cmd.Parameters.Add("@codprod", NpgsqlDbType.Integer);

                    try
                    {
                        lanConexão.Open();

                        cmd.Parameters["@qt_real"].Value = x.Qt;
                        cmd.Parameters["@codcarreg"].Value = Convert.ToInt32(x.NumCar);
                        cmd.Parameters["@codprod"].Value = Convert.ToInt32(x.CodProd);

                        cmd.ExecuteNonQuery(); ProdutosAlterados++;

                        lanConexão.Dispose();

                    }
                    catch (Exception Ex)
                    {
                        Logger("Carga de Produtos: Erro ao Atualizar Carga: " + Ex.Message); throw new Exception(Ex.ToString());
                    }

                });

                if (ProdutosAlterados != 0)
                {
                    Logger($"Atualização de Produtos: {ProdutosAlterados} Produto(s) atualizado(s)");
                }
            }
            
        }


    }
}
