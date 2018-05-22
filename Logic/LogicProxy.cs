#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Database;
using DataContract;
using DataTable;
using EventSystem;
using Logic.Enchance;
using LogicServerService;
using Newtonsoft.Json;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public class LogicProxyDefaultImpl : ILogicCharacterProxy
    {
        //设置扩展数据
        [Updateable("LogicProxy")]
        public static List<int> ClientCanChangeExdata = new List<int>
        {
            59,
            60,
            61,
            628,
            713
        };
        [Updateable("LogicProxy")]
        //允许设置标记位数据
        public static List<int> ClientCanChangeFlag = new List<int>
        {
            480, //拒绝组队       
            481, //拒绝私聊     
            482, //屏蔽其他玩家   
            483, //屏蔽他人特效   
            484, //自动加入战盟邀请
            485, //队伍自动同意申请
            486, //自动同意邀
            488, //技能是否自动瞄准
            489, //摄像机震动开关
            490, //屏幕变暗的节能开关
            492, //完成任何一个成就都设置.指引用
            500, //第一次组队了
            501, //家园指引第1处 建造许愿池
            502, //家园指引第2处 许愿1次
            503, //家园指引第3处 一键拾取蛋
            504, //家园指引第4处  建造孵化室
            505, //家园指引第5处  确定加速
            506, //等待第一个 指引蛋 的孵化成功
            507, //第一次随从蛋成熟的时候
            508, //家园指引2-1 点击完成孵化
            509, //家园指引2-2  点开始随从任务
            511, //已经播放过转生引导了
            512, //避免家园指引1步重复标记,许愿池造好
            517, //已经在任务界面点过前往了
            521, //避免家园指引1步重复标记-农场造好
            522, //新家园指引1-农场造好
            523, //新家园指引2-点农场商店
            524, //家园修改后新步骤3-收完菜该种地
            525, //家园修改后新步骤4-种完菜该点订单
            526, //家园修改后新步骤5-该修复合成屋
            527, //家园修改后新步骤6-合成屋造好
            528, //家园修改后新步骤7-点击属性果实按钮
            529, //家园修改后新步骤8-家园3级要指引
            530, //家园修改后新步骤9-角斗圣殿造好
            531, //家园修改后新步骤10-
            532, //点击荣誉兑换按钮
            533, //点击交易所的加号
            534, //点击交易所的加号
            535, //家园修改后新步骤14-
            540, //家园原始指引不让开启标记 
            541, //家园第一次提交订单
            542, //家园第一次收获
            543, //家园第一次种地
            549, //图鉴合成成功第一次指引标记
            550, //购买药品指引打开药品商店
            551, //仓库取出物品指引打开仓库
            559, //分享成功给奖励
			2800, //回收一次
            3499 //弹出广告
        };

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ErrorCodes CheckServerIds(LogicProxy proxy, int serverId, List<int> serverIds, RechargeActiveRecord tbRA)
        {
            serverId = SceneExtension.GetServerLogicId(serverId);
            if (!serverIds.Contains(-1) && !serverIds.Contains(serverId))
            {
                return ErrorCodes.ServerID;
            }
            var now = DateTime.Now;
            var rule = (eRechargeActivityOpenRule)tbRA.OpenRule;
            switch (rule)
            {
                case eRechargeActivityOpenRule.Last:
                    break;
                case eRechargeActivityOpenRule.LimitTime:
                    {
                        if (!string.IsNullOrWhiteSpace(tbRA.StartTime))
                        {
                            var startTime = DateTime.Parse(tbRA.StartTime);
                            if (now < startTime)
                            {
                                //没在活动时间内
                                return ErrorCodes.Error_AnswerNotTime;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(tbRA.EndTime))
                        {
                            var endTime = DateTime.Parse(tbRA.EndTime);

                            if (tbRA.Type == 2) //是投资活动 而且买了 就延长7天
                            {
                                var sonId = tbRA.SonType;
                                if (sonId >= 0)
                                {
                                    var tbTouZi = Table.GetRechargeActiveCumulative(sonId);
                                    if (tbTouZi != null && proxy.Character != null)
                                    {
                                        var exdataId = tbTouZi.FlagTrueId;
                                        if (proxy.Character.GetFlag(exdataId))
                                        {
                                            endTime = endTime.AddDays(7);
                                        }
                                    }
                                }
                            }

                            if (now > endTime)
                            {
                                //没在活动时间内
                                return ErrorCodes.Error_AnswerNotTime;
                            }
                        }
                    }
                    break;
                case eRechargeActivityOpenRule.NewServerAuto:
                    {
                        //开服一段时间内可投资
                        int startHour, endHour;
                        if (int.TryParse(tbRA.StartTime, out startHour) && int.TryParse(tbRA.EndTime, out endHour))
                        {
                            if (tbRA.Type == 2) //是投资活动 而且买了 就延长7天
                            {
                                var sonId = tbRA.SonType;
                                if (sonId >= 0)
                                {
                                    var tbTouZi = Table.GetRechargeActiveCumulative(sonId);
                                    if (tbTouZi != null && proxy.Character != null)
                                    {
                                        var exdataId = tbTouZi.FlagTrueId;
                                        if (proxy.Character.GetFlag(exdataId))
                                        {
                                            endHour = endHour + (7 * 24);
                                        }
                                    }
                                }
                            }

                            var age = Utils.GetServerAge(serverId);
                            var hour = age.TotalHours;
                            if (hour < startHour || hour > endHour)
                            {
                                return ErrorCodes.Error_AnswerNotTime;
                            }
                        }
                    }
                    break;
            }
            return ErrorCodes.OK;
        }

        public bool OnSyncRequested(LogicCharacterProxy charProxy, ulong characterId, uint syncId)
        {
            var proxy = (LogicProxy)charProxy;
            var characterController =
                CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (characterController == null)
            {
                return false;
            }

            var resType = (eResourcesType)syncId;

            proxy.SyncCenter.AddSyncData(proxy.CharacterId, syncId, characterController.mBag, resType.ToString(),
                () => { return characterController.mBag.GetRes(resType); });

            return true;
        }

        //获得审核状态   0 审核状态
        public IEnumerator GetReviewState(Coroutine coroutine, LogicCharacterProxy _this, GetReviewStateInMessage msg)
        {
            switch (msg.Request.Type)
            {
                case 0:
                    msg.Response = Table.GetServerConfig(913).ToInt();
                    break;
                default:
                    break;
            }
            msg.Reply();
            yield break;
        }

        public IEnumerator ApplySuperVIP(Coroutine coroutine, LogicCharacterProxy _this, ApplySuperVIPInMessage msg)
        {
            var characterController = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (characterController == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }
            if (characterController.moniterData == null)
            {
                msg.Reply((int)ErrorCodes.StateError);
                yield break;
            }
            SuperVipRecord record = null;
            Table.ForeachSuperVip(temp =>
            {
                if (temp.ServerID == characterController.serverId)
                {
                    record = temp;
                    if (characterController.GetExData((int)eExdataDefine.e654) >= temp.MonthRechargeNum || characterController.GetExData((int)eExdataDefine.e653) >= temp.DayRechargeNum)
                    {
                        characterController.SetExData((int)eExdataDefine.e753, 1);
                    }
                    return false;
                }
                return true;
            });
            var config = Table.GetServerConfig(1360);
            if (config == null)
            {
                msg.Reply((int)ErrorCodes.Error_Lode_ErrorId);
                yield break;
            }
            var list = config.Value.Split('|').ToList();
            if (record != null)
            {
                //1360
                SuperVIPData data = new SuperVIPData();
                data.HeadUrl = record.HeadUrl;
                data.DayNum = record.DayRechargeNum;
                data.MonthNum = record.MonthRechargeNum;
                data.QQ = record.QQ;
               
                if (list.IndexOf(characterController.moniterData.pid) != -1)
                {
                    data.State = 1;
                }
                else
                {
                    data.State = 0;
                }
                //data.State = record.IsShowIcon;
                msg.Response = data;
                msg.Reply((int) ErrorCodes.OK);
            }
            else
            {
                msg.Reply((int)ErrorCodes.Error_Lode_ErrorId);
            }
          
        }
        
        //角色链接
        public IEnumerator OnConnected(Coroutine coroutine, LogicCharacterProxy charProxy, uint packId)
        {
            //             LogicProxy proxy = (LogicProxy)charProxy;
            // 
            //             LogManager.GetLogger("ConnectLost").Info("character {0} - {1} Logic OnConnected 1", proxy.CharacterId, proxy.ClientId);
            //             PlayerLog.WriteLog(proxy.CharacterId, "-----Logic-----OnConnected----------{0}", proxy.CharacterId);
            //             CharacterController obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(proxy.CharacterId);
            //             if (obj == null)
            //             {
            //                 Logger.Fatal("onConnected CharacterId={0}", proxy.CharacterId);
            //                 yield break;
            //             }
            //             proxy.Character = obj;
            //             obj.Proxy = proxy;
            //             proxy.Connected = true;
            // 
            //             obj.lExdata64.SetTime(Exdata64TimeType.LastOnlineTime, DateTime.Now);
            //             //foreach (var waitingCheckConnectedInMessages in proxy.WaitingCheckConnectedInMessages)
            //             //{
            //             //    waitingCheckConnectedInMessages.Reply();
            //             //}
            //             //proxy.WaitingCheckConnectedInMessages.Clear();
            // 
            //             proxy.Character.State = CharacterState.Connected;
            // 
            //             proxy.Character.OnlineTime = DateTime.Now;
            //             var msg = LogicServer.Instance.LoginAgent.GetTodayOnlineSeconds(proxy.ClientId, proxy.CharacterId);
            //             yield return msg.SendAndWaitUntilDone(coroutine);
            //             if (msg.State == MessageState.Reply && msg.ErrorCode == (int)ErrorCodes.OK)
            //             {
            //                 proxy.Character.TodayTimes = msg.Response;
            //                 proxy.Character.SetExData(31, (int)msg.Response);
            //             }
            //             else
            //             {
            //                 proxy.Character.TodayTimes = 0;
            //                 proxy.Character.SetExData(31, 0);
            //             }
            //             CharacterManager.Instance.UpdateSimpleData(proxy.CharacterId);
            // 
            //             var dbLoginSimple = LogicServer.Instance.LoginAgent.GetLoginSimpleData(proxy.ClientId, proxy.CharacterId);
            //             yield return dbLoginSimple.SendAndWaitUntilDone(coroutine);
            //             if (dbLoginSimple.State != MessageState.Reply)
            //             {
            //                 yield break;
            //             }
            //             if (obj.Proxy == null)
            //             {
            //                 Logger.Warn("logic OnConnected obj.Proxy is null! type=1, objId={0}", obj.mGuid);
            //                 yield break;
            //             }
            //             proxy.Character.SetName(dbLoginSimple.Response.Name);
            //             var allianceSimple = LogicServer.Instance.TeamAgent.GetAllianceCharacterData(proxy.CharacterId, proxy.Character.serverId, proxy.CharacterId, proxy.Character.GetLevel());
            //             yield return allianceSimple.SendAndWaitUntilDone(coroutine);
            //             if (allianceSimple.State != MessageState.Reply)
            //             {                
            //                 yield break;
            //             }
            //             if (allianceSimple.ErrorCode != (int)ErrorCodes.OK)
            //             {                
            //                 yield break;
            //             }
            //             if (obj.Proxy == null)
            //             {
            //                 Logger.Warn("logic OnConnected obj.Proxy is null! type=2, objId={0}", obj.mGuid);
            //                 yield break;
            //             }
            //             var allianceData = allianceSimple.Response;
            //             if (proxy.Character.mAlliance.AllianceId != allianceData.AllianceId)
            //             {
            //                 Logger.Warn("Character Alliance not same!character={0}, logic ={1},team={2}", proxy.CharacterId,
            //                     proxy.Character.mAlliance.AllianceId, allianceData.AllianceId);
            //                 proxy.Character.mAlliance.AllianceId = allianceData.AllianceId;
            //             }
            //             if (allianceData.AllianceId != 0)
            //             {
            //                 proxy.Character.mAlliance.State = AllianceState.Have;
            //                 proxy.Character.mAlliance.CleanApplyList();
            // 
            //                 //修改城主称号
            //                 var titleId = allianceData.Ladder == (int)eAllianceLadder.Chairman ? 5000 : 5001;
            //                 obj.ModityTitle(titleId,
            //                     allianceData.AllianceId == StaticParam.AllianceWarInfo[obj.serverId].OccupantId);
            //             }
            //             else
            //             {
            //                 proxy.Character.mAlliance.State = AllianceState.None;
            //             }
            //             if (proxy.Character.mAlliance.Ladder != allianceData.Ladder)
            //             {
            //                 Logger.Warn("Character Ladder not same!character={0}, logic ={1},team={2}", proxy.CharacterId,
            //                     proxy.Character.mAlliance.Ladder, allianceData.Ladder);
            //                 proxy.Character.mAlliance.Ladder = allianceData.Ladder;
            //             }
            //             int index = 0;
            //             int[] Temp = new int[3];
            //             foreach (int apply in allianceData.Applys)
            //             {
            //                 Temp[index] = apply;
            //                 index++;
            //             }
            //             for (int i = 286; i <= 288; ++i)
            //             {
            //                 int tempExdata = proxy.Character.GetExData(i);
            //                 if (tempExdata != 0)
            //                 {
            //                     if (Temp.Contains(tempExdata))
            //                     {
            //                         Logger.Warn("Character Apply Alliance not same!character={4}, logic ={0},team={1},{2},{3}",
            //                             tempExdata, Temp[0], Temp[1], Temp[2], proxy.CharacterId);
            //                     }
            //                 }
            //                 proxy.Character.SetExData(i, Temp[i - 286]);
            //             }
            // 
            //             
            // 
            //             //var notifyConnectedMsg = LogicServer.Instance.LoginAgent.NotifyConnected(proxy.ClientId, proxy.CharacterId,
            //             //    (int)ServiceType.Logic, (int)ErrorCodes.OK);
            //             //yield return notifyConnectedMsg.SendAndWaitUntilDone(coroutine);
            // 
            //             RegisterAllSyncData(proxy.Character);
            // 
            //             var msg1 = LogicServer.Instance.GameMasterAgent.CharacterConnected(proxy.CharacterId, proxy.CharacterId,(int)ServiceType.Logic);
            //             yield return msg1.SendAndWaitUntilDone(coroutine);
            yield break;
        }

        //         public IEnumerator OnConnectedEx(Coroutine coroutine, LogicCharacterProxy charProxy, uint packId)
        //         {
        //             LogicProxy proxy = (LogicProxy) charProxy;
        // 
        //             LogManager.GetLogger("ConnectLost").Info("character {0} - {1} Logic OnConnected 1", proxy.CharacterId, proxy.ClientId);
        //             PlayerLog.WriteLog(proxy.CharacterId, "-----Logic-----OnConnected----------{0}", proxy.CharacterId);
        //             CharacterController obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(proxy.CharacterId);
        //             if (obj == null)
        //             {
        //                 Logger.Fatal("onConnected CharacterId={0}", proxy.CharacterId);
        //                 yield break;
        //             }
        //             proxy.Character = obj;
        //             obj.Proxy = proxy;
        //             proxy.Connected = true;
        // 
        //             //foreach (var waitingCheckConnectedInMessages in proxy.WaitingCheckConnectedInMessages)
        //             //{
        //             //    waitingCheckConnectedInMessages.Reply();
        //             //}
        //             //proxy.WaitingCheckConnectedInMessages.Clear();
        // 
        //             proxy.Character.State = CharacterState.Connected;
        // 
        //             proxy.Character.OnlineTime = DateTime.Now;
        //             var msg = LogicServer.Instance.LoginAgent.GetTodayOnlineSeconds(proxy.ClientId, proxy.CharacterId);
        //             yield return msg.SendAndWaitUntilDone(coroutine);
        //             if (msg.State == MessageState.Reply && msg.ErrorCode == (int)ErrorCodes.OK)
        //             {
        //                 proxy.Character.TodayTimes = msg.Response;
        //                 proxy.Character.SetExData(31, (int)msg.Response);
        //             }
        //             else
        //             {
        //                 proxy.Character.TodayTimes = 0;
        //                 proxy.Character.SetExData(31, 0);
        //             }
        //             CharacterManager.Instance.UpdateSimpleData(proxy.CharacterId);
        // 
        //             var dbLoginSimple = LogicServer.Instance.LoginAgent.GetLoginSimpleData(proxy.ClientId, proxy.CharacterId);
        //             yield return dbLoginSimple.SendAndWaitUntilDone(coroutine);
        //             if (dbLoginSimple.State != MessageState.Reply)
        //             {
        //                 var notifyConnectedMsg2 = LogicServer.Instance.LoginAgent.NotifyConnected(proxy.ClientId,
        //                     proxy.CharacterId, (int) ServiceType.Logic, (int) ErrorCodes.Unknow);
        //                 yield return notifyConnectedMsg2.SendAndWaitUntilDone(coroutine);
        //                 yield break;
        //             }
        //             var allianceSimple = LogicServer.Instance.TeamAgent.GetAllianceCharacterData(proxy.CharacterId, proxy.Character.serverId, proxy.CharacterId, proxy.Character.GetLevel());
        //             yield return allianceSimple.SendAndWaitUntilDone(coroutine);
        //             if (allianceSimple.State != MessageState.Reply)
        //             {
        //                 var notifyConnectedMsg2 = LogicServer.Instance.LoginAgent.NotifyConnected(proxy.ClientId,
        //                     proxy.CharacterId, (int) ServiceType.Logic, (int) ErrorCodes.Unknow);
        //                 yield return notifyConnectedMsg2.SendAndWaitUntilDone(coroutine);
        //                 yield break;
        //             }
        //             if (allianceSimple.ErrorCode != (int)ErrorCodes.OK)
        //             {
        //                 var notifyConnectedMsg2 = LogicServer.Instance.LoginAgent.NotifyConnected(proxy.ClientId,
        //                     proxy.CharacterId, (int) ServiceType.Logic, allianceSimple.ErrorCode);
        //                 yield return notifyConnectedMsg2.SendAndWaitUntilDone(coroutine);
        //                 yield break;
        //             }
        //             var allianceData = allianceSimple.Response;
        //             if (proxy.Character.mAlliance.AllianceId != allianceData.AllianceId)
        //             {
        //                 Logger.Warn("Character Alliance not same!character={0}, logic ={1},team={2}", proxy.CharacterId,
        //                     proxy.Character.mAlliance.AllianceId, allianceData.AllianceId);
        //                 proxy.Character.mAlliance.AllianceId = allianceData.AllianceId;
        //             }
        //             if (allianceData.AllianceId != 0)
        //             {
        //                 proxy.Character.mAlliance.State = AllianceState.Have;
        //                 proxy.Character.mAlliance.CleanApplyList();
        // 
        //                 //修改城主称号
        //                 var titleId = allianceData.Ladder == (int)eAllianceLadder.Chairman ? 5000 : 5001;
        //                 obj.ModityTitle(titleId,
        //                     allianceData.AllianceId == StaticParam.AllianceWarInfo[obj.serverId].OccupantId);
        //             }
        //             else
        //             {
        //                 proxy.Character.mAlliance.State = AllianceState.None;
        //             }
        //             if (proxy.Character.mAlliance.Ladder != allianceData.Ladder)
        //             {
        //                 Logger.Warn("Character Ladder not same!character={0}, logic ={1},team={2}", proxy.CharacterId,
        //                     proxy.Character.mAlliance.Ladder, allianceData.Ladder);
        //                 proxy.Character.mAlliance.Ladder = allianceData.Ladder;
        //             }
        //             int index = 0;
        //             int[] Temp = new int[3];
        //             foreach (int apply in allianceData.Applys)
        //             {
        //                 Temp[index] = apply;
        //                 index++;
        //             }
        //             for (int i = 286; i <= 288; ++i)
        //             {
        //                 int tempExdata = proxy.Character.GetExData(i);
        //                 if (tempExdata != 0)
        //                 {
        //                     if (Temp.Contains(tempExdata))
        //                     {
        //                         Logger.Warn("Character Apply Alliance not same!character={4}, logic ={0},team={1},{2},{3}",
        //                             tempExdata, Temp[0], Temp[1], Temp[2], proxy.CharacterId);
        //                     }
        //                 }
        //                 proxy.Character.SetExData(i, Temp[i - 286]);
        //             }
        //             proxy.Character.SetName(dbLoginSimple.Response.Name);
        //             var notifyConnectedMsg = LogicServer.Instance.LoginAgent.NotifyConnected(proxy.ClientId, proxy.CharacterId,
        //                 (int) ServiceType.Logic, (int) ErrorCodes.OK);
        //             yield return notifyConnectedMsg.SendAndWaitUntilDone(coroutine);
        // 
        //             RegisterAllSyncData(proxy.Character);
        // 
        //             var msg1 = LogicServer.Instance.GameMasterAgent.CharacterConnected(proxy.CharacterId, proxy.CharacterId,(int)ServiceType.Logic);
        //             yield return msg1.SendAndWaitUntilDone(coroutine);
        // 
        //             while (proxy.Connected)
        //             {
        //                 yield return proxy.Wait(coroutine, TimeSpan.FromSeconds(1));
        //                 try
        //                 {
        //                     proxy.Sync();
        //                 }
        //                 catch (Exception ex)
        //                 {
        //                     Logger.Error("Tick error.", ex);
        //                 }
        //             }
        //         }

        //角色断开链接
        public IEnumerator OnLost(Coroutine coroutine, LogicCharacterProxy charProxy, uint packId)
        {
            //             LogicProxy proxy = (LogicProxy)charProxy;
            // 
            //             LogManager.GetLogger("ConnectLost").Info("character {0} - {1} Logic OnLost 1", proxy.CharacterId, proxy.ClientId);
            //             PlayerLog.WriteLog(proxy.CharacterId, "----------Logic--------------------OnLost--------------------{0}", proxy.CharacterId);
            //             //TODO
            //             //断线删除
            //             //Character.TestBagDbIndex();
            // 
            //             if (proxy.Character== null)
            //             {
            //                 yield break;
            //             }
            //             RemoveAllSyncData(proxy.Character);
            //             foreach (var i in proxy.Character.mFriend.mDbData.BeHaveFriends)
            //             {
            //                 var SceneMsg = LogicServer.Instance.SceneAgent.SendOutLineFriend(i, 0, proxy.CharacterId, i);
            //                 yield return SceneMsg.SendAndWaitUntilDone(coroutine);
            //             }
            //             foreach (var i in proxy.Character.mFriend.mDbData.BeHaveEnemys)
            //             {
            //                 var SceneMsg = LogicServer.Instance.SceneAgent.SendOutLineFriend(i, 1, proxy.CharacterId, i);
            //                 yield return SceneMsg.SendAndWaitUntilDone(coroutine);
            //             }
            //             foreach (var i in proxy.Character.mFriend.mDbData.BeHaveShield)
            //             {
            //                 var SceneMsg = LogicServer.Instance.SceneAgent.SendOutLineFriend(i, 2, proxy.CharacterId, i);
            //                 yield return SceneMsg.SendAndWaitUntilDone(coroutine);
            //             }
            //             proxy.Character.OutLine();
            //             var co = CoroutineFactory.NewSubroutine(CharacterManager.Instance.RemoveCharacter, coroutine, proxy.CharacterId);
            //             if (co.MoveNext())
            //             {
            //                 yield return co;
            //             }
            //             proxy.Character.Proxy = null;
            // 
            //             proxy.Connected = false;
            //foreach (var waitingCheckLostInMessage in proxy.WaitingCheckLostInMessages)
            //{
            //    waitingCheckLostInMessage.Reply();
            //}
            //proxy.WaitingCheckLostInMessages.Clear();
            yield break;
        }

        //客户端请求技能数据
        public IEnumerator ApplySkill(Coroutine coroutine, LogicCharacterProxy charProxy, ApplySkillInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ApplySkill----------");
            var skilldata = proxy.Character.mSkill;
            foreach (var i in skilldata.mDbData.Skills)
            {
                msg.Response.Skill.Add(i.Key, i.Value);
            }
            var Talendata = proxy.Character.mTalent;
            foreach (var i in Talendata.Talents)
            {
                msg.Response.Innate.Add(i.Key, i.Value);
            }
            msg.Response.InnateCount = Talendata.TalentCount;
            foreach (var i in skilldata.mDbData.EquipSkills)
            {
                msg.Response.EquipSkills.Add(i);
            }
            foreach (var skill in Talendata.Skills)
            {
                msg.Response.SkillCount.Add(skill.Key, skill.Value);
            }
            msg.Reply();
            return null;
        }

        //升级天赋
        public IEnumerator UpgradeInnate(Coroutine coroutine, LogicCharacterProxy charProxy, UpgradeInnateInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var innateid = msg.Request.InnateId;
            var ret = proxy.Character.mTalent.AddTalent(proxy.Character, innateid);
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------UpgradeInnate----------:{0}", innateid);

            try
            {
                var klog = string.Format("upgradexiulian#{0}|{1}|{2}|{3}|{4}",
                    proxy.Character.mGuid,
                    proxy.Character.GetLevel(),
                    proxy.Character.serverId,
                    innateid,
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                PlayerLog.Kafka(klog, 2);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
            }

            msg.Reply((int)ret);
            return null;
        }

        //重置天赋
        public IEnumerator ClearInnate(Coroutine coroutine, LogicCharacterProxy charProxy, ClearInnateInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var TalentId = msg.Request.InnateId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ClearInnate----------:{0}", TalentId);
            if (TalentId < 0)
            {
                if (proxy.Character.mTalent.GetTalentCount(0) < 1)
                {
                    msg.Reply((int)ErrorCodes.Error_InnateZero);
                    yield break;
                }
                var needType = Table.GetServerConfig(258).ToInt();
                var needValue = Table.GetServerConfig(259).ToInt();
                if (proxy.Character.mBag.GetItemCount(needType) < needValue)
                {
                    msg.Reply((int)ErrorCodes.Error_ResNoEnough);
                    yield break;
                }
                proxy.Character.mBag.DeleteItem(needType, needValue, eDeleteItemType.ClearInnate);
                proxy.Character.mTalent.RefreshTalent(proxy.Character);
            }
            else
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
                //Character.mTalent.CleanTalent(TalentId);
            }
            msg.Response = proxy.Character.mTalent.TalentCount;
            msg.Reply();
        }

        //重置技能天赋
        public IEnumerator ResetSkillTalent(Coroutine coroutine,
                                            LogicCharacterProxy charProxy,
                                            ResetSkillTalentInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ResetSkillTalent----------{0}",
                msg.Request.SkillId);
            ;
            msg.Reply((int)proxy.Character.mTalent.ResetSkillTalent(proxy.Character, msg.Request.SkillId));
            yield break;
        }

        //客户端请求所有包裹数据
        public IEnumerator ApplyBags(Coroutine coroutine, LogicCharacterProxy charProxy, ApplyBagsInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ApplyBags----------");
            foreach (var bag in proxy.Character.mBag.mBags)
            {
                var dbBag = new BagBaseData
                {
                    BagId = bag.Value.GetBagId(),
                    NowCount = bag.Value.GetNowCount(),
                    NextSecond =
                        bag.Value.GetNextTime() - (int)DateTime.Now.GetDiffSeconds(proxy.Character.OnlineTime) +
                        bag.Value.RemoveBuyTimes
                };
                msg.Response.Bags.Add(bag.Key, dbBag);


                foreach (var item in bag.Value.mLogics)
                {
                    if (item.GetId() == -1)
                    {
                        continue;
                    }
                    var itemBase = new ItemBaseData
                    {
                        ItemId = item.GetId(),
                        Count = item.GetCount(),
                        Index = item.GetIndex()
                    };
                    itemBase.Exdata.Clear();
                    item.CopyTo(itemBase.Exdata);
                    dbBag.Items.Add(itemBase);
                }
                //break;
            }
            msg.Response.Resources.AddRange(proxy.Character.mBag.mDbData.Resources);
            msg.Reply();
            return null;
        }

        public IEnumerator ApplyBagByType(Coroutine coroutine,
                                          LogicCharacterProxy charProxy,
                                          ApplyBagByTypeInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ApplyBagByType----------");
            var bagId = msg.Request.BagType;
            var bag = proxy.Character.mBag.GetBag(bagId);
            if (bag == null)
            {
                msg.Reply((int)ErrorCodes.Error_BagID);
                yield break;
            }
            msg.Response.BagId = bag.GetBagId();
            msg.Response.NowCount = bag.GetNowCount();
            msg.Response.NextSecond = bag.GetNextTime() - (int)DateTime.Now.GetDiffSeconds(proxy.Character.OnlineTime) +
                                      bag.RemoveBuyTimes;
            foreach (var item in bag.mLogics)
            {
                if (item.GetId() == -1)
                {
                    continue;
                }
                var itemBase = new ItemBaseData
                {
                    ItemId = item.GetId(),
                    Count = item.GetCount(),
                    Index = item.GetIndex()
                };
                itemBase.Exdata.Clear();
                item.CopyTo(itemBase.Exdata);
                msg.Response.Items.Add(itemBase);
            }
            msg.Reply();
        }

        public IEnumerator BossHomeCost(Coroutine coroutine, LogicCharacterProxy _this, BossHomeCostInMessage msg)
        {
            //var proxy = (LogicProxy)_this;
            //PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------BossHomeCost----------");
            //var PlayerEnterCost = 0;

            //if (msg.Request.Id == 22000)
            //{
            //    PlayerEnterCost = Table.GetServerConfig(3003).ToInt();
            //}
            //else if (msg.Request.Id == 22001)
            //{
            //    PlayerEnterCost = Table.GetServerConfig(3004).ToInt();
            //}
            //proxy.Character.mBag.DeleteItem(3, PlayerEnterCost, eDeleteItemType.UseItem);
            yield break;
        }
        //商店购买
        public IEnumerator StoreBuyEquip(Coroutine coroutine, LogicCharacterProxy charProxy, StoreBuyEquipInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var storeid = msg.Request.StoreId;
            var bagid = msg.Request.BagId;
            var bagindex = msg.Request.BagIndex;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------StoreBuyEquip----------:{0},{1},{2}",
                storeid, bagid, bagindex);
            var result = 0;
            var errorCode = ErrorCodes.Unknow;
            if (msg.Request.ServiceType == (int)NpcService.NsBlackMarket)
            {
                var record = Table.GetStore(storeid);
                if (record == null)
                {
                    errorCode = ErrorCodes.Error_GoodId_Not_Exist;
                    msg.Reply();
                    msg.Response = 0;
                    yield break;
                }

                var storeRet = AsyncReturnValue<int>.Create();
                var co = CoroutineFactory.NewSubroutine(GetFubenStoreItemCount, coroutine, charProxy, record.Type, storeid, storeRet);
                if (co.MoveNext())
                    yield return co;
                var curCount = storeRet.Value;
                storeRet.Dispose();

                var buyCount = 1;
                if (buyCount > curCount) // not enough
                {
                    errorCode = ErrorCodes.Error_ResNoEnough;
                }
                else
                {
                    errorCode = proxy.Character.mStone.BuyEquipItem(storeid, bagid, bagindex, ref result);
                    if (errorCode == ErrorCodes.OK)
                    {
                        var buyRet = AsyncReturnValue<bool>.Create();
                        co = CoroutineFactory.NewSubroutine(ConsumeFubenItemCount, coroutine, charProxy, record.Type, storeid, buyCount, buyRet);
                        if (co.MoveNext())
                            yield return co;
                        buyRet.Dispose();
                        if (buyRet.Value == false)
                            errorCode = ErrorCodes.Error_DungeonShopItemsNotEnough;
                    }
                }
            }
            else
            {
                errorCode = proxy.Character.mStone.BuyEquipItem(storeid, bagid, bagindex, ref result);
            }

            msg.Reply((int)errorCode);
            msg.Response = result;
            yield break;
        }

        //获得当前题目
        public IEnumerator GetQuestionData(Coroutine coroutine,
                                           LogicCharacterProxy charProxy,
                                           GetQuestionDataInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------GetQuestionData----------");
            proxy.Character.GetNowQuestion(msg.Response);
            msg.Reply();
            yield break;
        }

        //回答当前题目
        public IEnumerator AnswerQuestion(Coroutine coroutine,
                                          LogicCharacterProxy charProxy,
                                          AnswerQuestionInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var answer = msg.Request.Answer;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------AnswerQuestion----------:{0}", answer);
            var result = proxy.Character.AnswerQuestion(answer == 1, msg.Response);
            msg.Reply((int)result);
            yield break;
        }

        public IEnumerator RemoveErrorAnswer(Coroutine coroutine,
                                             LogicCharacterProxy charProxy,
                                             RemoveErrorAnswerInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------RemoveErrorAnswer----------");
            var result = proxy.Character.RemoveErrorAnswer();
            msg.Reply((int)result);
            yield break;
        }

        public IEnumerator AnswerQuestionUseItem(Coroutine coroutine,
                                                 LogicCharacterProxy charProxy,
                                                 AnswerQuestionUseItemInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------AnswerQuestionUseItem----------");
            var result = proxy.Character.AnswerQuestionUseItem(msg.Response);
            msg.Reply((int)result);
            yield break;
        }

        //客户端请求标记位数据
        public IEnumerator ApplyFlag(Coroutine coroutine, LogicCharacterProxy charProxy, ApplyFlagInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var flagId = msg.Request.FlagId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ApplyFlag----------:{0}", flagId);

            if (flagId < 0)
            {
                msg.Response.Items.Clear();
                msg.Response.Items.Capacity = proxy.Character.lFlag.mData.GetData().Count;
                msg.Response.Items.AddRange(proxy.Character.lFlag.mData.GetData());
            }
            else
            {
                var result = proxy.Character.lFlag.mData.GetFlag(flagId);
                msg.Response.Items.Add(result);
            }
            msg.Reply();
            return null;
        }

        //客户端请求扩展数据
        public IEnumerator ApplyExdata(Coroutine coroutine, LogicCharacterProxy charProxy, ApplyExdataInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var exdataId = msg.Request.ExdataId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ApplyExdata----------{0}", exdataId);
            if (exdataId < 0)
            {
                msg.Response.Items.Clear();
                msg.Response.Items.AddRange(proxy.Character.lExdata.mData);
            }
            else
            {
                var result = proxy.Character.GetExData(exdataId);
                msg.Response.Items.Add(result);
            }
            msg.Reply();
            return null;
        }

        public IEnumerator ApplyExdata64(Coroutine coroutine, LogicCharacterProxy charProxy, ApplyExdata64InMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var exdataId = msg.Request.ExdataId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ApplyExdata64----------:{0}", exdataId);
            if (exdataId < 0)
            {
                msg.Response.Items.Clear();
                msg.Response.Items.AddRange(proxy.Character.lExdata64.mData);
            }
            else
            {
                var result = proxy.Character.GetExData64(exdataId);
                msg.Response.Items.Add(result);
            }
            msg.Reply();

            yield break;
        }

        //请求任务数据
        public IEnumerator ApplyMission(Coroutine coroutine, LogicCharacterProxy charProxy, ApplyMissionInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var missionId = msg.Request.MissionId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ApplyMission----------,{0}", missionId);
            if (missionId < 0)
            {
                msg.Response.Missions.Clear();
                foreach (var mission in proxy.Character.mTask.mData)
                {
                    var mis = mission.Value;
                    var mbd = new MissionBaseData
                    {
                        MissionId = mis.Id
                    };
                    mbd.Exdata.AddRange(mis.Data);
                    msg.Response.Missions[mis.Id] = mbd;
                }
            }
            else
            {
                var mis = proxy.Character.mTask.GetMission(missionId);
                if (mis == null)
                {
                    msg.Reply();
                    yield break;
                }
                var mbd = new MissionBaseData
                {
                    MissionId = mis.Id
                };
                mbd.Exdata.AddRange(mis.Data);
                msg.Response.Missions[mis.Id] = mbd;
            }
            msg.Reply();
        }

        public IEnumerator TowerSweep(Coroutine coroutine, LogicCharacterProxy charProxy, TowerSweepInMessage msg)
        {

            var proxy = (LogicProxy)charProxy;

            var result = proxy.Character.TowerSweep(msg.Response);
            msg.Reply((int)result);
            yield break;
        }
        public IEnumerator TowerBuySweepTimes(Coroutine coroutine, LogicCharacterProxy charProxy, TowerBuySweepTimesInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var result = proxy.Character.TowerBuySweepTimes();
            yield break;
        }
        public IEnumerator CheckTowerDailyInfo(Coroutine coroutine, LogicCharacterProxy charProxy, CheckTowerDailyInfoInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var result = proxy.Character.CheckTowerDailyInfo();
            yield break;
        }

        public IEnumerator ApplyFieldActivityReward(Coroutine coroutine, LogicCharacterProxy charProxy, ApplyFieldActivityRewardInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var tbMission = Table.GetObjectTable(msg.Request.Id);
            if (tbMission == null)
            {
                msg.Reply((int)ErrorCodes.Error_MissionID);
                yield break;
            }
            int score = proxy.Character.GetExData(949);
            int addScore = 0;
            int meritPoint = 0;
            Dictionary<int, int> rewards = new Dictionary<int, int>();
            for (int i = 0; i < tbMission.Reward.Length && i < tbMission.RewardNum.Length; i++)
            {
                if (tbMission.Reward[i] == (int)eResourcesType.MieshiScore)
                {
                    addScore += tbMission.RewardNum[i];
                }
                if (tbMission.Reward[i] == (int)eResourcesType.Contribution)
                {
                    meritPoint += tbMission.RewardNum[i];
                }
                rewards.modifyValue(tbMission.Reward[i], tbMission.RewardNum[i]);
            }

            {//logic验证本地数据
                if (true == proxy.Character.GetFlag(tbMission.IsGet))
                {//已经领取过
                    msg.Reply((int)ErrorCodes.Error_GiftAlreadyReceive);
                    yield break;
                }
                if (tbMission.TaskType == 0)
                {//个人任务
                    var count = proxy.Character.GetExData(tbMission.ExData);
                    if (count < tbMission.NeedCount)
                    {
                        msg.Reply((int)ErrorCodes.Unknow);
                        yield break;
                    }
                }
            }
            {//team验证远端数据
                FieldRankBaseData rankData = new FieldRankBaseData();
                rankData.Guid = proxy.CharacterId;
                rankData.Name = proxy.Character.GetName();
                rankData.Level = proxy.Character.GetLevel();
                rankData.Score = addScore + score;
                rankData.TypeId = proxy.Character.GetRole();
                {
                    var nameSimples = LogicServer.Instance.SceneAgent.GetSceneSimpleData(proxy.CharacterId, 0);
                    yield return nameSimples.SendAndWaitUntilDone(coroutine);
                    if (nameSimples.State == MessageState.Reply)
                    {
                        if (nameSimples.ErrorCode == (int)ErrorCodes.OK)
                        {
                            rankData.FightPoint = nameSimples.Response.FightPoint;
                        }
                    }
                }
                var msg1 = LogicServer.Instance.TeamAgent.SSApplyFieldActivityReward(proxy.CharacterId,
                    proxy.Character.serverId, proxy.Character.mAlliance.AllianceId, proxy.CharacterId, msg.Request.Id, meritPoint, addScore, rankData);
                yield return msg1.SendAndWaitUntilDone(coroutine);
                if (msg1.State != MessageState.Reply)
                {
                    Logger.Error("SSApplyFieldActivityReward replied with state = {0}", msg1.State);
                    msg.Reply((int)ErrorCodes.Unknow);
                    yield break;
                }
                if (msg1.ErrorCode != (int)ErrorCodes.OK)
                {
                    msg.Reply(msg1.ErrorCode);
                    yield break;
                }
            }
            {//发放奖励
                proxy.Character.AddExData(949, addScore);
                proxy.Character.SetFlag(tbMission.IsGet);
                proxy.Character.mBag.AddItemOrMail(503, rewards, null, eCreateItemType.FieldActive);
            }
            msg.Reply();
        }
        //public IEnumerator ApplyFriendListMsg(Coroutine coroutine, LogicCharacterProxy _this, ApplyFriendListMsgInMessage msg)
        //{
        //    var proxy = (LogicProxy)_this;
        //    var characterId = msg.Request.CharacterId;
        //    var character = CharacterManager.Instance.GetCharacterControllerFromMemroy((ulong)characterId);
        //    if (character == null)
        //    {
        //        Logger.Warn("FriendList Not Player OnLine----------LogicProxy------------ApplyFriendListMsg()");
        //        msg.Reply((int)ErrorCodes.Unline);
        //        yield break;
        //    }
        //    var data = character.GetSimpleData();
        //    MsgFriendist friendList = new MsgFriendist()
        //    {
        //        IsOnLine = 1,
        //        FriendLevel = data.Level,
        //        FriendRebronLV = data.Ladder,
        //    };
        //    msg.Response = friendList;
        //    msg.Reply();
        //}
        public IEnumerator SendSurvey(Coroutine coroutine, LogicCharacterProxy charProxy, SendSurveyInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            Dictionary<int, int> item = new Dictionary<int, int>();

            var tb = Table.GetSurvey(msg.Request.Id);
            if (tb != null)
            {
                if (proxy.Character.GetFlag(tb.flagHad) == true)
                    yield break;
                if (proxy.Character.GetFlag(tb.flagCan) == false)
                    yield break;

                item.Add(tb.reward, 1);
                proxy.Character.SetFlag(tb.flagHad);
                proxy.Character.mBag.AddItemOrMail(155, item, null, eCreateItemType.Survey);

                try
                {
                    var datas = string.Empty;
                    foreach (var vaule in msg.Request.Datas.List)
                    {
                        datas += vaule.x + "-";
                        datas += vaule.y + ",";
                    }

                    var klog = string.Format("wenjuandiaocha#{0}|{1}|{2}|{3}|{4}|{5}",
                        proxy.Character.serverId,
                        proxy.Character.mGuid,
                        proxy.Character.GetLevel(),
                        tb.Id,
                        datas,
                        DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                    PlayerLog.Kafka(klog, 2);
                }
                catch (Exception)
                {
                    //Logger.Error(exception.Message);
                }
            }

            yield break;
        }
        public IEnumerator SetHandbookFight(Coroutine coroutine, LogicCharacterProxy charProxy, SetHandbookFightInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;

            if (proxy.Character.mBook.Books.ContainsKey(msg.Request.Id))
            {
                proxy.Character.mBook.Fight = msg.Request.Id;

            }
            else
            {
                msg.Reply((int)ErrorCodes.Error_BookID);
                yield break;
            }
            msg.Reply();

            var msg1 = LogicServer.Instance.SceneAgent.SSBookFightingMonsterId(proxy.CharacterId, msg.Request.Id);
            yield return msg1.SendAndWaitUntilDone(coroutine);
        }
        //客户端请求图鉴
        public IEnumerator ApplyBooks(Coroutine coroutine, LogicCharacterProxy charProxy, ApplyBooksInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ApplyBooks----------");
            msg.Response.Books.AddRange(proxy.Character.mBook.Books);
            msg.Response.Group.AddRange(proxy.Character.mBook.Group);
            msg.Response.Fight = proxy.Character.mBook.Fight;
            msg.Reply();
            return null;
        }
        public IEnumerator AskMountData(Coroutine coroutine, LogicCharacterProxy charProxy, AskMountDataInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            msg.Response = proxy.Character.mMount.GetMountData();
            msg.Reply();
            yield break;
        }

        public IEnumerator AddMountSkin(Coroutine coroutine, LogicCharacterProxy charProxy, AddMountSkinInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var mountId = msg.Request.Id;

            var tb = Table.GetMount(mountId);
            if (tb == null)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            //检查物品
            var needItemId = tb.NeedItem;
            var needItemCount = tb.GetExp;//策划指定用此数据
            if (proxy.Character.mBag.GetItemCount(needItemId) < needItemCount)
            {
                msg.Reply((int)ErrorCodes.ItemNotEnough);
                yield break;
            }

            //删除物品
            proxy.Character.mBag.DeleteItem(needItemId, needItemCount, eDeleteItemType.MountSkinAdd);

            //添加皮肤
            var result = proxy.Character.mMount.AddSkin(mountId);
            if (result == ErrorCodes.OK)
            {
                proxy.SendMountData(proxy.Character.mMount.GetMountData());
            }
            else
            {
                msg.Reply((int)result);//添加坐骑皮肤失败
                yield break;
            }
            msg.Reply();
            yield break;
        }

        public IEnumerator MountUp(Coroutine coroutine, LogicCharacterProxy charProxy, MountUpInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;


            var tb = Table.GetMount(proxy.Character.mMount.mDbData.Id);
            if (tb == null)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            if (proxy.Character.mBag.GetItemCount(tb.NeedItem) <= 0)
            {
                msg.Reply((int)ErrorCodes.ItemNotEnough);
                yield break;
            }


            ErrorCodes err = proxy.Character.mMount.MountUp();
            if (ErrorCodes.OK != err)
            {
                msg.Reply((int)err);
                yield break;
            }
            msg.Response.Items.Add(proxy.Character.mMount.mDbData.Id);
            msg.Response.Items.Add(proxy.Character.mMount.mDbData.Step);
            msg.Response.Items.Add(proxy.Character.mMount.mDbData.Level);
            msg.Response.Items.Add(proxy.Character.mMount.mDbData.Exp);
            msg.Response.Items.Add(proxy.Character.mMount.mDbData.Ride);
            msg.Reply();
            proxy.Character.mBag.DeleteItem(tb.NeedItem, 1, eDeleteItemType.MountLevelup);
            proxy.Character.SetFlag(2683, true);
            yield break;
        }
        public IEnumerator RideMount(Coroutine coroutine, LogicCharacterProxy charProxy, RideMountInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            if (false == proxy.Character.mMount.Ride(msg.Request.Id))
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            msg.Reply();
        }
        public IEnumerator Mount(Coroutine coroutine, LogicCharacterProxy charProxy, MountInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            proxy.Character.Mount(msg.Request.ItemId);

            msg.Reply();
            yield break;
        }
        public IEnumerator MountSkill(Coroutine coroutine, LogicCharacterProxy charProxy, MountSkillInMessage msg)
        {//提升技能
            var proxy = (LogicProxy)charProxy;


            var tb = Table.GetMountSkill(msg.Request.SkillId);
            if (tb == null)
            {
                msg.Reply((int)ErrorCodes.Error_SkillID);
                yield break;
            }
            if (!proxy.Character.mMount.mDbData.Skills.ContainsKey(msg.Request.SkillId))
            {
                msg.Reply((int)ErrorCodes.Error_MountSkill_Limit);
                yield break;
            }
            int lv = proxy.Character.mMount.mDbData.Skills[msg.Request.SkillId];
            if (lv >= tb.MaxLevel)
            {
                msg.Reply((int)ErrorCodes.Error_MountSkill_MAX_Level);
                yield break;
            }

            var cost = Table.GetConsumArray(tb.CostList[lv]);
            if (cost == null)
            {
                msg.Reply((int)ErrorCodes.Error_MountSkill_MAX_Level);
                yield break;
            }
            if (proxy.Character.mBag.GetItemCount(cost.ItemId[0]) < cost.ItemCount[0])
            {
                msg.Reply((int)ErrorCodes.ItemNotEnough);
                yield break;
            }

            ErrorCodes err = proxy.Character.mMount.SkillUp(msg.Request.SkillId);
            if (ErrorCodes.OK != err)
            {
                msg.Reply((int)err);
                yield break;
            }
            msg.Reply();
            proxy.Character.mBag.DeleteItem(cost.ItemId[0], cost.ItemCount[0], eDeleteItemType.MountSkillUp);
            yield break;
        }


        public IEnumerator MountFeed(Coroutine coroutine, LogicCharacterProxy charProxy, MountFeedInMessage msg)
        {//喂养
            var proxy = (LogicProxy)charProxy;

            var tb = Table.GetMountFeed(msg.Request.ItemId);
            if (tb == null)
            {
                msg.Reply((int)ErrorCodes.Error_ItemID);
                yield break;
            }

            if (proxy.Character.mBag.GetItemCount(msg.Request.ItemId) <= 0)
            {
                msg.Reply((int)ErrorCodes.ItemNotEnough);
                yield break;
            }

            ErrorCodes err = proxy.Character.mMount.MountFeed(msg.Request.ItemId);
            if (ErrorCodes.OK != err)
            {
                msg.Reply((int)err);
                yield break;
            }
            msg.Reply();
            proxy.Character.mBag.DeleteItem(msg.Request.ItemId, 1, eDeleteItemType.MountFeed);
            yield break;
        }
        //GM命令
        public IEnumerator GMLogic(Coroutine coroutine, LogicCharacterProxy charProxy, GMLogicInMessage msg)
        {
            var logproxy = (LogicProxy)charProxy;
            if (null == logproxy.Character)
            {
                yield break;
            }

            var level = GMCommandLevel.GetCommandLevel();
            if (GMCommandLevel.GMCommandLevelType.ALLOW == level)
            {//所有人都允许
            }
            else if (GMCommandLevel.GMCommandLevelType.GMALLOW == level)
            {//GM允许
                if (!logproxy.Character.mDbData.IsGM)
                {
                    yield break;
                }
            }
            else
            {//都不允许
                yield break;
            }



            var proxy = (LogicProxy)charProxy;
            var command = msg.Request.Commond;
            PlayerLog.WriteLog(logproxy.CharacterId, "----------Logic----------GMLogic----------:{0}", command);
            var err = proxy.Character.GmCommand(command);
            msg.Reply((int)err);
        }

        //更换装备
        public IEnumerator ReplaceEquip(Coroutine coroutine, LogicCharacterProxy charProxy, ReplaceEquipInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var nBagItemId = msg.Request.BagItemId;
            var PartBag = msg.Request.Part;
            var Index = msg.Request.Index;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ReplaceEquip----------{0}---({1}:{2})",
                nBagItemId, PartBag, Index);
            //int nEquipPoint = 0;
            var ret = proxy.Character.UseEquip(nBagItemId, PartBag, Index);
            msg.Reply((int)ret);
            return null;
        }

        public IEnumerator UseShiZhuang(Coroutine coroutine, LogicCharacterProxy charProxy, UseShiZhuangInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var bagId = msg.Request.BagId;
            var bagIndex = msg.Request.BagItemIndex;
            var partId = msg.Request.Part;
            var ret = proxy.Character.UseShiZhuang(bagId, bagIndex, partId);
            msg.Reply((int)ret);
            return null;
        }

        public IEnumerator ChangeEquipState(Coroutine coroutine, LogicCharacterProxy charProxy, ChangeEquipStateInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var parts = msg.Request.Parts.Items;
            var state = msg.Request.State == true ? 0 : 1;
            Logger.Warn("ChangeEquipState:[{0}]", state);
            var ret = proxy.Character.SetEquipModelState(parts, state);
            msg.Reply((int)ret);
            return null;
        }

        public IEnumerator RefreshFashionInfo(Coroutine coroutine, LogicCharacterProxy charProxy, RefreshFashionInfoInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            Logger.Warn("RefreshFashionInfo");
            proxy.Character.RefreshFashionState();
            yield break;
        }

        //接受任务
        public IEnumerator AcceptMission(Coroutine coroutine, LogicCharacterProxy charProxy, AcceptMissionInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var missionId = msg.Request.MissionId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------AcceptMission----------{0}", missionId);
            var mis = proxy.Character.mTask.Accept(proxy.Character, missionId);
            if (mis == null)
            {
                msg.Reply((int)ErrorCodes.Error_AcceptMission);
                yield break;
            }
            msg.Response.MissionId = mis.Id;
            msg.Response.Exdata.AddRange(mis.Data);
            msg.Reply();

            var tbMission = Table.GetMission(mis.Id);
            if (tbMission != null)
            {
                if (tbMission.SceneTransferId >= 0)
                {
                    var reslut = LogicServer.Instance.SceneAgent.MissionChangeSceneRequest(proxy.CharacterId, tbMission.SceneTransferId);
                    yield return reslut.SendAndWaitUntilDone(coroutine);
                    if (reslut.State != MessageState.Reply)
                    {
                        Logger.Error("In MissionChangeSceneRequest(), return with State = {0}", reslut.State);
                        yield break;
                    }
                    if (reslut.ErrorCode != (int)ErrorCodes.OK)
                    {
                        Logger.Error("In MissionChangeSceneRequest(), .ErrorCode = {0}", reslut.ErrorCode);
                        yield break;
                    }
                }
            }

            //后台统计
            try
            {
                string v = string.Format("mission#{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                    proxy.Character.serverId,
                    proxy.Character.mGuid,
                    proxy.Character.GetLevel(),
                    -1,
                    missionId,
                    0,
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                PlayerLog.Kafka(v);
            }
            catch (Exception)
            {
            }
        }

        //提交任务
        public IEnumerator CommitMission(Coroutine coroutine, LogicCharacterProxy charProxy, CommitMissionInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var missionId = msg.Request.MissionId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------CompleteMission----------{0}", missionId);
            var result = proxy.Character.mTask.Commit(proxy.Character, missionId);
            if (result == ErrorCodes.OK)
            {
                proxy.Character.mTask.GetNetDirtyMissions(msg.Response);
                proxy.Character.mTask.CleanNetDirty();
            }
            //msg.Response = missionId;
            msg.Reply((int)result);

            //后台统计
            try
            {
                string v = string.Format("mission#{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                    proxy.Character.serverId,
                    proxy.Character.mGuid,
                    proxy.Character.GetLevel(),
                    -1,
                    missionId,
                    1,
                    DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                PlayerLog.Kafka(v);
            }
            catch (Exception)
            {
            }
            return null;
        }

        //完成任务
        public IEnumerator CompleteMission(Coroutine coroutine,
                                           LogicCharacterProxy charProxy,
                                           CompleteMissionInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var missionId = msg.Request.MissionId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------CompleteMission----------{0}", missionId);
            var result = proxy.Character.mTask.Complete(proxy.Character, missionId);
            msg.Response = missionId;
            msg.Reply((int)result);
            yield break;
        }

        //放弃任务
        public IEnumerator DropMission(Coroutine coroutine, LogicCharacterProxy charProxy, DropMissionInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var missionId = msg.Request.MissionId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------DropMission----------{0}", missionId);
            var result = proxy.Character.mTask.Drop(missionId);
            msg.Response = (int)result;
            msg.Reply();
            return null;
        }

        //装备技能
        public IEnumerator EquipSkill(Coroutine coroutine, LogicCharacterProxy charProxy, EquipSkillInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------EquipSkill----------:");
            foreach (var id in msg.Request.EquipSkills.Items)
            {
                PlayerLog.WriteLog(proxy.CharacterId, "----------Items----------{0}----------", id);
            }
            var result = proxy.Character.mSkill.EquipSkills(msg.Request.EquipSkills.Items);
            msg.Response = (int)result;
            msg.Reply((int)result);
            return null;
        }

        //升级技能
        public IEnumerator UpgradeSkill(Coroutine coroutine, LogicCharacterProxy charProxy, UpgradeSkillInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------UpgradeSkill----------:{0}",
                msg.Request.SkillId);
            var skillLevel = 0;
            var result = proxy.Character.mSkill.UpgradeSkill(msg.Request.SkillId, ref skillLevel);
            msg.Response = skillLevel;
            msg.Reply((int)result);
            return null;
        }

        //出售物品
        public IEnumerator SellBagItem(Coroutine coroutine, LogicCharacterProxy charProxy, SellBagItemInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var bagType = msg.Request.BagType;
            var itemId = msg.Request.ItemId;
            var count = msg.Request.Count;
            var index = msg.Request.Index;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------SellBagItem----------{0},{1},{2},{3}",
                bagType, index, itemId, count);
            //TODO
            var result = proxy.Character.SellItem(bagType, index, itemId, count);
            if (result == ErrorCodes.OK)
            {
                msg.Reply();
            }
            else
            {
                msg.Reply((int)ErrorCodes.OK);
            }

            yield break;
        }

        //道具回收
        public IEnumerator RecycleBagItem(Coroutine coroutine,
                                          LogicCharacterProxy charProxy,
                                          RecycleBagItemInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var bagType = msg.Request.BagType;
            var itemId = msg.Request.ItemId;
            var count = msg.Request.Count;
            var index = msg.Request.Index;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------SellBagItem----------{0},{1},{2},{3}",
                bagType, index, itemId, count);
            //TODO
            var result = proxy.Character.Recycletem(bagType, index, itemId, count);
            if (result == ErrorCodes.OK)
            {
                msg.Reply();
            }
            else
            {
                msg.Reply((int)ErrorCodes.OK);
            }

            yield break;
        }

        //道具回收
        public IEnumerator RecycleBagItemList(Coroutine coroutine,
                                          LogicCharacterProxy charProxy,
                                          RecycleBagItemListInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var recycleList = msg.Request.ItemList;
            for (var i = 0; i < recycleList.RecycleList.Count; ++i)
            {
                var recycleItem = recycleList.RecycleList[i];
                var bagType = recycleItem.BagType;
                var itemId = recycleItem.ItemId;
                var count = recycleItem.Count;
                var index = recycleItem.Index;

                PlayerLog.WriteLog(proxy.CharacterId, "----Logic----RecycleBagItemList-----{0},{1},{2},{3}",
                    bagType, index, itemId, count);

                var result = proxy.Character.Recycletem(bagType, index, itemId, count);
                if (result != ErrorCodes.OK)
                {
                    msg.Reply((int)result);
                    yield break;
                }
            }

            msg.Reply();
            yield break;
        }

        //强化装备
        public IEnumerator EnchanceEquip(Coroutine coroutine, LogicCharacterProxy charProxy, EnchanceEquipInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId,
                "----------Logic----------EnchanceEquip----------bag[{0}:{1}]---Blessing={2},UpRate={3}",
                msg.Request.BagType, msg.Request.BagIndex, msg.Request.Blessing, msg.Request.UpRate);
            var nextLevel = 0;
            var errorCodes = proxy.Character.EnchanceEquip(msg.Request.BagType, msg.Request.BagIndex,
                msg.Request.Blessing, msg.Request.UpRate, msg.Request.CostGoldBlessNum, ref nextLevel);
            msg.Response = nextLevel;
            msg.Reply((int)errorCodes);
            yield break;
        }

        //追加装备
        public IEnumerator AppendEquip(Coroutine coroutine, LogicCharacterProxy charProxy, AppendEquipInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var bagType = msg.Request.BagType;
            var bagIndex = msg.Request.BagIndex;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------AppendEquip----------:{0},{1}", bagType,
                bagIndex);
            var nextValue = 0;
            var errorCodes = proxy.Character.EquipAdditionalEquip(bagType, bagIndex, ref nextValue);
            msg.Response = nextValue;
            msg.Reply((int)errorCodes);
            yield break;
        }

        public IEnumerator RandEquipSkill(Coroutine coroutine, LogicCharacterProxy charProxy, RandEquipSkillInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId,
                "----------Logic----------RandEquipSkill----------bag[{0}:{1}]---itemId={2}",
                msg.Request.BagType, msg.Request.BagIndex, msg.Request.ItemId);
            var buffId = -1;
            var errorCodes = proxy.Character.RandEquipSkill(msg.Request.BagType, msg.Request.BagIndex, msg.Request.ItemId, ref buffId);
            msg.Response = buffId;
            msg.Reply((int)errorCodes);
            yield break;
        }

        public IEnumerator UseEquipSkill(Coroutine coroutine, LogicCharacterProxy charProxy, UseEquipSkillInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId,
                "----------Logic----------UseEquipSkill----------bag[{0}:{1}]",
                msg.Request.BagType, msg.Request.BagIndex);
            var buffId = -1;
            var errorCodes = proxy.Character.UseEquipSkill(msg.Request.BagType, msg.Request.BagIndex, msg.Request.Type, ref buffId);
            msg.Response = buffId;
            msg.Reply((int)errorCodes);
            yield break;
        }

        public IEnumerator ReplaceElfSkill(Coroutine coroutine, LogicCharacterProxy charProxy, ReplaceElfSkillInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId,
                "----------Logic----------ReplaceElfSkill------elf----bag[{0}:{1}]",
                msg.Request.ElfBagIndex, msg.Request.ItemBagId, msg.Request.ItemBagIndex);

            int buffId = -1;
            var errorCodes = proxy.Character.ReplaceElfSkill(msg.Request.ElfBagIndex, msg.Request.ExdataIndex,
                msg.Request.ItemBagId, msg.Request.ItemBagIndex, ref buffId);
            msg.Response = buffId;
            msg.Reply((int)errorCodes);
            yield break;
        }

        //public IEnumerator PresentGift(Coroutine coroutine, LogicCharacterProxy charProxy, PresentGiftInMessage msg)
        //{
        //    var proxy = (LogicProxy)charProxy;
        //    var character = proxy.Character;
        //    var itemId = msg.Request.ItemId;
        //    var count = msg.Request.Count;
        //    var characterId = msg.CharacterId;
        //    PlayerLog.WriteLog(proxy.CharacterId,
        //        "----------Logic----------PresentGift------itemid={0} count={1}",
        //        msg.Request.ItemId, msg.Request.Count);
        //    // 检查物品
        //    if (count <= 0)
        //    {
        //        msg.Reply((int)ErrorCodes.Error_ItemNotFind);
        //        yield break;
        //    }

        //    //var haveCount = character.mBag.GetItemCount(itemId);
        //    //if (haveCount < count)
        //    //{
        //    //    msg.Reply((int)ErrorCodes.ItemNotEnough);
        //    //    yield break;
        //    //}

        //    var giftRate = int.Parse(Table.GetServerConfig(404).Value);
        //    var needDiamond = giftRate*count;
        //    if (needDiamond > character.mBag.GetRes(eResourcesType.DiamondRes))
        //    {
        //        msg.Reply((int)ErrorCodes.DiamondNotEnough);
        //        yield break;
        //    }

        //    // 主播是否在
        //    var msg1 = LogicServer.Instance.ChatAgent.SSGetCurrentAnchor(characterId, 0);
        //    yield return msg1.SendAndWaitUntilDone(coroutine);
        //    if (msg1.State != MessageState.Reply)
        //    {
        //        Logger.Error("SSGetCurrentAnchor replied with state = {0}", msg1.State);
        //        msg.Reply((int)ErrorCodes.Unknow);
        //        yield break;
        //    }
        //    if (msg1.ErrorCode != (int) ErrorCodes.OK)
        //    {
        //        msg.Reply(msg1.ErrorCode);
        //        yield break;                
        //    }
        //    if (msg1.Response == 0)
        //    {
        //        msg.Reply((int)ErrorCodes.Error_AnchorNotInRoom);
        //        yield break;                        
        //    }

        //    // 删除物品
        //    ErrorCodes error = character.mBag.DelRes(eResourcesType.DiamondRes, needDiamond, eDeleteItemType.PresentGift);
        //    if (error != ErrorCodes.OK)
        //    {
        //        msg.Reply((int)ErrorCodes.DiamondNotEnough);
        //        yield break;
        //    }

        //    // 排行
        //    var tempList1 = new RankChangeDataList
        //    {
        //        CharacterId = characterId,
        //        Name = character.Name,
        //        ServerId = character.serverId
        //    };

        //    Action<RankType, eExdataDefine> changeFunc = (type, exdata) =>
        //    {
        //        character.AddExData((int)exdata, count);
        //        character.SetRankFlag(type);
        //        var temp = new RankChangeData
        //        {
        //            RankType = (int)type,
        //            Value = character.GetExData((int)exdata)
        //        };
        //        tempList1.Changes.Add(temp);
        //    };

        //    changeFunc(RankType.DailyGift, eExdataDefine.e595);
        //    changeFunc(RankType.WeeklyGift, eExdataDefine.e596);
        //    changeFunc(RankType.TotalGift, eExdataDefine.e597);

        //    if (tempList1.Changes.Count > 0)
        //    {
        //        var msg3 = LogicServer.Instance.RankAgent.SSCharacterChangeDataList(characterId, tempList1);
        //        yield return msg3.SendAndWaitUntilDone(coroutine);                
        //    }

        //    // 主播获得鲜花
        //    var anchorChar = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg1.Response);
        //    if (anchorChar != null)
        //    {
        //        anchorChar.AddExData((int)eExdataDefine.e626, count);
        //    }


        //    // 广播
        //    var tbSkillUpgrading = Table.GetSkillUpgrading(73001);
        //    var dictId = tbSkillUpgrading.GetSkillUpgradingValue(count);
        //    if (dictId <= 0)
        //    {
        //        msg.Reply((int)ErrorCodes.Unknow);
        //        yield break;
        //    }

        //    var strs = new List<string>();
        //    strs.Add(character.GetName());
        //    strs.Add(count.ToString());
        //    var content = Utils.WrapDictionaryId(dictId, strs);

        //    var msg2 = LogicServer.Instance.ChatAgent.SSBroadcastAllServerMsg((uint)character.serverId,
        //        (int)eChatChannel.Anchor, "", new ChatMessageContent { Content = content });
        //    yield return msg2.SendAndWaitUntilDone(coroutine);
        //    if (msg2.State == MessageState.Reply && msg2.ErrorCode == (int) ErrorCodes.OK)
        //    {
        //        msg.Response = 1;
        //        msg.Reply();                
        //    }
        //    msg.Reply((int)ErrorCodes.Unknow);
        //    yield break;            
        //}

        //重置卓越属性
        public IEnumerator ResetExcellentEquip(Coroutine coroutine,
                                               LogicCharacterProxy charProxy,
                                               ResetExcellentEquipInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var bagType = msg.Request.BagType;
            var bagIndex = msg.Request.BagIndex;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ResetExcellentEquip----------:{0},{1}",
                bagType, bagIndex);
            var errorCodes = proxy.Character.ResetExcellentEquip(bagType, bagIndex, msg.Response.Items);
            msg.Reply((int)errorCodes);
            yield break;
        }

        //确定使用绿色属性
        public IEnumerator ConfirmResetExcellentEquip(Coroutine coroutine,
                                                      LogicCharacterProxy charProxy,
                                                      ConfirmResetExcellentEquipInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var bagType = msg.Request.BagType;
            var bagIndex = msg.Request.BagIndex;
            PlayerLog.WriteLog(proxy.CharacterId,
                "----------Logic----------ConfirmResetExcellentEquip----------:{0},{1}", bagType, bagIndex);
            var errorCodes = proxy.Character.ConfirmResetExcellentEquip(bagType, bagIndex, msg.Request.Ok);
            msg.Reply((int)errorCodes);
            yield break;
        }

        //超级卓越属性
        public IEnumerator SuperExcellentEquip(Coroutine coroutine,
                                               LogicCharacterProxy charProxy,
                                               SuperExcellentEquipInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var bagType = msg.Request.BagType;
            var bagIndex = msg.Request.BagIndex;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------SuperExcellentEquip----------:{0},{1}",
                bagType, bagIndex);
            foreach (var item in msg.Request.LockList.Items)
            {
                PlayerLog.WriteLog(proxy.CharacterId, "----------LockList.Items----------id----------:{0}", item);
            }
            var errorCodes = proxy.Character.SuperExcellentEquip(bagType, bagIndex, msg.Request.LockList.Items,
                msg.Response.AttrId, msg.Response.AttrValue);
            msg.Reply((int)errorCodes);
            yield break;
        }
        //确定使用星级属性
        public IEnumerator SaveSuperExcellentEquip(Coroutine coroutine, LogicCharacterProxy _this, SaveSuperExcellentEquipInMessage msg)
        {
            var proxy = (LogicProxy)_this;
            var bagType = msg.Request.BagType;
            var bagIndex = msg.Request.BagIndex;
            PlayerLog.WriteLog(proxy.CharacterId,
                "----------Logic----------SaveSuperExcellentEquip----------:{0},{1}", bagType, bagIndex);
            var errorCodes = proxy.Character.SaveSuperExcellentEquip(bagType, bagIndex, msg.Request.Ok);
            msg.Reply((int)errorCodes);
            yield break;
        }

        //传承装备属性
        public IEnumerator SmritiEquip(Coroutine coroutine, LogicCharacterProxy charProxy, SmritiEquipInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var smrititype = msg.Request.SmritiType;
            var moneytype = msg.Request.MoneyType;
            var fromBagType = msg.Request.FromBagType;
            var fromBagIndex = msg.Request.FromBagIndex;
            var toBagType = msg.Request.ToBagType;
            var toBagIndex = msg.Request.ToBagIndex;
            PlayerLog.WriteLog(proxy.CharacterId,
                "----------Logic----------SmritiEquip----------:{0},{1},{2},{3},{4},{5}", smrititype, moneytype,
                fromBagType, fromBagIndex, toBagType, toBagIndex);
            var appendCount = 0;
            var errorCodes = proxy.Character.SmritiEquip(smrititype, moneytype, fromBagType, fromBagIndex, toBagType,
                toBagIndex, ref appendCount);
            msg.Response = appendCount;
            msg.Reply((int)errorCodes);
            yield break;
        }

        //使用物品
        public IEnumerator UseItem(Coroutine coroutine, LogicCharacterProxy charProxy, UseItemInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            //Character.UseItem(coroutine, msg);
            var bagtype = msg.Request.BagType;
            var bagindex = msg.Request.BagIndex;
            var count = msg.Request.Count;

            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------SmritiEquip----------:{0},{1},{2}", bagtype,
                bagindex, count);
            var co = CoroutineFactory.NewSubroutine(proxy.Character.UseItem, coroutine, msg);
            if (co.MoveNext())
            {
                yield return co;
            }
        }

        //活动奖励
        public IEnumerator ActivationReward(Coroutine coroutine,
                                            LogicCharacterProxy charProxy,
                                            ActivationRewardInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var type = (eActivationRewardType)msg.Request.TypeId;
            var giftId = msg.Request.GiftId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ActivationReward----------:{0}", giftId);
            var errorCodes = proxy.Character.Gift(type, giftId);
            msg.Reply((int)errorCodes);
            yield break;
        }

        //合成物品
        public IEnumerator ComposeItem(Coroutine coroutine, LogicCharacterProxy charProxy, ComposeItemInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var composeId = msg.Request.ComposeId;
            var count = msg.Request.Count;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ComposeItem----------:{0},{1}", composeId,
                count);
            var RewardId = -1;
            var errorCodes = proxy.Character.ComposeItem(composeId, count, ref RewardId);
            msg.Response = RewardId;
            msg.Reply((int)errorCodes);
            yield break;
        }

        //成就领奖
        public IEnumerator RewardAchievement(Coroutine coroutine,
                                             LogicCharacterProxy charProxy,
                                             RewardAchievementInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var achievementId = msg.Request.AchievementId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------RewardAchievement----------:{0}",
                achievementId);
            var errorCodes = proxy.Character.RewardAchievement(achievementId);
            msg.Reply((int)errorCodes);
            yield break;
        }

        //分配属性点
        public IEnumerator DistributionAttrPoint(Coroutine coroutine,
                                                 LogicCharacterProxy charProxy,
                                                 DistributionAttrPointInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var strength = msg.Request.Strength;
            var agility = msg.Request.Agility;
            var intelligence = msg.Request.Intelligence;
            var Endurance = msg.Request.Endurance;
            PlayerLog.WriteLog(proxy.CharacterId,
                "----------Logic----------DistributionAttrPoint----------:{0},{1},{2},{3}", strength, agility,
                intelligence, Endurance);
            var errorCodes = proxy.Character.AddAttrPoint(strength, agility, intelligence, Endurance);
            msg.Reply((int)errorCodes);
            yield break;
        }

        //洗一级属性点
        public IEnumerator RefreshAttrPoint(Coroutine coroutine,
                                            LogicCharacterProxy charProxy,
                                            RefreshAttrPointInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------RefreshAttrPoint----------");
            var newPoint = 0;
            var errorCodes = proxy.Character.RefreshAttrPoint(ref newPoint);
            msg.Response = newPoint;
            msg.Reply((int)errorCodes);
            yield break;
        }

        //自动加点
        public IEnumerator SetAttributeAutoAdd(Coroutine coroutine,
                                               LogicCharacterProxy charProxy,
                                               SetAttributeAutoAddInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------SetAttributeAutoAdd----------");
            var ret = msg.Request.IsAuto == 1;
            proxy.Character.SetFlag(1001, ret);
            msg.Reply();
            yield break;
        }

        public IEnumerator ApplyFriendListData(Coroutine coroutine, LogicCharacterProxy _this, ApplyFriendListDataInMessage msg)
        {
            var proxy = (LogicProxy)_this;
            var infos = new MsgFriendList();
            var id = new List<ulong>();
            foreach (var i in proxy.Character.mFriend.mDataFriend)
            {
                id.Add(i.Key);
            }
            for (int i = 0; i < id.Count; i++)
            {
                var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(id[i]);
                if (character == null)
                {
                    continue;
                }
                var dbLogicSimple = character.GetSimpleData();
                var frienddata = new MsgFriendData();
                frienddata.FriendID = dbLogicSimple.Id;
                frienddata.IsOnLine = 1;
                frienddata.FriendLevel = dbLogicSimple.Level;
                frienddata.FriendRebronLV = dbLogicSimple.Ladder;
                frienddata.FriendStar = dbLogicSimple.StarNum;
                frienddata.FriendName = dbLogicSimple.Name;
                infos.Records.Add(frienddata);
            }
            msg.Response = infos;
            msg.Reply((int)ErrorCodes.OK);
            yield break;
        }

        //同步好友
        public IEnumerator ApplyFriends(Coroutine coroutine, LogicCharacterProxy charProxy, ApplyFriendsInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var type = msg.Request.Type;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ApplyFriends----------:{0}", type);
            var templist = new Uint64Array();
            Dictionary<ulong, FriendData> ListFriends;
            switch (type)
            {
                case 1: //好友
                    if (proxy.Character.mFriend.FriendNextUpdateTime > DateTime.Now)
                    {
                        msg.Reply((int)ErrorCodes.Unknow);
                        yield break;
                    }
                    ListFriends = proxy.Character.mFriend.mDataFriend;
                    foreach (var i in proxy.Character.mFriend.mDataFriend)
                    {
                        templist.Items.Add(i.Key);
                    }
                    proxy.Character.mFriend.FriendNextUpdateTime = DateTime.Now.AddSeconds(55);
                    break;
                case 2: //仇人
                    if (proxy.Character.mFriend.EnemyNextUpdateTime > DateTime.Now)
                    {
                        msg.Reply((int)ErrorCodes.Unknow);
                        yield break;
                    }
                    ListFriends = proxy.Character.mFriend.mDataEnemy;
                    foreach (var i in proxy.Character.mFriend.mDataEnemy)
                    {
                        templist.Items.Add(i.Key);
                    }
                    proxy.Character.mFriend.EnemyNextUpdateTime = DateTime.Now.AddSeconds(55);
                    break;
                default: //屏蔽
                    if (proxy.Character.mFriend.ShieldNextUpdateTime > DateTime.Now)
                    {
                        msg.Reply((int)ErrorCodes.Unknow);
                        yield break;
                    }
                    ListFriends = proxy.Character.mFriend.mDataShield;
                    foreach (var i in proxy.Character.mFriend.mDataShield)
                    {
                        templist.Items.Add(i.Key);
                    }
                    proxy.Character.mFriend.ShieldNextUpdateTime = DateTime.Now.AddSeconds(55);
                    break;
            }
            if (templist.Items.Count < 1)
            {
                msg.Reply();
                yield break;
            }
            var isOnlineList = LogicServer.Instance.SceneAgent.SBCheckCharacterOnline(proxy.CharacterId, templist);
            yield return isOnlineList.SendAndWaitUntilDone(coroutine);
            if (isOnlineList.State != MessageState.Reply)
            {
                msg.Reply();
                yield break;
            }
            var index = 0;
            var OnlineList = new List<ulong>();
            if (isOnlineList.Response != null)
            {
                foreach (var i in isOnlineList.Response.Items)
                {
                    if (i == 1)
                    {
                        OnlineList.Add(templist.Items[index]);
                    }
                    index++;
                }

                foreach (var i in templist.Items)
                {
                    var uId = i;
                    var isOnline = OnlineList.Contains(uId) ? 1 : 0;
                    var dbSceneSimple = LogicServer.Instance.SceneAgent.GetFriendSceneSimpleData(uId, msg.Request.Type,
                        msg.CharacterId, uId);
                    yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);
                    if (dbSceneSimple.State != MessageState.Reply)
                    {
                        continue;
                    }
                    if (dbSceneSimple.ErrorCode != (int)ErrorCodes.OK)
                    {
                        continue;
                    }
                    var result = dbSceneSimple.Response;
                    var temp = new CharacterSimpleData
                    {
                        Id = result.Id,
                        TypeId = result.TypeId,
                        Name = result.Name,
                        SceneId = result.SceneId,
                        FightPoint = result.FightPoint,
                        Level = result.Level,
                        Ladder = result.Ladder,
                        ServerId = result.ServerId,
                        Vip = result.Vip,
                        StarNum = result.StarNum
                    };
                    temp.Online = isOnline;
                    FriendData friend;
                    if (ListFriends.TryGetValue(uId, out friend))
                    {
                        friend.PushInfoData(temp);
                        msg.Response.Characters.Add(temp);
                    }
                }
            }

            msg.Reply();
        }

        //模糊搜索名字
        public IEnumerator SeekCharacters(Coroutine coroutine,
                                          LogicCharacterProxy charProxy,
                                          SeekCharactersInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            if (proxy.Character.mFriend.SeekCharacterNameNextUpdateTime > DateTime.Now)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            var likeName = msg.Request.Name;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------SeekCharacters----------{0}", likeName);
            if (likeName.Length < 1)
            {
                msg.Reply((int)ErrorCodes.Error_StringIsNone);
                yield break;
            }
            //通知客户端已经去查找了
            msg.Reply();

            var result = new CharacterSimpleDataList();
            //根据名字去取玩家
            ulong sameId = 0;
            var ret = LogicServer.Instance.DB.Get<DBUlong>(coroutine, DataCategory.LoginCharacterName, likeName);
            yield return ret;

            if (ret.Status == DataStatus.Ok)
            {
                if (ret.Data != null)
                {
                    var guid = ret.Data.Value;
                    var nameSimples = LogicServer.Instance.SceneAgent.GetSceneSimpleData(guid, 0);
                    yield return nameSimples.SendAndWaitUntilDone(coroutine);
                    if (nameSimples.State == MessageState.Reply)
                    {
                        if (nameSimples.ErrorCode == (int)ErrorCodes.OK)
                        {
                            sameId = guid;
                            var nameCharacter = new CharacterSimpleData
                            {
                                Id = guid,
                                TypeId = nameSimples.Response.TypeId,
                                Name = nameSimples.Response.Name,
                                SceneId = nameSimples.Response.SceneId,
                                FightPoint = nameSimples.Response.FightPoint,
                                Level = nameSimples.Response.Level,
                                Ladder = nameSimples.Response.Ladder,
                                ServerId = nameSimples.Response.ServerId,
                                StarNum = nameSimples.Response.StarNum
                            };
                            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(guid);
                            if (character == null)
                            {
                                nameCharacter.Online = 0;
                            }
                            else
                            {
                                nameCharacter.Online = character.Proxy == null ? 0 : 1;
                            }
                            result.Characters.Add(nameCharacter);
                        }
                    }
                }
            }

            //
            var SceneSimples = LogicServer.Instance.SceneAgent.FindCharacterName(likeName);
            yield return SceneSimples.SendAndWaitUntilDone(coroutine, TimeSpan.FromSeconds(5));
            proxy.Character.mFriend.SeekCharacterNameNextUpdateTime = DateTime.Now.AddSeconds(3);
            //             if (SceneSimples.Response == null)
            //             {
            //                 Logger.Fatal("SceneSimples.State={0}, SceneSimples.ErrorCode={1}", SceneSimples.State, SceneSimples.ErrorCode);
            //             }
            //             else
            //             {
            //                 Logger.Fatal("SceneSimples.State={0}, SceneSimples.ErrorCode={1},SceneSimples.Response={2}", SceneSimples.State, SceneSimples.ErrorCode, SceneSimples.Response.Count);
            //             }
            //if (SceneSimples.State == MessageState.Reply)
            {
                //if (SceneSimples.ErrorCode == (int)ErrorCodes.OK)
                if (SceneSimples.Response != null)
                {
                    foreach (var characterSimpleDatas in SceneSimples.Response)
                    {
                        foreach (var data in characterSimpleDatas.Datas)
                        {
                            var key = data.Key;
                            if (key != msg.CharacterId && key != sameId)
                            {
                                result.Characters.Add(data.Value);
                            }
                        }
                    }
                }
            }
            proxy.SeekCharactersReceive(result);
            //msg.Reply(SceneSimples.ErrorCode);
        }

        //一键征友
        public IEnumerator SeekFriends(Coroutine coroutine, LogicCharacterProxy charProxy, SeekFriendsInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            if (proxy.Character.mFriend.SeekFriendNextUpdateTime > DateTime.Now)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            var characterId = msg.CharacterId;
            var name = msg.Request.Name;
            //通知客户端已经去查找了
            msg.Reply();

            var result = new CharacterSimpleDataList();

            var level = proxy.Character.GetLevel();
            var maxCount = 20;

            var SceneSimples = LogicServer.Instance.SceneAgent.FindCharacterFriend(proxy.Character.serverId, level);
            yield return SceneSimples.SendAndWaitUntilDone(coroutine, TimeSpan.FromSeconds(5));
            proxy.Character.mFriend.SeekFriendNextUpdateTime = DateTime.Now.AddSeconds(3);
            //if (SceneSimples.State == MessageState.Reply)
            {
                //  if (SceneSimples.ErrorCode == (int)ErrorCodes.OK)
                if (SceneSimples.Response != null)
                {
                    var tempList = new List<CharacterSimpleData>();
                    var indexCount = 0;
                    foreach (var characterSimpleDatas in SceneSimples.Response)
                    {
                        foreach (var data in characterSimpleDatas.Datas)
                        {
                            if (Math.Abs(data.Value.Level - level) < 50)
                            {
                                var key = data.Key;
                                if (key != characterId)
                                {
                                    if (!proxy.Character.mFriend.mDbData.Friends.ContainsKey(key))
                                    {
                                        result.Characters.Add(data.Value);
                                    }
                                }
                                indexCount++;
                                if (indexCount >= maxCount)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                tempList.Add(data.Value);
                            }
                        }
                    }
                    if (indexCount < maxCount && tempList.Count > 0)
                    {
                        foreach (var data in tempList)
                        {
                            if (!proxy.Character.mFriend.mDbData.Friends.ContainsKey(data.Id))
                            {
                                result.Characters.Add(data);
                            }
                            indexCount++;
                            if (indexCount >= maxCount)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------SeekFriends----------:{0}", name);
            foreach (var data in result.Characters)
            {
                if (string.IsNullOrEmpty(data.Name))
                {
                    PlayerLog.WriteLog((int)LogType.PlayerNameError, string.Format("id={0}", data.Id));
                }
            }
            proxy.SeekFriendsReceive(result);
            //msg.Reply(SceneSimples.ErrorCode);
        }

        //添加好友
        public IEnumerator AddFriendById(Coroutine coroutine, LogicCharacterProxy charProxy, AddFriendByIdInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var uid = msg.Request.CharacterId;
            var type = msg.Request.Type;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------AddFriendById----------:{0},{1}", uid, type);
            var errorCodes = ErrorCodes.OK; // Character.mFriend.CheckAddFriend(uid);
            switch (type)
            {
                case 1: //好友
                    //是否已经是自己的好友
                    errorCodes = proxy.Character.mFriend.CheckAddFriend(uid);
                    if (errorCodes != ErrorCodes.OK)
                    {
                        //Character.mFriend.DelFriend(uid);
                        msg.Reply((int)errorCodes);
                        yield break;
                    }
                    break;
                case 2: //仇人
                    //是否已经是仇人
                    errorCodes = proxy.Character.mFriend.CheckAddEnemy(uid);
                    if (errorCodes != ErrorCodes.OK)
                    {
                        msg.Reply((int)errorCodes);
                        yield break;
                    }
                    break;
                case 3: //屏蔽
                    //是否已经屏蔽
                    errorCodes = proxy.Character.mFriend.CheckAddShield(uid);
                    if (errorCodes != ErrorCodes.OK)
                    {
                        msg.Reply((int)errorCodes);
                        yield break;
                    }
                    break;
            }
            //是否有这个玩家
            var LoginData = LogicServer.Instance.LoginAgent.CheckIsHaveCharacter(proxy.ClientId, uid);
            yield return LoginData.SendAndWaitUntilDone(coroutine);
            if (LoginData.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(LoginData.ErrorCode);
                yield break;
            }

            switch (type)
            {
                case 1: //好友
                    errorCodes = proxy.Character.mFriend.AddFriend(uid);
                    break;
                case 2: //仇人
                    errorCodes = proxy.Character.mFriend.AddEnemy(uid, 1);
                    break;
                case 3: //屏蔽
                    errorCodes = proxy.Character.mFriend.AddShield(uid);
                    break;
            }

            if (errorCodes == ErrorCodes.OK)
            {
                //var dbSceneSimple = LogicServer.Instance.SceneAgent.GetSceneSimpleData(msg.CharacterId, uid);
                var dbSceneSimple = LogicServer.Instance.SceneAgent.GetFriendSceneSimpleData(uid, type,
                    proxy.CharacterId, uid);
                yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);
                var temp = new CharacterSimpleData
                {
                    Id = dbSceneSimple.Response.Id,
                    TypeId = dbSceneSimple.Response.TypeId,
                    Name = dbSceneSimple.Response.Name,
                    SceneId = dbSceneSimple.Response.SceneId,
                    FightPoint = dbSceneSimple.Response.FightPoint,
                    Level = dbSceneSimple.Response.Level,
                    Ladder = dbSceneSimple.Response.Ladder,
                    ServerId = dbSceneSimple.Response.ServerId,
                    StarNum = dbSceneSimple.Response.StarNum
                };
                var checklist = new Uint64Array();
                checklist.Items.Add(uid);
                var isOnlineList = LogicServer.Instance.SceneAgent.SBCheckCharacterOnline(proxy.CharacterId, checklist);
                yield return isOnlineList.SendAndWaitUntilDone(coroutine);
                temp.Online = isOnlineList.Response.Items[0];
                msg.Response = temp;
            }

            msg.Reply((int)errorCodes);
        }

        //添加好友
        public IEnumerator AddFriendByName(Coroutine coroutine,
                                           LogicCharacterProxy charProxy,
                                           AddFriendByNameInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var type = msg.Request.Type;
            var name = msg.Request.Name;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------AddFriendByName----------:{0},{1}", type,
                name);
            switch (type)
            {
                case 1: //好友
                    break;
                case 2: //仇人
                    break;
                case 3: //屏蔽
                    break;
            }
            //是否有这个玩家
            var LoginData = LogicServer.Instance.LoginAgent.GetCharacterIdByName(proxy.ClientId, name);
            yield return LoginData.SendAndWaitUntilDone(coroutine);
            if (LoginData.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(LoginData.ErrorCode);
                yield break;
            }
            //有这个玩家
            var errorCodes = proxy.Character.mFriend.AddFriend(LoginData.Response);
            msg.Reply((int)errorCodes);
        }

        //删除好友
        public IEnumerator DelFriendById(Coroutine coroutine, LogicCharacterProxy charProxy, DelFriendByIdInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var errorCodes = ErrorCodes.OK;
            var TargetId = msg.Request.CharacterId;
            var type = msg.Request.Type;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------DelFriendById----------:{0},{1}", TargetId,
                type);
            switch (type)
            {
                case 1: //好友
                    errorCodes = proxy.Character.mFriend.DelFriend(TargetId);
                    break;
                case 2: //仇人
                    errorCodes = proxy.Character.mFriend.DelEnemy(TargetId);
                    break;
                case 3: //屏蔽
                    errorCodes = proxy.Character.mFriend.DelShield(TargetId);
                    break;
            }
            msg.Reply((int)errorCodes);
            if (errorCodes == ErrorCodes.OK)
            {
                var SceneMsg = LogicServer.Instance.SceneAgent.SendDeleteFriend(TargetId, type, proxy.CharacterId,
                    TargetId);
                yield return SceneMsg.SendAndWaitUntilDone(coroutine);
            }
        }

        private void EnterFuben()
        {

        }

        //进入副本
        public IEnumerator EnterFuben(Coroutine co, LogicCharacterProxy charProxy, EnterFubenInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------EnterFuben------{0}----",
                msg.Request.FubenId);
            if (proxy.Character.mFuncBlock.GetFlag((int)FunctionBlock.EnterFuben) == 1)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            var errCode = AsyncReturnValue<ErrorCodes>.Create();
            var checkResult = AsyncReturnValue<int>.Create();
            proxy.Character.mFuncBlock.SetFlag((int)FunctionBlock.EnterFuben);
            var co1 = CoroutineFactory.NewSubroutine(proxy.Character.EnterFuben, co, msg.Request.FubenId, errCode,
                checkResult);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            proxy.Character.mFuncBlock.CleanFlag((int)FunctionBlock.EnterFuben);
            msg.Response = checkResult.Value;
            checkResult.Dispose();
            var result = errCode.Value;
            errCode.Dispose();
            msg.Reply((int)result, true);
        }

        //重置副本
        public IEnumerator ResetFuben(Coroutine coroutine, LogicCharacterProxy charProxy, ResetFubenInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var funbengid = msg.Request.FubenId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ResetFuben----------{0}", funbengid);
            var errorCodes = proxy.Character.BuyFubenCount(funbengid);
            msg.Reply((int)errorCodes);
            yield break;
        }

        //扫荡副本
        public IEnumerator SweepFuben(Coroutine co, LogicCharacterProxy charProxy, SweepFubenInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;

            var fubenId = msg.Request.FubenId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------SweepFuben----------:{0}", fubenId);

            #region 检查，如果在当前副本内，则不能再次进入

            var sceneSimpleMsg = LogicServer.Instance.SceneAgent.GetSceneSimpleData(proxy.CharacterId, 0);
            yield return sceneSimpleMsg.SendAndWaitUntilDone(co);
            if (sceneSimpleMsg.State != MessageState.Reply)
            {
                Logger.Error("In SweepFuben(), GetSceneSimpleData return with dbSceneSimple.State = {0}",
                    sceneSimpleMsg.State);
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (sceneSimpleMsg.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Error("In SweepFuben(), GetSceneSimpleData return with dbSceneSimple.ErrorCode = {0}",
                    sceneSimpleMsg.ErrorCode);
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            var tbScene = Table.GetScene(sceneSimpleMsg.Response.SceneId);
            if (tbScene != null && tbScene.FubenId == fubenId)
            {
                msg.Reply((int)ErrorCodes.Error_AlreadyInThisDungeon);
                yield break;
            }

            #endregion

            var errorCodes = proxy.Character.PassFuben(fubenId, msg.Response);
            msg.Reply((int)errorCodes);
        }

        public IEnumerator CSEnterEraById(Coroutine coroutine, LogicCharacterProxy charProxy, CSEnterEraByIdInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var character = proxy.Character;
            var eraId = msg.Request.EraId;
            if (eraId < 0)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            var tbMayaBase = Table.GetMayaBase(eraId);
            if (tbMayaBase == null)
            {
                msg.Reply((int)ErrorCodes.Error_TableData);
                yield break;
            }

            if (tbMayaBase.ActiveType == 2)
            { // 成就界面
                foreach (var achvId in tbMayaBase.ActiveParam)
                {
                    var tbAchv = Table.GetAchievement(achvId);
                    if (tbAchv == null)
                    {
                        continue;
                    }

                    if (!character.GetFlag(tbAchv.FinishFlagId))
                    {
                        msg.Reply((int)ErrorCodes.Unknow);
                        yield break;
                    }
                }

                character.SetFlag(tbMayaBase.FinishFlagId, true, 1);
                character.SetExData((int)eExdataDefine.e711, eraId);

                msg.Reply((int)ErrorCodes.OK);
                yield break;
            }

            // 副本
            if (tbMayaBase.FunBenId < 0)
            {
                msg.Reply((int)ErrorCodes.Error_FubenID);
                yield break;
            }
            var fubenId = tbMayaBase.FunBenId;

            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----CSEnterEraById------EnterFuben------{0}----",
                fubenId);

            if (proxy.Character.mFuncBlock.GetFlag((int)FunctionBlock.EnterFuben) == 1)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            if (tbMayaBase.ActiveType == 0)
            { // 不满足任务条件
                var missionId = tbMayaBase.ActiveParam[0];
                var missionData = proxy.Character.mTask.GetMission(missionId);
                if (missionData == null || missionData.Data[0] != (int)eMissionState.Unfinished)
                {
                    msg.Reply((int)ErrorCodes.Error_ConditionNoEnough);
                    yield break;
                }
            }

            var errCode = AsyncReturnValue<ErrorCodes>.Create();
            var checkResult = AsyncReturnValue<int>.Create();
            proxy.Character.mFuncBlock.SetFlag((int)FunctionBlock.EnterFuben);
            var co1 = CoroutineFactory.NewSubroutine(proxy.Character.EnterFuben, coroutine, fubenId, errCode,
                checkResult);
            if (co1.MoveNext())
            {
                yield return co1;
            }
            proxy.Character.mFuncBlock.CleanFlag((int)FunctionBlock.EnterFuben);
            msg.Response = checkResult.Value;
            checkResult.Dispose();
            var result = errCode.Value;
            errCode.Dispose();
            msg.Reply((int)result, true);
            yield break;
        }

        public IEnumerator EraPlayedSkill(Coroutine coroutine, LogicCharacterProxy charProxy, EraPlayedSkillInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----EraPlayedSkill-------");

            proxy.Character.SetExData((int)eExdataDefine.e711, -1, true);
            msg.Response = 0;
            msg.Reply((int)ErrorCodes.OK, true);
            yield break;
        }

        public IEnumerator EraTakeAchvAward(Coroutine coroutine, LogicCharacterProxy charProxy, EraTakeAchvAwardInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----EraTakeAchvAward-------");

            var errorCodes = proxy.Character.RewardAchievement(msg.Request.AchvId);
            msg.Response = 0;
            msg.Reply((int)errorCodes);
            yield break;
        }

        public IEnumerator EraTakeAward(Coroutine coroutine, LogicCharacterProxy charProxy, EraTakeAwardInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----EraTakeAchvAward-------");
            var character = proxy.Character;
            if (character == null)
            {
                msg.Reply((int)ErrorCodes.Error_CharacterNotFind);
                yield break;
            }

            var eraId = msg.Request.EraId;

            var tbMayaBase = Table.GetMayaBase(eraId);
            if (tbMayaBase == null)
            {
                msg.Reply((int)ErrorCodes.Error_TableData);
                yield break;
            }

            if (character.GetFlag(tbMayaBase.GotAward))
            { // 领过了
                msg.Reply((int)ErrorCodes.Error_GiftAlreadyReceive);
                yield break;
            }

            if (!character.GetFlag(tbMayaBase.FinishFlagId))
            { // 未完成
                msg.Reply((int)ErrorCodes.Error_AchievementNotFinished);
                yield break;
            }


            try
            {
                var role = character.GetRole();
                var rewardId = tbMayaBase.Award[role];
                if (rewardId >= 0)
                {
                    var tbConumeArray = Table.GetConsumArray(rewardId);
                    if (tbConumeArray != null)
                    {
                        var itemDict = new Dictionary<int, int>();
                        for (var i = 0; i < tbConumeArray.ItemId.Count(); ++i)
                        {
                            var itemid = tbConumeArray.ItemId[i];
                            var count = tbConumeArray.ItemCount[i];
                            if (itemid < 0)
                                continue;
                            itemDict[itemid] = count;
                        }

                        if (itemDict.Count > 0)
                        {
                            if (ErrorCodes.OK != BagManager.CheckAddItemList(character.mBag, itemDict))
                            {
                                msg.Reply((int)ErrorCodes.Error_ItemNoInBag_All);
                                yield break;
                            }
                            character.mBag.AddItems(itemDict, eCreateItemType.EraAward);
                        }
                    }
                }

                character.SetFlag(tbMayaBase.GotAward);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            msg.Response = 0;
            msg.Reply((int)ErrorCodes.OK);
        }

        // 领取战场奖励 
        public IEnumerator AcceptBattleAward(Coroutine coroutine,
                                             LogicCharacterProxy charProxy,
                                             AcceptBattleAwardInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var fubenId = msg.Request.FubenId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------AcceptBattleAward----------{0}", fubenId);
            var tbFuben = Table.GetFuben(fubenId);
            if (tbFuben == null)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            var isFistWin = proxy.Character.GetFlag(tbFuben.ScriptId);

            if (isFistWin == false)
            {
                msg.Reply((int)ErrorCodes.Error_BattleNoWin);
                yield break;
            }

            var hasAccpet = proxy.Character.GetFlag(tbFuben.FlagId);

            if (hasAccpet)
            {
                msg.Reply((int)ErrorCodes.Error_BattleHasAccept);
                yield break;
            }


            var itemList = new Dictionary<int, int>();
            for (var i = 0; i < tbFuben.RewardId.Length; i++)
            {
                if (tbFuben.RewardId[i] == -1 || tbFuben.RewardCount[i] < 1)
                {
                    continue;
                }
                itemList.modifyValue(tbFuben.RewardId[i], tbFuben.RewardCount[i]);
            }

            proxy.Character.SetFlag(tbFuben.FlagId, true);

            var ret = proxy.Character.mBag.AddItemOrMail(50, itemList, null, eCreateItemType.Battle);

            msg.Reply((int)ret);
        }

        public IEnumerator GetFubenStoreItems(Coroutine coroutine, LogicCharacterProxy charProxy, List<StoneItem> itemList, int shopType)
        {
            var proxy = (LogicProxy)charProxy;
            var serverId = proxy.Character.serverId;
            var msg = LogicServer.Instance.ActivityAgent.SSGetBlackStoreItems(charProxy.CharacterId, serverId);
            yield return msg.SendAndWaitUntilDone(coroutine);

            if (msg.ErrorCode == (int)ErrorCodes.OK)
                itemList.AddRange(msg.Response.items);
        }

        public IEnumerator GetTreasureShopItems(Coroutine coroutine, LogicCharacterProxy charProxy, List<StoneItem> itemList)
        {
            var proxy = (LogicProxy)charProxy;
            var serverId = proxy.Character.serverId;
            var msg = LogicServer.Instance.ActivityAgent.SSGetTreasureShopItems(charProxy.CharacterId, serverId);
            yield return msg.SendAndWaitUntilDone(coroutine);

            if (msg.ErrorCode == (int)ErrorCodes.OK)
                itemList.AddRange(msg.Response.items);
        }

        //同步商店
        public IEnumerator ApplyStores(Coroutine coroutine, LogicCharacterProxy charProxy, ApplyStoresInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var type = msg.Request.Type;
            var serviceType = msg.Request.ServiceType;

            if (serviceType == (int)NpcService.NsBlackMarket)
            {
                var co = CoroutineFactory.NewSubroutine(GetFubenStoreItems, coroutine, charProxy, msg.Response.items, type);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }
            else if (serviceType == (int)NpcService.NsTreasureShop)
            {
                var co = CoroutineFactory.NewSubroutine(GetTreasureShopItems, coroutine, charProxy, msg.Response.items);
                if (co.MoveNext())
                {
                    yield return co;
                }
            }
            else
            {
                proxy.Character.mStone.GetItemList(type, msg.Response.items);
            }

            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ApplyStores----------:{0}", type);

            msg.Reply();
            yield break;
        }

        //激活图鉴
        public IEnumerator ActivateBook(Coroutine coroutine, LogicCharacterProxy charProxy, ActivateBookInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var itemId = msg.Request.ItemId;
            var errorCodes = proxy.Character.mBook.ActivateBook(itemId);
            msg.Reply((int)errorCodes);
            yield break;
        }

        //包裹排序
        public IEnumerator SortBag(Coroutine coroutine, LogicCharacterProxy charProxy, SortBagInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------SortBag----------{0}", msg.Request.BagId);
            var bag = proxy.Character.mBag.SortBag(msg.Request.BagId);
            if (bag == null)
            {
                msg.Reply((int)ErrorCodes.Error_BagID);
                yield break;
            }
            msg.Response.BagId = bag.GetBagId();
            msg.Response.NowCount = bag.GetNowCount();
            msg.Response.NextSecond = bag.GetNextTime() - (int)DateTime.Now.GetDiffSeconds(proxy.Character.OnlineTime) +
                                      bag.RemoveBuyTimes;
            var i = 0;
            foreach (var item in bag.mLogics)
            {
                if (item.GetId() == -1)
                {
                    i++;
                    continue;
                }
                if (i != item.GetIndex())
                {
                    Logger.Warn("SortBag oldindex={0},oldindex={1}", item.GetIndex(), i);
                    item.SetIndex(i);
                }
                var itemBase = new ItemBaseData
                {
                    ItemId = item.GetId(),
                    Count = item.GetCount(),
                    Index = item.GetIndex()
                };
                itemBase.Exdata.Clear();
                item.CopyTo(itemBase.Exdata);
                msg.Response.Items.Add(itemBase);
                i++;
            }
            msg.Reply();
        }

        //获取玩家基本信息
        public IEnumerator ApplyPlayerHeadInfo(Coroutine coroutine,
                                               LogicCharacterProxy _this,
                                               ApplyPlayerHeadInfoInMessage msg)
        {
            var proxy = (LogicProxy)_this;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ApplyPlayerHeadInfo----------{0}",
                msg.Request.CharacterId);
            var targetId = msg.Request.CharacterId;
            var dbLogicSimple = LogicServer.Instance.LogicAgent.GetLogicSimpleData(targetId, 0);
            yield return dbLogicSimple.SendAndWaitUntilDone(coroutine);
            if (dbLogicSimple.State != MessageState.Reply)
            {
                Logger.Error("ApplyPlayerInfo LogicSimple State is {0}", dbLogicSimple.State);
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (dbLogicSimple.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Error("ApplyPlayerInfo LogicSimple ErrorCode is {0}", dbLogicSimple.ErrorCode);
                msg.Reply(dbLogicSimple.ErrorCode);
                yield break;
            }

            var infoMsg = new PlayerHeadInfoMsg
            {
                CharacterId = targetId,
                Level = dbLogicSimple.Response.Level,
                RoleId = dbLogicSimple.Response.TypeId,
                Ladder = dbLogicSimple.Response.Ladder,
                Name = dbLogicSimple.Response.Name
            };
            msg.Response = infoMsg;
            msg.Reply();

            //var proxy = (LogicProxy)_this;
            //var characterId = msg.Request.CharacterId;
            //var character = CharacterManager.Instance.GetCharacterControllerFromMemroy((ulong)characterId);
            //if (character == null)
            //{
            //    Logger.Warn("FriendList Not Player OnLine----------LogicProxy------------ApplyFriendListMsg()");
            //    msg.Reply((int)ErrorCodes.Unline);
            //    yield break;
            //}
            //var data = character.GetSimpleData();
            //var infoMsg = new PlayerHeadInfoMsg
            //{
            //    //IsOnLine = true,
            //    Level = data.Level,
            //    Ladder = data.Ladder,
            //    Name = data.Name,
            //    RoleId = data.TypeId
            //};
            //msg.Response = infoMsg;
            //msg.Reply((int)ErrorCodes.OK);
        }

        //获取补偿列表
        public IEnumerator GetCompensationList(Coroutine coroutine,
                                               LogicCharacterProxy _this,
                                               GetCompensationListInMessage msg)
        {
            var proxy = (LogicProxy)_this;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------GetCompensationList----------");
            if (proxy.Character.mDbData.Compensations == null)
            {
                proxy.Character.mDbData.Compensations = new CompensationList();
            }
            foreach (var compensation in proxy.Character.mDbData.Compensations.Compensations)
            {
                msg.Response.Compensations.Add(compensation.Key, compensation.Value);
            }
            msg.Reply();
            yield break;
        }

        //领取补偿 IndexType =-1 代表一键全部  type ： 0 = 金币 ，1 = 钻石
        public IEnumerator ReceiveCompensation(Coroutine coroutine,
                                               LogicCharacterProxy _this,
                                               ReceiveCompensationInMessage msg)
        {
            var proxy = (LogicProxy)_this;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ReceiveCompensation----------{0},{1}",
                msg.Request.IndexType, msg.Request.Type);
            var comList = proxy.Character.mDbData.Compensations;
            if (comList == null)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (msg.Request.IndexType != -1)
            {
                var id = msg.Request.IndexType;
                var result = proxy.Character.ReceiveCompensation(id, msg.Request.Type);
                msg.Reply((int)result);
            }
            else
            {
                var result = proxy.Character.ReceiveAllCompensation(msg.Request.Type);
                msg.Reply((int)result);
            }
        }

        public IEnumerator SelectTitle(Coroutine co, LogicCharacterProxy _this, SelectTitleInMessage msg)
        {
            //目前只能修改第三条title
            var proxy = (LogicProxy)_this;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ReceiveCompensation----------{0}",
                msg.Request.Id);
            var character = proxy.Character;
            var err = character.SetTitle(msg.Request.Id);
            msg.Reply((int)err);
            yield break;
        }

        public IEnumerator RetrainPet(Coroutine co, LogicCharacterProxy _this, RetrainPetInMessage msg)
        {
            var proxy = (LogicProxy)_this;
            var petid = msg.Request.PetId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------RetrainPet----------{0}", petid);
            var character = proxy.Character;
            var result = character.CheckCondition(StaticParam.PetRetrainConditionId);
            if (result != -2)
            {
                msg.Response = result;
                msg.Reply((int)ErrorCodes.Error_Condition, true);
                yield break;
            }
            var pet = character.GetPet(petid);
            if (pet == null)
            {
                msg.Reply((int)ErrorCodes.Error_PetNotFind);
                yield break;
            }
            var itemId = StaticParam.PetRetrainItemId;
            var itemCount = StaticParam.PetRetrainItemCount;
            var nowCount = character.mBag.GetItemCount(itemId);
            if (nowCount < itemCount)
            {
                msg.Reply((int)ErrorCodes.ItemNotEnough);
                yield break;
            }
            character.mBag.DeleteItem(itemId, itemCount, eDeleteItemType.PetExp);

            var tbPet = Table.GetPet(pet.GetId());
            PetItem.RefreshSpeciality(pet, tbPet);
            pet.MarkDirty();

            msg.Reply();
        }

        //提升战盟Buff
        public IEnumerator UpgradeAllianceBuff(Coroutine coroutine,
                                               LogicCharacterProxy _this,
                                               UpgradeAllianceBuffInMessage msg)
        {
            var proxy = (LogicProxy)_this;
            var buffId = msg.Request.BuffId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------AddAllianceBuff----------{0}", buffId);
            var result = proxy.Character.CheckAllianceBuff(buffId);
            if (result == ErrorCodes.Error_CheckAllianceLevel)
            {
                //同步战盟数据
                var alliance = LogicServer.Instance.TeamAgent.SSGetAllianceData(proxy.CharacterId,
                    proxy.Character.serverId);
                yield return alliance.SendAndWaitUntilDone(coroutine);
                if (alliance.State == MessageState.Reply)
                {
                    if (alliance.ErrorCode == (int)ErrorCodes.OK)
                    {
                        if (alliance.Response.Level < Table.GetGuildBuff(buffId).NeedUnionLevel)
                        {
                            msg.Reply((int)ErrorCodes.Error_AllianceLeveNotEnough);
                            yield break;
                        }
                    }
                }
            }
            else if (result != ErrorCodes.OK)
            {
                msg.Reply((int)result);
                yield break;
            }
            msg.Reply((int)proxy.Character.UpgradeAllianceBuff(buffId));
        }

        //投资
        public IEnumerator Investment(Coroutine co, LogicCharacterProxy _this, InvestmentInMessage msg)
        {
            var id = msg.Request.Id;
            var tbRAC = Table.GetRechargeActiveCumulative(id);
            if (tbRAC == null)
            {
                msg.Reply((int)ErrorCodes.ParamError);
                yield break;
            }
            var proxy = (LogicProxy)_this;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------Investment----------:{0}", id);
            var character = proxy.Character;
            var tbRA = Table.GetRechargeActive(tbRAC.ActivityId);
            if (tbRA == null)
            {
                Logger.Error("RechargeActive Table value is not correct! id = {0}", tbRAC.ActivityId);
                msg.Reply((int)ErrorCodes.Error_TableData);
                yield break;
            }
            var err = CheckServerIds(proxy, character.serverId, tbRA.ServerIds, tbRA);
            if (err != ErrorCodes.OK)
            {
                msg.Reply((int)err);
                yield break;
            }
            var result = character.CheckCondition(tbRAC.ConditionId);
            if (result != -2)
            {
                msg.Response = result;
                msg.Reply((int)ErrorCodes.Error_Condition, true);
                yield break;
            }
            character.mBag.DeleteItem(tbRAC.NeedItemId, tbRAC.NeedItemCount, eDeleteItemType.Investment);
            character.SetFlag(tbRAC.FlagTrueId);
            foreach (var flagId in tbRAC.FlagFalseId)
            {
                character.SetFlag(flagId, false);
            }
            if (tbRAC.ResetCount != -1)
            {
                var tbExdata = Table.GetExdata(tbRAC.ExtraId);
                if (tbExdata != null)
                {
                    character.SetExData(tbRAC.ExtraId,
                        MyRandom.Random(tbExdata.RefreshValue[0], tbExdata.RefreshValue[1]));
                }
            }
            msg.Reply();
        }

        //领取累计充值，累计投资奖励
        public IEnumerator GainReward(Coroutine co, LogicCharacterProxy _this, GainRewardInMessage msg)
        {
            var type = (eReChargeRewardType)msg.Request.Type;
            var id = msg.Request.Id;
            var proxy = (LogicProxy)_this;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------GainReward----------:{0}", type, id);
            var character = proxy.Character;

            switch (type)
            {
                case eReChargeRewardType.Recharge:
                    {
                        var tbRAIR = Table.GetRechargeActiveInvestmentReward(id);
                        if (tbRAIR == null)
                        {
                            msg.Reply((int)ErrorCodes.ParamError);
                            yield break;
                        }
                        var tbRAI = Table.GetRechargeActiveInvestment(tbRAIR.Type);
                        var tbRA = Table.GetRechargeActive(tbRAI.ActivityId);
                        var err = CheckServerIds(proxy, character.serverId, tbRA.ServerIds, tbRA);
                        if (err != ErrorCodes.OK)
                        {
                            msg.Reply((int)err);
                            yield break;
                        }
                        var result = character.CheckCondition(tbRAIR.ConditionId);
                        if (result != -2)
                        {
                            msg.Response = result;
                            msg.Reply((int)ErrorCodes.Error_Condition, true);
                            yield break;
                        }
                        character.SetFlag(tbRAIR.Flag);
                        var items = new Dictionary<int, int>();
                        for (int i = 0, imax = tbRAIR.ItemId.Length; i < imax; i++)
                        {
                            var itemId = tbRAIR.ItemId[i];
                            if (itemId < 0)
                            {
                                break;
                            }
                            items.modifyValue(itemId, tbRAIR.ItemCount[i]);
                        }
                        var result1 = character.mBag.AddItems(items, eCreateItemType.CumulativeRecharge);
                        if (result1 != ErrorCodes.OK)
                        {
                            msg.Reply((int)result1);
                            yield break;
                        }
                    }
                    break;
                case eReChargeRewardType.Investment:
                    {
                        var tbRACR = Table.GetRechargeActiveCumulativeReward(id);
                        if (tbRACR == null)
                        {
                            msg.Reply((int)ErrorCodes.ParamError);
                            yield break;
                        }
                        var tbRAC = Table.GetRechargeActiveCumulative(tbRACR.Type);
                        var tbRA = Table.GetRechargeActive(tbRAC.ActivityId);
                        var err = CheckServerIds(proxy, character.serverId, tbRA.ServerIds, tbRA);
                        if (err != ErrorCodes.OK)
                        {
                            msg.Reply((int)err);
                            yield break;
                        }
                        var result = character.CheckCondition(tbRACR.ConditionId);
                        if (result != -2)
                        {
                            msg.Response = result;
                            msg.Reply((int)ErrorCodes.Error_Condition, true);
                            yield break;
                        }
                        character.SetFlag(tbRACR.Flag);
                        var result1 = character.mBag.AddItem(tbRACR.ItemId, tbRACR.ItemCount, eCreateItemType.Investment);
                        if (result1 != ErrorCodes.OK)
                        {
                            msg.Reply((int)result1);
                            yield break;
                        }
                    }
                    break;
                default:
                    msg.Reply((int)ErrorCodes.ParamError);
                    yield break;
            }
            msg.Reply();
        }

        public IEnumerator Worship(Coroutine co, LogicCharacterProxy _this, WorshipInMessage msg)
        {
            var proxy = (LogicProxy)_this;
            var type = msg.Request.Type;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------Worship----------:{0}", type);
            var character = proxy.Character;
            var occupantId = StaticParam.AllianceWarInfo[character.serverId].OccupantId;
            if (occupantId == 0)
            {
                msg.Reply((int)ErrorCodes.Error_NoOccupant);
                yield break;
            }
            if (character.GetExData((int)eExdataDefine.e72) >= StaticParam.WorshipCountMax)
            {
                msg.Reply((int)ErrorCodes.Error_WorshipCount);
                yield break;
            }
            switch (type)
            {
                case 0: //金币
                    {
                        var err = character.mBag.DeleteItem((int)eResourcesType.GoldRes, StaticParam.WorshipCoinCost,
                            eDeleteItemType.WorshipLord);
                        if (err != ErrorCodes.OK)
                        {
                            msg.Reply((int)err);
                            yield break;
                        }
                        var exp =
                            Table.GetSkillUpgrading(StaticParam.WorshipCoinExpId)
                                .GetSkillUpgradingValue(character.GetLevel());
                        character.mBag.AddItem((int)eResourcesType.ExpRes, exp, eCreateItemType.WorshipLord);
                        character.mBag.AddItem((int)eResourcesType.Honor, StaticParam.WorshipCoinHonor,
                            eCreateItemType.WorshipLord);
                    }
                    break;
                case 1: //钻石
                    {
                        var err = character.mBag.DeleteItem((int)eResourcesType.DiamondRes, StaticParam.WorshipDiamondCost,
                            eDeleteItemType.WorshipLord);
                        if (err != ErrorCodes.OK)
                        {
                            msg.Reply((int)err);
                            yield break;
                        }
                        var exp =
                            Table.GetSkillUpgrading(StaticParam.WorshipDiamondExpId)
                                .GetSkillUpgradingValue(character.GetLevel());
                        character.mBag.AddItem((int)eResourcesType.ExpRes, exp, eCreateItemType.WorshipLord);
                        character.mBag.AddItem((int)eResourcesType.Honor, StaticParam.WorshipDiamondHonor,
                            eCreateItemType.WorshipLord);
                    }
                    break;
                default:
                    msg.Reply((int)ErrorCodes.ParamError);
                    yield break;
            }
            character.AddExData((int)eExdataDefine.e72, 1);
            msg.Reply();
        }

        public IEnumerator UseGiftCode(Coroutine coroutine, LogicCharacterProxy _this, UseGiftCodeInMessage msg)
        {
            var code = msg.Request.Code.ToUpper();
            PlayerLog.WriteLog(msg.CharacterId, "----------Logic----------UseGiftCode----------:{0}", code);
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (character == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }
            if (code.Length <= 8)
            {
                msg.Reply((int)ErrorCodes.ParamError);
                yield break;
            }
            var prefix = code.Substring(0, code.Length - 8);
            var id = (int)IdGenerator.RevertConfuse(prefix);
            var tbGC = Table.GetGiftCode(id);
            if (tbGC == null)
            {
                msg.Reply((int)ErrorCodes.ParamError);
                yield break;
            }
            if (character.GetFlag(tbGC.FlagId))
            {
                //已经领过这个礼品码了
                msg.Reply((int)ErrorCodes.Error_CantUseGiftCode);
                yield break;
            }
            var endTime = DateTime.Parse(tbGC.EndTime);
            if (DateTime.Now > endTime)
            {
                msg.Reply((int)ErrorCodes.Error_GiftCodeExpire);
                yield break;
            }

            var reslut = LogicServer.Instance.GameMasterAgent.UseGiftCode(msg.CharacterId, code, int.Parse(string.IsNullOrEmpty(character.moniterData.pid) ? "0" : character.moniterData.pid));
            yield return reslut.SendAndWaitUntilDone(coroutine);
            if (reslut.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Error_TimeOut);
                Logger.Error("GameMasterAgent.UseGiftCode() Error_TimeOut");
                yield break;
            }
            if (reslut.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Error("GameMasterAgent.UseGiftCode() ErrorCode = {0}", reslut.ErrorCode);
                msg.Reply(reslut.ErrorCode);
                yield break;
            }

            try
            {
                Dictionary<int, int> items;
                if (character.GetRole() == 0) // 剑士
                {
                    items = LogicServer.Instance.ServerControl.GiftCodeItems[id][0];
                }
                else if (character.GetRole() == 1) // 法师
                {
                    items = LogicServer.Instance.ServerControl.GiftCodeItems[id][1];
                }
                else // 弓手
                {
                    items = LogicServer.Instance.ServerControl.GiftCodeItems[id][2];
                }

                if (items == null)
                    items = new Dictionary<int, int>();

                character.DropMother(tbGC.DropId, items);

                PlayerLog.WriteLog(msg.CharacterId, "UseGiftCode success! code={0}, items={1}", code,
                    items.GetDataString());
                if (items.Count > 0)
                {
                    character.mBag.AddItemOrMail(56, items, null, eCreateItemType.GiftCode);
                }
            }
            catch (Exception e)
            {

                Logger.Error(e, "GameMasterAgent.UseGiftCode()");

            }
            finally
            {
                character.SetFlag(tbGC.FlagId);
            }
            msg.Reply();


        }

        public IEnumerator ApplyRechargeTables(Coroutine co, LogicCharacterProxy _this, ApplyRechargeTablesInMessage msg)
        {
            var prox = (LogicProxy)_this;
            if (prox.Character == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }
            StaticParam.RefreshRechargeActiveData(prox.Character.serverId);
            msg.Response = StaticParam.RechargeActivityData;
            msg.Reply();
        }

        public IEnumerator ApplyFirstChargeItem(Coroutine coroutine,
                                                LogicCharacterProxy _this,
                                                ApplyFirstChargeItemInMessage msg)
        {
            var prox = (LogicProxy)_this;
            if (prox.Character == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            foreach (var dicData in LogicServer.Instance.ServerControl.FirstChargeItemDict)
            {
                var itemList = new FirstChargeDataList();
                itemList.diamond = dicData.Key;
                itemList.isCharged = prox.Character.GetExData((int)eExdataDefine.e652) >= itemList.diamond ? 1 : 0;

                var tempFlag = -1;
                if (!LogicServer.Instance.ServerControl.FirstChargeFlagDict.TryGetValue(dicData.Key, out tempFlag))
                {
                    msg.Reply((int)ErrorCodes.Unknow);
                    yield break;
                }
                if (tempFlag == -1)
                {
                    msg.Reply((int)ErrorCodes.Unknow);
                    yield break;
                }
                itemList.flag = tempFlag;

                List<string> modelStrList;
                if (!LogicServer.Instance.ServerControl.FirstChargeModelDict.TryGetValue(dicData.Key, out modelStrList))
                {
                    msg.Reply((int)ErrorCodes.Unknow);
                    yield break;
                }
                itemList.modelPath.AddRange(modelStrList);

                List<FirstChargeItem> temp;
                if (!dicData.Value.TryGetValue(prox.Character.GetRole(), out temp))
                {
                    continue;
                }

                if (temp != null)
                {
                    foreach (var itemdata in temp)
                    {
                        itemList.items.Add(itemdata);
                    }
                    msg.Response.FirstChagreItemList.Add(itemList);
                }
            }

            msg.Reply();
        }

        public IEnumerator ApplyGetFirstChargeItem(Coroutine coroutine,
                                                   LogicCharacterProxy _this,
                                                   ApplyGetFirstChargeItemInMessage msg)
        {
            var prox = (LogicProxy)_this;
            if (prox.Character == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            var index = msg.Request.Index + 1;
            var tbFirsrtCharge = Table.GetFirstRecharge(index);

            if (tbFirsrtCharge == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            // 每次充值不累计金额达到该档位  且  没有领取该档位奖励   
            if (prox.Character.GetExData((int)eExdataDefine.e652) >= tbFirsrtCharge.diamond
                && !prox.Character.GetFlag(tbFirsrtCharge.flag))
            {
                Dictionary<int, List<FirstChargeItem>> dataList;
                if (!LogicServer.Instance.ServerControl.FirstChargeItemDict.TryGetValue(tbFirsrtCharge.diamond, out dataList))
                {
                    msg.Reply((int)ErrorCodes.RoleIdError);
                    yield break;
                }

                List<FirstChargeItem> itemList;
                if (!dataList.TryGetValue(prox.Character.GetRole(), out itemList))
                {
                    msg.Reply((int)ErrorCodes.RoleIdError);
                    yield break;
                }


                var itemDict = new Dictionary<int, int>();
                if (itemList != null)
                {
                    foreach (var item in itemList)
                    {
                        itemDict[item.itemid] = item.count;
                    }

                }

                var error = BagManager.CheckAddItemList(prox.Character.mBag, itemDict);
                if (error != ErrorCodes.OK)
                {
                    msg.Response = 0;
                    msg.Reply((int)ErrorCodes.Error_ItemNoInBag_All);
                    yield break;
                }

                error = prox.Character.mBag.AddItems(itemDict, eCreateItemType.FirstCharge);
                if (error != ErrorCodes.OK)
                {
                    msg.Response = 0;
                    msg.Reply((int)error);
                    yield break;
                }

                // 设置是否已经领取首冲奖励的标记
                prox.Character.SetFlag(tbFirsrtCharge.flag);
                prox.Character.SetExData((int)eExdataDefine.e652, 0);
                msg.Response = 1;

                if (!string.IsNullOrEmpty(tbFirsrtCharge.Announcement))
                {
                    var content = String.Format(tbFirsrtCharge.Announcement, prox.Character.Name);
                    var chatAgent = LogicServer.Instance.ChatAgent;
                    var serverId = SceneExtension.GetServerLogicId(prox.Character.serverId);
                    chatAgent.BroadcastWorldMessage((uint)serverId, (int)eChatChannel.SystemScroll, 0, string.Empty,
                        new ChatMessageContent { Content = content });
                }
            }
            else
            {
                msg.Response = 0;
            }

            msg.Reply();
        }

        public IEnumerator BuyWingCharge(Coroutine coroutine, LogicCharacterProxy _this, BuyWingChargeInMessage msg)
        {
            var prox = (LogicProxy)_this;
            if (prox.Character == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            var temp = DateTime.FromBinary(prox.Character.GetExData64((int)Exdata64TimeType.ServerStartTime));
            var time = new DateTime(temp.Year, temp.Month, temp.Day, 0, 0, 0);
            var delta = DateTime.Now - time;
            if (delta.Days >= 2)
            {
                msg.Reply((int)ErrorCodes.Error_AnswerNotTime);
                yield break;
            }

            var tbGift = Table.GetGift(4000);
            if (tbGift == null)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            var needRestype = tbGift.Param[6];
            var needDiamond = tbGift.Param[7];

            if (needRestype < 0 || needDiamond < 0)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            var ItemId1 = tbGift.Param[0];
            var Count1 = tbGift.Param[1];
            var ItemID2 = tbGift.Param[2];
            var Count2 = tbGift.Param[3];

            //资源是否足够
            var character = prox.Character;
            var haveResCount = character.mBag.GetItemCount(needRestype);
            if (haveResCount < needDiamond)
            {
                msg.Reply((int)ErrorCodes.DiamondNotEnough);
                yield break;
            }

            // 尝试添加
            var result = character.mBag.CheckAddItem(ItemId1, Count1);
            if (result != ErrorCodes.OK)
            {
                msg.Reply((int)ErrorCodes.Error_ItemNoInBag_All);
                yield break;
            }
            var result2 = character.mBag.CheckAddItem(ItemID2, Count2);
            if (result2 != ErrorCodes.OK)
            {
                msg.Reply((int)ErrorCodes.Error_ItemNoInBag_All);
                yield break;
            }


            //扣除资源
            character.mBag.DeleteItem(needRestype, needDiamond, eDeleteItemType.WingBuy);
            character.SetFlag(tbGift.Flag);

            //  添加物品
            var error = character.mBag.AddItem(ItemId1, Count1, eCreateItemType.StoreBuy);
            if (error != ErrorCodes.OK)
            {
                msg.Reply((int)ErrorCodes.Error_ItemNoInBag_All);
                yield break;
            }
            var error2 = character.mBag.AddItem(ItemID2, Count2, eCreateItemType.StoreBuy);
            if (error2 != ErrorCodes.OK)
            {
                msg.Reply((int)ErrorCodes.Error_ItemNoInBag_All);
                yield break;
            }

            var strs = new List<string> {
					character.GetName()
			};
            var content = Utils.WrapDictionaryId(301037, strs);
            if (!string.IsNullOrEmpty(content))
            {
                var chatAgent = LogicServer.Instance.ChatAgent;
                var serverId = SceneExtension.GetServerLogicId(prox.Character.serverId);
                chatAgent.BroadcastWorldMessage((uint)serverId, (int)eChatChannel.SystemScroll, 0, string.Empty,
                    new ChatMessageContent { Content = content });
            }

            msg.Reply();
        }

        public IEnumerator BuyEnergyByType(Coroutine coroutine, LogicCharacterProxy _this, BuyEnergyByTypeInMessage msg)
        {
            var prox = (LogicProxy)_this;
            if (prox.Character == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            var optType = msg.Request.Type;
            if (0 == optType)
            {
                //检查是否有购买次数
                var buyTimes = prox.Character.GetExData((int)eExdataDefine.e631);
                var vipLevel = prox.Character.mBag.GetRes(eResourcesType.VipLevel);
                var tbVip = Table.GetVIP(vipLevel);
                if (tbVip == null)
                {
                    msg.Reply((int)ErrorCodes.Unknow);
                    yield break;
                }

                if (buyTimes >= tbVip.PetIslandBuyTimes)
                {
                    msg.Reply((int)ErrorCodes.Error_BuyTili_NO_Times);
                    yield break;
                }

                //资源是否足够
                var needDiamond = Table.GetServerConfig(934).ToInt();
                var character = prox.Character;
                var haveResCount = character.mBag.GetItemCount(3);
                if (haveResCount < needDiamond)
                {
                    msg.Reply((int)ErrorCodes.DiamondNotEnough);
                    yield break;
                }

                //扣除资源
                var err = character.mBag.DeleteItem((int)eResourcesType.DiamondRes, needDiamond, eDeleteItemType.BuyTiliPet);
                if (err != ErrorCodes.OK)
                {
                    msg.Reply((int)err);
                    yield break;
                }

                //增加购买次数扩展计数 增加体力扩展计数
                var error = character.PetIslandBuyTili();
                if (error != ErrorCodes.OK)
                {
                    msg.Reply((int)err);
                    yield break;
                }

                msg.Reply();
            }
            else if (1 == optType)
            {
                //检查是否有购买次数
                var buyTimes = prox.Character.GetExData((int)eExdataDefine.e633);
                var vipLevel = prox.Character.mBag.GetRes(eResourcesType.VipLevel);
                var tbVip = Table.GetVIP(vipLevel);
                if (tbVip == null)
                {
                    msg.Reply((int)ErrorCodes.Unknow);
                    yield break;
                }

                if (buyTimes > tbVip.PetIslandBuyTimes)
                {
                    msg.Reply((int)ErrorCodes.Error_BuyTili_NO_Times);
                    yield break;
                }

                //资源是否足够
                var needDiamond = Table.GetServerConfig(941).ToInt();
                var character = prox.Character;
                var haveResCount = character.mBag.GetRes(eResourcesType.DiamondRes);
                if (haveResCount < needDiamond)
                {
                    msg.Reply((int)ErrorCodes.DiamondNotEnough);
                    yield break;
                }

                //扣除资源
                var err = character.mBag.DeleteItem((int)eResourcesType.DiamondRes, needDiamond, eDeleteItemType.BuyTiliGuYuZhanChang);
                if (err != ErrorCodes.OK)
                {
                    msg.Reply((int)err);
                    yield break;
                }

                var addExt = Table.GetServerConfig(942).ToInt();

                //增加购买次数扩展计数 增加体力扩展计数
                prox.Character.SetExData((int)eExdataDefine.e632, prox.Character.GetExData((int)eExdataDefine.e632) + addExt);
                prox.Character.SetExData((int)eExdataDefine.e633, prox.Character.GetExData((int)eExdataDefine.e633) + 1);

                msg.Reply();
            }
        }

        public IEnumerator ApplyKaiFuTeHuiData(Coroutine coroutine, LogicCharacterProxy charProxy, ApplyKaiFuTeHuiDataInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            if (proxy == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            if (proxy.Character == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            var tableRecoredTeHui = new PreferentialRecord();
            var tableRecoredWeek = new PreferentialRecord();

            // 现在是开服第几天了
            var temp = DateTime.FromBinary(proxy.Character.GetExData64((int)Exdata64TimeType.ServerStartTime));
            var time = new DateTime(temp.Year, temp.Month, temp.Day, 0, 0, 0);
            var delta = DateTime.Now - time;

            Table.ForeachPreferential((record) =>
            {
                if (record.Type == 1) // 前几天的
                {
                    if (record.DelaDay == (delta.Days + 1))
                    {
                        tableRecoredTeHui = record;
                    }
                }
                else if (record.Type == 2) // 周循环
                {
                    if ((int)DateTime.Now.DayOfWeek == record.DelaDay)
                    {
                        tableRecoredWeek = record;
                    }
                }

                return true;
            });

            if (tableRecoredTeHui != null && tableRecoredTeHui.Id != 0)
            {
                msg.Response.Id = tableRecoredTeHui.Id;
                msg.Response.Type = tableRecoredTeHui.Type;
                msg.Response.DelaDay = tableRecoredTeHui.DelaDay;
                msg.Response.ExData = tableRecoredTeHui.Exdata;
                msg.Response.ItemId = tableRecoredTeHui.ItemId;
                msg.Response.ItemCount = tableRecoredTeHui.Count;
                msg.Response.OldPrice = tableRecoredTeHui.OldPrice;
                msg.Response.NowPrice = tableRecoredTeHui.NowPrice;
                msg.Response.IconId = tableRecoredTeHui.IconId;
                msg.Response.More1 = tableRecoredTeHui.More1;
                msg.Response.More2 = tableRecoredTeHui.More2;
            }
            else if (tableRecoredWeek != null && tableRecoredWeek.Id != 0)
            {
                msg.Response.Id = tableRecoredWeek.Id;
                msg.Response.Type = tableRecoredWeek.Type;
                msg.Response.DelaDay = tableRecoredWeek.DelaDay;
                msg.Response.ExData = tableRecoredWeek.Exdata;
                msg.Response.ItemId = tableRecoredWeek.ItemId;
                msg.Response.ItemCount = tableRecoredWeek.Count;
                msg.Response.OldPrice = tableRecoredWeek.OldPrice;
                msg.Response.NowPrice = tableRecoredWeek.NowPrice;
                msg.Response.IconId = tableRecoredWeek.IconId;
                msg.Response.More1 = tableRecoredWeek.More1;
                msg.Response.More2 = tableRecoredWeek.More2;
            }
            else
            {
                msg.Reply((int)ErrorCodes.Error_TableData);
                yield break;
            }

            msg.Reply();
        }

        public IEnumerator BuyKaiFuTeHuiItem(Coroutine coroutine, LogicCharacterProxy charProxy, BuyKaiFuTeHuiItemInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            if (proxy == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            if (proxy.Character == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            if (msg.Request == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            var tableId = msg.Request.TableId;
            var tbTeHui = Table.GetPreferential(tableId);
            if (tbTeHui == null)
            {
                msg.Reply((int)ErrorCodes.Error_TableData);
                yield break;
            }

            // 如果是开服前几天确认天数，  如果是周循环确认今天周几
            if (tbTeHui.Type == 1) // 开服前几天
            {
                var temp = DateTime.FromBinary(proxy.Character.GetExData64((int)Exdata64TimeType.ServerStartTime));
                var time = new DateTime(temp.Year, temp.Month, temp.Day, 0, 0, 0);
                var delta = DateTime.Now - time;

                if (tbTeHui.DelaDay != (delta.Days + 1))
                {
                    msg.Reply((int)ErrorCodes.Error_BuyTeHui_Failed_Time_Error);
                    yield break;
                }
            }
            else if (tbTeHui.Type == 2) // 周循环
            {
                if ((int)DateTime.Now.DayOfWeek != tbTeHui.DelaDay)
                {
                    msg.Reply((int)ErrorCodes.Error_BuyTeHui_Failed_Time_Error);
                    yield break;
                }
            }
            else
            {
                msg.Reply((int)ErrorCodes.Error_TableData);
                yield break;
            }

            // 检查是否已经买过了
            if (proxy.Character.GetExData(tbTeHui.Exdata) <= 0)
            {
                msg.Reply((int)ErrorCodes.Error_BuyTeHui_Failed_No_Times);
                yield break;
            }

            // 检查钱钻石是否足够
            if (proxy.Character.mBag.GetRes(eResourcesType.DiamondRes) < tbTeHui.NowPrice)
            {
                msg.Reply((int)ErrorCodes.DiamondNotEnough);
                yield break;
            }

            // 扣钻石
            var errorCode = proxy.Character.mBag.DelRes(eResourcesType.DiamondRes, tbTeHui.NowPrice, eDeleteItemType.BuyKaiFuTeHui);
            if (errorCode != ErrorCodes.OK)
            {
                msg.Reply((int)errorCode);
                yield break;
            }

            // 扣扩展计数
            proxy.Character.SetExData(tbTeHui.Exdata, proxy.Character.GetExData(tbTeHui.Exdata) - 1);

            // 给东西
            Dictionary<int, int> item = new Dictionary<int, int>();
            item.Add(tbTeHui.ItemId, tbTeHui.Count);
            proxy.Character.mBag.AddItemOrMail(501, item, null, eCreateItemType.BuyKaiFuTeHui);

            msg.Reply();
        }
        //请求查看的玩家信息(支持离线)
        public IEnumerator ApplyPlayerInfo(Coroutine coroutine,
                                           LogicCharacterProxy charProxy,
                                           ApplyPlayerInfoInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ApplyPlayerInfo----------{0}",
                msg.Request.CharacterId);
            var targetId = msg.Request.CharacterId;

            //TODO
            if (StaticData.IsRobot(targetId))
            {
                var tbRobot = Table.GetJJCRoot((int)targetId);
                if (tbRobot == null)
                {
                    msg.Reply();
                    yield break;
                }
                var infoMsg = new PlayerInfoMsg
                {
                    Id = targetId,
                    Level = tbRobot.Level,
                    TypeId = tbRobot.Career,
                    Ladder = 0,
                    Name = tbRobot.Name,
                    FightPoint = tbRobot.CombatValue,
                    StarNum = 0
                };
                if (infoMsg.Equips == null)
                {
                    infoMsg.Equips = new ItemsChangeData();
                }
                if (tbRobot.EquipHand != -1)
                {
                    var bagId = 17;
                    var dbitem = new ItemBaseData();
                    var item = new ItemEquip2(tbRobot.EquipHand, dbitem);
                    item.SetExdata(0, tbRobot.EquipLevel);
                    infoMsg.Equips.ItemsChange.Add(bagId * 100, dbitem);
                    infoMsg.EquipsModel.Add(bagId, tbRobot.EquipHand * 100 + tbRobot.EquipLevel);
                }

                if (tbRobot.EquipHead != -1)
                {
                    var bagId = 7;
                    var dbitem = new ItemBaseData();
                    var item = new ItemEquip2(tbRobot.EquipHead, dbitem);
                    item.SetExdata(0, tbRobot.EquipLevel);
                    infoMsg.Equips.ItemsChange.Add(bagId * 100, dbitem);
                    infoMsg.EquipsModel.Add(bagId, tbRobot.EquipHead * 100 + tbRobot.EquipLevel);
                }
                if (tbRobot.EquipChest != -1)
                {
                    var bagId = 11;
                    var dbitem = new ItemBaseData();
                    var item = new ItemEquip2(tbRobot.EquipChest, dbitem);
                    item.SetExdata(0, tbRobot.EquipLevel);
                    infoMsg.Equips.ItemsChange.Add(bagId * 100, dbitem);
                    infoMsg.EquipsModel.Add(bagId, tbRobot.EquipChest * 100 + tbRobot.EquipLevel);
                }

                if (tbRobot.EquipGlove != -1)
                {
                    var bagId = 14;
                    var dbitem = new ItemBaseData();
                    var item = new ItemEquip2(tbRobot.EquipGlove, dbitem);
                    item.SetExdata(0, tbRobot.EquipLevel);
                    infoMsg.Equips.ItemsChange.Add(bagId * 100, dbitem);
                    infoMsg.EquipsModel.Add(bagId, tbRobot.EquipGlove * 100 + tbRobot.EquipLevel);
                }

                if (tbRobot.EquipTrouser != -1)
                {
                    var bagId = 15;
                    var dbitem = new ItemBaseData();
                    var item = new ItemEquip2(tbRobot.EquipTrouser, dbitem);
                    item.SetExdata(0, tbRobot.EquipLevel);
                    infoMsg.Equips.ItemsChange.Add(bagId * 100, dbitem);
                    infoMsg.EquipsModel.Add(bagId, tbRobot.EquipTrouser * 100 + tbRobot.EquipLevel);
                }

                if (tbRobot.EquipShoes != -1)
                {
                    var bagId = 16;
                    var dbitem = new ItemBaseData();
                    var item = new ItemEquip2(tbRobot.EquipShoes, dbitem);
                    item.SetExdata(0, tbRobot.EquipLevel);
                    infoMsg.Equips.ItemsChange.Add(bagId * 100, dbitem);
                    infoMsg.EquipsModel.Add(bagId, tbRobot.EquipShoes * 100 + tbRobot.EquipLevel);
                }

                if (tbRobot.WingID != -1)
                {
                    var bagId = 12;
                    var dbitem = new ItemBaseData();
                    var item = new WingItem(tbRobot.WingID, dbitem);
                    infoMsg.Equips.ItemsChange.Add(bagId * 100, dbitem);
                    infoMsg.EquipsModel.Add(bagId, tbRobot.WingID * 100 + tbRobot.EquipLevel);
                }

                if (tbRobot.EquipRange != -1)
                {
                    var bagId = 13;
                    for (var i = 0; i < 2; i++)
                    {
                        var dbitem = new ItemBaseData();
                        var item = new ItemEquip2(tbRobot.EquipRange, dbitem);
                        infoMsg.Equips.ItemsChange.Add(bagId * 100 + i, dbitem);

                        //infoMsg.EquipsModel.Add(bagId, tbRobot.EquipRange * 100 + i + tbRobot.EquipLevel);   
                    }
                }

                infoMsg.PhyPowerMin = tbRobot.AttackMin;
                infoMsg.PhyPowerMax = tbRobot.AttackMax;
                infoMsg.MagPowerMin = tbRobot.AttackMin;
                infoMsg.MagPowerMax = tbRobot.AttackMax;
                infoMsg.HpMax = tbRobot.LifeLimit;
                infoMsg.MpMax = tbRobot.MagicLimit;
                infoMsg.PhyArmor = tbRobot.PhysicsDefense;
                infoMsg.MagArmor = tbRobot.MagicDefense;
                ;


                var dbLogicV = LogicServer.Instance.DB.Get<DBCharacterLogicVolatile>(coroutine,
                    DataCategory.LogicCharacter, "__v_:" + targetId);
                yield return dbLogicV;
                var addValue = 0;
                if (dbLogicV.Data != null)
                {
                    dbLogicV.Data.ExdataChange.TryGetValue(313, out addValue);
                }
                infoMsg.WorshipCount = addValue;
                msg.Response = infoMsg;
            }
            else
            {
                var dbLogicSimple = LogicServer.Instance.LogicAgent.GetLogicSimpleData(targetId, 0);
                yield return dbLogicSimple.SendAndWaitUntilDone(coroutine);
                if (dbLogicSimple.State != MessageState.Reply)
                {
                    Logger.Error("ApplyPlayerInfo LogicSimple State is {0}", dbLogicSimple.State);
                    msg.Reply((int)ErrorCodes.Unknow);
                    yield break;
                }
                if (dbLogicSimple.ErrorCode != (int)ErrorCodes.OK)
                {
                    Logger.Error("ApplyPlayerInfo LogicSimple ErrorCode is {0}", dbLogicSimple.ErrorCode);
                    msg.Reply(dbLogicSimple.ErrorCode);
                    yield break;
                }
                var dbSceneSimple = LogicServer.Instance.SceneAgent.GetSceneSimpleData(targetId, 0);
                yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);
                if (dbSceneSimple.State != MessageState.Reply)
                {
                    Logger.Error("ApplyPlayerInfo SceneSimple State is {0}", dbSceneSimple.State);
                    msg.Reply((int)ErrorCodes.Unknow);
                    yield break;
                }
                if (dbSceneSimple.ErrorCode != (int)ErrorCodes.OK)
                {
                    Logger.Error("ApplyPlayerInfo SceneSimple ErrorCode is {0}", dbSceneSimple.ErrorCode);
                    msg.Reply(dbSceneSimple.ErrorCode);
                    yield break;
                }
                if (dbLogicSimple.Response == null)
                {
                    Logger.Error("ApplyPlayerInfo LogicSimple is null ,Id={0}", targetId);
                    msg.Reply((int)ErrorCodes.Unknow);
                    yield break;
                }
                if (dbSceneSimple.Response == null)
                {
                    Logger.Error("ApplyPlayerInfo SceneSimple is null ,Id={0}", targetId);
                    msg.Reply((int)ErrorCodes.Unknow);
                    yield break;
                }
                var infoMsg = new PlayerInfoMsg
                {
                    Id = targetId,
                    Level = dbLogicSimple.Response.Level,
                    TypeId = dbLogicSimple.Response.TypeId,
                    Ladder = dbSceneSimple.Response.Ladder,
                    Name = dbSceneSimple.Response.Name,
                    FightPoint = dbSceneSimple.Response.FightPoint,
                    VipLevel = dbSceneSimple.Response.Vip,
                    StarNum = dbSceneSimple.Response.StarNum
                };

                infoMsg.TitleList.Clear();
                if (dbLogicSimple.Response.TitleList != null)
                {
                    infoMsg.TitleList.AddRange(dbLogicSimple.Response.TitleList);
                }

                infoMsg.EquipsModel.AddRange(dbLogicSimple.Response.EquipsModel);
                if (dbSceneSimple.Response.CheckAttr.Count == 6)
                {
                    infoMsg.PhyPowerMin = dbSceneSimple.Response.CheckAttr[0];
                    infoMsg.PhyPowerMax = dbSceneSimple.Response.CheckAttr[1];
                    infoMsg.MagPowerMin = dbSceneSimple.Response.CheckAttr[0];
                    infoMsg.MagPowerMax = dbSceneSimple.Response.CheckAttr[1];
                    infoMsg.HpMax = dbSceneSimple.Response.CheckAttr[2];
                    infoMsg.MpMax = dbSceneSimple.Response.CheckAttr[3];
                    infoMsg.PhyArmor = dbSceneSimple.Response.CheckAttr[4];
                    infoMsg.MagArmor = dbSceneSimple.Response.CheckAttr[5];
                }
                else
                {
                    Logger.Error("ApplyPlayerInfo uId={0} lenth={1}", targetId, dbSceneSimple.Response.CheckAttr.Count);
                }
                if (infoMsg.Equips == null)
                {
                    infoMsg.Equips = new ItemsChangeData();
                }
                if (dbLogicSimple.Response.Equips != null)
                {
                    infoMsg.Equips.ItemsChange.AddRange(dbLogicSimple.Response.Equips.ItemsChange);
                }


                infoMsg.MountId = dbLogicSimple.Response.MountId;

                var alliId = 0;
                if (dbLogicSimple.Response.Exdatas.TryGetValue(282, out alliId))
                {
                    if (alliId != 0)
                    {
                        var msgAllianceName = LogicServer.Instance.TeamAgent.SSGetAllianceName(msg.CharacterId, alliId);
                        yield return msgAllianceName.SendAndWaitUntilDone(coroutine);
                        if (msgAllianceName.State == MessageState.Reply)
                        {
                            if (msgAllianceName.ErrorCode == (int)ErrorCodes.OK)
                            {
                                infoMsg.GuildName = msgAllianceName.Response;
                            }
                            else
                            {
                                Logger.Error("ApplyPlayerInfo LogicSimple ErrorCode is {0}", dbLogicSimple.ErrorCode);
                                //msg.Reply(dbLogicSimple.ErrorCode);
                                //yield break;
                            }
                        }
                        else
                        {
                            Logger.Error("ApplyPlayerInfo LogicSimple State is {0}", dbLogicSimple.State);
                            //msg.Reply((int)ErrorCodes.Unknow);
                            //yield break;
                        }
                    }
                }

                var dbLogicV = LogicServer.Instance.DB.Get<DBCharacterLogicVolatile>(coroutine,
                    DataCategory.LogicCharacter, "__v_:" + targetId);
                yield return dbLogicV;
                var addValue = 0;
                if (dbLogicV.Data != null)
                {
                    dbLogicV.Data.ExdataChange.TryGetValue(313, out addValue);
                }
                infoMsg.WorshipCount = dbLogicSimple.Response.WorshipCount + addValue;
                msg.Response = infoMsg;
            }

            msg.Reply();
        }

        public IEnumerator SetFlag(Coroutine coroutine, LogicCharacterProxy charProxy, SetFlagInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------SetFlag----------{0},{1}",
                msg.Request.TrueDatas.Items.GetDataString(), msg.Request.FalseDatas.Items.GetDataString());
            foreach (var data in msg.Request.TrueDatas.Items)
            {
                if (!ClientCanChangeFlag.Contains(data))
                {
                    Logger.Error("SetFlag True ClientCanChangeFlag not find Id={0}", data);
                    continue;
                }
                //                 if (data == 507)
                //                 {
                //                     proxy.Character.SetFlag(506, false);
                //                 }
                proxy.Character.SetFlag(data);
                // character teamflag notify
                if (data == 485)
                {
                    var fdafa = LogicServer.Instance.TeamAgent.SSGetCharacterTeamFlag(proxy.CharacterId, proxy.CharacterId, true);
                    fdafa.SendAndWaitUntilDone(coroutine);
                }
                //
            }
            foreach (var data in msg.Request.FalseDatas.Items)
            {
                if (!ClientCanChangeFlag.Contains(data))
                {
                    Logger.Error("SetFlag False ClientCanChangeFlag not find Id={0}", data);
                    continue;
                }
                proxy.Character.SetFlag(data, false);
                // character teamflag notify
                if (data == 485)
                {
                    var fdafa = LogicServer.Instance.TeamAgent.SSGetCharacterTeamFlag(proxy.CharacterId, proxy.CharacterId, false);
                    fdafa.SendAndWaitUntilDone(coroutine);
                }
                //
            }
            msg.Reply();
            yield break;
        }

        public IEnumerator SetExData(Coroutine coroutine, LogicCharacterProxy charProxy, SetExDataInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------SetExData----------{0}",
                msg.Request.Datas.Data.GetDataString());
            foreach (var data in msg.Request.Datas.Data)
            {
                if (!ClientCanChangeExdata.Contains(data.Key))
                {
                    Logger.Error("SetExData  ClientCanChangeExdata not find Id={0}", data.Key);
                    continue;
                }
                proxy.Character.SetExData(data.Key, data.Value);
            }
            msg.Reply();
            yield break;
        }

        //请求邮件数据
        public IEnumerator ApplyMails(Coroutine coroutine, LogicCharacterProxy charProxy, ApplyMailsInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ApplyMails----------");
            foreach (var mail in proxy.Character.mMail.Mails)
            {
                var tempMail = new MailCell
                {
                    Guid = mail.Value.Guid,
                    StartTime = mail.Value.OverTime,
                    Name = mail.Value.Name,
                    Type = mail.Value.Type
                };
                if (mail.Value.Reward.Count > 0)
                {
                    tempMail.State = mail.Value.State + 100;
                }
                else
                {
                    tempMail.State = mail.Value.State + 200;
                }
                msg.Response.Mails.Add(tempMail);
            }
            msg.Reply();
            yield break;
        }

        //请求邮件详细数据
        public IEnumerator ApplyMailInfo(Coroutine coroutine, LogicCharacterProxy charProxy, ApplyMailInfoInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ApplyMailInfo----------{0}",
                msg.Request.MailId);
            var Mail = proxy.Character.mMail.GetMail(msg.Request.MailId);
            if (Mail == null)
            {
                msg.Reply((int)ErrorCodes.Error_MailNotFind);
            }
            else
            {
                if (Mail.State == (int)MailStateType.NewMail)
                {
                    proxy.Character.AddExData((int)eExdataDefine.e3, 1);
                }
                if (Mail.State == (int)MailStateType.Receive)
                {
                    Mail.State = (int)MailStateType.Receive;
                    msg.Response.State = Mail.State;
                }
                else
                {
                    Mail.State = (int)MailStateType.OldMail;
                    if (Mail.Reward.Count > 0)
                    {
                        msg.Response.State = Mail.State + 100;
                    }
                    else
                    {
                        msg.Response.State = Mail.State + 200;
                    }
                }
                msg.Response.Guid = Mail.Guid;
                if (Mail.ExtendType == (int)SendToCharacterMailType.BeKillInfo)
                {
                    msg.Response.Text = string.Format(Mail.Text, Mail.ExtendPara1,DateTime.Now);
                    msg.Response.ExtendType = Mail.ExtendType;
                    msg.Response.ExtendPara0 = Mail.ExtendPara0;
                    msg.Response.ExtendPara1 = Mail.ExtendPara1;
                }
                else
                {
                    msg.Response.Text = Mail.Text;
                }
                msg.Response.Items.AddRange(Mail.Reward);
                msg.Response.Send = Mail.Send;
                msg.Reply();
            }
            yield break;
        }

        //领取邮件
        public IEnumerator ReceiveMail(Coroutine coroutine, LogicCharacterProxy charProxy, ReceiveMailInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ReceiveMail----------");
            var resCount = 0;
            foreach (var mailId in msg.Request.Mails.Items)
            {
                var res = proxy.Character.mMail.ReceiveMail(mailId);
                if (res == ErrorCodes.OK || res == ErrorCodes.Error_MailReceiveOver)
                {
                    resCount++;
                }
                else
                {
                    break;
                }
                PlayerLog.WriteLog(proxy.CharacterId, "----------Mails.Items.id----------{0}----------", mailId);
            }

            msg.Response = resCount;
            msg.Reply();
            yield break;
        }

        //删除邮件
        public IEnumerator DeleteMail(Coroutine coroutine, LogicCharacterProxy charProxy, DeleteMailInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var items = msg.Request.Mails.Items;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------DeleteMail----------{0}", items.ToString());
            foreach (var mailId in items)
            {
                proxy.Character.mMail.DeleteMail(mailId);
            }
            msg.Reply();
            yield break;
        }

        //修理装备
        public IEnumerator RepairEquip(Coroutine coroutine, LogicCharacterProxy charProxy, RepairEquipInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------RepairEquip----------");
            var vipLevel = proxy.Character.mBag.GetRes(eResourcesType.VipLevel);
            if (Table.GetVIP(vipLevel).Repair != 1)
            {
                msg.Reply((int)ErrorCodes.VipLevelNotEnough);
            }
            else
            {
                msg.Reply((int)proxy.Character.RepairEquip());
            }
            yield break;
        }

        //从仓库取出道具
        public IEnumerator DepotTakeOut(Coroutine coroutine, LogicCharacterProxy charProxy, DepotTakeOutInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var index = msg.Request.Index;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------DepotTakeOut----------", index);
            msg.Reply((int)proxy.Character.DepotTakeOut(index));
            yield break;
        }

        //仓库放入道具
        public IEnumerator DepotPutIn(Coroutine coroutine, LogicCharacterProxy charProxy, DepotPutInInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var bagid = msg.Request.BagId;
            var index = msg.Request.Index;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------DepotPutIn----------:{0},{1}", bagid, index);
            msg.Reply((int)proxy.Character.DepotPutIn(bagid, index));
            yield break;
        }

        //从许愿池仓库取出道具
        public IEnumerator WishingPoolDepotTakeOut(Coroutine coroutine,
                                                   LogicCharacterProxy charProxy,
                                                   WishingPoolDepotTakeOutInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var index = msg.Request.Index;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------WishingPoolDepotTakeOut----------{0}", index);
            msg.Reply((int)proxy.Character.WishingPoolDepotTakeOut(index));
            yield break;
        }

        public IEnumerator ConsumeFubenItemCount(Coroutine coroutine, LogicCharacterProxy charProxy,
            int shopType, int id, int count, AsyncReturnValue<bool> result)
        {
            var sceneItem = LogicServer.Instance.SceneAgent.SSChangeFubenStoreItem(charProxy.CharacterId, shopType, id, count);
            yield return sceneItem.SendAndWaitUntilDone(coroutine);

            result.Value = false;
            if (sceneItem.ErrorCode == (int)ErrorCodes.OK)
                result.Value = sceneItem.Response;
        }

        public IEnumerator ConsumeTreasureStoreItem(Coroutine coroutine,
                                                    LogicCharacterProxy charProxy,
                                                    int storeId,
                                                    int consumeCount,
                                                    AsyncReturnValue<int> result)
        {
            var proxy = (LogicProxy)charProxy;
            var serverId = proxy.Character.serverId;
            var consumeMsg = LogicServer.Instance.ActivityAgent.SSConsumeTreasureShopItem(charProxy.CharacterId, serverId, storeId, consumeCount);
            yield return consumeMsg.SendAndWaitUntilDone(coroutine);

            result.Value = 0;
            if (consumeMsg.ErrorCode == (int)ErrorCodes.OK)
                result.Value = consumeMsg.Response;
        }

        public IEnumerator GetTreasureStoreItemCount(Coroutine coroutine,
                                             LogicCharacterProxy charProxy,
                                             int storeId,
                                             AsyncReturnValue<int> result)
        {
            var proxy = (LogicProxy)charProxy;
            var serverId = proxy.Character.serverId;
            var itemCountMsg = LogicServer.Instance.ActivityAgent.SSGetTreasureShopItemCount(charProxy.CharacterId, serverId, storeId);
            yield return itemCountMsg.SendAndWaitUntilDone(coroutine);

            result.Value = 0;
            if (itemCountMsg.ErrorCode == (int)ErrorCodes.OK)
                result.Value = itemCountMsg.Response;
        }

        public IEnumerator GetFubenStoreItemCount(Coroutine coroutine, LogicCharacterProxy charProxy,
            int shopType, int id, AsyncReturnValue<int> result)
        {
            var sceneItem = LogicServer.Instance.SceneAgent.SSGetFubenStoreItemCount(charProxy.CharacterId, shopType, id);
            yield return sceneItem.SendAndWaitUntilDone(coroutine);

            result.Value = 0;
            if (sceneItem.ErrorCode == (int)ErrorCodes.OK)
                result.Value = sceneItem.Response;
        }

        public IEnumerator BuyFubenStore(Coroutine coroutine, LogicCharacterProxy charProxy,
            int storeId, int buyCount, AsyncReturnValue<ErrorCodes> error)
        {
            error.Value = ErrorCodes.OK;
            var proxy = (LogicProxy)charProxy;
            var character = proxy.Character;
            var record = Table.GetStore(storeId);
            if (record == null)
            {
                error.Value = ErrorCodes.Error_GoodId_Not_Exist;
                yield break;
            }

            //var storeRet = AsyncReturnValue<int>.Create();
            //var co = CoroutineFactory.NewSubroutine(GetFubenStoreItemCount, coroutine, charProxy, record.Type, storeId, storeRet);
            //if (co.MoveNext())
            //    yield return co;
            //var curCount = storeRet.Value;
            //storeRet.Dispose();

            var curCount = character.GetExData(record.DayCount);

            if (buyCount > curCount) // not enough
            {
                error.Value = ErrorCodes.Error_ResNoEnough;
            }
            else
            {
                error.Value = proxy.Character.mStone.BuyItem(storeId, buyCount);
                if (error.Value == ErrorCodes.OK)
                {
                    //var buyRet = AsyncReturnValue<bool>.Create();
                    //co = CoroutineFactory.NewSubroutine(ConsumeFubenItemCount, coroutine, charProxy, record.Type, storeId, buyCount, buyRet);
                    //if (co.MoveNext())
                    //    yield return co;
                    //buyRet.Dispose();
                    //if (buyRet.Value == false)
                    //    error.Value = ErrorCodes.Error_DungeonShopItemsNotEnough;
                }
            }
        }

        public IEnumerator BuyTreasureStore(Coroutine coroutine,
                                            LogicCharacterProxy charProxy,
                                            int storeId,
                                            int buyCount,
                                            AsyncReturnValue<ErrorCodes> error)
        {
            error.Value = ErrorCodes.OK;
            var proxy = (LogicProxy)charProxy;
            var character = proxy.Character;
            var record = Table.GetStore(storeId);
            if (record == null)
            {
                error.Value = ErrorCodes.Error_GoodId_Not_Exist;
                yield break;
            }

            var countRet = AsyncReturnValue<int>.Create();
            var co = CoroutineFactory.NewSubroutine(GetTreasureStoreItemCount, coroutine, charProxy, storeId, countRet);
            if (co.MoveNext())
                yield return co;
            var curCount = countRet.Value;
            countRet.Dispose();
            if (buyCount > curCount)
            {
                error.Value = ErrorCodes.Error_TreasureStoreItemCountNotEnough;
                yield break;
            }

            error.Value = proxy.Character.mStone.BuyItem(storeId, buyCount);

            if (error.Value != (int)ErrorCodes.OK)
            {
                yield break;
            }

            var consumeRet = AsyncReturnValue<int>.Create();
            co = CoroutineFactory.NewSubroutine(ConsumeTreasureStoreItem, coroutine, charProxy, storeId, buyCount, consumeRet);
            if (co.MoveNext())
                yield return co;
            var result = consumeRet.Value;
            consumeRet.Dispose();
            if (result != (int)ErrorCodes.OK)
            {
                error.Value = ErrorCodes.Error_TreasureStoreBuyFailed;
                yield break;
            }
        }

        //商店买东西
        public IEnumerator StoreBuy(Coroutine coroutine, LogicCharacterProxy charProxy, StoreBuyInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var storeid = msg.Request.StoreId;
            var count = msg.Request.Count;
            var error = ErrorCodes.Unknow;
            
            if (msg.Request.ServiceType == (int)NpcService.NsBlackMarket)
            {
                var ret = AsyncReturnValue<ErrorCodes>.Create();
                var co = CoroutineFactory.NewSubroutine(BuyFubenStore, coroutine, charProxy, storeid, count, ret);
                if (co.MoveNext())
                {
                    yield return co;
                }
                error = ret.Value;
                ret.Dispose();
            }
            else if (msg.Request.ServiceType == (int)NpcService.NsTreasureShop)
            {
                var ret = AsyncReturnValue<ErrorCodes>.Create();
                var co = CoroutineFactory.NewSubroutine(BuyTreasureStore, coroutine, charProxy, storeid, count, ret);
                if (co.MoveNext())
                {
                    yield return co;
                }
                error = ret.Value;
                ret.Dispose();
            }
            else
            {
                error = proxy.Character.mStone.BuyItem(storeid, count);
            }
            var vipLevel = proxy.Character.mBag.GetRes(eResourcesType.VipLevel);
            if (vipLevel > 0)
            {
                var tbStore = Table.GetStore(storeid);
                var tbVip = Table.GetVIP(vipLevel);
                if (tbStore != null && tbVip != null)
                {
                    if (tbStore.ItemId == tbVip.ItemId)
                    {
                        proxy.Character.SetFlag(tbVip.BuyFlag);
                    }
                }
            }


            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------StoreBuy----------:{0},{1}", storeid, count);
            msg.Reply((int)error);
            yield break;
        }

        //请求家园数据
        public IEnumerator ApplyCityData(Coroutine coroutine, LogicCharacterProxy charProxy, ApplyCityDataInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var buildingId = msg.Request.BuildingId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ApplyCityData----------:{0}", buildingId);
            msg.Response.Buildings = new BuildingList();
            foreach (var pair in proxy.Character.mCity.Buildings)
            {
                var building = pair.Value;
                msg.Response.Buildings.Data.Add(building.GetBuildingData());
            }

            msg.Response.Missions = new PetMissionList();
            foreach (var pair in proxy.Character.mPetMission.mData)
            {
                var mission = pair.Value;
                //PetMissionData data = new PetMissionData();
                //data.Id = mission.Id;
                //data.State = mission.State;
                //data.PetList.AddRange(mission.PetList);
                //data.OverTime = mission.OverTime.ToBinary();
                //data.FinishPro = mission.FinishPro;
                //data.PetCount = mission.PetCount;
                msg.Response.Missions.Data.Add(mission.GetNetData());
            }
            proxy.Character.mCity.CheckMissionRefresh();
            foreach (var cityMission in proxy.Character.mCity.GetCityMissions())
            {
                var temp = new BuildMissionOne
                {
                    MissionId = cityMission.MissionId,
                    GiveMoney = cityMission.GiveMoney,
                    GiveExp = cityMission.GiveExp,
                    GiveItem = cityMission.GiveItem,
                    State = cityMission.State,
                    RefreshTime = cityMission.RefreshTime
                };
                temp.ItemIdList.AddRange(cityMission.ItemIdList);
                temp.ItemCountList.AddRange(cityMission.ItemCountList);
                msg.Response.CityMissions.Add(temp);
            }

            msg.Reply();
            yield break;
        }

        //请求装备耐久
        public IEnumerator ApplyEquipDurable(Coroutine coroutine,
                                             LogicCharacterProxy charProxy,
                                             ApplyEquipDurableInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ApplyEquipDurable----------");
            proxy.Character.ApplyEquipDurable(msg.Response.Data);
            msg.Reply();
            yield break;
        }


        public IEnumerator ResolveElfList(Coroutine coroutine, LogicCharacterProxy charProxy, ResolveElfListInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var elfIndexList = msg.Request.ElfIndexList;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ResolveElfList----------{0}", elfIndexList.Items.Count);
            var resolveGet = 0;
            var error = proxy.Character.ResolveElfList(elfIndexList.Items, ref resolveGet);
            msg.Response = resolveGet;
            msg.Reply((int)error);
            yield break;
        }

        //精灵相关接口
        public IEnumerator ElfOperate(Coroutine coroutine, LogicCharacterProxy charProxy, ElfOperateInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var index = msg.Request.Index;
            var type = msg.Request.Type;
            var targetIndex = msg.Request.TargetIndex;
            //int state = 0;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ElfOperate----------{0},{1},{2}", type,
                index, targetIndex);

            switch (type)
            {
                case 0: //休息
                    {
                        if (proxy.Character.mBag.NetDirty)
                        {
                            proxy.Character.mBag.NetDirtyHandle();
                        }
                        var result = proxy.Character.DisBattleElf(index);
                        proxy.Character.mBag.CleanNetDirty();
                        msg.Reply((int)result);
                    }
                    yield break;
                case 1: //出战
                    {
                        if (proxy.Character.mBag.NetDirty)
                        {
                            proxy.Character.mBag.NetDirtyHandle();
                        }
                        var result = proxy.Character.BattleElf(index, targetIndex);
                        proxy.Character.mBag.CleanNetDirty();
                        msg.Reply((int)result);
                    }
                    yield break;
                case 2: //展示
                    {
                        if (proxy.Character.mBag.NetDirty)
                        {
                            proxy.Character.mBag.NetDirtyHandle();
                        }
                        var result = proxy.Character.BattleMainElf(index);
                        proxy.Character.mBag.CleanNetDirty();
                        msg.Reply((int)result);
                    }
                    yield break;
                //case 3:
                //    var ret = Character.ElfState(index, type, ref state);
                //    msg.Response = (ulong)state;
                //    msg.Reply((int)ret);
                //    yield break;
                case 4:
                    {
                        //升级阵法
                        PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------EnchanceFormation----------");
                        var nextLevel = 0;
                        var result = proxy.Character.EnchanceFormation(ref nextLevel);
                        msg.Response = (ulong)nextLevel;
                        msg.Reply((int)result);
                    }
                    yield break;
                case 5:
                    {
                        //精灵强化
                        PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------EnchanceElf----------");
                        var nextLevel = 0;
                        var result = proxy.Character.EnchanceElf(index, ref nextLevel);
                        msg.Response = (ulong)nextLevel;
                        msg.Reply((int)result);
                    }
                    yield break;
                case 6:
                    {
                        //精灵分解
                        PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ResolveElf----------");
                        ulong resolveValue = 0;
                        var result = proxy.Character.ResolveElf(index, ref resolveValue);
                        msg.Response = resolveValue;
                        msg.Reply((int)result);
                    }
                    yield break;
                case 7:
                    { // 升星
                        PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------EnchanceElfStar----------");
                        int nextStar = 0;
                        var result = proxy.Character.EnchanceElfStar(index, ref nextStar);
                        msg.Response = (ulong)nextStar;
                        msg.Reply((int)result);
                    }
                    yield break;
                case 8:
                    {
                        PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------EnchanceElfStar----------");
                        int nextSkill = 0;
                        var result = proxy.Character.EnchanceElfSkill(index, targetIndex, ref nextSkill);
                        msg.Response = (ulong)nextSkill;
                        msg.Reply((int)result);
                    }
                    yield break;
            }
            msg.Reply((int)ErrorCodes.Unknow);
        }

        public IEnumerator ElfReplace(Coroutine coroutine, LogicCharacterProxy charProxy, ElfReplaceInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            //var ret = Character.ElfReplace(msg.Request.From, msg.Request.To);
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ElfReplace----------{0},{1}",
                msg.Request.From, msg.Request.To);

            msg.Reply();
            yield break;
        }

        //翅膀升阶
        public IEnumerator WingFormation(Coroutine coroutine, LogicCharacterProxy charProxy, WingFormationInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------WingFormation----------");
            var result = 0;
            var ec = proxy.Character.WingFormation(ref result);
            if (ec == ErrorCodes.OK)
            {
                msg.Response.AdvanceRet = result;
                msg.Response.Items = new BagsChangeData();
                msg.Response.Resources.Add((int)eResourcesType.GoldRes,
                    proxy.Character.mBag.GetRes(eResourcesType.GoldRes));
                proxy.Character.mBag.GetNetDirtyMissions(msg.Response.Items);
                proxy.Character.SetFlag(2680, true);
            }

            msg.Reply((int)ec);
            yield break;
        }

        //翅膀培养
        public IEnumerator WingTrain(Coroutine coroutine, LogicCharacterProxy charProxy, WingTrainInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------WingTrain----------{0}", msg.Request.Type);
            var result = 0;
            var ec = proxy.Character.WingTrain(msg.Request.Type, ref result);
            if (ec == ErrorCodes.OK)
            {
                msg.Response.TrainRet = result;
                msg.Response.Items = new BagsChangeData();
                msg.Response.Resources.Add((int)eResourcesType.GoldRes,
                    proxy.Character.mBag.GetRes(eResourcesType.GoldRes));
                proxy.Character.mBag.GetNetDirtyMissions(msg.Response.Items);
            }

            msg.Reply((int)ec);
            yield break;
        }

        //家园操作
        public IEnumerator CityOperationRequest(Coroutine coroutine,
                                                LogicCharacterProxy charProxy,
                                                CityOperationRequestInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var areaId = msg.Request.BuildingIdx;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------CityOperationRequest----------{0}", areaId);
            var building = proxy.Character.mCity.GetBuildByAreaId(areaId);
            if (null == building)
            {
                msg.Reply((int)ErrorCodes.Error_BuildNotFind);
                yield break;
            }

            var opType = (CityOperationType)msg.Request.OpType;
            var ret = ErrorCodes.OK;

            switch (opType)
            {
                case CityOperationType.BUILD:
                    {
                        if (msg.Request.Param.Items.Count < 1)
                        {
                            msg.Reply((int)ErrorCodes.Unknow);
                            yield break;
                        }
                        var buildingType = msg.Request.Param.Items[0];
                        ret = proxy.Character.mCity.CreateBuild(buildingType, areaId);
                    }
                    break;
                case CityOperationType.UPGRADE:
                    {
                        ret = proxy.Character.mCity.UpgradeBuild(areaId);
                    }
                    break;
                case CityOperationType.DESTROY:
                    {
                        ret = proxy.Character.mCity.DestroyBuild(areaId);
                    }
                    break;
                case CityOperationType.ASSIGNPET:
                    {
                        var oldPetList = new List<PetItem>(building.GetPets());

                        var petList = new List<int>();
                        foreach (var pet in building.PetList)
                        {
                            petList.Add(pet);
                        }
                        var andList = petList.And(msg.Request.Param.Items);
                        foreach (var pet in petList)
                        {
                            if (!andList.Contains(pet))
                            {
                                building.TakeBackPet(pet);
                            }
                        }

                        foreach (var pet in msg.Request.Param.Items)
                        {
                            if (!andList.Contains(pet))
                            {
                                building.AssignPet(pet);
                            }
                        }

                        building.OnPetChanged(oldPetList);
                    }
                    break;
                case CityOperationType.ASSIGNPETINDEX:
                    {
                        if (msg.Request.Param.Items.Count != 2)
                        {
                            msg.Reply((int)ErrorCodes.Unknow);
                            yield break;
                        }
                        var loc = msg.Request.Param.Items[0];
                        if (loc < 0 || loc > 4)
                        {
                            msg.Reply((int)ErrorCodes.Error_DataOverflow);
                            yield break;
                        }

                        var oldPetList = new List<PetItem>(building.GetPets());

                        var pet = msg.Request.Param.Items[1];
                        ret = building.AssignPetIndex(loc, pet);
                        if (building.TbBuild.Type == 6)
                        {
                            proxy.Character.mCity.SetAttrFlag();
                            proxy.Character.BooksChange();
                        }

                        building.OnPetChanged(oldPetList);
                    }
                    break;
                case CityOperationType.SPEEDUP:
                    {
                        ret = building.Speedup();
                    }
                    break;
            }

            if (ret != ErrorCodes.OK)
            {
                msg.Reply((int)ret);
                yield break;
            }

            msg.Reply();
        }

        //进入家园
        public IEnumerator EnterCity(Coroutine coroutine, LogicCharacterProxy charProxy, EnterCityInMessage msg)
        {
            //var proxy = (LogicProxy)charProxy;
            //var character = proxy.Character;
            //PlayerLog.WriteLog(character.mGuid, "----------Logic----------EnterCity----------{0}", msg.Request.CityId);
            //var req = LogicServer.Instance.SceneAgent.AskEnterDungeon(character.mGuid, character.serverId, 1000,
            //    character.mGuid, new SceneParam());
            //yield return req.SendAndWaitUntilDone(coroutine);

            msg.Reply((int)ErrorCodes.Unknow);
            yield break;
        }

        //宠物操作
        public IEnumerator OperatePet(Coroutine coroutine, LogicCharacterProxy charProxy, OperatePetInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------OperatePet----------{0},{1}",
                msg.Request.PetId, msg.Request.Type);
            //Logger.Info("Character[{0}] OperatePet({1},{2})----------begin", Character.mGuid, msg.Request.PetId, msg.Request.Type);
            var ret = proxy.Character.OperatePet(msg.Request.PetId, (PetOperationType)msg.Request.Type,
                msg.Request.Param);
            //Logger.Info("Character[{0}] OperatePet({1},{2})----------ret={3}", Character.mGuid, msg.Request.PetId, msg.Request.Type, ret);
            msg.Reply((int)ret);
            yield break;
        }

        //操作宠物任务
        public IEnumerator OperatePetMission(Coroutine coroutine,
                                             LogicCharacterProxy charProxy,
                                             OperatePetMissionInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var id = msg.Request.Id;
            var type = (PetMissionOpt)msg.Request.Type;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------OperatePetMission----------{0},{1}", id,
                type);
            var ret = ErrorCodes.Unknow;

            if (PetMissionOpt.START == type)
            {
                ret = proxy.Character.mPetMission.DoMission(id, msg.Request.Param.Items, false);
            }
            else if (PetMissionOpt.COMPLETE == type)
            {
                ret = proxy.Character.mPetMission.CommitMission(id);
            }
            else if (PetMissionOpt.DELETE == type)
            {
                ret = proxy.Character.mPetMission.DeleteMission(id);
            }
            else if (PetMissionOpt.BUYTIMES == type)
            {
                ret = proxy.Character.mPetMission.DoMission(id, msg.Request.Param.Items, true);
            }
            //else if (PetMissionOpt.COMMIT == type)
            //{
            //    var mis = proxy.Character.mPetMission.GetMission(id);
            //    if ((int)PetMissionStateType.Finish2 != mis.State)
            //    {
            //        ret = ErrorCodes.Error_PetState;
            //    }
            //    else
            //    {
            //        ret = proxy.Character.mPetMission.CommitMission(id);
            //    }

            //}
            msg.Reply((int)ret);
            yield break;
        }
        //分解符文
        public IEnumerator SplitMedal(Coroutine coroutine, LogicCharacterProxy charProxy, SplitMedalInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var errorCode = proxy.Character.SplitMedal(msg.Request.BagId, msg.Request.BagIndex, msg.Request.Flag);
            msg.Reply((int)errorCode);
            yield break;
        }

        //装备勋章
        public IEnumerator EquipMedal(Coroutine coroutine, LogicCharacterProxy charProxy, EquipMedalInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------EquipMedal----------{0},{1}",
                msg.Request.BagId, msg.Request.BagIndex);
            var putIndex = -1;
            var errorCode = proxy.Character.UseMedal(msg.Request.BagId, msg.Request.BagIndex, ref putIndex);
            msg.Response = putIndex;
            msg.Reply((int)errorCode);
            yield break;
        }

        //拾取勋章
        public IEnumerator PickUpMedal(Coroutine coroutine, LogicCharacterProxy charProxy, PickUpMedalInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------PickUpMedal----------{0}", msg.Request.Index);
            msg.Reply((int)proxy.Character.PickUpMedal(msg.Request.Index, msg.Request.Flag));
            yield break;
        }

        //强化勋章	
        public IEnumerator EnchanceMedal(Coroutine coroutine, LogicCharacterProxy charProxy, EnchanceMedalInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------EnchanceMedal----------{0},{1}",
                msg.Request.BagId, msg.Request.BagIndex);
            msg.Reply(
                (int)
                    proxy.Character.EnchanceMedal(msg.Request.BagId, msg.Request.BagIndex));
            yield break;
        }

        // 购买包裹
        public IEnumerator BuySpaceBag(Coroutine coroutine, LogicCharacterProxy charProxy, BuySpaceBagInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------BuySpaceBag----------{0},{1}",
                msg.Request.BagId, msg.Request.BagIndex);
            msg.Reply((int)proxy.Character.BuySpaceBag(msg.Request.BagId, msg.Request.BagIndex, msg.Request.NeedCount));
            yield break;
        }

        // 使用建筑服务
        public IEnumerator UseBuildService(Coroutine coroutine,
                                           LogicCharacterProxy charProxy,
                                           UseBuildServiceInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------UseBuildService----------{0},{1}",
                msg.Request.ServiceId, msg.Request.Param.Items.GetDataString());
            var result = msg.Response;
            var errorCodes = proxy.Character.mCity.UseBuildService(msg.Request.AreaId, msg.Request.ServiceId,
                msg.Request.Param.Items, ref result);
            msg.Reply((int)errorCodes);
            yield break;
        }

        //根据区域id请求建筑数据
        public IEnumerator ApplyCityBuildingData(Coroutine coroutine,
                                                 LogicCharacterProxy charProxy,
                                                 ApplyCityBuildingDataInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ApplyCityBuildingData----------{0}",
                msg.Request.AreaId);
            if (proxy.Character.mCity == null)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            var areaId = msg.Request.AreaId;
            var build = proxy.Character.mCity.GetBuildByArea(areaId);
            if (build == null)
            {
                msg.Reply((int)ErrorCodes.Error_BuildNotFind);
                yield break;
            }
            msg.Response = build.GetBuildingData();
            msg.Reply();
        }

        //请求天梯1V1的玩家列表
        public IEnumerator GetP1vP1LadderPlayer(Coroutine coroutine,
                                                LogicCharacterProxy charProxy,
                                                GetP1vP1LadderPlayerInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------GetP1vP1LadderPlayer----------");
            var rankList = LogicServer.Instance.RankAgent.Rank_GetP1vP1List(proxy.CharacterId, proxy.Character.serverId,
                proxy.CharacterId, proxy.Character.GetName());
            yield return rankList.SendAndWaitUntilDone(coroutine);
            if (rankList.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            msg.Response.NowLadder = rankList.Response.nowRank;

            msg.Response.Ranks.AddRange(rankList.Response.ranks);

            foreach (var targetId in rankList.Response.characters)
            {
                if (StaticData.IsRobot(targetId))
                {
                    var tbRobot = Table.GetJJCRoot((int)targetId);
                    if (tbRobot == null)
                    {
                        continue;
                    }

                    var rebornLadder = 0;
                    Table.ForeachTransmigration(record =>
                    {
                        if (record.TransLevel <= tbRobot.Level)
                        {
                            if (record.PropPoint < rebornLadder)
                            {
                                return false;
                            }
                            rebornLadder = record.PropPoint;
                        }
                        else
                        {
                            return false;
                        }
                        return true;
                    });

                    var infoMsgRobot = new PlayerInfoMsg
                    {
                        Id = targetId,
                        Level = tbRobot.Level,
                        TypeId = tbRobot.Career,
                        Ladder = rebornLadder,
                        Name = tbRobot.Name,
                        FightPoint = tbRobot.CombatValue,
                        StarNum = 0
                    };
                    msg.Response.Players.Add(infoMsgRobot);
                    continue;
                }
                var dbLogicSimple = LogicServer.Instance.LogicAgent.GetLogicSimpleData(targetId, 0);
                yield return dbLogicSimple.SendAndWaitUntilDone(coroutine);
                if (dbLogicSimple.State != MessageState.Reply)
                {
                    Logger.Error("ApplyPlayerInfo LogicSimple State is {0}", dbLogicSimple.State);
                    yield break;
                }
                if (dbLogicSimple.ErrorCode != (int)ErrorCodes.OK)
                {
                    Logger.Error("ApplyPlayerInfo LogicSimple ErrorCode is {0}", dbLogicSimple.ErrorCode);
                    yield break;
                }
                var dbSceneSimple = LogicServer.Instance.SceneAgent.GetSceneSimpleData(targetId, 0);
                yield return dbSceneSimple.SendAndWaitUntilDone(coroutine);
                if (dbSceneSimple.State != MessageState.Reply)
                {
                    Logger.Error("ApplyPlayerInfo SceneSimple State is {0}", dbSceneSimple.State);
                    yield break;
                }
                if (dbSceneSimple.ErrorCode != (int)ErrorCodes.OK)
                {
                    Logger.Error("ApplyPlayerInfo SceneSimple ErrorCode is {0}", dbSceneSimple.ErrorCode);
                    yield break;
                }
                if (dbLogicSimple.Response == null)
                {
                    Logger.Error("ApplyPlayerInfo LogicSimple is null ,Id={0}", targetId);
                    yield break;
                }
                if (dbSceneSimple.Response == null)
                {
                    Logger.Error("ApplyPlayerInfo SceneSimple is null ,Id={0}", targetId);
                    yield break;
                }
                var infoMsg = new PlayerInfoMsg
                {
                    Id = targetId,
                    Level = dbLogicSimple.Response.Level,
                    TypeId = dbLogicSimple.Response.TypeId,
                    Ladder = dbSceneSimple.Response.Ladder,
                    Name = dbSceneSimple.Response.Name,
                    FightPoint = dbSceneSimple.Response.FightPoint,
                    StarNum = dbLogicSimple.Response.StarNum
                };

                //infoMsg.EquipsModel.AddRange(dbLogicSimple.Response.EquipsModel);
                //if (dbSceneSimple.Response.CheckAttr.Count == 6)
                //{
                //    if (infoMsg.TypeId == 0 || infoMsg.TypeId == 2)
                //    {
                //        infoMsg.PhyPowerMin = dbSceneSimple.Response.CheckAttr[0];
                //        infoMsg.PhyPowerMax = dbSceneSimple.Response.CheckAttr[1];
                //    }
                //    else
                //    {
                //        infoMsg.MagPowerMin = dbSceneSimple.Response.CheckAttr[0];
                //        infoMsg.MagPowerMax = dbSceneSimple.Response.CheckAttr[1];
                //    }
                //    infoMsg.HpMax = dbSceneSimple.Response.CheckAttr[2];
                //    infoMsg.MpMax = dbSceneSimple.Response.CheckAttr[3];
                //    infoMsg.PhyArmor = dbSceneSimple.Response.CheckAttr[4];
                //    infoMsg.MagArmor = dbSceneSimple.Response.CheckAttr[5];
                //}
                //else
                //{
                //    Logger.Error("ApplyPlayerInfo uId={0} lenth={1}", targetId, dbSceneSimple.Response.CheckAttr.Count);
                //}
                //if (infoMsg.Equips == null)
                //{
                //    infoMsg.Equips = new ItemsChangeData();
                //}
                //if (dbLogicSimple.Response.Equips != null)
                //{
                //    infoMsg.Equips.ItemsChange.AddRange(dbLogicSimple.Response.Equips.ItemsChange);
                //}
                msg.Response.Players.Add(infoMsg);
            }
            msg.Reply();
        }

        //攻击某个玩家
        public IEnumerator GetP1vP1FightPlayer(Coroutine coroutine,
                                               LogicCharacterProxy charProxy,
                                               GetP1vP1FightPlayerInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var character = proxy.Character;
            var vipLevel = character.mBag.GetRes(eResourcesType.VipLevel);
            var tbVip = Table.GetVIP(vipLevel);
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------GetP1vP1FightPlayer----------{0},{1}",
                msg.Request.Rank, msg.Request.Guid);

            //0  正常
            //1  cd购买
            //2  次数购买
            var type = msg.Request.Type;
            //条件
            var checkResult = character.CheckP1vP1();
            if (checkResult != ErrorCodes.OK)
            {
                if (checkResult == ErrorCodes.Error_CountNotEnough && type == 2)
                {
                    //购买次数
                    checkResult = character.BuyP1vP1Count();
                    if (checkResult != ErrorCodes.OK)
                    {
                        msg.Reply((int)checkResult);
                        yield break;
                    }
                }
                else if (checkResult == ErrorCodes.Error_LadderTime && type == 1)
                {
                    if (tbVip.PKChallengeCD == 0)
                    {
                        //不免CD，才需要购买CD
                        //购买CD
                        checkResult = character.BuyP1vP1CD();
                        if (checkResult != ErrorCodes.OK)
                        {
                            msg.Reply((int)checkResult);
                            yield break;
                        }
                    }
                }
                else
                {
                    msg.Reply((int)checkResult);
                    yield break;
                }
            }
            //排行服务器检验
            var rankList = LogicServer.Instance.RankAgent.CompareRank(proxy.CharacterId, character.serverId,
                msg.Request.Guid, msg.Request.Rank);
            yield return rankList.SendAndWaitUntilDone(coroutine);
            if (rankList.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (rankList.Response != 1)
            {
                msg.Reply((int)ErrorCodes.Error_LadderChange);
                yield break;
            }

            //执行
            var gotoScene = LogicServer.Instance.SceneAgent.SSGoToSceneAndPvP(proxy.CharacterId, 1001, msg.Request.Guid);
            yield return gotoScene.SendAndWaitUntilDone(coroutine);
            var error = (ErrorCodes)gotoScene.ErrorCode;
            if (ErrorCodes.OK != error)
            {
                msg.Reply(gotoScene.ErrorCode);
                Logger.Error("GetP1vP1FightPlayer error{0}", gotoScene.ErrorCode);
                yield break;
            }
            character.AddExData((int)eExdataDefine.e98, -1);
            character.AddExData((int)eExdataDefine.e26, 1);
            if (tbVip.PKChallengeCD == 0)
            {
                //不免CD，才要加CD
                character.lExdata64.SetTime(Exdata64TimeType.P1vP1CoolDown,
                    DateTime.Now.AddSeconds(Table.GetServerConfig(202).ToInt()));
            }

            //潜规则引导标记位
            if (!character.GetFlag(531))
            {
                character.SetFlag(531);
                character.SetFlag(530, false);
            }

            msg.Reply();
        }

        //天梯战斗历史
        public IEnumerator GetP1vP1LadderOldList(Coroutine coroutine,
                                                 LogicCharacterProxy charProxy,
                                                 GetP1vP1LadderOldListInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------GetP1vP1LadderOldList----------");
            proxy.Character.GetP1vP1OldList(msg.Response.Data);
            msg.Reply();
            yield break;
        }

        //购买天梯次数
        public IEnumerator BuyP1vP1Count(Coroutine coroutine, LogicCharacterProxy charProxy, BuyP1vP1CountInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------BuyP1vP1Count----------");
            msg.Reply((int)proxy.Character.BuyP1vP1Count());
            yield break;
        }

        //回收装备
        public IEnumerator RecoveryEquip(Coroutine coroutine, LogicCharacterProxy charProxy, RecoveryEquipInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var type = msg.Request.Type;
            var items = msg.Request.IndexList.Items;
            msg.Reply((int)proxy.Character.RecoveryEquip(type, items));
            PlayerLog.WriteLog(proxy.CharacterId, "----------RecoveryEquip-----Type:{0}-----Items:{1}----------", type,
                items.ToString());
            yield break;
        }

        //宠物蛋抽奖  //精灵抽奖
        public IEnumerator DrawLotteryPetEgg(Coroutine coroutine,
                                             LogicCharacterProxy charProxy,
                                             DrawLotteryPetEggInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------DrawLotteryPetEgg----------{0}",
                msg.Request.Type);
            var type = msg.Request.Type;
            var result = ErrorCodes.OK;
            //判断建筑条件

            //执行
            var motherDropId = 200;
            switch (type)
            {
                case 0:
                    {
                        //孵化单抽

                        var needResId = Table.GetServerConfig(217).ToInt();
                        var needResCount = Table.GetServerConfig(218).ToInt();
                        if (proxy.Character.mBag.GetItemCount(needResId) < needResCount)
                        {
                            result = ErrorCodes.ItemNotEnough;
                            msg.Reply((int)result);
                            yield break;
                        }
                        //消耗
                        proxy.Character.mBag.DeleteItem(needResId, needResCount, eDeleteItemType.Hatch0);
                        if (proxy.Character.GetExData((int)eExdataDefine.e266) == 0)
                        {
                            motherDropId = Table.GetServerConfig(209).ToInt();
                        }
                        else
                        {
                            motherDropId = Table.GetServerConfig(206).ToInt();
                        }
                        var itemList = new Dictionary<int, int>();
                        proxy.Character.DropMother(motherDropId, itemList);
                        foreach (var i in itemList)
                        {
                            msg.Response.Items.Add(i.Key);
                            proxy.Character.mBag.AddItem(i.Key, i.Value, eCreateItemType.DrawPetEgg);
                        }
                        proxy.Character.AddExData((int)eExdataDefine.e266, 1);
                    }
                    break;
                case 1:
                    {
                        //孵化10连抽奖

                        var needResId = Table.GetServerConfig(219).ToInt();
                        var needResCount = Table.GetServerConfig(220).ToInt();
                        if (proxy.Character.mBag.GetItemCount(needResId) < needResCount)
                        {
                            result = ErrorCodes.ItemNotEnough;
                            msg.Reply((int)result);
                            yield break;
                        }

                        //消耗
                        proxy.Character.mBag.DeleteItem(needResId, needResCount, eDeleteItemType.Hatch0);
                        var itemList = new Dictionary<int, int>();
                        var exdataCount = proxy.Character.GetExData((int)eExdataDefine.e266);
                        for (var i = 0; i != 10; ++i)
                        {
                            if (exdataCount % 10 == 5)
                            {
                                motherDropId = Table.GetServerConfig(223).ToInt();
                            }
                            else
                            {
                                motherDropId = Table.GetServerConfig(207).ToInt();
                            }
                            exdataCount++;
                            proxy.Character.DropMother(motherDropId, itemList);
                        }

                        proxy.Character.SetExData((int)eExdataDefine.e266, exdataCount);
                        foreach (var i in itemList)
                        {
                            msg.Response.Items.Add(i.Key);
                            proxy.Character.mBag.AddItem(i.Key, i.Value, eCreateItemType.DrawPetEgg);
                        }
                    }
                    break;
                case 2:
                    {
                        //稀有孵化抽奖
                        var needResId = Table.GetServerConfig(221).ToInt();
                        var needResCount = Table.GetServerConfig(222).ToInt();
                        if (proxy.Character.mBag.GetItemCount(needResId) < needResCount)
                        {
                            result = ErrorCodes.ItemNotEnough;
                            msg.Reply((int)result);
                            yield break;
                        }
                        //消耗
                        proxy.Character.mBag.DeleteItem(needResId, needResCount, eDeleteItemType.Hatch0);

                        motherDropId = Table.GetServerConfig(208).ToInt();

                        var itemList = new Dictionary<int, int>();
                        proxy.Character.DropMother(motherDropId, itemList);
                        foreach (var i in itemList)
                        {
                            msg.Response.Items.Add(i.Key);
                            proxy.Character.mBag.AddItem(i.Key, i.Value, eCreateItemType.DrawPetEgg);
                        }
                        proxy.Character.AddExData((int)eExdataDefine.e267, 1);
                    }
                    break;
                case 200:
                    {
                        //精灵单抽
                        var bag = proxy.Character.GetBag((int)eBagType.Elf);
                        if (bag.GetFreeCount() < 1)
                        {
                            result = ErrorCodes.Error_ItemNoInBag_All;
                            msg.Reply((int)result);
                            yield break;
                        }
                        var tbServerConfig = Table.GetServerConfig(914);
                        if (null == tbServerConfig)
                        {
                            yield break;
                        }
                        var FreeElfTime = int.Parse(tbServerConfig.Value);
                        var time = proxy.Character.lExdata64.GetTime(Exdata64TimeType.FreeElfTime);
                        if (time < DateTime.Now)
                        {
                            proxy.Character.lExdata64.SetTime(Exdata64TimeType.FreeElfTime, DateTime.Now.AddMinutes(FreeElfTime));
                        }
                        else
                        {
                            var needResId = Table.GetServerConfig(500).ToInt();
                            var needResCount = Table.GetServerConfig(501).ToInt();
                            if (proxy.Character.mBag.GetItemCount(needResId) < needResCount)
                            {
                                result = ErrorCodes.ItemNotEnough;
                                msg.Reply((int)result);
                                yield break;
                            }
                            //消耗
                            proxy.Character.mBag.DeleteItem(needResId, needResCount, eDeleteItemType.DrawElf);
                        }

                        //抽奖次数
                        var exdataCount = proxy.Character.GetExData((int)eExdataDefine.e410);
                        //第一次抽奖
                        //if (exdataCount > 0)
                        //{
                        motherDropId = Table.GetServerConfig(505).ToInt();
                        //}
                        //else
                        //{
                        //    motherDropId = Table.GetServerConfig(504).ToInt();
                        //}

                        var itemList = new Dictionary<int, int>();
                        proxy.Character.DropMother(motherDropId, itemList);
                        if (itemList.Count != 1)
                        {
                            Logger.Warn("WishingPool itemCount is {0}", itemList.Count);
                            itemList.Clear();
                            itemList[22000] = 1;
                        }
                        var temps = new DrawItemResult();
                        foreach (var i in itemList)
                        {
                            var item = Table.GetItemBase(i.Key);
                            if (item.Quality >= 4)
                            {
                                var args = new List<string>
                        {
                        Utils.AddCharacter(proxy.Character.mGuid,proxy.Character.GetName()),
                        item.Name,   
                        };
                                var exExdata = new List<int>();
                                proxy.Character.SendSystemNoticeInfo(291007, args, exExdata);
                            }

                            proxy.Character.mBag.AddItemToElf(i.Key, i.Value, temps.Items);
                        }
                        if (proxy.Character.Proxy != null)
                        {
                            proxy.Character.Proxy.ElfDrawOver(temps, DateTime.Now.ToBinary());
                        }

                        proxy.Character.AddExData((int)eExdataDefine.e410, 1);
                        proxy.Character.SetFlag(2681, true);
                    }
                    break;
                case 201:
                    {
                        //精灵10连
                        var bag = proxy.Character.GetBag((int)eBagType.Elf);
                        if (bag.GetFreeCount() < 10)
                        {
                            result = ErrorCodes.Error_ItemNoInBag_All;
                            msg.Reply((int)result);
                            yield break;
                        }
                        var needResId = Table.GetServerConfig(502).ToInt();
                        var needResCount = Table.GetServerConfig(503).ToInt();
                        if (proxy.Character.mBag.GetItemCount(needResId) < needResCount)
                        {
                            result = ErrorCodes.ItemNotEnough;
                            msg.Reply((int)result);
                            yield break;
                        }
                        //消耗
                        proxy.Character.mBag.DeleteItem(needResId, needResCount, eDeleteItemType.DrawElf);
                        var exdataCount = proxy.Character.GetExData((int)eExdataDefine.e410);
                        var temps = new DrawItemResult();
                        var itemList = new Dictionary<int, int>();
                        for (var i = 0; i != 10; ++i)
                        {
                            //motherDropId = Table.GetServerConfig(505).ToInt();
                            //第一次抽奖
                            //if (exdataCount > 0)
                            //{
                            motherDropId = Table.GetServerConfig(505).ToInt();
                            //}
                            //else
                            //{
                            //    motherDropId = Table.GetServerConfig(504).ToInt();
                            //}
                            //motherDropId = Table.GetServerConfig(211).ToInt();
                            exdataCount++;
                            proxy.Character.DropMother(motherDropId, itemList);
                            if (itemList.Count != 1)
                            {
                                Logger.Warn("WishingPool itemCount is {0}", itemList.Count);
                                itemList.Clear();
                                itemList[22000] = 1;
                            }
                            foreach (var j in itemList)
                            {
                                var item = Table.GetItemBase(j.Key);
                                if (item.Quality >= 4)
                                {
                                    var args = new List<string>
                                {
                                Utils.AddCharacter(proxy.Character.mGuid,proxy.Character.GetName()),
                                item.Name,   
                                };
                                    var exExdata = new List<int>();
                                    proxy.Character.SendSystemNoticeInfo(291007, args, exExdata);
                                }
                                proxy.Character.mBag.AddItemToElf(j.Key, j.Value, temps.Items);
                            }
                            itemList.Clear();
                        }
                        if (proxy.Character.Proxy != null)
                        {
                            proxy.Character.Proxy.ElfDrawOver(temps, DateTime.Now.ToBinary());
                        }

                        proxy.Character.AddExData((int)eExdataDefine.e410, 10);
                    }
                    break;
                default:
                    {
                        msg.Reply((int)ErrorCodes.Error_DataOverflow);
                        yield break;
                    }
            }
            msg.Reply((int)ErrorCodes.OK);
        }

        public IEnumerator DrawWishingPool(Coroutine coroutine,
                                           LogicCharacterProxy charProxy,
                                           DrawWishingPoolInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------DrawWishingPool----------{0}",
                msg.Request.Type);
            var type = msg.Request.Type;
            //ErrorCodes result = ErrorCodes.OK;
            //判断建筑条件

            //执行
            //int motherDropId = 200;
            //switch (type)
            //{
            //    case 100:
            //        {//许愿池单抽
            //            var build = Character.mCity.GetBuildByType((int)BuildingType.WishingPool);
            //            if (build == null)
            //            {
            //                result = ErrorCodes.Error_BuildNotFind;
            //                msg.Reply((int)result);
            //                yield break;
            //            }
            //            int freeCount = Character.GetExData(251);
            //            if (freeCount > 0 && Character.lExdata64.GetTime(Exdata64TimeType.FreeWishingTime) < DateTime.Now)
            //            {
            //                Character.SetExData(251, freeCount - 1);
            //                Character.lExdata64.SetTime(Exdata64TimeType.FreeWishingTime, DateTime.Now.AddMinutes(Table.GetBuildingService(build.TbBuild.ServiceId).Param[0]));
            //            }
            //            else
            //            {
            //                int needResId = Table.GetServerConfig(213).ToInt();
            //                int needResCount = Table.GetServerConfig(214).ToInt();
            //                if (Character.mBag.GetItemCount(needResId) < needResCount)
            //                {
            //                    result = ErrorCodes.ItemNotEnough;
            //                    msg.Reply((int)result);
            //                    yield break;
            //                }
            //                //消耗
            //                Character.mBag.DeleteItem(needResId, needResCount);
            //            }

            //            int exdataCount = Character.GetExData((int)eExdataDefine.e251);
            //            if (exdataCount > 0)
            //            {
            //                motherDropId = Table.GetServerConfig(212).ToInt();
            //                //Character.SetExData((int)eAchievementExdata.e251, exdataCount - 1);
            //            }
            //            else
            //            {
            //                motherDropId = Table.GetServerConfig(210).ToInt();
            //            }

            //            Dictionary<int, int> itemList = new Dictionary<int, int>();
            //            Character.DropMother(motherDropId, itemList);
            //            foreach (var i in itemList)
            //            {
            //                msg.Response.Items.Add(i.Key);
            //                Character.mBag.AddItem(i.Key, i.Value);
            //            }
            //            Character.AddExData((int)eExdataDefine.e270, 1);
            //            Character.mBag.NetDirtyHandle();
            //        }
            //        break;
            //    case 101:
            //        {//许愿池10连
            //            var build = Character.mCity.GetBuildByType((int)BuildingType.WishingPool);
            //            if (build == null)
            //            {
            //                result = ErrorCodes.Error_BuildNotFind;
            //                msg.Reply((int)result);
            //                yield break;
            //            }
            //            int needResId = Table.GetServerConfig(215).ToInt();
            //            int needResCount = Table.GetServerConfig(216).ToInt();
            //            if (Character.mBag.GetItemCount(needResId) < needResCount)
            //            {
            //                result = ErrorCodes.ItemNotEnough;
            //                msg.Reply((int)result);
            //                yield break;
            //            }
            //            //消耗
            //            Character.mBag.DeleteItem(needResId, needResCount);
            //            int exdataCount = Character.GetExData((int)eExdataDefine.e270);
            //            Dictionary<int, int> itemList = new Dictionary<int, int>();
            //            for (int i = 0; i != 10; ++i)
            //            {
            //                motherDropId = Table.GetServerConfig(211).ToInt();
            //                //if (exdataCount % 10 == 5)
            //                //{
            //                //    motherDropId = Table.GetServerConfig(224).ToInt();
            //                //}
            //                //else
            //                //{
            //                //    motherDropId = Table.GetServerConfig(211).ToInt();
            //                //}
            //                exdataCount++;
            //                Character.DropMother(motherDropId, itemList);
            //            }

            //            foreach (var i in itemList)
            //            {
            //                for (int j = 0; j < i.Value; j++)
            //                {
            //                    msg.Response.Items.Add(i.Key);
            //                }
            //                Character.mBag.AddItem(i.Key, i.Value);
            //            }
            //            Character.SetExData((int)eExdataDefine.e270, exdataCount);
            //        }
            //        break;
            //    default:
            //        {
            //            msg.Reply((int)ErrorCodes.Error_DataOverflow);
            //            yield break;
            //        }
            //}
            msg.Reply((int)ErrorCodes.OK);
            yield break;
        }

        //机器人完成副本
        public IEnumerator RobotcFinishFuben(Coroutine coroutine,
                                             LogicCharacterProxy charProxy,
                                             RobotcFinishFubenInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var funbenid = msg.Request.FubenId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------RobotcFinishFuben----------:{0}", funbenid);
            proxy.Character.CompleteFubenSaveData(funbenid, 200);
            msg.Reply();
            yield break;
        }

        //战盟操作:创建
        public IEnumerator CreateAlliance(Coroutine coroutine,
                                          LogicCharacterProxy charProxy,
                                          CreateAllianceInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var character = proxy.Character;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------CreateAlliance----------{0}",
                msg.Request.Name);
            //判断创建条件
            if (!SensitiveWord.CheckString(msg.Request.Name))
            {
                Logger.Error("CreateAlliance client need check name={0}", msg.Request.Name);
                msg.Reply((int)ErrorCodes.Error_NAME_Sensitive);
                yield break;
            }
            var resultCodes = character.mAlliance.CreateNewAlliance(msg.Request.Name);
            if (resultCodes != ErrorCodes.OK)
            {
                msg.Reply((int)resultCodes);
                yield break;
            }
            //请求创建战盟
            var teamMsg = LogicServer.Instance.TeamAgent.Logic2TeamCreateAlliance(character.mGuid, character.serverId,
                character.mGuid, msg.Request.Name, 0);
            yield return teamMsg.SendAndWaitUntilDone(coroutine);
            if (teamMsg.State == MessageState.Reply)
            {
                if (teamMsg.ErrorCode == (int)ErrorCodes.OK)
                {
                    //请求创建成功，再次检查条件
                    resultCodes = character.mAlliance.CreateNewAlliance(msg.Request.Name);
                    if (resultCodes != ErrorCodes.OK)
                    {
                        //二次判断条件不符合
                        var teamMsgFaild = LogicServer.Instance.TeamAgent.Logic2TeamCreateAlliance(character.mGuid,
                            character.serverId, character.mGuid, msg.Request.Name, -1);
                        yield return teamMsgFaild.SendAndWaitUntilDone(coroutine);
                        msg.Reply((int)resultCodes);

                        if (resultCodes == ErrorCodes.OK)
                        {
                            //尝试激活战盟首领的称号
                            character.ModityTitle(2000, true);
                        }
                        yield break;
                    }
                    //允许成功创建
                    character.mBag.DelRes(eResourcesType.GoldRes, AllianceManager.CreateNewAllianceNeedMoney,
                        eDeleteItemType.CreateAlliance);
                    character.mAlliance.State = AllianceState.Have;
                    character.mAlliance.AllianceId = teamMsg.Response;
                    character.mAlliance.Ladder = 3;
                    msg.Response = teamMsg.Response;
                    character.mAlliance.CleanApplyList();
                    var teamMsgSuccess = LogicServer.Instance.TeamAgent.Logic2TeamCreateAlliance(character.mGuid,
                        character.serverId, character.mGuid, msg.Request.Name, 1);
                    yield return teamMsgSuccess.SendAndWaitUntilDone(coroutine);
                    var result = LogicServer.Instance.SceneAgent.SSAllianceDataChange(character.mGuid, teamMsg.Response,
                        0, msg.Request.Name);
                    yield return result.SendAndWaitUntilDone(coroutine);

                    //尝试激活战盟首领的称号
                    character.ModityTitle(2000, true);
                    character.SetFlag(2801);
                }
                else
                {
                    msg.Reply(teamMsg.ErrorCode);
                    yield break;
                }
            }

            msg.Reply();
        }

        //战盟操作:其他操作 type：0=申请加入（value=战盟ID）  1=取消申请（value=战盟ID）  2=退出战盟   3=同意邀请（value=战盟ID）  4=拒绝邀请（value=战盟ID）
        public IEnumerator AllianceOperation(Coroutine coroutine,
                                             LogicCharacterProxy charProxy,
                                             AllianceOperationInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------AllianceOperation----------{0},{1}",
                msg.Request.Type, msg.Request.Value);
            var character = proxy.Character;
            //判断创建条件
            var resultCodes = character.mAlliance.CheckOperation(msg.Request.Type);
            if (resultCodes != ErrorCodes.OK)
            {
                msg.Reply((int)resultCodes);
                yield break;
            }
            var serverId = character.serverId;
            var occupantId = StaticParam.AllianceWarInfo[serverId].OccupantId;
            var oldAllianceId = character.mAlliance.AllianceId;
            var newAllianceId = msg.Request.Value;
            var oldLadder = character.mAlliance.Ladder;
            var RequestValue = newAllianceId;
            if (msg.Request.Type == 2)
            {
                RequestValue = serverId;
            }
            var teamMsg = LogicServer.Instance.TeamAgent.Logic2TeamAllianceOperation(character.mGuid, msg.Request.Type,
                RequestValue);
            yield return teamMsg.SendAndWaitUntilDone(coroutine);
            if (teamMsg.State == MessageState.Reply)
            {
                if (teamMsg.ErrorCode == (int)ErrorCodes.OK)
                {
                    if (msg.Request.Type == 2)
                    {
                        var result = LogicServer.Instance.SceneAgent.SSAllianceDataChange(proxy.CharacterId, 0, 1, "");
                        yield return result.SendAndWaitUntilDone(coroutine);

                        var titles = new List<int>();
                        var states = new List<bool>();
                        for (var i = 2000; i <= 2003; i++)
                        {
                            titles.Add(i);
                            states.Add(false);
                        }
                        if (oldAllianceId == occupantId)
                        {
                            titles.Add(5000);
                            states.Add(false);
                            titles.Add(5001);
                            states.Add(false);
                        }
                        character.ModityTitles(titles, states);
                    }
                    else if (msg.Request.Type == 3)
                    {
                        character.ModityTitle(2003, true);
                        var result = LogicServer.Instance.SceneAgent.SSAllianceDataChange(proxy.CharacterId,
                            RequestValue, 0, teamMsg.Response);
                        yield return result.SendAndWaitUntilDone(coroutine);
                        if (newAllianceId == occupantId)
                        {
                            character.ModityTitle(5001, true);
                        }
                    }
                    character.mAlliance.CheckOver(msg.Request.Type, RequestValue, msg.Response);
                }
                else if (teamMsg.ErrorCode == (int)ErrorCodes.Error_AllianceApplyJoinOK)
                {
                    character.ModityTitle(2003, true);
                    character.mAlliance.CheckOver(-1, RequestValue, msg.Response);
                    msg.Reply(teamMsg.ErrorCode);
                    var result = LogicServer.Instance.SceneAgent.SSAllianceDataChange(proxy.CharacterId, RequestValue, 0,
                        teamMsg.Response);
                    yield return result.SendAndWaitUntilDone(coroutine);
                    if (newAllianceId == occupantId)
                    {
                        character.ModityTitle(5001, true);
                    }
                    yield break;
                }
                else if (teamMsg.ErrorCode != (int)ErrorCodes.OK)
                {
                    msg.Reply(teamMsg.ErrorCode);
                    yield break;
                }
            }
            msg.Reply();
        }

        //战盟操作:其他操作     type：0=邀请加入 1=同意申请加入 2：拒绝申请加入
        public IEnumerator AllianceOperationCharacter(Coroutine coroutine,
                                                      LogicCharacterProxy charProxy,
                                                      AllianceOperationCharacterInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------AllianceOperationCharacter----------{0},{1}",
                msg.Request.Type, msg.Request.Guid);
            switch (msg.Request.Type)
            {
                case 0:
                    {
                        //if (Character.mAlliance.Ladder < 2)
                        if (Table.GetGuildAccess(proxy.Character.mAlliance.Ladder).CanAddMember == 0)
                        {
                            msg.Reply((int)ErrorCodes.Error_JurisdictionNotEnough);
                            yield break;
                        }
                        //是否在线
                        var LogicSimpleData = LogicServer.Instance.LogicAgent.GetLogicSimpleData(msg.Request.Guid, 0);
                        //var isOnlineList = LogicServer.Instance.SceneAgent.SBCheckCharacterOnline(CharacterId, templist);
                        yield return LogicSimpleData.SendAndWaitUntilDone(coroutine);
                        if (LogicSimpleData.State != MessageState.Reply)
                        {
                            msg.Reply((int)ErrorCodes.NameNotFindCharacter);
                            yield break;
                        }
                        if (LogicSimpleData.ErrorCode != (int)ErrorCodes.OK)
                        {
                            msg.Reply((int)ErrorCodes.NameNotFindCharacter);
                            yield break;
                        }
                        if (LogicSimpleData.Response.Online == 0)
                        {
                            msg.Reply((int)ErrorCodes.Unline);
                            yield break;
                        }
                        if (LogicSimpleData.Response.Level < AllianceManager.EnjoinAllianceNeedLevel)
                        {
                            msg.Reply((int)ErrorCodes.Error_LevelNoEnough);
                            yield break;
                        }
                    }
                    break;
                case 1:
                    {
                        //if (Character.mAlliance.Ladder < 3)
                        if (Table.GetGuildAccess(proxy.Character.mAlliance.Ladder).CanAddMember == 0)
                        {
                            msg.Reply((int)ErrorCodes.Error_JurisdictionNotEnough);
                            yield break;
                        }
                    }
                    break;
                case 2:
                    {
                        //if (Character.mAlliance.Ladder < 3)
                        if (Table.GetGuildAccess(proxy.Character.mAlliance.Ladder).CanAddMember == 0)
                        {
                            msg.Reply((int)ErrorCodes.Error_JurisdictionNotEnough);
                            yield break;
                        }
                    }
                    break;
            }
            //是否有这个玩家
            var LogicData = LogicServer.Instance.LoginAgent.GetLoginSimpleData(proxy.ClientId, msg.Request.Guid);
            yield return LogicData.SendAndWaitUntilDone(coroutine);
            if (LogicData.State == MessageState.Reply)
            {
                if (LogicData.ErrorCode != (int)ErrorCodes.OK)
                {
                    msg.Reply(LogicData.ErrorCode);
                    yield break;
                }
            }
            if (SceneExtension.GetServerLogicId(LogicData.Response.ServerId) !=
                SceneExtension.GetServerLogicId(proxy.Character.serverId))
            {
                msg.Reply((int)ErrorCodes.ServerNotSame);
                yield break;
            }
            //toTeam
            var teamMsg = LogicServer.Instance.TeamAgent.Logic2TeamAllianceOperationCharacter(proxy.CharacterId,
                msg.Request.Type, proxy.Character.GetName(), proxy.Character.mAlliance.AllianceId, msg.Request.Guid);
            yield return teamMsg.SendAndWaitUntilDone(coroutine);
            if (teamMsg.State == MessageState.Reply)
            {
                if (teamMsg.ErrorCode != (int)ErrorCodes.OK)
                {
                    msg.Reply(teamMsg.ErrorCode);
                    yield break;
                }
                //switch (msg.Request.Type)
                //{
                //    case 0:
                //        {
                //            //Character.Proxy.LogicSyncAllianceMessage(0, Character.GetName(), Character.mAlliance.AllianceId,LogicData.Response.Name);
                //        }
                //        break;
                //    case 1:
                //        {
                //            //Character.Proxy.LogicSyncAllianceMessage(1, Character.GetName(), Character.mAlliance.AllianceId,LogicData.Response.Name);
                //        }
                //        break;
                //    case 2:
                //        {
                //            //Character.Proxy.LogicSyncAllianceMessage(2, Character.GetName(), Character.mAlliance.AllianceId,LogicData.Response.Name);
                //        }
                //        break;
                //}
            }

            msg.Reply();
        }

        //战盟操作:其他操作     type：0=邀请加入 (暂时客户端不需要)1=同意申请加入 2：拒绝申请加入
        public IEnumerator AllianceOperationCharacterByName(Coroutine coroutine,
                                                            LogicCharacterProxy charProxy,
                                                            AllianceOperationCharacterByNameInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var type = msg.Request.Type;
            var name = msg.Request.Name;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------WorshipCharacter----------:{0},{1}", type,
                name);
            //权限检查
            switch (type)
            {
                case 0:
                    {
                        if (proxy.Character.mAlliance.Ladder < 2)
                        {
                            msg.Reply((int)ErrorCodes.Error_JurisdictionNotEnough);
                            yield break;
                        }
                    }
                    break;
                case 1:
                    {
                        if (proxy.Character.mAlliance.Ladder < 3)
                        {
                            msg.Reply((int)ErrorCodes.Error_JurisdictionNotEnough);
                            yield break;
                        }
                    }
                    break;
                case 2:
                    {
                        if (proxy.Character.mAlliance.Ladder < 3)
                        {
                            msg.Reply((int)ErrorCodes.Error_JurisdictionNotEnough);
                            yield break;
                        }
                    }
                    break;
            }
            //查询玩家

            var netMsg = LogicServer.Instance.LoginAgent.GetCharacterIdByName(proxy.ClientId, name);
            yield return netMsg.SendAndWaitUntilDone(coroutine);
            if (netMsg.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (netMsg.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(netMsg.ErrorCode);
                yield break;
            }
            if (netMsg.Response == 0)
            {
                msg.Reply((int)ErrorCodes.NameNotFindCharacter);
                yield break;
            }
            var targetId = netMsg.Response;
            //是否在线
            if (msg.Request.Type == 0)
            {
                var LogicSimpleData = LogicServer.Instance.LogicAgent.GetLogicSimpleData(targetId, 0);
                yield return LogicSimpleData.SendAndWaitUntilDone(coroutine);
                if (LogicSimpleData.State != MessageState.Reply)
                {
                    msg.Reply((int)ErrorCodes.Error_CharacterNotFind);
                    yield break;
                }
                if (LogicSimpleData.ErrorCode != (int)ErrorCodes.OK)
                {
                    msg.Reply((int)ErrorCodes.NameNotFindCharacter);
                    yield break;
                }
                if (LogicSimpleData.Response.Online == 0)
                {
                    msg.Reply((int)ErrorCodes.Unline);
                    yield break;
                }
                if (LogicSimpleData.Response.Level < AllianceManager.EnjoinAllianceNeedLevel)
                {
                    msg.Reply((int)ErrorCodes.Error_LevelNoEnough);
                    yield break;
                }
            }
            //是否有这个玩家
            var LogicData = LogicServer.Instance.LoginAgent.GetLoginSimpleData(proxy.ClientId, targetId);
            yield return LogicData.SendAndWaitUntilDone(coroutine);
            if (LogicData.State == MessageState.Reply)
            {
                if (LogicData.ErrorCode != (int)ErrorCodes.OK)
                {
                    msg.Reply(LogicData.ErrorCode);
                    yield break;
                }
            }
            if (LogicData.Response.ServerId != proxy.Character.serverId)
            {
                msg.Reply((int)ErrorCodes.ServerNotSame);
                yield break;
            }

            //toTeam
            var teamMsg = LogicServer.Instance.TeamAgent.Logic2TeamAllianceOperationCharacter(proxy.CharacterId,
                msg.Request.Type, proxy.Character.GetName(), proxy.Character.mAlliance.AllianceId, targetId);
            yield return teamMsg.SendAndWaitUntilDone(coroutine);
            if (teamMsg.State == MessageState.Reply)
            {
                if (teamMsg.ErrorCode != (int)ErrorCodes.OK)
                {
                    msg.Reply(teamMsg.ErrorCode);
                }
            }
        }

        //崇拜
        public IEnumerator WorshipCharacter(Coroutine coroutine,
                                            LogicCharacterProxy charProxy,
                                            WorshipCharacterInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var toGuid = msg.Request.Guid;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------WorshipCharacter----------{0}", toGuid);
            if (proxy.CharacterId == toGuid)
            {
                msg.Reply((int)ErrorCodes.Error_CharacterSame);
                yield break;
            }
            var exData312 = proxy.Character.GetExData((int)eExdataDefine.e312);
            if (exData312 < 1)
            {
                msg.Reply((int)ErrorCodes.Error_WorshipCount);
                yield break;
            }
            if (proxy.Character.GetData().Worships.Contains(toGuid))
            {
                //已崇拜
                msg.Reply((int)ErrorCodes.Error_WorshipAlready);
                yield break;
            }
            proxy.Character.AddExData((int)eExdataDefine.e420, 1);
            proxy.Character.SetExData((int)eExdataDefine.e312, exData312 - 1);
            msg.Response = exData312 - 1;
            var tbLevel = Table.GetLevelData(proxy.Character.GetLevel());
            if (tbLevel != null)
            {
                proxy.Character.mBag.AddRes(eResourcesType.ExpRes, tbLevel.WorshipExp, eCreateItemType.Worship);
            }
            proxy.Character.GetData().Worships.Add(toGuid);
            var tempChanges = new Dict_int_int_Data();
            tempChanges.Data[313] = 1;
            tempChanges.Data[324] = 1;
            var changeMsg = LogicServer.Instance.LogicAgent.SSChangeExdata(toGuid, tempChanges);
            yield return changeMsg.SendAndWaitUntilDone(coroutine);
            msg.Reply();
        }

        //战盟捐献
        public IEnumerator DonationAllianceItem(Coroutine coroutine,
                                                LogicCharacterProxy charProxy,
                                                DonationAllianceItemInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var type = msg.Request.Type;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------DonationAllianceItem----------{0}", type);
            var resultCodes = proxy.Character.mAlliance.CheckDonationAllianceItem(type);
            if (resultCodes != ErrorCodes.OK)
            {
                msg.Reply((int)resultCodes);
                yield break;
            }
            var changeMsg = LogicServer.Instance.TeamAgent.Logic2TeamDonationAllianceItem(proxy.Character.mGuid,
                proxy.Character.serverId, type, proxy.Character.GetName());
            yield return changeMsg.SendAndWaitUntilDone(coroutine);
            if (changeMsg.State != MessageState.Reply)
            {
                msg.Reply();
                yield break;
            }
            if (changeMsg.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(changeMsg.ErrorCode);
                yield break;
            }
            {//修改扩展计数
                proxy.Character.AddExData(952, 1);
            }
            proxy.Character.mAlliance.SuccessDonationAllianceItem(type, changeMsg.Response);
            msg.Response = changeMsg.Response;
            msg.Reply();
        }

        public IEnumerator BattleUnionDonateEquip(Coroutine coroutine,
                                                  LogicCharacterProxy charProxy,
                                                  BattleUnionDonateEquipInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var character = proxy.Character;
            var bagIndex = msg.Request.BagIndex;
            if (null == character)
            {
                msg.Reply((int)ErrorCodes.Error_CharacterNotFind);
                yield break;
            }

            //检查装备的可靠性
            var bag = proxy.Character.GetBag(0); //获得装备包裹
            ItemBase donateItem = bag.GetItemByIndex(bagIndex);
            if (null == donateItem)
            {
                msg.Reply((int)ErrorCodes.Error_ItemNotFind);
                yield break;
            }
            var itemId = donateItem.GetId();
            var tbItemBase = Table.GetItemBase(itemId);
            if (null == tbItemBase)
            {
                msg.Reply((int)ErrorCodes.Error_ItemNotFind);
                yield break;
            }
            var tbEquipBase = Table.GetEquip(itemId);
            if (null == tbEquipBase)
            {
                msg.Reply((int)ErrorCodes.Error_ItemNotFind);
                yield break;
            }
            if (tbItemBase.Quality < 3) //该装备的品质是否紫色及其以上
            {
                msg.Reply((int)ErrorCodes.Error_AllianceDepotDonateWrongQuality);
                yield break;
            }
            if (tbEquipBase.Ladder < 3) //该装备的阶数是否>=3
            {
                msg.Reply((int)ErrorCodes.Error_AllianceDepotDonateWrongLadder);
                yield break;
            }
            var isBinding = donateItem.GetExdata(23);
            if (isBinding == 1)//该装备是否绑定 1 绑定 0 未绑定
            {
                msg.Reply((int)ErrorCodes.Error_AllianceDepotDonateWrongBinding);
                yield break;
            }

            //向TeamServer发送消息通知战盟仓库添加Item
            {
                var msg1 = LogicServer.Instance.TeamAgent.SSAllianceDepotDonate(proxy.CharacterId,
                                                                                proxy.Character.serverId,
                                                                                proxy.CharacterId,
                                                                                proxy.Character.GetName(),
                                                                                donateItem.mDbData);
                yield return msg1.SendAndWaitUntilDone(coroutine);
                if (msg1.State != MessageState.Reply)
                {
                    Logger.Warn("SSAllianceDepotDonate False!");
                    msg.Reply((int)ErrorCodes.Unknow);
                    yield break;
                }
                if (msg1.ErrorCode != (int)ErrorCodes.OK)
                {
                    msg.Reply(msg1.ErrorCode);
                    yield break;
                }
            }

            //删除已捐赠的装备
            character.mBag.GetBag(0).ReduceCountByIndex(bagIndex, 1, eDeleteItemType.AllianceDepotDonate);

            //获得贡献点数
            {
                var tbItembase = Table.GetItemBase(itemId);
                if (tbItembase == null)
                {
                    msg.Reply((int)ErrorCodes.Error_ItemNotFind);
                    yield break;
                }
                character.mBag.AddRes(eResourcesType.Contribution, tbItembase.DonatePrice, eCreateItemType.AllianceDepotDonate);
            }
            {//修改扩展计数
                character.AddExData(955, 1);
            }
            msg.Reply((int)ErrorCodes.OK);
        }

        public IEnumerator BattleUnionTakeOutEquip(Coroutine coroutine,
                                                   LogicCharacterProxy charProxy,
                                                   BattleUnionTakeOutEquipInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var character = proxy.Character;
            var bagIndex = msg.Request.BagIndex;
            var itemId = msg.Request.ItemId;
            var tbItembase = Table.GetItemBase(itemId);
            if (null == tbItembase)
            {
                msg.Reply((int)ErrorCodes.Error_ItemNotFind);
                yield break;
            }
            if (null == character)
            {
                msg.Reply((int)ErrorCodes.Error_CharacterNotFind);
                yield break;
            }

            //检查是否有足够的贡献值
            var takeOutPrice = tbItembase.TakeoutPrice;
            if (character.mBag.GetRes(eResourcesType.Contribution) < takeOutPrice)
            {
                msg.Reply((int)ErrorCodes.Error_GongjiNotEnough);
                yield break;
            }

            //判断个人背包是否满
            var error = character.mBag.CheckAddItem(itemId, 1);
            if (error != ErrorCodes.OK)
            {
                msg.Reply((int)error);
                yield break;
            }

            //扣除贡献点数
            character.mBag.DelRes(eResourcesType.Contribution, tbItembase.TakeoutPrice, eDeleteItemType.AllianceDepotTakeOut);

            //向TeamServer发送消息取出装备
            var msg1 = LogicServer.Instance.TeamAgent.SSAllianceDepotTakeOut(proxy.CharacterId,
                                                                             proxy.Character.serverId,
                                                                             proxy.CharacterId,
                                                                             proxy.Character.GetName(),
                                                                             bagIndex,
                                                                             itemId);
            yield return msg1.SendAndWaitUntilDone(coroutine);
            if (msg1.State != MessageState.Reply)
            {
                Logger.Warn("SSAllianceDepotTakeOut False!");
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (msg1.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(msg1.ErrorCode);
                yield break;
            }

            var takeOutItemData = msg1.Response;
            //给玩家添加装备
            character.mBag.AddItem(takeOutItemData, eCreateItemType.AllianceDepotTakeOut);

            msg.Reply((int)ErrorCodes.OK);
        }

        //家园任务：type 0=提交 1=放弃 
        public IEnumerator CityMissionOperation(Coroutine coroutine,
                                                LogicCharacterProxy charProxy,
                                                CityMissionOperationInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var type = msg.Request.Type;
            var misIndex = msg.Request.MissIndex;

            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------CityMissionOperation----------{0},{1}", type,
                misIndex);
            switch (type)
            {
                case 0:
                    {
                        var mis = proxy.Character.mCity.GetMission(misIndex);
                        if (mis == null)
                        {
                            msg.Reply((int)ErrorCodes.Error_DataOverflow);
                            yield break;
                        }
                        var result = proxy.Character.mCity.CommitMission(misIndex, mis.MissionId);
                        msg.Response = new BagsChangeData();
                        proxy.Character.mBag.GetNetDirtyMissions(msg.Response);
                        msg.Reply((int)result);
                        yield break;
                    }
                case 1:
                    {
                        var mis = proxy.Character.mCity.GetMission(misIndex);
                        if (mis == null)
                        {
                            msg.Reply((int)ErrorCodes.Error_DataOverflow);
                            yield break;
                        }
                        var result = proxy.Character.mCity.DropMission(mis);
                        msg.Reply((int)result);
                        yield break;
                    }
                case 2:
                    {
                        var costClient = msg.Request.Cost;
                        var mis = proxy.Character.mCity.GetMission(misIndex);
                        if (mis == null)
                        {
                            msg.Reply((int)ErrorCodes.Error_DataOverflow);
                            yield break;
                        }
                        var result = proxy.Character.mCity.BuyRefreshMission(misIndex, costClient);
                        msg.Reply((int)result);
                        yield break;
                    }
            }
            msg.Reply((int)ErrorCodes.Unknow);
        }

        public IEnumerator DropCityMission(Coroutine coroutine,
                                           LogicCharacterProxy charProxy,
                                           DropCityMissionInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var misIndex = msg.Request.MissIndex;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------DropCityMission----------{0}", misIndex);
            var mis = proxy.Character.mCity.GetMission(misIndex);
            if (mis == null)
            {
                msg.Reply((int)ErrorCodes.Error_DataOverflow);
                yield break;
            }
            var result = proxy.Character.mCity.DropMission(mis);
            if (result == ErrorCodes.OK)
            {
                if (msg.Response == null)
                {
                    msg.Response = new BuildMissionOne();
                }
                msg.Response.MissionId = mis.MissionId;
                msg.Response.GiveMoney = mis.GiveMoney;
                msg.Response.GiveExp = mis.GiveExp;
                msg.Response.GiveItem = mis.GiveItem;
                msg.Response.State = mis.State;
                msg.Response.RefreshTime = mis.RefreshTime;
                msg.Response.ItemIdList.AddRange(mis.ItemIdList);
                msg.Response.ItemCountList.AddRange(mis.ItemCountList);
            }
            msg.Reply((int)result);
        }

        //家园任务刷新
        public IEnumerator CityRefreshMission(Coroutine coroutine,
                                              LogicCharacterProxy charProxy,
                                              CityRefreshMissionInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------CityRefreshMission----------:{0}",
                msg.Request.Type);
            //if (msg.Request.Type == 0)
            //{//0类型请求下时间是否到刷新
            //    ErrorCodes result = Character.mCity.CheckMissionRefresh();  
            //}//-1类型只是请求

            //全部检查下时间
            proxy.Character.mCity.CheckMissionRefresh();
            foreach (var cityMission in proxy.Character.mCity.GetCityMissions())
            {
                var temp = new BuildMissionOne
                {
                    MissionId = cityMission.MissionId,
                    GiveMoney = cityMission.GiveMoney,
                    GiveExp = cityMission.GiveExp,
                    GiveItem = cityMission.GiveItem,
                    State = cityMission.State,
                    RefreshTime = cityMission.RefreshTime
                };
                temp.ItemIdList.AddRange(cityMission.ItemIdList);
                temp.ItemCountList.AddRange(cityMission.ItemCountList);
                msg.Response.CityMissions.Add(temp);
            }
            msg.Reply();
            yield break;
        }

        //拍卖行系统：上架道具
        public IEnumerator OnItemAuction(Coroutine coroutine, LogicCharacterProxy charProxy, OnItemAuctionInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var build = proxy.Character.mCity.GetBuildByType((int)BuildingType.Exchange);
            if (build == null)
            {
                msg.Reply((int)ErrorCodes.Error_BuildNotFind);
                yield break;
            }
            var needType = msg.Request.Type;
            var bagId = msg.Request.BagId;
            var bagIndex = msg.Request.BagIndex;
            var count = msg.Request.Count;
            var needCount = msg.Request.NeedCount;
            var storeIndex = msg.Request.StoreIndex;
            PlayerLog.WriteLog(proxy.CharacterId,
                "----------Logic----------OnItemAuction----------{0},{1},{2},{3},{4},{5}", needType, bagId, bagIndex,
                count, needCount, storeIndex);
            ExchangeItem resultItem = null;
            var result = proxy.Character.mExchange.PushItem(-1, bagId, bagIndex, count, needType, needCount, storeIndex,
                ref resultItem);
            if (result == ErrorCodes.OK)
            {
                var ExchangeMsg = LogicServer.Instance.TeamAgent.SSOnItemAuction(proxy.CharacterId,
                    proxy.Character.serverId, proxy.CharacterId, proxy.Character.GetName(), resultItem.mDbdata.ItemData,
                    needType, needCount, resultItem.mDbdata.Id);
                yield return ExchangeMsg.SendAndWaitUntilDone(coroutine, new TimeSpan(0, 0, 5));
                if (ExchangeMsg.State != MessageState.Reply || ExchangeMsg.ErrorCode != (int)ErrorCodes.OK)
                {
                    Logger.Error(ExchangeMsg.ErrorCode);
                    msg.Reply(ExchangeMsg.ErrorCode);
                    yield break;
                }
                resultItem.mDbdata.ManagerId = ExchangeMsg.Response;
                resultItem.mDbdata.ManagerOverTime = DateTime.Now.AddHours(StaticParam.AuctionTime).ToBinary();
            }
            msg.Reply((int)result);
        }

        //交易系统：上架道具
        public IEnumerator StoreOperationAdd(Coroutine coroutine,
                                             LogicCharacterProxy charProxy,
                                             StoreOperationAddInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var build = proxy.Character.mCity.GetBuildByType((int)BuildingType.Exchange);
            if (build == null)
            {
                msg.Reply((int)ErrorCodes.Error_BuildNotFind);
                yield break;
            }
            var type = msg.Request.Type;
            var bagId = msg.Request.BagId;
            var bagIndex = msg.Request.BagIndex;
            var count = msg.Request.Count;
            var needCount = msg.Request.NeedCount;
            var storeIndex = msg.Request.StoreIndex;
            PlayerLog.WriteLog(proxy.CharacterId,
                "----------Logic----------StoreOperationAdd----------{0},{1},{2},{3},{4},{5}", type, bagId, bagIndex,
                count, needCount, storeIndex);
            ExchangeItem resultItem = null;
            var result = proxy.Character.mExchange.PushItem(type, bagId, bagIndex, count, 0, needCount, storeIndex,
                ref resultItem);
            if (result == ErrorCodes.OK)
            {
                proxy.Character.AddExData((int)eExdataDefine.e338, 1);
                proxy.Character.AddExData((int)eExdataDefine.e418, 1);
                switch (type)
                {
                    case 0:
                        resultItem.mDbdata.ManagerId = 0;
                        resultItem.mDbdata.ManagerOverTime = 0;
                        break;
                    case 1:
                    case 2:
                        var broadcastMinutes = proxy.Character.mExchange.GetBroadcastMinutes();
                        var ExchangeMsg = LogicServer.Instance.TeamAgent.BroadcastExchangeItem(proxy.CharacterId,
                            proxy.CharacterId, proxy.Character.GetName(), resultItem.mDbdata.ItemData, needCount,
                            broadcastMinutes);
                        yield return ExchangeMsg.SendAndWaitUntilDone(coroutine);
                        if (ExchangeMsg.State != MessageState.Reply)
                        {
                            break;
                        }
                        if (ExchangeMsg.ErrorCode != (int)ErrorCodes.OK)
                        {
                            break;
                        }
                        resultItem.mDbdata.ManagerId = ExchangeMsg.Response;
                        resultItem.mDbdata.ManagerOverTime = DateTime.Now.AddMinutes(broadcastMinutes).ToBinary();
                        //proxy.Character.mCity.CityAddExp(StaticParam.ExchangeExp);
                        break;
                }
                msg.Response = resultItem.mDbdata.Id;
            }
            msg.Reply((int)result);
        }

        //交易系统：广播道具
        public IEnumerator StoreOperationBroadcast(Coroutine coroutine,
                                                   LogicCharacterProxy charProxy,
                                                   StoreOperationBroadcastInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var build = proxy.Character.mCity.GetBuildByType((int)BuildingType.Exchange);
            if (build == null)
            {
                msg.Reply((int)ErrorCodes.Error_BuildNotFind);
                yield break;
            }
            var type = msg.Request.Type;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------StoreOperationBroadcast----------{0}", type);
            if (build.TbBs == null)
            {
                msg.Reply((int)ErrorCodes.Error_BuildNotService);
                yield break;
            }
            var lookCount = build.TbBs.Param[1];
            //先看缓存
            if (type == 0)
            {
                var ChechTemp = proxy.Character.mExchange.GetChech(lookCount);
                if (ChechTemp != null)
                {
                    foreach (var item in ChechTemp.Items)
                    {
                        msg.Response.Items.Add(item);
                    }
                    msg.Response.CacheOverTime = proxy.Character.mExchange.ChechOverTime.ToBinary();
                    msg.Reply();
                    yield break;
                }
            }
            //钱是否够
            if (type == 1)
            {
                var cast = Table.GetServerConfig(305).ToInt();
                if (proxy.Character.mBag.GetRes(eResourcesType.DiamondRes) < cast)
                {
                    msg.Reply((int)ErrorCodes.DiamondNotEnough);
                    yield break;
                }
                proxy.Character.mBag.DelRes(eResourcesType.DiamondRes, cast, eDeleteItemType.ExchangeRefresh);
            }
            //从远端获取
            var ExchangeMsg = LogicServer.Instance.TeamAgent.GetExchangeItem(proxy.CharacterId, proxy.CharacterId,
                proxy.Character.GetLevel(), lookCount, type);
            yield return ExchangeMsg.SendAndWaitUntilDone(coroutine);
            if (ExchangeMsg.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (ExchangeMsg.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(ExchangeMsg.ErrorCode);
                yield break;
            }
            proxy.Character.mExchange.ChechTemp(ExchangeMsg.Response);
            foreach (var item in ExchangeMsg.Response.Items)
            {
                msg.Response.Items.Add(item);
            }
            msg.Response.CacheOverTime = proxy.Character.mExchange.ChechOverTime.ToBinary();
            msg.Reply();
        }

        //拍卖行系统：购买道具
        public IEnumerator BuyItemAuction(Coroutine coroutine,
                                          LogicCharacterProxy charProxy,
                                          BuyItemAuctionInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var targetId = msg.Request.CharacterId;
            var storeId = msg.Request.ManagerId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------BuyItemAuction----------{0},{1}", targetId,
                storeId);
            //查询道具是否存在
            var teamItemSelect = LogicServer.Instance.TeamAgent.SSSelectItemAuction(targetId, proxy.Character.serverId,
                targetId, storeId);
            yield return teamItemSelect.SendAndWaitUntilDone(coroutine);
            if (teamItemSelect.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (teamItemSelect.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(teamItemSelect.ErrorCode);
                yield break;
            }
            if (teamItemSelect.Response == -1)
            {
                msg.Reply((int)ErrorCodes.Error_ItemNotFind);
                yield break;
            }
            var targetServerId = teamItemSelect.Response;
            //检查玩家是否存在
            var dbLogicSimple = LogicServer.Instance.LogicAgent.GetLogicSimpleData(targetId, 0);
            yield return dbLogicSimple.SendAndWaitUntilDone(coroutine);

            if (dbLogicSimple.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (dbLogicSimple.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(dbLogicSimple.ErrorCode);
                yield break;
            }
            //这里没有检查targetId玩家的ServerId是否满足
            //道具是否存在检查
            OtherStoreOne tempOne = null;
            var guid = msg.Request.Guid;
            foreach (var item in dbLogicSimple.Response.Exchange.Items)
            {
                if (item.Id == guid)
                {
                    if (item.State == (int)StoreItemType.Free)
                    {
                        msg.Reply((int)ErrorCodes.Error_ItemNotFind);
                        yield break;
                    }
                    if (item.State == (int)StoreItemType.Buyed)
                    {
                        msg.Reply((int)ErrorCodes.Error_ExchangeItemState);
                        yield break;
                    }
                    tempOne = item;
                    break;
                }
            }
            if (tempOne == null)
            {
                msg.Reply((int)ErrorCodes.Error_ItemNotFind);
                yield break;
            }


            //资源检查
            var old = 0;
            var resid = 0;
            if (tempOne.NeedType == 0 || tempOne.NeedType == 10) // 0是  交易所  10 是拍卖行
            {
                resid = (int)eResourcesType.Other16;
                old = proxy.Character.mBag.mDbData.Resources[resid];
                if (old < tempOne.NeedCount)
                {
                    msg.Reply((int)ErrorCodes.Error_ResNoEnough);
                    yield break;
                }
                proxy.Character.mBag.mDbData.Resources[resid] = old - tempOne.NeedCount;
            }
            else if (tempOne.NeedType == 1 || tempOne.NeedType == 11) // 1是  交易所  11 是拍卖行
            {
                resid = (int)eResourcesType.DiamondRes;
                old = proxy.Character.mBag.mDbData.Resources[resid];
                if (old < tempOne.NeedCount)
                {
                    msg.Reply((int)ErrorCodes.Error_ResNoEnough);
                    yield break;
                }
                proxy.Character.mBag.mDbData.Resources[resid] = old - tempOne.NeedCount;
            }
            else
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            //远程购买
            var LogicBuy = LogicServer.Instance.LogicAgent.SSStoreOperationBuy(targetId, guid, proxy.CharacterId,
                proxy.Character.GetName(), resid, tempOne.NeedCount, tempOne.ItemData);
            yield return LogicBuy.SendAndWaitUntilDone(coroutine);
            if (LogicBuy.State != MessageState.Reply)
            {
                old = proxy.Character.mBag.mDbData.Resources[resid];
                proxy.Character.mBag.mDbData.Resources[resid] = old + tempOne.NeedCount;
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (LogicBuy.ErrorCode != (int)ErrorCodes.OK)
            {
                old = proxy.Character.mBag.mDbData.Resources[resid];
                proxy.Character.mBag.mDbData.Resources[resid] = old + tempOne.NeedCount;
                msg.Reply(LogicBuy.ErrorCode);
                yield break;
            }
            //增加道具
            if (proxy.Character.mBag.CheckAddItem(tempOne.ItemData.ItemId, tempOne.ItemData.Count) == ErrorCodes.OK)
            {
                proxy.Character.mBag.AddItem(tempOne.ItemData, eCreateItemType.ExchangeBuy);
            }
            else
            {
                var tbMail = Table.GetMail(117);
                var mail = proxy.Character.mMail.PushMail(tbMail.Title, tbMail.Text,
                    new List<ItemBaseData> { tempOne.ItemData });
                mail.Send = tbMail.Sender;
            }

            proxy.Character.AddExData((int)eExdataDefine.e339, 1);
            var his = proxy.Character.mDbData.SellHistory;
            if (his == null)
            {
                his = new SellHistoryList();
                proxy.Character.mDbData.SellHistory = his;
            }
            his.items.Add(new SellHistoryOne
            {
                sellTime = DateTime.Now.ToBinary(),
                ItemData = tempOne.ItemData,
                buyCharacterId = targetId,
                buyCharacterName = dbLogicSimple.Response.Exchange.SellCharacterName,
                resType = resid,
                resCount = tempOne.NeedCount,
                type = 1
            });


            msg.Reply();
            if (tempOne.NeedType < 10)
            {
                if (tempOne.ManagerId != 0)
                {
                    var ExchangeMsg = LogicServer.Instance.TeamAgent.CancelExchangeItem(proxy.CharacterId, targetId,
                        tempOne.ManagerId);
                    yield return ExchangeMsg.SendAndWaitUntilDone(coroutine);
                }
            }
            else
            {
                var ExchangeMsg = LogicServer.Instance.TeamAgent.SSDownItemAuction(targetId, targetServerId, targetId,
                    tempOne.ManagerId);
                yield return ExchangeMsg.SendAndWaitUntilDone(coroutine);
            }
        }

        //请求交易所的交易历史
        public IEnumerator ApplySellHistory(Coroutine coroutine,
                                            LogicCharacterProxy charProxy,
                                            ApplySellHistoryInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            if (proxy.Character.mDbData.SellHistory == null)
            {
                msg.Reply();
                yield break;
            }
            foreach (var item in proxy.Character.mDbData.SellHistory.items)
            {
                msg.Response.items.Add(item);
            }
            msg.Reply();
        }
        [Updateable("LogicProxy")]
        //许愿池初始数据
        public static List<DropMotherRecord>[] s_WishPoolData = null;
        //         {
        //             Table.GetDropMother(Table.GetServerConfig(250).ToInt()),
        //             Table.GetDropMother(Table.GetServerConfig(251).ToInt()),
        //             Table.GetDropMother(Table.GetServerConfig(252).ToInt()),
        //             Table.GetDropMother(Table.GetServerConfig(253).ToInt()),
        //             Table.GetDropMother(Table.GetServerConfig(254).ToInt()),
        //             Table.GetDropMother(Table.GetServerConfig(255).ToInt()),
        //             Table.GetDropMother(Table.GetServerConfig(256).ToInt()),
        //             Table.GetDropMother(Table.GetServerConfig(257).ToInt())
        //         };

        #region 灭世相关协议

        public IEnumerator ApplyGetTowerReward(Coroutine coroutine, LogicCharacterProxy charProxy,
            ApplyGetTowerRewardInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;


            var tb = Table.GetMieshiTowerReward(msg.Request.Idx);
            if (tb == null)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            var rewardID = tb.StepReward;
            var tb_item = Table.GetItemBase(rewardID);


            var errorResult = proxy.Character.mBag.mBags[tb_item.InitInBag].CheckAddItem(rewardID, 1);
            if (errorResult != ErrorCodes.OK)
            {
                msg.Reply((int)errorResult);
                yield break;
            }
            var serverId = SceneExtension.GetServerLogicId(msg.Request.ServerId);
            var timesMsg = LogicServer.Instance.ActivityAgent.SSAskMieshiTowerReward(proxy.CharacterId, serverId, msg.Request.ActivityId, charProxy.CharacterId, msg.Request.Idx);
            yield return timesMsg.SendAndWaitUntilDone(coroutine);
            if (timesMsg.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (timesMsg.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(timesMsg.ErrorCode);
                yield break;
            }

            proxy.Character.mBag.AddItem(rewardID, 1, eCreateItemType.PromoteBatteryAward);
            msg.Response = timesMsg.Response;
            msg.Reply((int)ErrorCodes.OK);
            yield break;
        }
        //请求提升炮台血量
        public IEnumerator ApplyPromoteHP(Coroutine coroutine, LogicCharacterProxy charProxy, ApplyPromoteHPInMessage msg)
        {

            var proxy = (LogicProxy)charProxy;
            var timesMsg = LogicServer.Instance.ActivityAgent.SSAskMieshiTowerUpTimes(proxy.CharacterId, msg.Request.ServerId, msg.Request.ActivityId, charProxy.CharacterId);
            yield return timesMsg.SendAndWaitUntilDone(coroutine);
            if (timesMsg.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            var times = timesMsg.Response + 1;


            MieshiTowerRewardRecord tb = null;
            Table.ForeachMieshiTowerReward((record) =>
            {
                if (record.TimesStep.Count == 2)
                {
                    tb = record;
                    return !(record.TimesStep[0] <= times && record.TimesStep[1] >= times);
                }
                return true;
            });

            if (tb == null)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            int free = proxy.Character.GetExData((int)eExdataDefine.e672);
            if (proxy.Character.mBag.GetRes(eResourcesType.DiamondRes) < tb.DiamondCost && free <= 0)
            {
                msg.Reply((int)ErrorCodes.DiamondNotEnough);
                yield break;
            }
            var serverId = SceneExtension.GetServerLogicId(msg.Request.ServerId);
            //通知活动服务器改变炮台属性
            var retMsg = LogicServer.Instance.ActivityAgent.SSApplyPromoteHP(charProxy.CharacterId,
                serverId, msg.Request.ActivityId, msg.Request.BatteryId, msg.Request.PromoteType, proxy.CharacterId, proxy.Character.GetName());
            yield return retMsg.SendAndWaitUntilDone(coroutine);
            if (retMsg.State != MessageState.Reply)
            {
                Logger.Error("IsActivityCanEnter() return with state = {0}", retMsg.State);
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (retMsg.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Error("IsActivityCanEnter() return with ErrorCode = {0}", retMsg.ErrorCode);
                msg.Reply(retMsg.ErrorCode);
                yield break;
            }

            msg.Response = retMsg.Response;
            int var = Table.GetServerConfig(1306).ToInt();//
            proxy.Character.mBag.AddRes(eResourcesType.MieshiScore, tb.DiamondCost * var,
                 eCreateItemType.PromoteBatteryAward);
            //扣除消耗
            if (free <= 0)
            {
                proxy.Character.mBag.DelRes(eResourcesType.DiamondRes, tb.DiamondCost, eDeleteItemType.PromoteBattery);
                //获得奖励
                proxy.Character.mBag.AddItem(tb.OnceReward, 1, eCreateItemType.PromoteBatteryAward);
            }
            else
            {
                proxy.Character.SetExData((int)eExdataDefine.e672, free - 1);
            }

            msg.Reply((int)ErrorCodes.OK);
            yield break;
        }

        public IEnumerator ApplyPromoteSkill(Coroutine coroutine, LogicCharacterProxy charProxy,
            ApplyPromoteSkillInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var type = msg.Request.PromoteType;
            var batteryId = msg.Request.BatteryId;
            var tbMieShiPublic = Table.GetMieShiPublic(1);

            if (type == 0)
            {
                var cast = tbMieShiPublic.CostNum;
                if (proxy.Character.mBag.GetRes(eResourcesType.DiamondRes) < cast)
                {
                    msg.Reply((int)ErrorCodes.DiamondNotEnough);
                    yield break;
                }
            }
            else
            {
                var cast = tbMieShiPublic.ItemNum;
                if (proxy.Character.mBag.GetItemCount(tbMieShiPublic.ItemId) < cast)
                {
                    msg.Reply((int)ErrorCodes.ItemNotEnough);
                    yield break;
                }
            }

            var serverId = SceneExtension.GetServerLogicId(msg.Request.ServerId);
            //通知场景服务器，查询NPC的血量
            var tbMieShi = Table.GetMieShi(msg.Request.ActivityId);
            var sceneMsg = LogicServer.Instance.SceneAgent.SSApplyNpcHP(proxy.CharacterId, serverId, tbMieShi.FuBenID, msg.Request.BatteryGuid);
            yield return sceneMsg.SendAndWaitUntilDone(coroutine);
            if (sceneMsg.State != MessageState.Reply)
            {
                Logger.Error("SSApplyNpcHP() return with state = {0}", sceneMsg.State);
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            // 如果血量为0，表示炮台已被摧毁  && 不是活动未开始时候的升级
            if (sceneMsg.Response <= 0 && sceneMsg.ErrorCode != (int)ErrorCodes.Error_NoScene)
            {
                msg.Reply((int)ErrorCodes.Error_MieShi_BatteryDestory);
                yield break;
            }

            //通知活动服务器改变炮台属性
            var retMsg = LogicServer.Instance.ActivityAgent.SSApplyPromoteSkill(charProxy.CharacterId,
                serverId, msg.Request.ActivityId, msg.Request.BatteryId, msg.Request.PromoteType, proxy.CharacterId, proxy.Character.GetName());
            yield return retMsg.SendAndWaitUntilDone(coroutine);
            if (retMsg.State != MessageState.Reply)
            {
                Logger.Error("SSApplyPromoteSkill() return with state = {0}", retMsg.State);
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (retMsg.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Error("SSApplyPromoteSkill() return with ErrorCode = {0}", retMsg.ErrorCode);
                msg.Reply(retMsg.ErrorCode);
                yield break;
            }

            msg.Response = retMsg.Response;
            //扣除消耗
            if (type == 0)
            {
                proxy.Character.mBag.DelRes(eResourcesType.DiamondRes, tbMieShiPublic.CostNum, eDeleteItemType.PromoteBattery);
            }
            else
            {
                proxy.Character.mBag.DeleteItem(tbMieShiPublic.ItemId, tbMieShiPublic.ItemNum, eDeleteItemType.PromoteBattery);
            }

            //获得奖励
            proxy.Character.mBag.AddItem(tbMieShiPublic.PromoteAwardId, 1, eCreateItemType.PromoteBatteryAward);
            msg.Reply((int)ErrorCodes.OK);
            yield break;
        }

        public IEnumerator ApplyPickUpBox(Coroutine coroutine, LogicCharacterProxy _this, ApplyPickUpBoxInMessage msg)
        {
            var proxy = (LogicProxy)_this;
            var serverId = SceneExtension.GetServerLogicId(msg.Request.ServerId);
            var activityId = msg.Request.ActivityId;
            var npcId = msg.Request.NpcId;
            var tbMieShi = Table.GetMieShi(activityId);
            if (null == tbMieShi)
            {
                msg.Reply((int)ErrorCodes.Error_MieShi_Config);
                yield break;
            }

            var activityMsg = LogicServer.Instance.ActivityAgent.SSApplyPickUpBox(proxy.CharacterId, serverId, activityId, npcId);
            yield return activityMsg.SendAndWaitUntilDone(coroutine);
            if (activityMsg.State != MessageState.Reply)
            {
                Logger.Error("SSApplyPickUpBox() return with state = {0}", activityMsg.State);
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (activityMsg.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Error("SSApplyPickUpBox() return with ErrorCode = {0}", activityMsg.ErrorCode);
                msg.Reply(activityMsg.ErrorCode);
                yield break;
            }

            //获得奖励
            proxy.Character.mBag.AddItem(tbMieShi.BoxAwardId, 1, eCreateItemType.MieShiBossAward);
            msg.Reply((int)ErrorCodes.OK);
            yield break;
        }

        public IEnumerator ApplyJoinActivity(Coroutine coroutine, LogicCharacterProxy _this,
            ApplyJoinActivityInMessage msg)
        {
            var proxy = (LogicProxy)_this;
            var serverId = SceneExtension.GetServerLogicId(msg.Request.ServerId);
            var activityId = msg.Request.ActivityId;
            var tbActive = Table.GetMieShi(activityId);
            if (tbActive != null)
            {
                var tbFuben = Table.GetFuben(tbActive.FuBenID);
                if (tbFuben != null)
                {
                    var tbScene = Table.GetScene(tbFuben.SceneId);
                    if (tbScene != null && proxy.Character.GetLevel() < tbScene.LevelLimit)
                    {
                        msg.Reply((int)ErrorCodes.Error_LevelNoEnough);
                        yield break;
                    }
                }
            }
            //Todo 改成配置


            var activityMsg = LogicServer.Instance.ActivityAgent.SSApplyJoinActivity(proxy.CharacterId, serverId, activityId);
            yield return activityMsg.SendAndWaitUntilDone(coroutine);
            if (activityMsg.State != MessageState.Reply)
            {
                Logger.Error("SSApplyJoinActivity() return with state = {0}", activityMsg.State);
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (activityMsg.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Error("SSApplyJoinActivity() return with ErrorCode = {0}", activityMsg.ErrorCode);
                msg.Reply(activityMsg.ErrorCode);
                yield break;
            }
            msg.Reply((int)ErrorCodes.OK);
            yield break;
        }

        public IEnumerator ApplyPortraitAward(Coroutine coroutine, LogicCharacterProxy _this,
            ApplyPortraitAwardInMessage msg)
        {
            var proxy = (LogicProxy)_this;
            var serverId = SceneExtension.GetServerLogicId(msg.Request.ServerId);
            var tbMieShiPublic = Table.GetMieShiPublic(1);
            if (null == tbMieShiPublic)
            {
                msg.Reply((int)ErrorCodes.Error_MieShi_Config);
                yield break;
            }

            var activityMsg = LogicServer.Instance.ActivityAgent.SSApplyPortraitAward(proxy.CharacterId, serverId);
            yield return activityMsg.SendAndWaitUntilDone(coroutine);
            if (activityMsg.State != MessageState.Reply)
            {
                Logger.Error("SSApplyPortraitAward() return with state = {0}", activityMsg.State);
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (activityMsg.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Error("SSApplyPortraitAward() return with ErrorCode = {0}", activityMsg.ErrorCode);
                msg.Reply(activityMsg.ErrorCode);
                yield break;
            }

            //获得奖励
            proxy.Character.mBag.AddItem(tbMieShiPublic.WorshipItemId, tbMieShiPublic.WorshipItemNum, eCreateItemType.MieShiPortraitAward);
            msg.Response = 1;
            msg.Reply((int)ErrorCodes.OK);
            yield break;
        }
        #endregion


        public IEnumerator DrawWishItem(Coroutine coroutine, LogicCharacterProxy charProxy, DrawWishItemInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var Character = proxy.Character;
            if (null == proxy)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            if (null == proxy.Character)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            //初始化
            if (null == s_WishPoolData)
            {
                s_WishPoolData = new List<DropMotherRecord>[3];
                for (int i = 0; i < 3; i++)
                {
                    s_WishPoolData[i] = new List<DropMotherRecord>();

                    var str = Table.GetServerConfig(250 + i).Value.Trim();
                    if (string.IsNullOrEmpty(str))
                    {
                        yield return null;
                    }
                    var dropMotherIds = str.Split('|');
                    foreach (var id in dropMotherIds)
                    {
                        var tb = Table.GetDropMother(int.Parse(id));
                        if (null == tb)
                        {
                            Logger.Error("Error::Table.GetDropMother(int.Parse(id))");
                            continue;
                        }
                        s_WishPoolData[i].Add(tb);
                    }
                }
            }


            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------DrawWishItem----------{0}", msg.Request.Param.Items.GetDataString());

            var param = msg.Request.Param.Items;
            var resultValue = new DrawWishItemResult();
            msg.Response = resultValue;

            //int typeCount = s_WishPoolData.Count;//抽奖类型总数
            int freeTimeInterval = Table.GetServerConfig(929).ToInt();//free时间间隔

            if (param.Count < 2)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }


            var drops = new List<DropMotherRecord>();
            var chatList = new List<int>();
            var value = param[1];


            /*
             var passCount = 0;
            int reduceCount = 0;
            var vipLevel = Character.mBag.GetRes(eResourcesType.VipLevel);
            Table.ForeachVIP((tb) =>
            {
                if (vipLevel < tb.Id)
                {
                    return false;
                }
                reduceCount = Math.Max(tb.WishPoolFilterNum, reduceCount);
                return true;
            });
            for (var i = 0; i != typeCount; ++i)
            {
                if (BitFlag.GetLow(value, i) && reduceCount > 0)
                {
                    passCount++;
                    reduceCount--;
                }
                else
                {
                    drops.Add(s_WishPoolData[i]);
                }
            }*/

            var role = proxy.Character.GetRole();
            if (role >= 0 && role < s_WishPoolData.Length)
            {
                drops = s_WishPoolData[role];
            }
            if (drops.Count < 1)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            var motherDropId = 200;
            switch (param[0])
            {
                case 100:
                    {
                        //许愿池单抽免费
                        var bag = Character.GetBag((int)eBagType.WishingPool);
                        if (bag.GetFreeCount() < 1)
                        {
                            msg.Reply((int)ErrorCodes.Error_ItemNoInBag_All);
                            yield break;
                        }
                        if (Character.lExdata64.GetTime(Exdata64TimeType.FreeWishingTime) < DateTime.Now)
                        {
                            //基础数量
                            Character.lExdata64.SetTime(Exdata64TimeType.FreeWishingTime, DateTime.Now.AddMinutes(freeTimeInterval));
                        }
                        else
                        {
                            msg.Reply((int)ErrorCodes.Error_TimeNotOver);
                            yield break;
                        }

                        var exdataCount = Character.GetExData((int)eExdataDefine.e270);
                        //第一次抽奖
                        //if (exdataCount > 0)
                        //{
                        motherDropId = drops.Range().Id;
                        //}
                        //else
                        //{
                        //    motherDropId = Table.GetServerConfig(285).ToInt();
                        //    if (0 == Character.GetRole())
                        //    {
                        //        motherDropId = Table.GetServerConfig(285).ToInt();
                        //    }
                        //    else if (1 == Character.GetRole())
                        //    {
                        //        motherDropId = Table.GetServerConfig(286).ToInt();
                        //    }
                        //    else if (2 == Character.GetRole())
                        //    {
                        //        motherDropId = Table.GetServerConfig(287).ToInt();
                        //    }

                        //}

                        var itemList = new Dictionary<int, int>();
                        Character.DropMother(motherDropId, itemList);
                        if (itemList.Count != 1)
                        {
                            Logger.Warn("WishingPool itemCount is {0}", itemList.Count);
                            itemList.Clear();
                            itemList[22000] = 1;
                        }
                        foreach (var i in itemList)
                        {
                            Character.mBag.AddItemToWishingPool(i.Key, i.Value, resultValue.Items);
                            chatList.Add(i.Key);
                        }
                        resultValue.Data64.Add(DateTime.Now.AddMinutes(freeTimeInterval).ToBinary());

                        Character.AddExData((int)eExdataDefine.e270, 1);
                        Character.AddExData((int)eExdataDefine.e479, 1);
                        //潜规则引导标记位
                        if (!Character.GetFlag(502))
                        {
                            Character.SetFlag(502);
                            Character.SetFlag(501, false);
                        }
                    }
                    break;
                case 101:
                    {
                        //许愿池单抽收费
                        var bag = Character.GetBag((int)eBagType.WishingPool);
                        if (bag.GetFreeCount() < 1)
                        {
                            msg.Reply((int)ErrorCodes.Error_ItemNoInBag_All);
                            yield break;
                        }
                        var needResId = Table.GetServerConfig(213).ToInt();
                        var needResCount = Table.GetServerConfig(214).ToInt();
                        if (Character.mBag.GetItemCount(needResId) < needResCount)
                        {
                            msg.Reply((int)ErrorCodes.ItemNotEnough);
                            yield break;
                        }
                        //消耗
                        Character.mBag.DeleteItem(needResId, needResCount, eDeleteItemType.DrawWishingPool);

                        //第一次抽奖
                        var exdataCount = Character.GetExData((int)eExdataDefine.e270);
                        //if (exdataCount > 0)
                        //{
                        motherDropId = drops.Range().Id;
                        //}
                        //else
                        //{
                        //    motherDropId = Table.GetServerConfig(285).ToInt();
                        //    if (0 == Character.GetRole())
                        //    {
                        //        motherDropId = Table.GetServerConfig(285).ToInt();
                        //    }
                        //    else if (1 == Character.GetRole())
                        //    {
                        //        motherDropId = Table.GetServerConfig(286).ToInt();
                        //    }
                        //    else if (2 == Character.GetRole())
                        //    {
                        //        motherDropId = Table.GetServerConfig(287).ToInt();
                        //    }
                        //}

                        var itemList = new Dictionary<int, int>();
                        Character.DropMother(motherDropId, itemList);
                        if (itemList.Count != 1)
                        {
                            Logger.Warn("WishingPool itemCount is {0}", itemList.Count);
                            itemList.Clear();
                            itemList[22000] = 1;
                        }
                        foreach (var i in itemList)
                        {
                            Character.mBag.AddItemToWishingPool(i.Key, i.Value, resultValue.Items);
                            chatList.Add(i.Key);
                        }
                        resultValue.Data64.Add(DateTime.Now.ToBinary());

                        Character.AddExData((int)eExdataDefine.e270, 1);
                        Character.AddExData((int)eExdataDefine.e479, 1);
                    }
                    break;
                case 102:
                    {
                        //许愿池10连
                        var bag = Character.GetBag((int)eBagType.WishingPool);
                        if (bag.GetFreeCount() < 10)
                        {
                            msg.Reply((int)ErrorCodes.Error_ItemNoInBag_All);
                            yield break;
                        }
                        var needResId = Table.GetServerConfig(215).ToInt();
                        var needResCount = Table.GetServerConfig(216).ToInt();
                        if (Character.mBag.GetItemCount(needResId) < needResCount)
                        {
                            msg.Reply((int)ErrorCodes.ItemNotEnough);
                            yield break;
                        }
                        //消耗
                        Character.mBag.DeleteItem(needResId, needResCount, eDeleteItemType.DrawWishingPool);
                        //var exdataCount = Character.GetExData((int)eExdataDefine.e270);
                        var itemList = new Dictionary<int, int>();
                        for (var i = 0; i != 10; ++i)
                        {
                            //if (exdataCount > 0)
                            //{
                            motherDropId = drops.Range().Id;
                            //}
                            //else
                            //{
                            //    motherDropId = Table.GetServerConfig(285).ToInt();
                            //    if (0 == Character.GetRole())
                            //    {
                            //        motherDropId = Table.GetServerConfig(285).ToInt();
                            //    }
                            //    else if (1 == Character.GetRole())
                            //    {
                            //        motherDropId = Table.GetServerConfig(286).ToInt();
                            //    }
                            //    else if (2 == Character.GetRole())
                            //    {
                            //        motherDropId = Table.GetServerConfig(287).ToInt();
                            //    }
                            //}
                            //motherDropId = Table.GetServerConfig(211).ToInt();
                            //exdataCount++;
                            Character.DropMother(motherDropId, itemList);
                            if (itemList.Count != 1)
                            {
                                Logger.Warn("WishingPool itemCount is {0}", itemList.Count);
                                itemList.Clear();
                                itemList[22000] = 1;
                            }
                            foreach (var j in itemList)
                            {
                                Character.mBag.AddItemToWishingPool(j.Key, j.Value, resultValue.Items);
                                chatList.Add(j.Key);
                            }
                            itemList.Clear();
                        }
                        resultValue.Data64.Add(DateTime.Now.ToBinary());
                        Character.AddExData((int)eExdataDefine.e270, 1);
                        Character.AddExData((int)eExdataDefine.e479, 10);
                    }
                    break;
            }
            var idIndex = 0;
            var serverId = SceneExtension.GetServerLogicId(Character.serverId);
            foreach (var i in resultValue.Items)
            {
                var itemId = resultValue.Items[idIndex].ItemId;
                var tbBaseItem = Table.GetItemBase(itemId);
                if (null == tbBaseItem)
                {
                    continue;
                }
                //if (tbBaseItem.Quality < 3)
                //{
                //    continue;	
                //}
                if (tbBaseItem.WishBroadcast == 1)
                {
                    var strs = new List<string>
			     	{
                        //许愿中的人名需要客户端点击互动
				        Utils.AddCharacter(Character.mGuid,Character.GetName()),
					    Utils.AddItemId(itemId)
				    };
                    var exData = new List<int>(resultValue.Items[idIndex].Exdata);
                    var content = Utils.WrapDictionaryId(300417, strs, exData);
                    var chatAgent = LogicServer.Instance.ChatAgent;
                    chatAgent.BroadcastWorldMessage((uint)serverId, (int)eChatChannel.WishingGroup, 0, string.Empty,
                        new ChatMessageContent { Content = content });
                }

                idIndex++;
            }
            chatList.Clear();

            msg.Reply((int)ErrorCodes.OK);
        }

        public IEnumerator ApplyOperationActivity(Coroutine coroutine, LogicCharacterProxy charProxy, ApplyOperationActivityInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            if (null == proxy)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            var character = proxy.Character;
            if (null == character)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (null == character.mOperActivity)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            var role = character.GetRole();

            var msgTerms = msg.Response.Terms;

            foreach (var kv in character.mOperActivity.DictActivity)
            {
                var act = kv.Value;

                var tb = Table.GetOperationActivity(act.Id);
                if (null == tb)
                {
                    continue;
                }
                //新增逻辑  到期的不显示
                if (!(DateTime.Now >= act.StartTime && DateTime.Now < act.EndTime))
                {
                    continue;
                }
                var msgTerm = new MsgOperActivtyTerm();
                msgTerm.Id = act.Id;
                msgTerm.Name = tb.Name;
                msgTerm.Desc = tb.Desc;
                msgTerm.BkgIconId = tb.BkgIconId;
                msgTerm.SmallIcon = tb.SmallIcon;
                msgTerm.ParentTypeId = tb.ParentTypeId;
                msgTerm.RedDotLimit = tb.RedDotShows;
                msgTerm.UIType = tb.UIType;
                msgTerm.Type = (int)act.Type;
                msgTerm.StarTime = act.StartTime.ToBinary();
                msgTerm.EndTime = act.EndTime.ToBinary();
                msgTerm.ScoreTime = act.ScoreTime.ToBinary();
                msgTerm.SortId = tb.RankWeight;
                msgTerm.ActivityType = tb.Categorytags;

              

                if (act.Type == OperationActivityType.Lottery)
                {
                    msgTerm.Param.Add(act.mDBData.Param);
                    var lottery = act as PlayerOperationActivityLottery;
                    msgTerm.Param.Add(lottery.ResetNeedMoney);
                    msgTerm.Param.AddRange(lottery.DrawLotteryLadder.ToArray());
                }

                if (tb.UIType == (int)OperationActivityUIType.ShowModel)
                { // 显示模型(添加XYZ坐标 及旋转的XYZ)，坐标放大了100倍，用来显示小数
                    msgTerm.Param.AddRange(tb.StrParam);    
                }

                if (role >= 0 && role < tb.ModelPath.Count)
                {
                    msgTerm.ModelPath = tb.ModelPath[role];
                }
                else
                {
                    if (tb.ModelPath.Count > 0)
                    {
                        msgTerm.ModelPath = tb.ModelPath[0];
                    }
                    else
                    {
                        msgTerm.ModelPath = -1;
                    }
                }

                foreach (var item in act.Items)
                {
                    var tbItem = Table.GetYunYing(item.Id);
                    if (null == tbItem)
                    {
                        continue;
                    }
                    var msgItem = new MsgOperActivtyItem();
                    msgItem.Id = item.Id;
                    msgItem.Name = tbItem.Name;
                    msgItem.Desc = item.Desc;
                    if (string.IsNullOrEmpty(msgItem.Desc))
                    {
                        msgItem.Desc = tbItem.Desc;
                    }
                    msgItem.Icon = tbItem.BtnIcon;
                    msgItem.Count = item.Counter;
                    msgItem.Need = item.Need;
                    msgItem.AquiredTimes = item.AquiredTimes;
                    msgItem.TotalTimes = item.TotalTimes;
                    msgItem.StarTime = item.RewardBegin.ToBinary();
                    msgItem.EndTime = item.End.ToBinary();
                    msgItem.NeedItemId = tbItem.NeetItem;
                    msgItem.NeedItemCount = tbItem.NeetItemCount;
                    msgItem.Condition = tbItem.ConditionId;
                    msgItem.GuideActivityId = tbItem.GuideOperationActivityId;
                    msgItem.GuideUI = tbItem.GuideUI;


                    if (0 == role)
                    {
                        msgItem.Rewards = tbItem.JiangLiJob1;
                    }
                    else if (1 == role)
                    {
                        msgItem.Rewards = tbItem.JiangLiJob2;
                    }
                    else if (2 == role)
                    {
                        msgItem.Rewards = tbItem.JiangLiJob3;
                    }
                    msgTerm.Items.Add(msgItem);
                }

                msgTerms.Add(msgTerm);
            }


            msg.Reply((int)ErrorCodes.OK);
            yield break;
        }

        public IEnumerator ClaimOperationReward(Coroutine coroutine, LogicCharacterProxy charProxy, ClaimOperationRewardInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            if (null == proxy)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            var character = proxy.Character;
            if (null == character)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (null == character.mOperActivity)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            PlayerOperationActivity act = null;
            if (!character.mOperActivity.DictActivity.TryGetValue(msg.Request.Type, out act))
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            int result = -1;
            var code = act.AquireReward(msg.Request.Id, out result);
            msg.Response = result;
            msg.Reply((int)code);
            yield break;

        }

        //祭拜
        public IEnumerator WorshipMonument(Coroutine coroutine, LogicCharacterProxy charProxy, WorshipMonumentInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var character = proxy.Character;
            var tb = Table.GetLode(msg.Request.MonumentId);
            if (tb == null)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            //检查道具
            if (character.mBag.GetItemCount(tb.WorshipConsumeItemId) >= tb.WorshipConsumeItemCount)
            {
                character.mBag.DeleteItem(tb.WorshipConsumeItemId, tb.WorshipConsumeItemCount, eDeleteItemType.WorshipMonument);
            }
            else
            {
                msg.Reply((int)ErrorCodes.ItemNotEnough);
                yield break;
            }

            //添加掉落
            var dropItems = new Dictionary<int, int>();
            var extraItems = new Dictionary<int, int>();
            character.DropMother(tb.LodeOutput[0], dropItems);
            foreach (var item in dropItems)
            {
                var itemId = item.Key;
                var count = item.Value;
                if (character.mBag.AddItem(itemId, count, eCreateItemType.WorshipMonument) != ErrorCodes.OK)
                {
                    extraItems.modifyValue(itemId, count);
                }
                msg.Response.Data.modifyValue(itemId, count);
            }
            if (extraItems.Count >= 1)
            {
                var tbMail = Table.GetMail(503);
                character.mMail.PushMail(tbMail.Title, tbMail.Text, extraItems);
            }
            msg.Reply((int)ErrorCodes.OK);
            yield break;
        }

        //<summary>
        //修改玩家名字
        //</summary>
        //<param name="coroutine"></param>
        //<param name="_this"></param>
        //<param name="msg"></param>
        //<returns></returns>
        public IEnumerator ModifyPlayerName(Coroutine coroutine, LogicCharacterProxy _this, ModifyPlayerNameInMessage msg)
        {

            var cc = CharacterManager.Instance.GetCharacterControllerFromMemroy(msg.CharacterId);
            if (cc == null)
            {
                msg.Reply((int)ErrorCodes.Error_Login_NotLogin);
                yield break;
            }
            var name = msg.Request.ModifyName.Trim();
            if (cc.Name == name)
            {
                msg.Reply((int)ErrorCodes.Error_NAME_IN_USE);
                yield break;
            }
            bool isNotFirst=cc.GetFlag(3610);//是否第一次改名
            if (isNotFirst)
            {
                var record=Table.GetServerConfig(1300);
                string[] temp = record.Value.Split('|');
                if(temp.Length!=2){
                     msg.Reply((int)ErrorCodes.Unknow);
                    yield break;
                }
                int resType   =Convert.ToInt32(temp[0]);
                int needCount = Convert.ToInt32(temp[1]);
                var hasCount = cc.mBag.GetRes((eResourcesType)resType);
                if (needCount > hasCount)
                {
                    msg.Reply((int)ErrorCodes.DiamondNotEnough);
                    yield break;
                }
                var msg_login = Logic.LogicServer.Instance.LoginAgent.TryModifyPlayerName(msg.ClientId, name);
                yield return msg_login.SendAndWaitUntilDone(coroutine);
                if (msg_login.ErrorCode != (int)ErrorCodes.OK)
                {
                    msg.Reply((int)msg_login.ErrorCode);
                    yield break;
                }
                
                cc.mBag.DelRes((eResourcesType)resType, needCount, eDeleteItemType.ModifyName);
                
            }
            else
            {
                var msg_login = Logic.LogicServer.Instance.LoginAgent.TryModifyPlayerName(msg.ClientId, name);
                yield return msg_login.SendAndWaitUntilDone(coroutine);
                if (msg_login.ErrorCode != (int)ErrorCodes.OK)
                {
                    msg.Reply((int)msg_login.ErrorCode);
                    yield break;
                }
                cc.SetFlag(3610, true);
            }
            cc.SetName(name);
            var sData=cc.GetSimpleData();
            sData.Name = name;
            CharacterManager.Instance.UpdateSimpleData(sData.Id);
            cc.MarkDirty();
            msg.Response =name;

            var msg_scene = Logic.LogicServer.Instance.SceneAgent.NodifyModifyPlayerName(msg.CharacterId,name);
            yield return msg_scene.SendAndWaitUntilDone(coroutine);

            var msg_chat = Logic.LogicServer.Instance.ChatAgent.NodifyModifyPlayerName(msg.CharacterId, name);
            yield return msg_chat.SendAndWaitUntilDone(coroutine);

            var msg_rank = Logic.LogicServer.Instance.RankAgent.NodifyModifyPlayerName(cc.mGuid, cc.serverId, msg.CharacterId,name);
            yield return msg_rank.SendAndWaitUntilDone(coroutine);


            if (cc.mAlliance.AllianceId > 0)
            {
                var msg_team = Logic.LogicServer.Instance.TeamAgent.NodifyModifyAllianceMemberName(cc.mGuid,cc.serverId,msg.CharacterId,cc.mAlliance.AllianceId, name);
                yield return msg_team.SendAndWaitUntilDone(coroutine);
            }
            msg.Reply((int)ErrorCodes.OK);
        }

        /// <summary>
        /// 发起角斗申请
        /// </summary>
        public IEnumerator InviteChallenge(Coroutine coroutine, LogicCharacterProxy charProxy, InviteChallengeInMessage msg)
        {
            //1、使用ss消息通知Scene服务器来确认目标玩家的状态
            var msg1 = LogicServer.Instance.SceneAgent.CheckCanAcceptChallenge(msg.Request.CharacterId, 0);
            yield return msg1.SendAndWaitUntilDone(coroutine);
            if (MessageState.Reply != msg1.State)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            if ((int) ErrorCodes.OK != msg1.ErrorCode)
            {
                msg.Reply(msg1.ErrorCode);
                yield break;
            }

            var proxy = (LogicProxy)charProxy;
            var character = proxy.Character;

            //2、消耗100钻石
            int cost = 100;
            var config = Table.GetServerConfig(750);
            if (null != config)
            {
                var value = config.ToInt();
                if (0 < value)
                {
                    cost = value;
                }
            }

            int haveCount = character.mBag.GetRes(eResourcesType.DiamondRes);
            if (haveCount < cost)
            {
                msg.Reply((int) ErrorCodes.DiamondNotEnough);
                yield break;
            }

            var ret = character.mBag.DelRes(eResourcesType.DiamondRes, cost, eDeleteItemType.InviteChallenge);
            if (ErrorCodes.OK != ret)
            {
                msg.Reply((int) ErrorCodes.DiamondNotEnough);
                yield break;
            }

            //3、通知对方
            var msg2 = LogicServer.Instance.LogicAgent.NotifyInviteChallenge(msg.Request.CharacterId, proxy.CharacterId,
                character.GetName(), character.serverId);
            yield return msg2.SendAndWaitUntilDone(coroutine);
            msg.Reply((int) ErrorCodes.OK);
        }

        /// <summary>
        /// 是否接受角斗申请
        /// </summary>
        public IEnumerator AcceptChallenge(Coroutine coroutine, LogicCharacterProxy charProxy, AcceptChallengeInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var character = proxy.Character;

            // 已经处理过了
            if (!character.ChallengeInvitors.ContainsKey(msg.Request.CharacterId))
            {
                msg.Reply();
                yield break;
            }

            // 对角斗邀请做出回应，从列表中移除
            var invitor = character.ChallengeInvitors[msg.Request.CharacterId];
            character.ChallengeInvitors.Remove(msg.Request.CharacterId);

            // 拒绝
            if (!msg.Request.Accept)
            {
                // 世界广播
                var args = new List<string>();
                args.Add(character.GetName());
                args.Add(invitor.Name);
                var content = Utils.WrapDictionaryId(100003324, args);

                var msg2 = LogicServer.Instance.ChatAgent.SSBroadcastAllServerMsg(proxy.CharacterId, (int)eChatChannel.SystemScroll, character.GetName(), new ChatMessageContent { Content = content });
                yield return msg2.SendAndWaitUntilDone(coroutine);
                msg.Reply((int)ErrorCodes.OK);
                yield break;
            }

            // 如果有组队，先离开队伍
            var msg3 = LogicServer.Instance.TeamAgent.SSLeaveTeam(msg.Request.CharacterId, 0);
            yield return msg3.SendAndWaitUntilDone(coroutine);
            var msg4 = LogicServer.Instance.TeamAgent.SSLeaveTeam(proxy.CharacterId, 0);
            yield return msg4.SendAndWaitUntilDone(coroutine);

            // 一起进入角斗副本
            CoroutineFactory.NewCoroutine(CreateEnterDuelScene, character.serverId, msg.Request.CharacterId, proxy.CharacterId).MoveNext();

            msg.Reply((int)ErrorCodes.OK);
        }

        // 进入角斗副本
        private IEnumerator CreateEnterDuelScene(Coroutine co, int serverId, ulong characterId, ulong targetId)
        {
            //排队创建场景时，根据合服ID进行
            var serverLogicId = SceneExtension.GetServerLogicId(serverId);
            var sceneInfo = new ChangeSceneInfo
            {
                SceneId = 110001,
                ServerId = serverLogicId,
                SceneGuid = 0,
                Type = (int)eScnenChangeType.EnterDungeon
            };
            sceneInfo.Guids.Add(characterId);
            sceneInfo.Guids.Add(targetId);
            var msgChgScene = LogicServer.Instance.SceneAgent.SBChangeSceneByTeam(characterId, sceneInfo);
            yield return msgChgScene.SendAndWaitUntilDone(co);
            // 需要处理切换场景识别的信息吗？？
        }

        public IEnumerator SendJsonData(Coroutine coroutine, LogicCharacterProxy charProxy, SendJsonDataInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            if (null == proxy)
            {
                yield break;
            }
            msg.Response = "test";
            msg.Reply();
            //throw new NotImplementedException();
        }

        //交易系统：购买道具 
        public IEnumerator StoreOperationBuy(Coroutine coroutine,
                                             LogicCharacterProxy charProxy,
                                             StoreOperationBuyInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var targetId = msg.Request.Guid;
            var storeId = msg.Request.StoreId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------StoreOperationBuy----------{0},{1}",
                targetId, storeId);
            var isCanBuy = false;
            foreach (var one in proxy.Character.mExchange.ChechTempList.Items)
            {
                if (one.SellCharacterId == targetId)
                {
                    isCanBuy = true;
                    break;
                }
            }
            if (!isCanBuy)
            {
                msg.Reply((int)ErrorCodes.Error_NoBuyCharacterItem);
                yield break;
            }
            var dbLogicSimple = LogicServer.Instance.LogicAgent.GetLogicSimpleData(targetId, 0);
            yield return dbLogicSimple.SendAndWaitUntilDone(coroutine);
            if (dbLogicSimple.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (dbLogicSimple.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(dbLogicSimple.ErrorCode);
                yield break;
            }
            //是否存在检查
            OtherStoreOne tempOne = null;
            foreach (var item in dbLogicSimple.Response.Exchange.Items)
            {
                if (item.Id == storeId)
                {
                    if (item.State == (int)StoreItemType.Free)
                    {
                        msg.Reply((int)ErrorCodes.Error_ItemNotFind);
                        yield break;
                    }
                    if (item.State == (int)StoreItemType.Buyed)
                    {
                        msg.Reply((int)ErrorCodes.Error_ExchangeItemState);
                        yield break;
                    }
                    tempOne = item;
                    break;
                }
            }
            if (tempOne == null)
            {
                msg.Reply((int)ErrorCodes.Error_ItemNotFind);
                yield break;
            }
            //资源检查
            var old = 0;
            var resid = 0;
            if (tempOne.NeedType >= 10)
            {
                msg.Reply((int)ErrorCodes.Error_ExchangeItemState);
                yield break;
            }
            if (tempOne.NeedType == 0) // 0是  交易所  10 是拍卖行
            {
                resid = (int)eResourcesType.Other16;
                old = proxy.Character.mBag.mDbData.Resources[resid];
                if (old < tempOne.NeedCount)
                {
                    msg.Reply((int)ErrorCodes.Error_ResNoEnough);
                    yield break;
                }
                proxy.Character.mBag.mDbData.Resources[resid] = old - tempOne.NeedCount;
            }
            else if (tempOne.NeedType == 1) // 1是  交易所  11 是拍卖行
            {
                resid = (int)eResourcesType.DiamondRes;
                old = proxy.Character.mBag.mDbData.Resources[resid];
                if (old < tempOne.NeedCount)
                {
                    msg.Reply((int)ErrorCodes.Error_ResNoEnough);
                    yield break;
                }
                proxy.Character.mBag.mDbData.Resources[resid] = old - tempOne.NeedCount;
            }
            else
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            //int old = proxy.Character.mBag.mDbData.Resources[(int)eResourcesType.Other16];
            //if (old < tempOne.NeedCount)
            //{
            //    msg.Reply((int)ErrorCodes.Error_ResNoEnough);
            //    yield break;
            //}
            //proxy.Character.mBag.mDbData.Resources[(int)eResourcesType.Other16] = old - tempOne.NeedCount;
            //远程购买
            var LogicBuy = LogicServer.Instance.LogicAgent.SSStoreOperationBuy(targetId, storeId, proxy.CharacterId,
                proxy.Character.GetName(), resid, tempOne.NeedCount, tempOne.ItemData);
            yield return LogicBuy.SendAndWaitUntilDone(coroutine);
            if (LogicBuy.State != MessageState.Reply)
            {
                old = proxy.Character.mBag.mDbData.Resources[resid];
                proxy.Character.mBag.mDbData.Resources[resid] = old + tempOne.NeedCount;
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (LogicBuy.ErrorCode != (int)ErrorCodes.OK)
            {
                old = proxy.Character.mBag.mDbData.Resources[resid];
                proxy.Character.mBag.mDbData.Resources[resid] = old + tempOne.NeedCount;
                msg.Reply(LogicBuy.ErrorCode);
                yield break;
            }
            if (proxy.Character.mBag.CheckAddItem(tempOne.ItemData.ItemId, tempOne.ItemData.Count) == ErrorCodes.OK)
            {
                proxy.Character.mBag.AddItem(tempOne.ItemData, eCreateItemType.ExchangeBuy);
            }
            else
            {
                var tbMail = Table.GetMail(117);
                var mail = proxy.Character.mMail.PushMail(tbMail.Title, tbMail.Text,
                    new List<ItemBaseData> { tempOne.ItemData });
                mail.Send = tbMail.Sender;
            }

            proxy.Character.AddExData((int)eExdataDefine.e339, 1);
            msg.Reply();
            if (tempOne.ManagerId != 0)
            {
                var ExchangeMsg = LogicServer.Instance.TeamAgent.CancelExchangeItem(proxy.CharacterId, targetId,
                    tempOne.ManagerId);
                yield return ExchangeMsg.SendAndWaitUntilDone(coroutine);
            }
        }

        //交易系统：收回道具  
        public IEnumerator StoreOperationCancel(Coroutine coroutine,
                                                LogicCharacterProxy charProxy,
                                                StoreOperationCancelInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var storeid = msg.Request.StoreId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------StoreOperationCancel----------:{0}", storeid);
            ExchangeItem resultItem = null;
            var result = proxy.Character.mExchange.CancelItem(storeid, ref resultItem);
            if (result == ErrorCodes.OK)
            {
                if (resultItem != null)
                {
                    if (resultItem.NeedType < 10) // 交易所
                    {
                        if (resultItem.mDbdata.ManagerId != 0 &&
                            DateTime.FromBinary(resultItem.mDbdata.ManagerOverTime) > DateTime.Now)
                        {
                            //需要同步到Team，取消该物品的广播
                            var ExchangeMsg = LogicServer.Instance.TeamAgent.CancelExchangeItem(proxy.CharacterId,
                                proxy.CharacterId, resultItem.mDbdata.ManagerId);
                            yield return ExchangeMsg.SendAndWaitUntilDone(coroutine);
                        }
                    }
                    else
                    {
                        var ExchangeMsg = LogicServer.Instance.TeamAgent.SSDownItemAuction(proxy.CharacterId,
                            proxy.Character.serverId, proxy.CharacterId, resultItem.mDbdata.ManagerId);
                        yield return ExchangeMsg.SendAndWaitUntilDone(coroutine);
                    }
                }
            }
            msg.Reply((int)result);
        }

        //交易系统：查看某人  
        public IEnumerator StoreOperationLook(Coroutine coroutine,
                                              LogicCharacterProxy charProxy,
                                              StoreOperationLookInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var targetId = msg.Request.Guid;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------StoreOperationLook----------targetId:{0}",
                targetId);

            if (targetId == 0)
            {
                msg.Reply();
                yield break;
            }
            var dbLogicSimple = LogicServer.Instance.LogicAgent.GetExchangeData(targetId, targetId);
            yield return dbLogicSimple.SendAndWaitUntilDone(coroutine);
            if (dbLogicSimple.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (dbLogicSimple.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(dbLogicSimple.ErrorCode);
                yield break;
            }
            msg.Response.SellCharacterId = dbLogicSimple.Response.SellCharacterId;
            msg.Response.SellCharacterName = dbLogicSimple.Response.SellCharacterName;
            foreach (var item in dbLogicSimple.Response.Items)
            {
                msg.Response.Items.Add(item);
            }
            msg.Reply();
        }

        //交易系统：查看自己 
        public IEnumerator StoreOperationLookSelf(Coroutine coroutine,
                                                  LogicCharacterProxy charProxy,
                                                  StoreOperationLookSelfInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------StoreOperationLookSelf----------");
            msg.Response.NextFreeTime = proxy.Character.mExchange.mDbData.NextFreeTime;
            foreach (var item in proxy.Character.mExchange.mDataList)
            {
                msg.Response.Items.Add(new SelfStoreOne
                {
                    Id = item.mDbdata.Id,
                    ItemData = item.mDbdata.ItemData,
                    NeedCount = item.mDbdata.NeedCount,
                    State = item.mDbdata.State,
                    BroadcastOverTime = item.mDbdata.ManagerOverTime,
                    ItemType = item.mDbdata.NeedType
                });
            }
            msg.Reply();
            yield break;
        }

        //交易系统：获取自己已贩卖的收获
        public IEnumerator StoreOperationHarvest(Coroutine coroutine,
                                                 LogicCharacterProxy charProxy,
                                                 StoreOperationHarvestInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var storeId = msg.Request.StoreId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------StoreOperationHarvest----------:{0}",
                storeId);
            var result = proxy.Character.mExchange.Harvest(storeId);

            msg.Reply((int)result);
            yield break;
        }

        //交易系统：系统兑换
        public IEnumerator SSStoreOperationExchange(Coroutine coroutine,
                                                    LogicCharacterProxy charProxy,
                                                    SSStoreOperationExchangeInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var tradeId = msg.Request.Trade;
            var itemCount = msg.Request.ItemCount;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------SSStoreOperationExchange----------{0}",
                tradeId, itemCount);
            var tbTrade = Table.GetTrade(tradeId);
            if (tbTrade == null)
            {
                msg.Reply((int)ErrorCodes.Error_TradeID);
                yield break;
            }
            var itemId = tbTrade.ItemID;
            var nowCount = proxy.Character.mBag.GetItemCount(itemId);
            if (nowCount < itemCount)
            {
                msg.Reply((int)ErrorCodes.Error_CountNotEnough);
                yield break;
            }
            var resultCodes = proxy.Character.mBag.AddItem(tbTrade.MoneyType, tbTrade.Price * itemCount,
                eCreateItemType.ExchangeSwap);
            if (resultCodes != ErrorCodes.OK)
            {
                msg.Reply((int)resultCodes);
                yield break;
            }
            proxy.Character.mBag.DeleteItem(itemId, itemCount, eDeleteItemType.ExchangeSwap);
            msg.Reply();
        }

        public IEnumerator ClickMayaTip(Coroutine coroutine, LogicCharacterProxy _this, ClickMayaTipInMessage msg)
        {
            var proxy = (LogicProxy)_this;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ClickMayaTip----------:{0}");
            var id = proxy.Character.GetExData((int)eExdataDefine.e648);
            if (id > 0)
            {
                var e = new TollgateNextFinish(proxy.Character, id);
                EventDispatcher.Instance.DispatchEvent(e);
            }
            yield break;
        }
        //团购申请
        public IEnumerator ApplyGroupShopItems(Coroutine co,
                                               LogicCharacterProxy charProxy,
                                               ApplyGroupShopItemsInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var types = msg.Request.Types;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------ApplyGroupShopItems----------:{0}", types);
            if (types.Items.Any(type => type < 0 || type > 3))
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            var charactyer = proxy.Character;
            var items = charactyer.mCity.GetItems(types.Items);
            var itemList = new Int64ArrayList();
            itemList.Items.AddRange(items);
            var netGroupShop = LogicServer.Instance.TeamAgent.SSApplyGroupShopItems(proxy.CharacterId, types, itemList,
                charactyer.mDbData.TypeId);
            yield return netGroupShop.SendAndWaitUntilDone(co);

            if (netGroupShop.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (netGroupShop.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(netGroupShop.ErrorCode);
                yield break;
            }
            charactyer.mCity.CacheItems(netGroupShop);
            msg.Response.Lists.AddRange(netGroupShop.Response.Items.Lists);
            msg.Reply();
        }

        //购买团购
        public IEnumerator BuyGroupShopItem(Coroutine co, LogicCharacterProxy charProxy, BuyGroupShopItemInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var guid = msg.Request.Guid;
            var count = msg.Request.Count;
            var gropId = msg.Request.GropId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------BuyGroupShopItem----------:{0},{1},{2}",
                guid, count, gropId);
            //检查是否在cache的list里面
            if (!proxy.Character.mCity.IsContainsItemid(guid))
            {
                msg.Reply((int)ErrorCodes.Error_CanBuy);
                yield break;
            }
            var tbGS = Table.GetGroupShop(gropId);
            if (tbGS == null)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (proxy.Character.mBag.GetItemCount(tbGS.SaleType) < tbGS.SaleCount * count)
            {
                msg.Reply((int)ErrorCodes.Error_ResNoEnough);
                yield break;
            }
            var netGroupShop = LogicServer.Instance.TeamAgent.SSBuyGroupShopItem(proxy.CharacterId, guid, count);
            yield return netGroupShop.SendAndWaitUntilDone(co);
            if (netGroupShop.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (netGroupShop.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(netGroupShop.ErrorCode);
                yield break;
            }
            proxy.Character.mCity.AddToBuyItems(guid);
            var newCount = netGroupShop.Response;
            proxy.Character.AddExData((int)eExdataDefine.e343, newCount);
            //proxy.Character.mCity.CityAddExp(StaticParam.BuyGroupShopExp*count);
            msg.Response = newCount;
            msg.Reply();
        }

        // 领取多倍经验
        public IEnumerator TakeMultyExpAward(Coroutine coroutine, LogicCharacterProxy charProxy, TakeMultyExpAwardInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var error = proxy.Character.TakeMultyExpAward(msg.Request.Id);
            if (error == ErrorCodes.OK)
            {
                msg.Response = 1;
            }
            else
            {
                msg.Response = 0;
            }
            msg.Reply((int)error);
            yield break;
        }

        //获取我当前的愿望
        public IEnumerator GetBuyedGroupShopItems(Coroutine co,
                                                  LogicCharacterProxy charProxy,
                                                  GetBuyedGroupShopItemsInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------GetBuyedGroupShopItems----------");
            var buyed = proxy.Character.mCity.GetBuyedItems();
            var buyedArray = new Int64Array();
            buyedArray.Items.AddRange(buyed);
            var netGroupShop = LogicServer.Instance.TeamAgent.SSGetBuyedGroupShopItems(proxy.CharacterId, buyedArray);
            yield return netGroupShop.SendAndWaitUntilDone(co);
            if (netGroupShop.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (netGroupShop.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(netGroupShop.ErrorCode);
                yield break;
            }
            msg.Response = netGroupShop.Response;
            msg.Reply();
        }

        //获取团购历史
        public IEnumerator GetGroupShopHistory(Coroutine co,
                                               LogicCharacterProxy charProxy,
                                               GetGroupShopHistoryInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------GetGroupShopHistory----------");
            var buyed = proxy.Character.mCity.GetBuyedItems();
            var buyedArray = new Int64Array();
            buyedArray.Items.AddRange(buyed);
            var histroy = proxy.Character.mCity.GetHistoryItems();
            var historyArray = new Int64Array();
            historyArray.Items.AddRange(histroy);
            var netGroupShop = LogicServer.Instance.TeamAgent.SSGetGroupShopHistory(proxy.CharacterId, buyedArray,
                historyArray);
            yield return netGroupShop.SendAndWaitUntilDone(co);
            if (netGroupShop.State != MessageState.Reply)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }
            if (netGroupShop.ErrorCode != (int)ErrorCodes.OK)
            {
                msg.Reply(netGroupShop.ErrorCode);
                yield break;
            }
            msg.Response = netGroupShop.Response.Items;
            msg.Reply();
            //缓存history
            proxy.Character.mCity.CacheHistoryItems(netGroupShop.Response);
        }

        //占星台宝石升级
        public IEnumerator AstrologyLevelUp(Coroutine coroutine,
                                            LogicCharacterProxy charProxy,
                                            AstrologyLevelUpInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var bagid = msg.Request.BagId;
            var bagindex = msg.Request.BagIndex;
            var items = msg.Request.NeedList.Items;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------AstrologyLevelUp----------:{0},{1},{2}",
                bagid, bagindex, items);
            msg.Reply((int)proxy.Character.AstrologyLevelUp(bagid, bagindex, items));
            yield break;
        }

        //占星台宝石穿上
        public IEnumerator AstrologyEquipOn(Coroutine coroutine,
                                            LogicCharacterProxy charProxy,
                                            AstrologyEquipOnInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var bagindex = msg.Request.BagIndex;
            var astrogyid = msg.Request.AstrologyId;
            var index = msg.Request.Index;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------AstrologyEquipOn----------:{0},{1},{2}",
                bagindex, astrogyid, index);
            msg.Reply((int)proxy.Character.AstrologyEquipOn(bagindex, astrogyid, index));
            yield break;
        }

        //占星台宝石卸下
        public IEnumerator AstrologyEquipOff(Coroutine coroutine,
                                             LogicCharacterProxy charProxy,
                                             AstrologyEquipOffInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var astrogyid = msg.Request.AstrologyId;
            var index = msg.Request.Index;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------AstrologyEquipOff----------:{0},{1}",
                astrogyid, index);
            msg.Reply((int)proxy.Character.AstrologyEquipOff(astrogyid, index));
            yield break;
        }

        //对随从使用经验药
        public IEnumerator UsePetExpItem(Coroutine coroutine, LogicCharacterProxy charProxy, UsePetExpItemInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var petid = msg.Request.PetId;
            var itemId = msg.Request.ItemId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------UsePetExpItem----------{0},{1}", petid,
                itemId);
            var pet = proxy.Character.GetPet(petid);
            if (pet == null)
            {
                msg.Reply((int)ErrorCodes.Error_PetNotFind);
                yield break;
            }
            var tbItem = Table.GetItemBase(itemId);
            if (tbItem == null)
            {
                msg.Reply((int)ErrorCodes.Error_ItemNotFind);
                yield break;
            }
            if (tbItem.Type != 26500)
            {
                msg.Reply((int)ErrorCodes.Error_ItemNotUse);
                yield break;
            }
            var nowCount = proxy.Character.mBag.GetItemCount(itemId);
            var itemCount = msg.Request.ItemCount;
            if (nowCount < itemCount)
            {
                msg.Reply((int)ErrorCodes.ItemNotEnough);
                yield break;
            }
            var needExp = pet.GetTotleNeedExp();
            if (needExp <= tbItem.Exdata[0] * (itemCount - 1))
            {
                msg.Reply((int)ErrorCodes.Error_ItemWaste);
                yield break;
            }


            var oldLevel = pet.GetLevel();
            pet.PetAddExp(tbItem.Exdata[0] * itemCount);
            var newLevel = pet.GetLevel();
            if (newLevel > oldLevel)
            {
                proxy.Character.AddExData((int)eExdataDefine.e331, newLevel - oldLevel);
                var fp = pet.GetFightPoint();
                proxy.Character.SetExdataToMore(68, fp);
                proxy.Character.SetExdataToMore((int)eExdataDefine.e70, newLevel);
            }
            proxy.Character.mBag.DeleteItem(itemId, itemCount, eDeleteItemType.PetExp);
            msg.Reply();
        }

        //转生接口
        public IEnumerator Reincarnation(Coroutine coroutine, LogicCharacterProxy charProxy, ReincarnationInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var ladder = proxy.Character.GetExData((int)eExdataDefine.e51);
            var typeId = ladder;//msg.Request.TypeId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------Reincarnation----------{0}", typeId);
            var tbRe = Table.GetTransmigration(typeId);
            if (tbRe == null || tbRe.PropPoint == -1)
            {
                msg.Reply((int)ErrorCodes.Error_TransmigrationID);
                yield break;
            }
            if (tbRe.ConditionCount != -1)
            {
                if (proxy.Character.CheckCondition(tbRe.ConditionCount) != -2)
                {
                    msg.Reply((int)ErrorCodes.Error_ConditionNoEnough);
                    yield break;
                }
            }
            var level = proxy.Character.GetLevel();
            if (level < tbRe.TransLevel)
            {
                msg.Reply((int)ErrorCodes.Error_ConditionNoEnough);
                yield break;
            }
            var gold = proxy.Character.mBag.GetRes(eResourcesType.GoldRes);
            if (gold < tbRe.NeedMoney)
            {
                msg.Reply((int)ErrorCodes.MoneyNotEnough);
                yield break;
            }
            var dust = proxy.Character.mBag.GetRes(eResourcesType.MagicDust);
            if (dust < tbRe.NeedDust)
            {
                msg.Reply((int)ErrorCodes.MoneyNotEnough);
                yield break;
            }
            proxy.Character.mBag.DelRes(eResourcesType.GoldRes, tbRe.NeedMoney, eDeleteItemType.Reincarnation);
            proxy.Character.mBag.DelRes(eResourcesType.MagicDust, tbRe.NeedDust, eDeleteItemType.Reincarnation);
            proxy.Character.AddExData((int)eExdataDefine.e51, 1);
            var roleid = proxy.Character.GetRole();
            var tbActor = Table.GetActor(roleid);
            ladder++;

            if (tbActor == null || ladder < 1 || ladder > 4)
            {
                Logger.Error("Reincarnation  roleid={0},ladder={1}", roleid, ladder);
                msg.Reply((int)ErrorCodes.Error_DataOverflow);
                yield break;
            }
            proxy.Character.SetFlag(ladder + 2801);//设置转生成功标记位
            var diffLevel = level - tbRe.TransLevel;
            proxy.Character.AddExData((int)eExdataDefine.e52,
                (tbActor.FreePoint[ladder] - tbActor.FreePoint[ladder - 1]) * diffLevel + tbActor.FreePoint[ladder]);
            msg.Reply();
            proxy.Character.BooksChange();
        }

        //升级军衔
        public IEnumerator UpgradeHonor(Coroutine coroutine, LogicCharacterProxy charProxy, UpgradeHonorInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var typeId = msg.Request.TypeId;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------UpgradeHonor----------{0}", typeId);
            var result = proxy.Character.UpgradeHonor(typeId);
            msg.Reply((int)result);
            yield break;
        }

        //通知logic，我选择的奖励
        public IEnumerator SelectDungeonReward(Coroutine co,
                                               LogicCharacterProxy charProxy,
                                               SelectDungeonRewardInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            var fubenId = msg.Request.FubenId;
            var select = msg.Request.Select;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------SelectDungeonReward----------{0},{1}",
                fubenId, select);
            var err = proxy.Character.SelectDungeonReward(fubenId, select);
            msg.Reply((int)err);
            yield break;
        }

        //收集客户端报错
        public IEnumerator ClientErrorMessage(Coroutine coroutine,
                                              LogicCharacterProxy charProxy,
                                              ClientErrorMessageInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog((int)LogType.ClientError, "ClientErrorMessage id={0},type={1},msg={2}", msg.CharacterId,
                msg.Request.ErrorType, msg.Request.ErrorMsg);
            msg.Reply();
            yield break;
        }

        public IEnumerator SendQuestion(Coroutine coroutine,
                                              LogicCharacterProxy charProxy,
                                              SendQuestionInMessage msg)
        {

            var proxy = (LogicProxy)charProxy;
            if (proxy.Character.GetExData((int)eExdataDefine.e750) <= 0)
            {
                msg.Reply((int)ErrorCodes.Error_SendMailNotEnough);
            }
            var msg1 = LogicServer.Instance.GameMasterAgent.SendQuestion(proxy.CharacterId, msg.Request.Mail);
            yield return msg1.SendAndWaitUntilDone(coroutine);
            proxy.Character.AddExData((int)eExdataDefine.e750, -1);
            yield break;
        }
        public IEnumerator CSApplyOfflineExpData(Coroutine coroutine, LogicCharacterProxy charProxy, CSApplyOfflineExpDataInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            PlayerLog.WriteLog(proxy.CharacterId, "----------Logic----------CSApplyOfflineExpData----------");
            var prox = (LogicProxy)charProxy;
            if (prox.Character == null)
            {
                msg.Reply((int)ErrorCodes.Unline);
                yield break;
            }

            var curLeftTime = prox.Character.GetExData((int)eExdataDefine.e742);
            if (curLeftTime <= 0) // 大多数玩家都没有时间 单独写个判断 在这返回 减少计算
            {
                prox.Character.SetExData((int)eExdataDefine.e742, 0);
                msg.Reply();
                yield break;
            }
            //int cost = int.Parse(Table.GetServerConfig(589).Value);
            

           

            var type = msg.Request.Placeholder;


            // 算出离线时间   
            var lastoffTime = prox.Character.lExdata64.GetTime(Exdata64TimeType.LastOutlineTime);//本来在线又重新登陆，比如挤号，会记录强行踢下线，并记录下离线时间
            var lastOnTime = prox.Character.lExdata64.GetTime(Exdata64TimeType.LastOnlineTime);
            var offlineTime = lastOnTime - lastoffTime;

            if (lastoffTime == DateTime.MinValue)
            {
                msg.Response.OfflineTime = 0;
            }
            else
            {
                msg.Response.OfflineTime = (long)offlineTime.TotalSeconds;
            }
           

            if (msg.Response.OfflineTime <= 0)
            {
                msg.Reply((int)ErrorCodes.Unknow);
                yield break;
            }

            var lastGetTime = prox.Character.lExdata64.GetTime(Exdata64TimeType.LastGetOfflineExpTime);
            if (lastGetTime != DateTime.MinValue)
            {
                if (lastGetTime > lastoffTime) // 上次领完没下线  又走到这里来了   说明客户端闪退 又立刻上线了  服务器没执行下线逻辑  这个时候不给奖励
                {
                    msg.Reply((int)ErrorCodes.Unknow);
                    yield break;
                }
            }
            //玩家一上线就会请求一次，type=0的用于显示，这个时候就会计算一边并把值写入e678,以便万一没领取掉线了，
            //再上来的时候还能取到所以如果type>0说明这个值已经算好了，就不用再累加了，type>0的时候算出来的值就是这里写的未领取值,所以不应该是累加，大于type>0时应该是=，type=0时才是累加
            if (type == 0)
            {
                msg.Response.OfflineTime += prox.Character.GetExData((int)eExdataDefine.e678);//未领取奖励的离线时间
            }
            else
            {
                msg.Response.OfflineTime = prox.Character.GetExData((int)eExdataDefine.e678);//未领取奖励的离线时间
            }
           

            // 算出奖励时间
            var jiangliTime = 0L;
            if (msg.Response.OfflineTime < curLeftTime)
            {
                jiangliTime = msg.Response.OfflineTime;
            }
            else
            {
                jiangliTime = curLeftTime;
            }

           

            msg.Response.RewardTime = jiangliTime;
            // 设置领取时间
            if (type > 0)
                prox.Character.lExdata64.SetTime(Exdata64TimeType.LastGetOfflineExpTime, DateTime.Now);

            // 计算出等级匹配的表格数据
            var offlineRecord = new OfflineExperienceRecord();
            Table.ForeachOfflineExperience((record) =>
            {
                if (proxy.Character.GetLevel() >= record.levelMin && proxy.Character.GetLevel() <= record.levelMax)
                {
                    offlineRecord = record;
                    return false;
                }
                return true;
            });

            if (offlineRecord == null || offlineRecord.DropCD == null || offlineRecord.DropId == null)
            {
                msg.Reply((int)ErrorCodes.Error_DataOverflow);
                yield break;
            }

            if (type == 2)
            {
                double peer = new TimeSpan(0, 0, (int)jiangliTime).TotalMinutes * 0.2f;//*0.2相当于/5，,计算机乘法比除法速度快
                double d = Math.Truncate(peer);
                int rounding_Hours_OfflineTime = Convert.ToInt32(d);
                if (d < peer)
                {
                    rounding_Hours_OfflineTime++;
                }


                int cost = int.Parse(Table.GetServerConfig(290).Value) * rounding_Hours_OfflineTime;

                ErrorCodes error = proxy.Character.mBag.DelRes(eResourcesType.DiamondRes, cost, eDeleteItemType.GetLeaveExp);
                if (error != ErrorCodes.OK)
                {
                    msg.Reply((int)ErrorCodes.DiamondNotEnough);
                    yield break;
                }
            }


            // 先给经验 和  金币   exp = 挂机时间/掉落时间*掉落经验   money = 挂机时间/掉落时间*掉落金钱
            if (offlineRecord.Time != 0 && offlineRecord.Exp != 0)
            {
                var addExp = jiangliTime / offlineRecord.Time * offlineRecord.Exp;
                if (type > 0)
                    proxy.Character.mBag.AddRes(eResourcesType.ExpRes, (int)addExp * type, eCreateItemType.Offline);
                msg.Response.AddExp = addExp;
            }
            if (offlineRecord.Time != 0 && offlineRecord.Money != 0)
            {
                var addMoney = jiangliTime / offlineRecord.Time * offlineRecord.Money;
                if (type > 0)
                    proxy.Character.mBag.AddRes(eResourcesType.GoldRes, (int)addMoney, eCreateItemType.Offline);
                msg.Response.AddMoney = addMoney;
            }

            //计算出掉落子表随机物品
            msg.Response.Items.Clear();
            var itemList = new Dictionary<int, int>();
            itemList.Clear();
            if (proxy.Character.dicOfflineItems.Count > 0)
            {
                itemList.AddRange(proxy.Character.dicOfflineItems);
            }
            else
            {
                for (int j = 0; j < offlineRecord.DropId.Count(); j++)
                {
                    if (offlineRecord.DropCD.Count() > j && offlineRecord.DropCD[j] > 0 &&
                        offlineRecord.DropId.Count() > j && offlineRecord.DropId[j] >= 0)
                    {
                        var num = jiangliTime / offlineRecord.DropCD[j];
                        if (num <= 0)
                        {
                            continue;
                        }

                        // 随机物品
                        for (int i = 0; i < num; i++) //此处num次数会比较大 循环内要注意效率问题
                        {
                            var tmpDroplist = new Dictionary<int, int>();
                            tmpDroplist.Clear();
                            prox.Character.DropSon(offlineRecord.DropId[j], tmpDroplist);
                            if (tmpDroplist.Count > 0)
                            {
                                foreach (var value in tmpDroplist)
                                {
                                    if (itemList.ContainsKey(value.Key))
                                    {
                                        itemList[value.Key] += value.Value;
                                    }
                                    else
                                    {
                                        itemList.Add(value.Key, value.Value);
                                    }

                                    var tmp = new OfflineExpItem();
                                    tmp.itemid = value.Key;
                                    tmp.count = value.Value;
                                    msg.Response.Items.Add(tmp);
                                }
                            }
                        }
                    }
                }
                proxy.Character.dicOfflineItems.AddRange(itemList);
            }


            // 加物品
            if (itemList.Count > 0 && type > 0)
            {
                prox.Character.mBag.AddItemOrMail(62, itemList, null, eCreateItemType.Offline);
            }

            // 扣时间
            if (itemList.Count > 0 || msg.Response.AddMoney > 0 || msg.Response.AddExp > 0)
            {
                var result = Math.Max((curLeftTime - msg.Response.OfflineTime), 0);
                if (type > 0)
                {
                    prox.Character.SetExData((int)eExdataDefine.e742, (int)result);
                    prox.Character.SetExData((int)eExdataDefine.e678, 0);

                }
                else
                {
                    prox.Character.SetExData((int)eExdataDefine.e678, (int)jiangliTime);
                }

                msg.Response.LeftTime = (int)result;
            }

            msg.Reply();
            yield break;
        }

        public IEnumerator RereshTiralTime(Coroutine coroutine, LogicCharacterProxy charProxy, RereshTiralTimeInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            if (proxy != null && proxy.Character != null)
            {
                proxy.Character.RefreshTrialTime();
            }
            msg.Reply();
            yield break;
        }

        public IEnumerator RefreshHunterMission(Coroutine coroutine, LogicCharacterProxy charProxy,
            RefreshHunterMissionInMessage msg)
        {
            var proxy = (LogicProxy)charProxy;
            if (proxy != null && proxy.Character != null)
            {
                var itemId = StaticParam.RereshHunterItemId;
                var itemCount = StaticParam.RereshHunterItemCount;

                if (itemId < 0 || itemCount < 0)
                {
                    msg.Reply((int)ErrorCodes.Unknow);
                    yield break;
                }

                if (proxy.Character.mBag.GetItemCount(itemId) < itemCount)
                {
                    msg.Reply((int)ErrorCodes.ItemNotEnough);
                    yield break;
                }

                var error = proxy.Character.mTask.RefreshHunterMission(proxy.Character);
                if (error != ErrorCodes.OK)
                {
                    msg.Reply((int)error);
                    yield break;
                }

                proxy.Character.mBag.DeleteItem(itemId, itemCount, eDeleteItemType.RefreshHunterMission);
                msg.Reply((int)ErrorCodes.OK);
                yield break;
            }

            msg.Reply((int)ErrorCodes.Error_PlayerId_Not_Exist);
            yield break;
        }
    }

    public class LogicProxy : LogicCharacterProxy
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public LogicProxy(LogicService service, ulong characterId, ulong clientId)
            : base(service, characterId, clientId)
        {
        }

        //public List<CheckConnectedInMessage> WaitingCheckConnectedInMessages = new List<CheckConnectedInMessage>();
        //public List<CheckLostInMessage> WaitingCheckLostInMessages = new List<CheckLostInMessage>();
        public CharacterController Character { get; set; }
        public bool Connected { get; set; }
    }
}