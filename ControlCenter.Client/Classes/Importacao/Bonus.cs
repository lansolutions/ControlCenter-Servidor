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
    public class Bonus
    {
        public Bonus()
        {
            try
            {
                new InserirBonus();
                new AtualizaBonus();
                new CargaDeProduto();

            }
            catch (Exception Ex)
            {
                Logger(Ex.ToString());
            }
        }
       
        internal class InserirBonus
        {
            public InserirBonus()
            {
                ImportaBonus();
            }
            private DataTable ExportaBonusSistemaParceiro()
            {
                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select distinct to_char(tab1.numbonus), tab2.fornecedor from(select pcbonusc.NUMBONUS from pcbonusc where pcbonusc.DATABONUS >= trunc(sysdate) - 30 and pcbonusc.DTCANCEL is null) tab1, (select pcnfent.codfornec, pcnfent.NUMBONUS, pcfornec.FORNECEDOR from pcnfent, pcfornec where pcfornec.CODFORNEC = pcnfent.CODFORNEC and pcfornec.revenda = 'S') tab2 where tab1.numbonus = tab2.numbonus and tab1.numbonus != 8539 ";
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

            private DataTable ExportaBonusControlCenter()
            {
                
                NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
                string SQL = "select codbonus::varchar from lanexpedicao_bonus";
                
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
                    Logger("Importação de Bônus: Erro ao importar: " + Ex.Message); throw new Exception(Ex.Message);
                }

                return dt;
            }

            public void ImportaBonus()
            {
                DataTable dt = ExportaBonusControlCenter().Copy();
                DataTable dt2 = ExportaBonusSistemaParceiro().Copy();

                int BonusAdicionados = 0;

                dt2.AsEnumerable().Where(x => !dt.AsEnumerable().Any(y => y.Field<string>(0) == x.Field<string>(0))).ToList().ForEach(x =>
                {
                    NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
                    string SQL = "insert into lanexpedicao_bonus(codbonus, descricao, data_importacao, idparceiro) values(@codbonus, @descricao, now(), @idparceiro)";
                    NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

                    cmd.Parameters.Add(new NpgsqlParameter("@codbonus", NpgsqlDbType.Integer)).Value = Convert.ToInt32(x[0]);
                    cmd.Parameters.Add(new NpgsqlParameter("@descricao", OleDbType.VarChar)).Value = x[1];
                    cmd.Parameters.Add(new NpgsqlParameter("@idparceiro", NpgsqlDbType.Integer)).Value = 2;

                    try
                    {
                        lanConexão.Open();
                        
                        cmd.ExecuteNonQuery(); ++BonusAdicionados;

                        lanConexão.Dispose();

                    }
                    catch (Exception Ex)
                    {
                        Logger("Importação de Bônus: Erro ao importar: " + Ex.Message); throw new Exception(Ex.Message);
                    }
                });                

                if (BonusAdicionados != 0)
                {
                    Logger($"Importação de Bônus: {BonusAdicionados} Bônus(s) Importado(s)"); 

                }
            }
        }

        internal class AtualizaBonus
        {
            public AtualizaBonus()
            {
                AtualizarBonus();
            }
            
            private DataTable ExportaBonusSistemaParceiro()
            {
                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select distinct to_char(numbonus) as numbonus, dtcancel from pcbonusc where databonus >= trunc(sysdate)-30 and dtcancel is not null";
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
                    Logger("Atualização de Bônus: Erro ao Exportar: " + Ex.Message); throw new Exception(Ex.Message);
                }

                return dt;
            }

            private DataTable ExportaBonusControlCenter()
            {
                NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
                string SQL = "select codbonus::varchar from lanexpedicao_bonus where data_cancelamento is null";

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
                    Logger("Atualização de Bônus: Erro ao Filtrar: " + Ex.Message); throw new Exception(Ex.Message);
                }

                return dt;
            }

            public void AtualizarBonus()
            {
                DataTable dt = ExportaBonusControlCenter().Copy();
                DataTable dt2 = ExportaBonusSistemaParceiro().Copy();

                int BonusAtualizados = 0;

                dt2.AsEnumerable().Where(x => !dt.AsEnumerable().Any(y => y.Field<string>("codbonus") == x.Field<string>("numbonus"))).ToList().ForEach(x =>
                {

                    NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
                    string SQL = "update lanexpedicao_bonus set data_cancelamento = @data_cancelamento, idusuario_cancelamento = 9999, idusuario_master_cancelamento = 9999 where codbonus = @codbonus ";
                    NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

                    cmd.Parameters.Add(new NpgsqlParameter("@data_cancelamento", OleDbType.Date)).Value = x[1];
                    cmd.Parameters.Add(new NpgsqlParameter("@codbonus", NpgsqlDbType.Integer)).Value = Convert.ToInt32(x[0]);

                    try
                    {
                        lanConexão.Open();

                        cmd.ExecuteNonQuery(); ++BonusAtualizados;

                        lanConexão.Dispose();

                    }
                    catch (Exception Ex)
                    {
                        Logger("Atualização de Produtos: Erro ao Atualizar: " + Ex.Message); throw new Exception(Ex.Message);
                    }
                });

                if (BonusAtualizados != 0)
                {
                    Logger($"Atualização de Bônus: {BonusAtualizados} Bônus Atualizado(s)");
                }
            }
        }
        
        internal class CargaDeProduto
        {
            public CargaDeProduto()
            {
                CargaProduto();
            }
            private DataTable Bonus()
            {
                NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
                string SQL = "select expe.id, expe.codbonus from lanexpedicao_bonus as expe where expe.id not in(select distinct idbonus from lanconferencia_bonus)";
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
                    Logger("Carga de Produtos: Erro ao Exportar Bônus: " + Ex.Message); throw new Exception(Ex.Message);
                }

                return dt;
            }

            private DataTable ExportaProduto(int numbonus)
            {
                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select codprod, sum(qtnf) as qt from pcbonusi where numbonus = ? group by codprod";

                DataTable dt = new DataTable();
                OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

                adapter.SelectCommand.Parameters.Add("@numbonus", OleDbType.Integer).Value = numbonus;

                try
                {
                    WinthorLogin.Open();

                    adapter.Fill(dt);

                    WinthorLogin.Dispose();
                }
                catch (Exception Ex)
                {
                    Logger("Carga de Produtos: Erro ao Exportar Produtos: " + Ex.Message); throw new Exception(Ex.Message);
                }

                return dt;

            }

            public void CargaProduto()
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
                            NpgsqlConnection lanConexão = new NpgsqlConnection(BancoPostGres.StringConexao);
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
