#region using

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Scorpion;
using NLog;
using PayDb;

#endregion

namespace Activity
{
    public enum ConnectDataType
    {
        GetData = 0,
        PushData = 1,
        ModifyData = 2,
        Error = 99
    }

    public class ConnectData
    {
        public PayDbConnection connect;
        public Coroutine coroutine;
        public ConnectDataType mType;
        public PreOrder preOrder;
        public ResultOrder resultOrder;
        public ePayDbReturn returnValue;
    }

    public class PayDbManager
    {
        private readonly Logger Logger = LogManager.GetLogger("RechargeLogger");
        private readonly BlockingCollection<ConnectData> mQueue = new BlockingCollection<ConnectData>();
        private Thread mThread;

        public object DoOrder(Coroutine co, ConnectData data)
        {
            if (!mQueue.IsAddingCompleted)
            {
                data.coroutine = co;
                mQueue.Add(data);
            }

            return null;
        }

        private async Task DoTask(ConnectData data, ServerAgentBase agentBase)
        {
            try
            {
                switch (data.mType)
                {
                    case ConnectDataType.GetData:
                    {
                        var ret = await data.connect.GetWaittingResultOrderAsync();
                        data.resultOrder = ret;
                    }
                        break;
                    case ConnectDataType.PushData:
                    {
                        var ret = await data.connect.NewPreOrderAsync(data.preOrder);
                        data.returnValue = ret;
                    }
                        break;
                    case ConnectDataType.ModifyData:
                    {
                        await
                            data.connect.UpdateResultOrderByIdAsync(data.resultOrder.OrderId,
                                (eOrderState) data.resultOrder.State);
                    }
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("PayDbManager NewPreOrder Exception {0} {1}", data.preOrder.OrderId, ex);
                data.mType = ConnectDataType.Error;
            }
            finally
            {
                agentBase.mWaitingEvents.Add(new ContinueEvent(data.coroutine));
            }
        }

        public void Init(ServerAgentBase agentBase)
        {
            //Debug.Assert(false);
            mThread =
                new Thread(
                    () =>
                    {
                        while (!mQueue.IsAddingCompleted)
                        {
                            ConnectData task = null;
                            try
                            {
                                task = mQueue.Take();
                            }
                            catch
                            {
                                // ...
                            }
                            if (task == null)
                                continue;

                            var t = DoTask(task, agentBase);
                        }
                    });

            mThread.Start();
        }

        public void Stop()
        {
            mQueue.CompleteAdding();
            mThread.Join();
        }
    }
}