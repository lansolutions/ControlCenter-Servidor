﻿using Npgsql;
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
    public class Bonus
    {

        public static void SincronizaBonus()
        {
            InserirBonus.ImportaBonus();
            AtualizaBonus._AtualizaBonus();
            CargaDeProduto.CargaProduto();
        }

        internal class InserirBonus
        {
            private static DataTable ExportaBonus()
            {
                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select distinct tab1.numbonus, tab2.fornecedor from(select pcbonusc.NUMBONUS from pcbonusc where pcbonusc.DATABONUS >= trunc(sysdate) - 30 and pcbonusc.DTCANCEL is null) tab1, (select pcnfent.codfornec, pcnfent.NUMBONUS, pcfornec.FORNECEDOR from pcnfent, pcfornec where pcfornec.CODFORNEC = pcnfent.CODFORNEC) tab2 where tab1.numbonus = tab2.numbonus ";
                DataTable Bonus = new DataTable();
                OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

                try
                {
                    WinthorLogin.Open();

                    adapter.Fill(Bonus);

                    WinthorLogin.Dispose();

                    return Bonus;

                }
                catch (Exception Ex)
                {
                    Logger("Importação de Produtos: Erro ao Exportar: " + Ex.Message);
                    return null;
                }


            }

            private static DataTable FiltraBonus()
            {
                DataTable BonusBruto = ExportaBonus();

                DataTable BonusFiltrado = BonusBruto;

                NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                string SQL = "select codbonus from lanexpedicao_bonus";
                
                DataTable Bonus = new DataTable();
                Bonus.Columns.Add("codbonus");
                Bonus.PrimaryKey = new DataColumn[] { Bonus.Columns[0] };

                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(SQL, lanConexão);

                try
                {
                    lanConexão.Open();

                    adapter.Fill(Bonus);

                    lanConexão.Dispose();


                    for (int i = BonusBruto.Rows.Count - 1; i >= 0; i--)
                    {
                        if (Bonus.Rows.Contains(BonusBruto.Rows[i][0]))
                        {
                            DataRow dr = BonusFiltrado.Rows[i];
                            dr.Delete();
                            BonusFiltrado.AcceptChanges();
                        }

                    }

                    return BonusFiltrado;

                }
                catch (Exception Ex)
                {
                    Logger("Importação de Bônus: Erro ao importar: " + Ex.Message);
                    MessageBox.Show(Ex.ToString());
                    return null;
                }
            }

            public static void ImportaBonus()
            {
                DataTable Bonus = FiltraBonus();

                foreach (DataRow rw in Bonus.Rows)
                {
                    NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                    string SQL = "insert into lanexpedicao_bonus(codbonus, descricao, data_importacao) values(@codbonus, @descricao, now())";
                    NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

                    cmd.Parameters.Add(new NpgsqlParameter("@codbonus", NpgsqlDbType.Integer)).Value = Convert.ToInt32(rw[0]);
                    cmd.Parameters.Add(new NpgsqlParameter("@descricao", OleDbType.VarChar)).Value = rw[1];

                    try
                    {
                        lanConexão.Open();

                        cmd.ExecuteNonQuery();

                        lanConexão.Dispose();

                    }
                    catch (Exception Ex)
                    {
                        Logger("Importação de Bônus: Erro ao importar: " + Ex.Message);

                    }
                }

                if (Bonus.Rows.Count != 0)
                {
                    Logger($"Importação de Bônus: {Bonus.Rows.Count} Bônus(s) Importado(s)");

                }


            }
        }

        internal class AtualizaBonus
        {
            private static DataTable ExportaBonus()
            {
                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select numbonus as codbonus, dtcancel from pcbonusc where databonus >= trunc(sysdate)-30 and dtcancel is not null";
                DataTable Bonus = new DataTable();
                OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

                try
                {
                    WinthorLogin.Open();

                    adapter.Fill(Bonus);

                    WinthorLogin.Dispose();

                    return Bonus;

                }
                catch (Exception Ex)
                {
                    Logger("Atualização de Bônus: Erro ao Exportar: " + Ex.Message);
                    return null;
                }


            }

            private static DataTable FiltraBonus()
            {
                DataTable BonusBruto = ExportaBonus();

                DataTable BonusFiltrado = BonusBruto;

                NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                string SQL = "select codbonus from lanexpedicao_bonus where data_cancelamento is null";

                DataTable Bonus = new DataTable();
                Bonus.Columns.Add("codbonus");
                Bonus.PrimaryKey = new DataColumn[] { Bonus.Columns[0] };

                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(SQL, lanConexão);

                try
                {
                    lanConexão.Open();

                    adapter.Fill(Bonus);

                    lanConexão.Dispose();

                    for (int i = BonusBruto.Rows.Count - 1; i >= 0; i--)
                    {
                        if (Bonus.Rows.Contains(BonusBruto.Rows[i][0]))
                        {
                            DataRow dr = BonusFiltrado.Rows[i];
                            dr.Delete();
                            BonusFiltrado.AcceptChanges();
                        }

                    }

                    return BonusFiltrado;

                }
                catch (Exception Ex)
                {
                    Logger("Atualização de Bônus: Erro ao Filtrar: " + Ex.Message);

                    return null;
                }
            }

            public static void _AtualizaBonus()
            {
                DataTable Bonus = FiltraBonus();

                foreach (DataRow rw in Bonus.Rows)
                {
                    NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                    string SQL = "update lanexpedicao_bonus set data_cancelamento = @data_cancelamento, idusuario_cancelamento = 9999, idusuario_master_cancelamento = 9999 where codbonus = @codbonus ";
                    NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

                    cmd.Parameters.Add(new NpgsqlParameter("@data_cancelamento", OleDbType.Date)).Value = rw[1];
                    cmd.Parameters.Add(new NpgsqlParameter("@codbonus", NpgsqlDbType.Integer)).Value = Convert.ToInt32(rw[0]);

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

                if (Bonus.Rows.Count != 0)
                {
                    Logger($"Atualização de Bônus: {Bonus.Rows.Count} Bônus Atualizado(s)");
                }


            }
        }
        
        internal class CargaDeProduto
        {
            private static DataTable Bonus()
            {
                NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                //string SQL = "select expe.id, expe.codcarreg from lanexpedicao_carregamento as expe, lanconferencia_carregamento as conf ";

                string SQL = "select expe.id, expe.codbonus from lanexpedicao_bonus as expe where expe.id not in(select distinct idbonus from lanconferencia_bonus)";

                DataTable Bonus = new DataTable();
                /*Carregamentos.Columns.Add("codcarreg");
                Carregamentos.PrimaryKey = new DataColumn[] { Carregamentos.Columns[0] };*/

                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(SQL, lanConexão);

                try
                {
                    lanConexão.Open();

                    adapter.Fill(Bonus);

                    lanConexão.Dispose();

                    return Bonus;

                }
                catch (Exception Ex)
                {
                    Logger("Carga de Produtos: Erro ao Exportar Bônus: " + Ex.Message);
                    return null;
                }
            }

            private static DataTable ExportaProduto(int numbonus)
            {

                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select codprod, sum(qtnf) as qt from pcbonusi where numbonus = ? group by codprod";

                DataTable Bonus = new DataTable();
                OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

                adapter.SelectCommand.Parameters.Add("@numbonus", OleDbType.Integer).Value = numbonus;

                try
                {
                    WinthorLogin.Open();

                    adapter.Fill(Bonus);

                    WinthorLogin.Dispose();

                    return Bonus;

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
                DataTable _Bonus = Bonus();
                int CargaRealizada = 0;

                foreach (DataRow Row in _Bonus.Rows)
                {
                    DataTable Produtos = ExportaProduto(Convert.ToInt32(Row[1]));


                    if (Produtos.Rows.Count != 0)
                    {
                        foreach (DataRow row in Produtos.Rows)
                        {
                            NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                            string SQL = "insert into lanconferencia_bonus(codbonus, codprod, qt_real, idbonus) values(@codbonus, @codprod, @qt_real, @idbonus)";
                            NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

                            cmd.Parameters.Add(new NpgsqlParameter("@codbonus", NpgsqlDbType.Integer)).Value = Convert.ToInt32(Row[1]);
                            cmd.Parameters.Add(new NpgsqlParameter("@codprod", NpgsqlDbType.Integer)).Value = Convert.ToInt32(row[0]);
                            cmd.Parameters.Add(new NpgsqlParameter("@qt_real", NpgsqlDbType.Numeric)).Value = Convert.ToDecimal(row[1]);
                            cmd.Parameters.Add(new NpgsqlParameter("@idbonus", NpgsqlDbType.Numeric)).Value = Convert.ToInt32(Row[0]);

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
