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
using Scorpion;
using Shared;

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
        private const string GetRechargeProcedureName = "getrecharge";
        private const string UpdateReStateProcedureName = "updaterestate";

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
                if (!conn.Ping())
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

        public AsyncReturnValue<int, int, int> GetUngetPlayerLevel(Coroutine co, string userid, ClientAgentBase agent)
        {
            AsyncReturnValue<int, int, int> ret = AsyncReturnValue<int, int, int>.Create();
            ret.Value1 = -1;
            ret.Value2 = -1;
            ret.Value3 = -1;
            Task.Run(() =>
            {
                MySqlConnection conn = null;
                try
                {
                    var iData = new MySqlParameter[4];
                    iData[0] = new MySqlParameter("@incode", userid) { MySqlDbType = MySqlDbType.VarChar };
                    iData[1] = new MySqlParameter("@returnLevel", MySqlDbType.Int16);
                    iData[2] = new MySqlParameter("@returnRecharge", MySqlDbType.Int32);
                    iData[3] = new MySqlParameter("@returnVipExp", MySqlDbType.Int32);

                    iData[1].Direction = ParameterDirection.Output;
                    iData[2].Direction = ParameterDirection.Output;
                    iData[3].Direction = ParameterDirection.Output;

                    conn = CheckConnection();

                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = GetLevelProcedureName;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddRange(iData);
                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                ret.Value1 = -1;
                                ret.Value2 = -1;
                                ret.Value3 = -1;
                            }
                            else
                            {
                                if (reader.FieldCount < 3)
                                {
                                    reader.Close();
                                }
                                else
                                {
                                    ret.Value1 = reader.GetInt16("returnLevel");
                                    ret.Value2 = reader.GetInt16("returnRecharge");
                                    ret.Value3 = reader.GetInt16("returnVipExp");
                                    reader.Close();
                                }
                            }
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

                    if (!agent.mWaitingEvents.IsAddingCompleted)
                    {
                        agent.mWaitingEvents.Add(new Scorpion.ContinueEvent(co));
                    }
                }
            });


            return ret;
        }

        public AsyncReturnValue<int> UpdateStateAsync(Coroutine co, string code, ClientAgentBase agent)
        {
            var ret = AsyncReturnValue<int>.Create();
            ret.Value = -1;

            Task.Run(() =>
            {
                var iData = new MySqlParameter[2];
                iData[0] = new MySqlParameter("@incode", code) { MySqlDbType = MySqlDbType.VarChar };
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
                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                ret.Value = -1;
                            }
                            else
                            {
                                ret.Value = reader.GetInt16("myreturn");
                                reader.Close();
                            }
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
                    if (!agent.mWaitingEvents.IsAddingCompleted)
                    {
                        agent.mWaitingEvents.Add(new Scorpion.ContinueEvent(co));
                    }
                }

               
            });

            return ret;
        }
        public AsyncReturnValue<int> GetUngetPlayerRecharge(Coroutine co, string userid, ClientAgentBase agent)
        {
            AsyncReturnValue<int> ret = AsyncReturnValue<int>.Create();
            ret.Value = -1;

            Task.Run(() =>
            {
                MySqlConnection conn = null;
                try
                {
                    var iData = new MySqlParameter[2];
                    iData[0] = new MySqlParameter("@incode", userid) { MySqlDbType = MySqlDbType.VarChar };
                    iData[1] = new MySqlParameter("@returnrecharge", MySqlDbType.Int16);

                    iData[1].Direction = ParameterDirection.Output;
                    conn = CheckConnection();

                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = GetRechargeProcedureName;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddRange(iData);
                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                ret.Value = -1;
                            }
                            else
                            {
                                ret.Value = reader.GetInt16("returnrecharge");
                                reader.Close();
                            }
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

                    if (!agent.mWaitingEvents.IsAddingCompleted)
                    {
                        agent.mWaitingEvents.Add(new Scorpion.ContinueEvent(co));
                    }
                }
            });


            return ret;
        }

        public AsyncReturnValue<int> UpdateReStateAsync(Coroutine co, string code, ClientAgentBase agent)
        {
            var ret = AsyncReturnValue<int>.Create();
            ret.Value = -1;

            Task.Run(() =>
            {
                var iData = new MySqlParameter[2];
                iData[0] = new MySqlParameter("@incode", code) { MySqlDbType = MySqlDbType.VarChar };
                iData[1] = new MySqlParameter("@myreturn", MySqlDbType.Int16);
                iData[1].Direction = ParameterDirection.Output;

                MySqlConnection conn = null;
                try
                {
                    conn = CheckConnection();

                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = UpdateReStateProcedureName;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddRange(iData);
                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                ret.Value = -1;
                            }
                            else
                            {
                                ret.Value = reader.GetInt16("myreturn");
                                reader.Close();
                            }
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

                if (!agent.mWaitingEvents.IsAddingCompleted)
                {
                    agent.mWaitingEvents.Add(new Scorpion.ContinueEvent(co));
                }
            });

            return ret;
        }

    }
}

