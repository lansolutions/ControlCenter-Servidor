using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ControlCenter.Client.Classes.Logs.Log;

namespace ControlCenter.Client.Classes.Importacao
{
    /// <summary>
    /// Classe de Atualizações de Controle de Validade dos Produtos
    /// </summary>
    public class ControleValidadeProdutos
    {
        public ControleValidadeProdutos()
        {
            try
            {
                AtualizarDatas();
            }

            catch (Exception Ex)
            {
                Logger("Erro ao Atualizar Data de Validade.\n" + Ex.Message);
            }
        }

        ~ControleValidadeProdutos()
        {
            
        }

        public DataTable GerarDatasNovas()
        {             
            OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
            string SQL = "select distinct case when(qt1 - (qtinser - (pcest.qtestger - (qtbloqueada + qtpendente + qtreserv)))) > 0 then 'DTVAL1' when((qt1 + qt2) - (qtinser - (pcest.qtestger - (qtbloqueada + qtpendente + qtreserv)))) > 0 then 'DTVAL2' when((qt1 + qt2 + qt3) - (qtinser - (pcest.qtestger - (qtbloqueada + qtpendente + qtreserv)))) > 0 then 'DTVAL3' when((qt1 + qt2 + qt3 + qt4) - (qtinser - (pcest.qtestger - (qtbloqueada + qtpendente + qtreserv)))) > 0 then 'DTVAL4' else 'DTVAL1' end as qtatual, controlevalidade.codprod from pcest, controlevalidade where pcest.codprod = controlevalidade.codprod and pcest.codfilial = ? and pcest.QTESTGER > 0";
            OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

            DataTable dt = new DataTable();

            adapter.SelectCommand.Parameters.Add("@codfilial", OleDbType.VarChar).Value = File.ReadAllLines(@"C:\LanSolutions\ControlCenter-Servidor\Filial.ini")[0];

            try
            {
                WinthorLogin.Open();

                adapter.Fill(dt);

                WinthorLogin.Dispose();

            }
            catch (Exception Ex)
            {
                Logger("Erro ao verificar atualizações de controle de validade de produtos.\n" + Ex.Message); throw new Exception(Ex.Message);

            }
         
            return dt;
        }

        public DataTable BuscarDatasNovas()
        {
            DataTable dt = new DataTable();

            foreach (DataRow dr in GerarDatasNovas().Rows)
            {
                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = $"select distinct to_char({dr[0]}),  to_char(codprod) as codprod from controlevalidade where codprod = ?";
                OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin); 

                adapter.SelectCommand.Parameters.Add("@codprod", OleDbType.Integer).Value = Convert.ToInt32(dr[1].ToString());
            
                try
                {
                    WinthorLogin.Open();
                    
                    adapter.Fill(dt); 

                    WinthorLogin.Dispose();

                }
                catch (Exception Ex)
                {
                    Logger("Erro ao verificar atualizações de controle de validade de produtos.\n" + Ex.Message); throw new Exception(Ex.Message);
                }
            }          
           
           


            return dt;
        }
        
        public DataTable BuscarDatasAntigas()
        {

            DataTable dt = new DataTable();
            
            foreach (DataRow dr in BuscarDatasNovas().Rows)
            {
                if (dr[0].ToString() != string.Empty)
                {
                    OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                    string SQL = "select to_char(codprod) as codprod, to_char(dtvenc) as dtvenc from pcprodut where codprod = ?";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

                    adapter.SelectCommand.Parameters.Add("@codprod", OleDbType.Integer).Value = Convert.ToInt32(dr[1].ToString());

                    try
                    {
                        WinthorLogin.Open();

                        adapter.Fill(dt);

                        WinthorLogin.Dispose();

                    }
                    catch (Exception Ex)
                    {
                        Logger("Erro ao verificar atualizações de controle de validade de produtos.\n" + Ex.Message); throw new Exception(Ex.Message);

                    } 
                }
                    
                
            }
            return dt;  
        }
        
        private void AtualizarDatas()
        {
            int DataAtualizadas = 0;

            
            DataTable dt = BuscarDatasNovas().Copy();
            DataTable dt2 = BuscarDatasAntigas().Copy();

            dt.AsEnumerable().Where(x => dt2.AsEnumerable().Any(y => y.Field<string>("codprod") == x.Field<string>("codprod") && y.Field<string>("dtvenc") != x.Field<string>(0))).ToList().ForEach(x =>
            {
                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = $"update pcprodut set dtvenc = ? where codprod = ?";
                OleDbCommand cmd = new OleDbCommand(SQL, WinthorLogin);

                cmd.Parameters.Add("@dtvenc", OleDbType.Date).Value = Convert.ToDateTime(x.Field<string>(0));
                cmd.Parameters.Add(new OleDbParameter("@codprod", OleDbType.Integer)).Value = Convert.ToInt32(x.Field<string>("codprod"));

                try
                {
                    WinthorLogin.Open();

                    cmd.ExecuteNonQuery(); ++DataAtualizadas;

                    WinthorLogin.Dispose();
                }
                catch (Exception Ex)
                {
                    Logger("Erro ao Atualizar Data de Validade.\n" + Ex.Message); throw new Exception(Ex.Message);
                }
            });

            if (DataAtualizadas != 0)
            {
                Logger($"Atualizações de Controle de Validade: {DataAtualizadas} Produto()s Atualizado(s) com sucesso.");
            }



        }
       
    }
}
