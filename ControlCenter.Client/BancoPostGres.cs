using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ControlCenter.Client
{
    public class BancoPostGres
    {
        public static string StringConexao = String.Empty;       

        private string LocalArquivoString = @"C:\LanSolutions\ControlCenter-Servidor\ControlCenter-Servidor.ini";

        public BancoPostGres()
        {
            string StringCryptografada = string.Empty;
            
            try
            {
                if (File.Exists(LocalArquivoString))
                {
                    string Linha = File.ReadAllText(LocalArquivoString);             

                    if (string.IsNullOrEmpty(Linha))
                    {
                        throw new Exception("Arquivo de configuração não encontrado");
                    }

                    StringCryptografada = Linha;
                }

                else
                {
                    throw new Exception("Arquivo de configuração não encontrado");
                }

                StringConexao = Cript.Decrypt(StringCryptografada);
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

       
    }
}
