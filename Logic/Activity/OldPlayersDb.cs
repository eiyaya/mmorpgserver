#region using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using NLog;

#endregion

namespace OldPlayers
{
    public class OldPlayerEntry
    {
        public string Userid;
        public ushort Level;
        public ushort State;
    }

    public class OldPlayersDbConnection
    {
        private const string UpdateStateProcedureName = "updatestate";
        private const string GetLevelProcedureName = "getlevel";
        private readonly string ConnectionString;
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ConcurrentStack<MySqlConnection> mConnections = new ConcurrentStack<MySqlConnection>();

        public OldPlayersDbConnection(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public bool CreateConnection()
        {
            try
            {
                var conn = new MySqlConnection(ConnectionString);
                if (conn.Ping())
                {
                    conn.Open();
                    mConnections.Push(conn);                    
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "DbConnection start failed.");
                return false;
            }

            return true;
        }

        private MySqlConnection CheckConnection()
        {
            MySqlConnection connection = null;
            try
            {
                if (!mConnections.TryPop(out connection))
                {
                    connection = new MySqlConnection(ConnectionString);
                }

                try
                {
                    if (!connection.Ping())
                    {
                        Logger.Error("connect.ping return fasle!!!");
                        Logger.Error("CheckConnection 1 !!! connect.state={0}", connection.State);
                        connection.Close();
                        Logger.Error("CheckConnection 2 !!! connect.state={0}", connection.State);
                        connection.Open();
                        Logger.Error("CheckConnection 3 !!! connect.state={0}", connection.State);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("CheckConnection 4  Exception!!! connect.state={0}, ex: {1}", connection.State, ex);
                    connection.Close();
                    Logger.Error("CheckConnection 5  Exception!!! connect.state={0}", connection.State);
                    connection.Open();
                    Logger.Error("CheckConnection 6  Exception!!! connect.state={0}", connection.State);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Try to reconnect to database {0} failed.", connection.ConnectionString);
            }

            return connection;
        }

        public async Task<int> GetUngetPlayerLevel(string userid)
        {
            var iData = new MySqlParameter[2];
            iData[0] = new MySqlParameter("@incode", userid) { MySqlDbType = MySqlDbType.VarChar };
            iData[1] = new MySqlParameter("@returnLevel", MySqlDbType.Int16);
            iData[1].Direction = ParameterDirection.Output;

            MySqlConnection conn = null;
            try
            {
                conn = CheckConnection();

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = GetLevelProcedureName;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddRange(iData);
                    using (var reader = (MySqlDataReader) await command.ExecuteReaderAsync())
                    {
                        if (!reader.Read())
                        {
                            return -1;
                        }
                        var level = (int)reader.GetInt16("returnLevel");
                        reader.Close();
                        return level;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("RunProcedure error !! storedProcName: {0} ,ex{1}", UpdateStateProcedureName, ex);
            }
            finally
            {
                if (conn != null)
                {
                    mConnections.Push(conn);
                }
            }

            return -1;
        }

        public async Task<int> UpdateStateAsync(string code)
        {
            var iData = new MySqlParameter[2];
            iData[0] = new MySqlParameter("@incode", code) {MySqlDbType = MySqlDbType.VarChar};
            iData[1] = new MySqlParameter("@myreturn", MySqlDbType.Int16);
            iData[1].Direction = ParameterDirection.Output;

            MySqlConnection conn = null;
            try
            {
                conn = CheckConnection();

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = UpdateStateProcedureName;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddRange(iData);
                    using (var reader = (MySqlDataReader) await command.ExecuteReaderAsync())
                    {
                        if (!reader.Read())
                        {
                            return -1;
                        }
                        var myreturn = reader.GetInt16("myreturn");
                        reader.Close();
                        return myreturn;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("RunProcedure error !! storedProcName: {0} ,ex{1}", UpdateStateProcedureName, ex);
            }
            finally
            {
                if (conn != null)
                {
                    mConnections.Push(conn);
                }
            }
            return -1;
        }
    }
}

