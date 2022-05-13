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
        internal class AtualizarNovasDatas
        {
            static DataTable DatasAntigas = new DataTable();         

            private static void GerarDatasAntigas()
            {
                if(DatasAntigas != null)
                {
                    DatasAntigas.Clear();
                }
               
                
                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select case when(qt1 - dif) > 0 then 'DT1' when((qt1 + qt2) - dif) > 0 then 'DT2' when((qt1 + qt2 + qt3) - dif) > 0 then 'DT3' when((qt1 + qt2 + qt3 + qt4) - dif) > 0 then 'DT4' else 'DT0' end as qtatual, codprod from(select codprod, qt1, qt2, qt3, qt4, qtinser, qtdisp, qtinser - qtdisp as dif from(select controlevalidade.codprod, qt1, qt2, qt3, qt4, (nvl(qt1, 0) + nvl(qt2, 0) + nvl(qt3, 0) + nvl(qt4, 0)) as qtotal, qtinser, (pcest.qtestger - (qtbloqueada + qtpendente + qtreserv)) as qtdisp from controlevalidade, pcest where qt2 is not null and pcest.codprod = controlevalidade.codprod and pcest.codfilial in(?) and pcest.qtestger > 0 and(nvl(qt1, 0) + nvl(qt2, 0) + nvl(qt3, 0) + nvl(qt4, 0)) = qtinser)) tab1";

                OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

                adapter.SelectCommand.Parameters.Add("@codfilial", OleDbType.VarChar).Value = File.ReadAllLines(@"C:\ControlCenter\bin\Filial.ini")[0];

                try
                {
                    WinthorLogin.Open();

                    adapter.Fill(DatasAntigas);

                    WinthorLogin.Dispose();
                 
                }
                catch
                {
                    return;
                }              

            }

            public static void VerificarAtualizações()
            {               
                if (DatasAntigas == null)
                {
                    GerarDatasAntigas();                   
                }

                

                int ProdutosAlterados = 0;

                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select case when(qt1 - dif) > 0 then 'DT1' when((qt1 + qt2) - dif) > 0 then 'DT2' when((qt1 + qt2 + qt3) - dif) > 0 then 'DT3' when((qt1 + qt2 + qt3 + qt4) - dif) > 0 then 'DT4' else 'DT0' end as qtatual, tab1.codprod from(select codprod, qt1, qt2, qt3, qt4, qtinser, qtdisp, qtinser - qtdisp as dif from(select controlevalidade.codprod, qt1, qt2, qt3, qt4, (nvl(qt1, 0) + nvl(qt2, 0) + nvl(qt3, 0) + nvl(qt4, 0)) as qtotal, qtinser, (pcest.qtestger - (qtbloqueada + qtpendente + qtreserv)) as qtdisp from controlevalidade, pcest where qt2 is not null and pcest.codprod = controlevalidade.codprod and pcest.codfilial in(?) and dtval1 > trunc(sysdate) and pcest.qtestger > 0 and(nvl(qt1, 0) + nvl(qt2, 0) + nvl(qt3, 0) + nvl(qt4, 0)) = qtinser)) tab1, pcprodut where tab1.codprod = pcprodut.codprod";
                OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

                DataTable DatasNovas = new DataTable();

                adapter.SelectCommand.Parameters.Add("@codfilial", OleDbType.VarChar).Value = File.ReadAllLines(@"C:\ControlCenter\bin\Filial.ini")[0];

                try
                {
                    WinthorLogin.Open();

                    adapter.Fill(DatasNovas);

                    WinthorLogin.Dispose();

                }
                catch
                {
                    return;
                }              

                foreach (DataRow Row in DatasAntigas.Rows)
                {
                    foreach (DataRow rw in DatasNovas.Rows)
                    {
                        if (Row[0].ToString() != rw[0].ToString() && Row[1].ToString() == rw[1].ToString().Split('-')[0].Trim())
                        {
                            if (rw[0].ToString() != "DT0")
                            {
                                AtualizarDtVencPcProduto(rw[0].ToString(), rw[1].ToString());
                                ProdutosAlterados += 1;
                            }
                        }
                    }
                }

                
                GerarDatasAntigas();     
                if(ProdutosAlterados != 0)
                {
                    Logger($"Atualização de Data de Validade: {ProdutosAlterados} Produto(s) Atualizados");
                }
                
            }

            private static  void AtualizarDtVencPcProduto(string Dt, string codprod)
            {
                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = $"update pcprodut set dtvenc = (select dtval{Dt.Replace("DT", "")} from controlevalidade where dtinsercao is not null and codprod = {codprod}) where codprod = {codprod}";
                OleDbCommand cmd = new OleDbCommand(SQL, WinthorLogin);

                try
                {
                    WinthorLogin.Open();

                    cmd.ExecuteNonQuery();

                    WinthorLogin.Dispose();
                }
                catch
                {                   
                  
                }
            }
        }
       
        public static void Atualizar()
        {
           AtualizarNovasDatas.VerificarAtualizações();
        }

       

    }
}
