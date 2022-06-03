using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using static ControlCenter.Client.Classes.Logs.Log;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using System.Windows;

namespace ControlCenter.Client.Classes.Importacao
{
    public class Usuarios
    {
        public static void SincronizaUsuarios()
        {
            ImportaUsuarios.ImportarUsuarios();
        }

        public class ImportaUsuarios
        {
            public static DataTable ExportaUsuariosControlCenter()
            {
                DataTable UsuariosControlCenter = new DataTable();

                NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                string SQL = "select idusuario_parceiro from lanusuarios";
                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(SQL, lanConexão);

                try
                {
                    lanConexão.Open();

                    adapter.Fill(UsuariosControlCenter);

                    lanConexão.Dispose();

                }
                catch (Exception Ex)
                {
                    Logger("Importação de Produtos: Erro ao importar: " + Ex.Message);
                }


                return UsuariosControlCenter;
            }

            public static void ImportarUsuarios()
            {
                DataTable UsuariosControlCenter = ExportaUsuariosControlCenter();
                DataTable UsuariosSistemaParceiro = ExportaUsuariosSistemaParceiro();

                DataTable NovosUsuarios = UsuariosSistemaParceiro.Clone();

                UsuariosControlCenter.PrimaryKey = new DataColumn[] { UsuariosControlCenter.Columns[0] };

                try
                {
                    foreach (DataRow rw in UsuariosSistemaParceiro.Rows)
                    {
                        if (!UsuariosControlCenter.Rows.Contains(rw[3].ToString()))
                        {
                            NovosUsuarios.Rows.Add(rw.ItemArray);
                            NovosUsuarios.AcceptChanges();
                        }
                    }

                    foreach (DataRow rw in NovosUsuarios.Rows)
                    {
                        NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                        string SQL = "insert into lanusuarios(nome, usuario, senha, idsetor, idusuario_parceiro, usuario_utiliza_coletor, usuario_master_coletor, idusuario_inclusao, data_inclusao) values(@nome, @usuario, @senha, @idsetor, @idusuario_parceiro, @usuario_utiliza_coletor, @usuario_master_coletor, @idusuario_inclusao, now())";

                        NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

                        cmd.Parameters.Add(new NpgsqlParameter("nome", NpgsqlDbType.Varchar)).Value = rw[0].ToString();
                        cmd.Parameters.Add(new NpgsqlParameter("usuario", OleDbType.VarChar)).Value = rw[1].ToString();
                        cmd.Parameters.Add(new NpgsqlParameter("senha", OleDbType.VarChar)).Value = Cript.Encrypt(rw[2].ToString());
                        cmd.Parameters.Add(new NpgsqlParameter("idusuario_parceiro", OleDbType.Integer)).Value = Convert.ToInt32(rw[3].ToString());
                        cmd.Parameters.Add(new NpgsqlParameter("idsetor", OleDbType.Integer)).Value = 9999;
                        cmd.Parameters.Add(new NpgsqlParameter("usuario_utiliza_coletor", NpgsqlDbType.Varchar)).Value = "N";
                        cmd.Parameters.Add(new NpgsqlParameter("usuario_master_coletor", NpgsqlDbType.Varchar)).Value = "N";
                        cmd.Parameters.Add(new NpgsqlParameter("idusuario_inclusao", OleDbType.Integer)).Value = 9999;

                        try
                        {
                            lanConexão.Open();

                            cmd.ExecuteNonQuery();

                            lanConexão.Dispose();

                        }
                        catch (Exception Ex)
                        {
                            Logs.Log.Logger("Importação de Usuários@ Erro ao Importar@ " + Ex.Message); 
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger(e.ToString());
                }

                if(NovosUsuarios.Rows.Count != 0)
                {
                    Logs.Log.Logger($"Importação de Usuários: {NovosUsuarios.Rows.Count} Novo(s) Usuário(s) Importado(s)");
                }


            }

            public static DataTable ExportaUsuariosSistemaParceiro()
            {
                DataTable UsuariosSistemaParceiro = new DataTable();

                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select nome, usuariobd, decrypt(senhabd, usuariobd), matricula from pcempr where situacao = 'A' and tipo = 'F' ";

                OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

                try
                {
                    WinthorLogin.Open();

                    adapter.Fill(UsuariosSistemaParceiro);

                    WinthorLogin.Dispose();

                }
                catch (Exception Ex)
                {
                    Logger("Importação de Usuários: Erro ao Importar: " + Ex.Message);

                }

                return UsuariosSistemaParceiro;
            }
        }

        public class AtualizaUsuarios
        {
            public static DataTable ExportaUsuariosControlCenter()
            {
                DataTable UsuariosControlCenter = new DataTable();

                NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                string SQL = "select idusuario_parceiro from lanusuarios";
                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(SQL, lanConexão);

                try
                {
                    lanConexão.Open();

                    adapter.Fill(UsuariosControlCenter);

                    lanConexão.Dispose();

                }
                catch (Exception Ex)
                {
                    Logger("Importação de Produtos: Erro ao importar: " + Ex.Message);
                }


                return UsuariosControlCenter;
            }

            public static void ImportarUsuarios()
            {
                DataTable UsuariosControlCenter = ExportaUsuariosControlCenter();
                DataTable UsuariosSistemaParceiro = ExportaUsuariosSistemaParceiro();

                DataTable NovosUsuarios = UsuariosSistemaParceiro.Clone();

                UsuariosControlCenter.PrimaryKey = new DataColumn[] { UsuariosControlCenter.Columns[0] };

                try
                {
                    foreach (DataRow rw in UsuariosSistemaParceiro.Rows)
                    {
                        if (!UsuariosControlCenter.Rows.Contains(rw[3].ToString()))
                        {
                            NovosUsuarios.Rows.Add(rw.ItemArray);
                            NovosUsuarios.AcceptChanges();
                        }
                    }

                    foreach (DataRow rw in NovosUsuarios.Rows)
                    {

                        NpgsqlConnection lanConexão = new NpgsqlConnection("Server = 10.40.100.90; Port = 5432; User Id = sulfrios; Password = Eus00o19; Database = postgres;");
                        string SQL = "insert into lanusuarios(nome, usuario, senha, idsetor, idusuario_parceiro, usuario_utiliza_coletor, usuario_master_coletor, data_inclusao, idusuario_inclusao) values(@nome, @usuario,  crypt(@senha, gen_salt('bf')), @idsetor, @idusuario_parceiro, @usuario_utiliza_coletor, @usuario_master_coletor, now(), @idusuario_inclusao)";


                        NpgsqlCommand cmd = new NpgsqlCommand(SQL, lanConexão);

                        cmd.Parameters.Add(new NpgsqlParameter("@nome", NpgsqlDbType.Varchar)).Value = rw[0].ToString();
                        cmd.Parameters.Add(new NpgsqlParameter("@usuario", OleDbType.VarChar)).Value = rw[1].ToString();
                        cmd.Parameters.Add(new NpgsqlParameter("@senha", OleDbType.VarChar)).Value = rw[2].ToString();
                        cmd.Parameters.Add(new NpgsqlParameter("@idusuario_parceiro", OleDbType.Integer)).Value = Convert.ToInt32(rw[3].ToString());
                        cmd.Parameters.Add(new NpgsqlParameter("@idsetor", OleDbType.Integer)).Value = 9999;
                        cmd.Parameters.Add(new NpgsqlParameter("@usuario_utiliza_coletor", NpgsqlDbType.Varchar)).Value = "N";
                        cmd.Parameters.Add(new NpgsqlParameter("@usuario_master_coletor", NpgsqlDbType.Varchar)).Value = "N";
                        cmd.Parameters.Add(new NpgsqlParameter("@idusuario_inclusao", OleDbType.Integer)).Value = 9999;

                        try
                        {
                            lanConexão.Open();

                            cmd.ExecuteNonQuery();

                            lanConexão.Dispose();

                        }
                        catch (Exception Ex)
                        {
                            Logs.Log.Logger("Importação de Usuários: Erro ao Importar: " + Ex.Message);
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger(e.ToString());
                }



                Logs.Log.Logger($"Importação de Usuários: {NovosUsuarios.Rows.Count} Novo(s) Usuário(s) Importado(s)");

            }

            public static DataTable ExportaUsuariosSistemaParceiro()
            {
                DataTable UsuariosSistemaParceiro = new DataTable();

                OleDbConnection WinthorLogin = new OleDbConnection(BancoParceiro.StringConexao);
                string SQL = "select nome, usuariobd, decrypt(senhabd, usuariobd), matricula from pcempr where situacao = 'A' and tipo = 'F' ";

                OleDbDataAdapter adapter = new OleDbDataAdapter(SQL, WinthorLogin);

                try
                {
                    WinthorLogin.Open();

                    adapter.Fill(UsuariosSistemaParceiro);

                    WinthorLogin.Dispose();

                }
                catch (Exception Ex)
                {
                    Logger("Importação de Usuários: Erro ao Importar: " + Ex.Message);

                }

                return UsuariosSistemaParceiro;
            }
        }
    }
}
