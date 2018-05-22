#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using ActivityServerService;
using DataContract;
using DataTable;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scorpion;
using NLog;
using PayDb;
using Shared;

#endregion

namespace Activity
{
    public class InAppPurchaseDefautImpl : IInAppPurchase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger RechargeLogger = LogManager.GetLogger("RechargeLogger");

        private IEnumerator RechageOnce(Coroutine co, ActivityServerControl _this)
        {
            InAppPurchase.ChargingState = ChargingState.GettingPayOrder;
            var data = new ConnectData
            {
                mType = ConnectDataType.GetData,
                connect = InAppPurchase.PayDbConnection
            };
            //var order = InAppPurchase.PayDbConnection.GetWaittingResultOrder();
            yield return _this.payDbManagerManager.DoOrder(co, data);
            ResultOrder order = null;
            string platform;
            try
            {
                order = data.resultOrder;
                if (order == null)
                {
                    InAppPurchase.ChargingState = ChargingState.Waiting;
                    yield break;
                }

                RechargeLogger.Info("GetWaittingResultOrder  orderid : {0},playerid : {1},uid: {2} , channel:{3}",
                    order.OrderId,
                    order.PlayerId, order.Uid, order.Channel);
                RechargeLogger.Info("RechageOnce  get order channel:{0} step 1", order.Channel);

                var strs = order.Channel.Split('.');
                platform = strs[0];

                RechargeLogger.Info("RechageOnce  split channel, platfrom :{0} step 2", platform);

                InAppPurchase.ChargingState = ChargingState.AddingItem;

                RechargeLogger.Info(
                    "RechageOnce  ss to logic RechargeSuccess, playerid:{0}, platfrom:{1}, orderType:{2}, amount:{3},orderid:{4} step 3",
                    order.PlayerId, platform, order.PayType, order.Amount, order.OrderId);
            }
            catch (Exception)
            {
                InAppPurchase.ChargingState = ChargingState.Waiting;
                yield break;
            }

            var reslut = ActivityServer.Instance.LogicAgent.RechargeSuccess(order.PlayerId, platform, order.PayType,
                order.Amount, order.OrderId, order.Channel);
            yield return reslut.SendAndWaitUntilDone(co);


            if (reslut.State == MessageState.Reply)
            {
                if (reslut.ErrorCode == (int) ErrorCodes.OK)
                {
                    RechargeLogger.Info("RechargeSuccess return ok ,orderid:{0} step 4", order.OrderId);
                    InAppPurchase.ChargingState = ChargingState.WriteOrderBack;

                    order.State = (short) eOrderState.Success;
                    var connectData = new ConnectData
                    {
                        resultOrder = order,
                        mType = ConnectDataType.ModifyData,
                        connect = InAppPurchase.PayDbConnection
                    };
                    yield return _this.payDbManagerManager.DoOrder(co, connectData);
                    //InAppPurchase.PayDbConnection.UpdateResultOrderById(order.OrderId, eOrderState.Success);
                }
                else
                {
                    RechargeLogger.Error(
                        "ActivityServer.Instance.LogicAgent.RechargeSuccess return error ErrorCode = {0}!");
                    Logger.Error("ActivityServer.Instance.LogicAgent.RechargeSuccess return error ErrorCode = {0}!",
                        reslut.ErrorCode);
                    order.State = (short) eOrderState.Error;
                    yield return _this.payDbManagerManager.DoOrder(co, new ConnectData
                    {
                        resultOrder = order,
                        mType = ConnectDataType.ModifyData,
                        connect = InAppPurchase.PayDbConnection
                    });
                    //InAppPurchase.PayDbConnection.UpdateResultOrderById(order.OrderId, eOrderState.Error);
                }
            }
            else
            {
                RechargeLogger.Error("ActivityServer.Instance.LogicAgent.RechargeSuccess did not reply!");
                Logger.Error("ActivityServer.Instance.LogicAgent.RechargeSuccess did not reply!");
            }

            InAppPurchase.ChargingState = ChargingState.Waiting;
            RechargeLogger.Info("RechageOnce finished oid:{1} step 5", order.OrderId);
        }

        public IEnumerator ApplyOrderSerial(Coroutine coroutine, ActivityService _this, ApplyOrderSerialInMessage msg)
        {

            var inMsg = msg.Request.Msg;
            var table = Table.GetRecharge(inMsg.GoodId);

            RechargeLogger.Info("ApplyOrderSerial Request GoodId:{0} Channel:{1} ExtInfo:{2} CharacterId:{3},step 1 ",
                inMsg.GoodId, inMsg.Channel, inMsg.ExtInfo, msg.CharacterId);
            if (table == null)
            {
                Logger.Error("ApplyOrderSerial tableid : {0} does not exists in RechargeTable!!!", inMsg.GoodId);
                msg.Reply((int) ErrorCodes.Error_GoodId_Not_Exist);
                yield break;
            }

             var __this = (ActivityServerControl) _this;

            var infos = inMsg.Channel.Split('.');
            if (infos.Length <= 1)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            var platform = infos[0];
            var channel = infos[1];
            string guid;
            string extinfo = inMsg.ExtInfo;
            if (channel.Equals("moe"))
            {
                const string apikey = "mayakey";
                var extInfos = inMsg.ExtInfo.Split('.');


                //var bytes = BitConverter.GetBytes(++__this.udidSeed);
                //guid = Convert.ToBase64String(bytes);
                guid = (++__this.udidSeed).ToString();
                
                var sb = new StringBuilder();
                sb.Append(guid);
                sb.Append('|');
                sb.Append("nouse");
                sb.Append('|');
                sb.Append(apikey);
                var sign = Shared.RequestManager.Encrypt_MD5_UTF8(sb.ToString());

                var dic = new Dictionary<string, string>();
                dic.Add("cporder", guid);
                dic.Add("data", "nouse");
                dic.Add("notifyurl", __this.PayServerNotifyAddress );
                dic.Add("verifyurl", __this.PayServerVerifyAddress);
                dic.Add("sign", sign);

                var url = string.Format(@"http://sdk.uborm.com:40000/{0}/{1}/SaveOrder/", extInfos[0], extInfos[1]);
                var result = AsyncReturnValue<string>.Create();
                yield return ((ActivityServerControl)_this).WebRequestManager.DoRequest(coroutine, url, dic, result);

                if (string.IsNullOrEmpty(result.Value))
                {
                    Logger.Error("ApplyOrderSerial get webResponse is null.");
                    msg.Reply((int)ErrorCodes.Error_GoodId_Not_Exist);
                    yield break;
                }

                var jsonResult = (JObject)JsonConvert.DeserializeObject(result.Value);
                var resultCode = jsonResult["code"].ToString();
                var resultDesc = jsonResult["msg"].ToString();

                if (!resultCode.Equals("0"))
                {
                    Logger.Error("ApplyOrderSerial resultCode is." + resultCode + "Desc:" + resultDesc);
                    msg.Reply((int)ErrorCodes.Error_GoodId_Not_Exist);
                    yield break;
                }

                JObject joObject = new JObject();
                joObject["code"] = 0;
                joObject["msg"] = "OK";
                joObject["id"] = extInfos[2];
                joObject["cporder"] = guid;
                joObject["order"] = jsonResult["cporder"];
                var price = table.Price*100;
                joObject["amount"] = price.ToString();
                joObject["createtime"] = jsonResult["orderdate"];
                joObject["Itemid"] = "";
                joObject["Itemquantity"] = 1;
                joObject["status"] = 1;
                joObject["info"] = "";

                var bytes = System.Text.Encoding.Default.GetBytes(joObject.ToString());
                extinfo = Convert.ToBase64String(bytes);
            }
            else
            {
                guid = Guid.NewGuid().ToString("N");
            }

            var tempType = table.Type;
            if (tempType == 3)
            {
                tempType = tempType * 1000 + table.Id;
            }

            var order = new PreOrder
            {
                Amount = table.Price,
                Channel = inMsg.Channel,
                OrderId = guid,
                ExtInfo = extinfo,
                PayType = (short)tempType,
                Uid = msg.ClientId,
                PlayerId = msg.CharacterId
            };


            // var ret = InAppPurchase.PayDbConnection.NewPreOrder(order);
           

            var orderData = new ConnectData
            {
                preOrder = order,
                connect = InAppPurchase.PayDbConnection,
                returnValue = ePayDbReturn.Exception,
                mType = ConnectDataType.PushData
            };


            yield return __this.payDbManagerManager.DoOrder(coroutine, orderData);

            var ret = orderData.returnValue;

            if (ret != ePayDbReturn.Success)
            {
                RechargeLogger.Fatal("ApplyOrderSerial NewPreOrder to sql failed, ret :{0} guid:{1}", ret, guid);

                Logger.Fatal("ApplyOrderSerial NewPreOrder to sql failed, ret :{0}",
                    Enum.GetName(typeof (ePayDbReturn), ret));

                msg.Reply((int) ErrorCodes.Error_GoodId_Not_Exist);
                yield break;
            }

            RechargeLogger.Info("ApplyOrderSerial NewPreOrder to db success orderid:{0} CharacterId:{1},step 3 ", guid,
                msg.CharacterId);


            if (inMsg.Channel.Equals("ios.appstore"))
            {
                msg.Response = new OrderSerialData {OrderId = table.GoodsId};
            }
            else
            {
                msg.Response = new OrderSerialData {OrderId = guid};
            }

            msg.Reply();

        }

        public void Update(ActivityServerControl _this)
        {
            if (InAppPurchase.ChargingState != ChargingState.Waiting)
            {
                return;
            }

            CoroutineFactory.NewCoroutine(RechageOnce, _this).MoveNext();
        }

    }

    public interface IInAppPurchase
    {
        IEnumerator ApplyOrderSerial(Coroutine coroutine, ActivityService _this, ApplyOrderSerialInMessage msg);
        void Update(ActivityServerControl _this);
    }

    public enum ChargingState
    {
        Waiting,
        GettingPayOrder,
        AddingItem,
        WriteOrderBack
    }

    public class InAppPurchase
    {
        public static ChargingState ChargingState = ChargingState.Waiting;
        private static IInAppPurchase mImpl;
        public static PayDbConnection PayDbConnection;



        static InAppPurchase()
        {
            ActivityServer.Instance.UpdateManager.InitStaticImpl(typeof (InAppPurchase),
                typeof (InAppPurchaseDefautImpl),
                o => { mImpl = (IInAppPurchase) o; });
        }

        public static IEnumerator ApplyOrderSerial(Coroutine coroutine,
                                                   ActivityService _this,
                                                   ApplyOrderSerialInMessage msg)
        {
            return mImpl.ApplyOrderSerial(coroutine, _this, msg);
        }

        public static void Init()
        {
            var dbConfig = File.ReadAllLines("../Config/paydb.config");
            PayDbConnection = new PayDbConnection(dbConfig[0].Trim());
        }

        public static void Update(ActivityServerControl _this)
        {
            mImpl.Update(_this);
        }
    }
}