#region using

using System;
using System.Collections;
using System.Collections.Generic;
using Database;
using DataContract;
using DataTable;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Logic
{

    #region GameMaster

    public interface IGameMaster
    {
        void EnterFuben(CharacterController character, int fubenId, SceneParam param);
        int GmAddItem(CharacterController character, int nId, int nCount);
        void GmAddSkillLevel(CharacterController character, int nId, int nLevel);
        void GmAddTalent(CharacterController character, int nPoint, int nId = -1);
        void GmMissionComplete(CharacterController character, int nId, int type = 0);
        void GmResetTalent(CharacterController character, int nId = -1);
        void GmSendMail(CharacterController character, string name, string text, Dictionary<int, int> items,int type);
        void GmSetEquip(CharacterController character, int nId);
        void GmSetEquipAttr(CharacterController character, int nPart, int nAddCount);

        void GmSetEquipGem(CharacterController character,
                           int nPart,
                           int Gem1,
                           int Gem2 = -2,
                           int Gem3 = -2,
                           int Gem4 = -2);

        void GmSetLevel(CharacterController character, int nLevel);
        void GmSetMissionParam(CharacterController character, int nId, int nIndex, int nValue);
        void GmSetSkillLevel(CharacterController character, int nId, int nLevel);
        void GmTestDrop(CharacterController character, int nDropId, int nCount);
        void GmTestDrop2(CharacterController character, int nDropId, int nCount);
        void Init();
        void PetMissionDone(CharacterController character);
        void PetMissionRefresh(CharacterController character);
        void PushMailToSomeone(ulong characterId, string name, string content, Dictionary<int, int> items,int type=0);
        void ReloadTable(string tableName);
        IEnumerator ReloadTableCoroutine(Coroutine coroutine, string tableName);

        IEnumerator SaveSnapShot(Coroutine coroutine, CharacterController character, string key, bool forceSave);
        IEnumerator LoadSnapShot(Coroutine coroutine, CharacterController character, string key);

    }

    public class GameMasterDefaultImpl : IGameMaster
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public void Init()
        {
            GameMaster.mData.RegisterCommand(new GMEquipGemCommand());
            GameMaster.mData.RegisterCommand(new GMExdataCommand());
            GameMaster.mData.RegisterCommand(new GMItemCommand());
            GameMaster.mData.RegisterCommand(new GMFalgCommand());
        }

        //调玩家等级
        public void GmSetLevel(CharacterController character, int nLevel)
        {
            //character.mBag.SetRes(eResourcesType.LevelRes, nLevel);
            var nowLevel = character.mBag.GetRes(eResourcesType.LevelRes);
            if (nowLevel == nLevel)
            {
                return;
            }
            if (nowLevel < nLevel)
            {
                character.mBag.AddRes(eResourcesType.LevelRes, nLevel - nowLevel, eCreateItemType.GMAdd);
            }
            else
            {
                character.mBag.DelRes(eResourcesType.LevelRes, nowLevel - nLevel, eDeleteItemType.GMDel);
            }
        }

        //调道具
        public int GmAddItem(CharacterController character, int nId, int nCount)
        {
            return (int) character.mBag.AddItem(nId, nCount, eCreateItemType.GMAdd);
        }

        //直接设置装备
        public void GmSetEquip(CharacterController character, int nId)
        {
            var tbItem = Table.GetItemBase(nId);
            if (tbItem == null)
            {
                return;
            }
            var tbEquip = Table.GetEquip(tbItem.Exdata[0]);
            if (tbEquip == null)
            {
                return;
            }
            var partbag = -1;
            foreach (var i in EquipExtension.Equips)
            {
                if (ItemEquip2.IsCanEquip(tbEquip, i))
                {
                    partbag = i;
                    break;
                }
            }
            if (partbag == -1)
            {
                return;
            }
            var pEquip = character.GetItemByBagByIndex(partbag, 0);
            var SceneChangeType = 1;
            if (pEquip != null)
            {
                character.mBag.mBags[partbag].CleanItemByIndex(0);
                SceneChangeType = 2;
            }
            character.mBag.mBags[partbag].ResetItemByItemId(0, nId);
            character.EquipChange(SceneChangeType, partbag, 0, character.mBag.mBags[partbag].GetItemByIndex(0));
        }

        //设置某个部位的装备重新随机属性
        public void GmSetEquipAttr(CharacterController character, int nPart, int nAddCount)
        {
            var pEquip = character.GetItemByBagByIndex(nPart, 0);
            if (pEquip == null)
            {
                return;
            }
            var tbItem = Table.GetItemBase(pEquip.GetId());
            if (tbItem == null)
            {
                return;
            }
            var tbEquip = Table.GetEquip(tbItem.Exdata[0]);
            if (tbEquip == null)
            {
                return;
            }
            var pEquipItem = (ItemEquip2) pEquip;
            pEquipItem.InitAddAttr(tbEquip, nAddCount);
            character.EquipChange(2, nPart, 0, pEquip);
        }

        //设置某个装备的部位的宝石内容
        public void GmSetEquipGem(CharacterController character,
                                  int nPart,
                                  int Gem1,
                                  int Gem2 = -2,
                                  int Gem3 = -2,
                                  int Gem4 = -2)
        {
            var pEquip = character.GetItemByBagByIndex(nPart, 0);
            if (pEquip == null)
            {
                return;
            }
            var Gem = new int[4] {Gem1, Gem2, Gem3, Gem4};

            for (var i = 0; i < Gem.Length; i++)
            {
                if (Gem[i] < 0)
                {
//设置数据
                    pEquip.SetExdata(i + 2, Gem[i]);
                    continue;
                }
                if (!CheckGeneral.CheckItemType(Gem[i], eItemType.Elf))
                {
//ID不是宝石
                    continue;
                }
                var tbItem = Table.GetItemBase(Gem[i]);
                if (tbItem == null)
                {
//ID没有找到
                    continue;
                }
                pEquip.SetExdata(i + 2, Gem[i]);
            }
            pEquip.MarkDirty();
            character.EquipChange(2, nPart, 0, pEquip);
        }

        //设置技能等级
        public void GmSetSkillLevel(CharacterController character, int nId, int nLevel)
        {
            if (nId != -1)
            {
                character.mSkill.LevelUpSkill(nId, nLevel);
                return;
            }
            var skills = new List<int>();

            foreach (var i in character.mSkill.mDbData.Skills)
            {
                skills.Add(i.Key);
            }
            foreach (var i in skills)
            {
                character.mSkill.LevelUpSkill(i, nLevel);
            }
        }

        //新增技能等级
        public void GmAddSkillLevel(CharacterController character, int nId, int nLevel)
        {
            character.mSkill.LearnSkill(nId, nLevel);
            //character.mSkill.LevelUpSkill(nId, nLevel);
        }

        //新增技能天赋点数
        public void GmAddTalent(CharacterController character, int nPoint, int nId = -1)
        {
            if (nId == -1)
            {
                character.mTalent.AddTalentPoint(nPoint);
                return;
            }
            character.mTalent.AddSkillPoint(nId, nPoint);
            //character.mSkill.LevelUpSkill(nId, nLevel);
        }

        //新增技能天赋点数
        public void GmResetTalent(CharacterController character, int nId = -1)
        {
            if (nId == -1)
            {
                character.mTalent.RefreshTalent(character);
                return;
            }
            character.mTalent.ResetSkillTalent(nId);
            //character.mSkill.LevelUpSkill(nId, nLevel);
        }

        //设置任务完成
        public void GmMissionComplete(CharacterController character, int nId, int type = 0)
        {
            switch (type)
            {
                case 0: //完成任务
                    character.mTask.Complete(character, nId, true);
                    break;
                case 1: //可接任务
                {
                    var tbMis = Table.GetMission(nId);
                    if (tbMis == null)
                    {
                        return;
                    }
                    if (tbMis.FlagId >= 0)
                    {
                        character.SetFlag(tbMis.FlagId, false);
                    }
                }
                    break;
                case 2: //接受任务
                {
                    var tbMis = Table.GetMission(nId);
                    if (tbMis == null)
                    {
                        return;
                    }
                    Mission mis;
                    if (character.mTask.mData.TryGetValue(nId, out mis))
                    {
                        mis.Init(character, Table.GetMission(mis.Id));
                    }
                    else
                    {
                        mis = new Mission(character, nId);
                        character.mTask.mDbData.Missions.Add(nId, mis.mDbData);
                        character.mTask.AddChild(mis);
                        character.mTask.mData[nId] = mis;
                    }
                    mis.MarkDirty();
                }
                    break;
                case 3:
                    break;
            }
        }

        //尝试掉落

        public void GmTestDrop(CharacterController character, int nDropId, int nCount)
        {
            var drops = new Dictionary<int, int>();
            var Totledrops = new Dictionary<int, int>();
            var s = "";
            for (var i = 0; i < nCount; i++)
            {
                drops.Clear();
                character.DropMother(nDropId, drops);
                foreach (var v in drops)
                {
                    Totledrops.modifyValue(v.Key, v.Value);
                }
            }
            var colors = new Dictionary<int, int>();
            var Types = new Dictionary<int, int>();
            foreach (var v in Totledrops)
            {
                var item = Table.GetItemBase(v.Key);
                colors.modifyValue(item.Quality, v.Value);
                Types.modifyValue(item.Type, v.Value);
                PlayerLog.WriteLog((int) LogType.TestDrop, string.Format("[{0},{1},{2}]", v.Key, item.Name, v.Value));
            }
            foreach (var v in colors)
            {
                PlayerLog.WriteLog((int) LogType.TestDrop, string.Format("color = [{0},{1}]", v.Key, v.Value));
            }
            foreach (var v in Types)
            {
                PlayerLog.WriteLog((int) LogType.TestDrop, string.Format("type = [{0},{1}]", v.Key, v.Value));
            }
            if (s.Length > 0)
            {
                PlayerLog.WriteLog((int) LogType.TestDrop, s);
            }
        }

        public void GmTestDrop2(CharacterController character, int nDropId, int nCount)
        {
            var drops = new Dictionary<int, int>();
            var Totledrops = new Dictionary<int, int>();
            var index = 0;
            var s = "";
            for (var i = 0; i < nCount; i++)
            {
                drops.Clear();
                character.DropMother(nDropId, drops);
                index = 0;
                s = "";
                foreach (var v in drops)
                {
                    Totledrops.modifyValue(v.Key, v.Value);
                    if (index == 0)
                    {
                        s = string.Format("{3}:[{0},{1},{2}]", v.Key, Table.GetItemBase(v.Key).Name, v.Value, i);
                    }
                    else
                    {
                        s = string.Format("{0}[{1},{2},{3}]", s, v.Key, Table.GetItemBase(v.Key).Name, v.Value);
                    }
                    index++;
                }
                if (s.Length > 0)
                {
                    PlayerLog.WriteLog((int) LogType.TestDrop, s);
                }
            }
            index = 0;
            s = "";
            foreach (var v in Totledrops)
            {
                if (index == 0)
                {
                    s = string.Format("Totle:[{0},{1},{2}]", v.Key, Table.GetItemBase(v.Key).Name, v.Value);
                }
                else
                {
                    s = string.Format("{0}[{1},{2},{3}]", s, v.Key, Table.GetItemBase(v.Key).Name, v.Value);
                }
                index++;
            }
            if (s.Length > 0)
            {
                PlayerLog.WriteLog((int) LogType.TestDrop, s);
            }
        }

        //设置邮件参数
        public void GmSetMissionParam(CharacterController character, int nId, int nIndex, int nValue)
        {
            character.mTask.SetMissionParam(character, nId, nIndex, nValue);
        }

        //设置邮件
        public void GmSendMail(CharacterController character, string name, string text, Dictionary<int, int> items,int type =0)
        {
            character.mMail.PushMail(name, text, items,"",null,type);
        }

        public void PushMailToSomeone(ulong characterId, string name, string content, Dictionary<int, int> items, int type = 0)
        {
            var character = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (character != null)
            {
                GmSendMail(character, name, content, items,type);
            }
            else
            {
                CharacterManager.Instance.ModifyVolatileData(characterId, DataCategory.LogicCharacter, oldData =>
                {
                    var mail = new DBMail_One();
                    mail.StartTime = DateTime.Now.ToBinary();
                    mail.OverTime = DateTime.Now.AddDays(15).ToBinary();
                    mail.State = 0;
                    mail.Name = name;
                    mail.Text = content;
                    mail.Type = type;
                    if (items != null)
                    {
                        //mail.Reward.AddRange(items);
                        foreach (var i in items)
                        {
                            var itemDb = new ItemBaseData();
                            ShareItemFactory.Create(i.Key, itemDb);
                            itemDb.Count = i.Value;
                            mail.Items.Add(itemDb);
                        }
                    }
                    oldData.NewMails.Add(mail);
                    return oldData;
                });
            }
        }

        //重载表格
        public void ReloadTable(string tableName)
        {
            //Table.ReloadTable(tableName);
            CoroutineFactory.NewCoroutine(ReloadTableCoroutine, tableName).MoveNext();
        }

        //完成当前所有处于“任务中”的随从任务  
        public void PetMissionDone(CharacterController character)
        {
            character.mPetMission.FinishNowDoMission();
        }

        //执行一次 事件表的刷新事件，   就是每隔几小时刷一次的事件   
        public void PetMissionRefresh(CharacterController character)
        {
            character.mPetMission.Refresh();
        }

        public IEnumerator ReloadTableCoroutine(Coroutine coroutine, string tableName)
        {
			var Reloadtable = LogicServer.Instance.LogicAgent.ServerGMCommand("ReloadTable", tableName);
            yield return Reloadtable.SendAndWaitUntilDone(coroutine);
        }

        public void EnterFuben(CharacterController character, int fubenId, SceneParam param)
        {
            var tbFuben = Table.GetFuben(fubenId);
            if (tbFuben == null)
            {
                return;
            }
            var tbScene = Table.GetScene(tbFuben.SceneId);
            if (tbScene == null)
            {
                return;
            }
            var serverId = tbScene.CanCrossServer == 1 ? -1 : character.serverId;
            CoroutineFactory.NewCoroutine(character.AskEnterDungeon, serverId, tbFuben.SceneId, (ulong) 0, param)
                .MoveNext();
        }

        public IEnumerator SaveSnapShot(Coroutine coroutine, CharacterController character, string key, bool forceSave)
        {
            var result = LogicServer.Instance.DB.Get<DBUlong>(coroutine, DataCategory.LoginPlayerName,
                key);
            yield return result;
            if (result.Data != null)
            {
                if (!forceSave)
                {
                    character.Proxy.NotifySnapShotResult((int) eSnapShotResult.SnapShotExist);
                    yield break;
                }
            }

            var msg = LogicServer.Instance.LoginAgent.CreateCharacterByAccountName(character.mGuid, key);
            yield return msg.SendAndWaitUntilDone(coroutine);

            if (msg.ErrorCode!= (int)ErrorCodes.OK)
            {
                character.Proxy.NotifySnapShotResult((int)eSnapShotResult.SnapShotNotExist);
                yield break;
            }

            var loginMsg = LogicServer.Instance.LoginAgent.CloneCharacterDbById(character.mGuid, character.mGuid,
               msg.Response);
            yield return loginMsg.SendAndWaitUntilDone(coroutine);
            if (loginMsg.ErrorCode != 0)
            {
                yield break;
            }

            var logicMsg = LogicServer.Instance.LogicAgent.CloneCharacterDbById(character.mGuid, character.mGuid,
                msg.Response);
            yield return logicMsg.SendAndWaitUntilDone(coroutine);
            if (logicMsg.ErrorCode != 0)
            {
                yield break;
            }

            var sceneMsg = LogicServer.Instance.SceneAgent.CloneCharacterDbById(character.mGuid, character.mGuid,
                msg.Response);
            yield return sceneMsg.SendAndWaitUntilDone(coroutine);
            if (sceneMsg.ErrorCode != 0)
            {
                yield break;
            }

            var chatMsg = LogicServer.Instance.ChatAgent.CloneCharacterDbById(character.mGuid, character.mGuid,
                msg.Response);
            yield return chatMsg.SendAndWaitUntilDone(coroutine);
            if (chatMsg.ErrorCode != 0)
            {
                yield break;
            }
        }


        public IEnumerator LoadSnapShot(Coroutine coroutine, CharacterController character, string key)
        {
            var msg = LogicServer.Instance.LoginAgent.GetCharacterIdByAccountName(0ul, key);
            yield return msg.SendAndWaitUntilDone(coroutine);

            if (msg.Response == 0ul)
            {
                character.Proxy.NotifySnapShotResult((int)eSnapShotResult.SnapShotNotExist);
                yield break;
            }

            var fromId = msg.Response;
            var toId = character.mGuid;

            var loginMsg = LogicServer.Instance.LoginAgent.CloneCharacterDbById(character.mGuid, fromId,toId);
            yield return loginMsg.SendAndWaitUntilDone(coroutine);
            if (loginMsg.ErrorCode != 0)
            {
                yield break;
            }

            var logicMsg = LogicServer.Instance.LogicAgent.CloneCharacterDbById(character.mGuid, fromId, toId);
            yield return logicMsg.SendAndWaitUntilDone(coroutine);
            if (logicMsg.ErrorCode != 0)
            {
                yield break;
            }

            var sceneMsg = LogicServer.Instance.SceneAgent.CloneCharacterDbById(character.mGuid, fromId, toId);
            yield return sceneMsg.SendAndWaitUntilDone(coroutine);
            if (sceneMsg.ErrorCode != 0)
            {
                yield break;
            }

            var chatMsg = LogicServer.Instance.ChatAgent.CloneCharacterDbById(character.mGuid, fromId, toId);
            yield return chatMsg.SendAndWaitUntilDone(coroutine);
            if (chatMsg.ErrorCode != 0)
            {
                yield break;
            }

            character.Proxy.NotifySnapShotResult((int)eSnapShotResult.NeedReload);
        }
    }

    public static class GameMaster
    {
        public static GmManager mData = new GmManager();
        private static IGameMaster mImpl;

        static GameMaster()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (GameMaster), typeof (GameMasterDefaultImpl),
                o => { mImpl = (IGameMaster) o; });
        }

        public static void EnterFuben(CharacterController character, int fubenId, SceneParam param)
        {
            mImpl.EnterFuben(character, fubenId, param);
        }

        //调道具
        public static int GmAddItem(CharacterController character, int nId, int nCount)
        {
            return mImpl.GmAddItem(character, nId, nCount);
        }

        //新增技能等级
        public static void GmAddSkillLevel(CharacterController character, int nId, int nLevel)
        {
            mImpl.GmAddSkillLevel(character, nId, nLevel);
        }

        //新增技能天赋点数
        public static void GmAddTalent(CharacterController character, int nPoint, int nId = -1)
        {
            mImpl.GmAddTalent(character, nPoint, nId);
        }

        //设置任务完成
        public static void GmMissionComplete(CharacterController character, int nId, int type = 0)
        {
            mImpl.GmMissionComplete(character, nId, type);
        }

        //新增技能天赋点数
        public static void GmResetTalent(CharacterController character, int nId = -1)
        {
            mImpl.GmResetTalent(character, nId);
        }

        //设置邮件
        public static void GmSendMail(CharacterController character,
                                      string name,
                                      string text,
                                      Dictionary<int, int> items,int type=0)
        {
            mImpl.GmSendMail(character, name, text, items,type);
        }

        //直接设置装备
        public static void GmSetEquip(CharacterController character, int nId)
        {
            mImpl.GmSetEquip(character, nId);
        }

        //设置某个部位的装备重新随机属性
        public static void GmSetEquipAttr(CharacterController character, int nPart, int nAddCount)
        {
            mImpl.GmSetEquipAttr(character, nPart, nAddCount);
        }

        //设置某个装备的部位的宝石内容
        public static void GmSetEquipGem(CharacterController character,
                                         int nPart,
                                         int Gem1,
                                         int Gem2 = -2,
                                         int Gem3 = -2,
                                         int Gem4 = -2)
        {
            mImpl.GmSetEquipGem(character, nPart, Gem1, Gem2, Gem3, Gem4);
        }

        //调玩家等级
        public static void GmSetLevel(CharacterController character, int nLevel)
        {
            mImpl.GmSetLevel(character, nLevel);
        }

        //设置邮件参数
        public static void GmSetMissionParam(CharacterController character, int nId, int nIndex, int nValue)
        {
            mImpl.GmSetMissionParam(character, nId, nIndex, nValue);
        }

        //设置技能等级
        public static void GmSetSkillLevel(CharacterController character, int nId, int nLevel)
        {
            mImpl.GmSetSkillLevel(character, nId, nLevel);
        }

        //尝试掉落

        public static void GmTestDrop(CharacterController character, int nDropId, int nCount)
        {
            mImpl.GmTestDrop(character, nDropId, nCount);
        }

        public static void GmTestDrop2(CharacterController character, int nDropId, int nCount)
        {
            mImpl.GmTestDrop2(character, nDropId, nCount);
        }

        public static void Init()
        {
            mImpl.Init();
        }

        //完成当前所有处于“任务中”的随从任务  
        public static void PetMissionDone(CharacterController character)
        {
            mImpl.PetMissionDone(character);
        }

        //执行一次 事件表的刷新事件，   就是每隔几小时刷一次的事件   
        public static void PetMissionRefresh(CharacterController character)
        {
            mImpl.PetMissionRefresh(character);
        }

        public static void PushMailToSomeone(ulong characterId, string name, string content, Dictionary<int, int> items,int type = 0)
        {
            mImpl.PushMailToSomeone(characterId, name, content, items,type);
        }

        //重载表格
        public static void ReloadTable(string tableName)
        {
            mImpl.ReloadTable(tableName);
        }

        public static IEnumerator ReloadTableCoroutine(Coroutine coroutine, string tableName)
        {
            return mImpl.ReloadTableCoroutine(coroutine, tableName);
        }

        public static IEnumerator SaveSnapShot(Coroutine coroutine, CharacterController character, string key, bool forceSave = false)
        {
           return mImpl.SaveSnapShot(coroutine, character, key, forceSave);
        }
        public static IEnumerator LoadSnapShot(Coroutine coroutine, CharacterController character, string key)
        {
            return mImpl.LoadSnapShot(coroutine, character, key);
        }

    }

    #endregion

    #region 装备宝石

    public interface IGMEquipGemCommand
    {
        int ExecuteAdd(GMEquipGemCommand _this, ulong characterId);
        int ExecuteDel(GMEquipGemCommand _this, ulong characterId);
        int ExecuteGet(GMEquipGemCommand _this, ulong characterId);
        int ExecuteSet(GMEquipGemCommand _this, ulong characterId);
        object GetResult(GMEquipGemCommand _this);
        bool ValidateAdd(GMEquipGemCommand _this, params string[] args);
        bool ValidateDel(GMEquipGemCommand _this, params string[] args);
        bool ValidateGet(GMEquipGemCommand _this, params string[] args);
        bool ValidateSet(GMEquipGemCommand _this, params string[] args);
    }

    public class GMEquipGemCommandDefaultImpl : IGMEquipGemCommand
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public bool ValidateAdd(GMEquipGemCommand _this, params string[] args)
        {
            return false;
        }

        public int ExecuteAdd(GMEquipGemCommand _this, ulong characterId)
        {
            return -1;
        }

        public bool ValidateDel(GMEquipGemCommand _this, params string[] args)
        {
            return false;
        }

        public int ExecuteDel(GMEquipGemCommand _this, ulong characterId)
        {
            return -1;
        }

        public bool ValidateSet(GMEquipGemCommand _this, params string[] args)
        {
            var nIndex = 0;
            _this.ArgsInt = new List<int>();
            foreach (var s in args)
            {
                int TempInt;
                if (!Int32.TryParse(s, out TempInt))
                {
                    return false;
                }
                _this.ArgsInt.Add(TempInt);
                nIndex++;
            }
            if (nIndex < 2 || nIndex > 5)
            {
                return false;
            }
            return true;
        }

        public int ExecuteSet(GMEquipGemCommand _this, ulong characterId)
        {
            var obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (obj == null)
            {
                return -1;
            }
            switch (_this.ArgsInt.Count)
            {
                case 2:
                {
                    GameMaster.GmSetEquipGem(obj, _this.ArgsInt[0], _this.ArgsInt[1]);
                }
                    break;
                case 3:
                {
                    GameMaster.GmSetEquipGem(obj, _this.ArgsInt[0], _this.ArgsInt[1], _this.ArgsInt[2]);
                }
                    break;
                case 4:
                {
                    GameMaster.GmSetEquipGem(obj, _this.ArgsInt[0], _this.ArgsInt[1], _this.ArgsInt[2], _this.ArgsInt[3]);
                }
                    break;
                case 5:
                {
                    GameMaster.GmSetEquipGem(obj, _this.ArgsInt[0], _this.ArgsInt[1], _this.ArgsInt[2], _this.ArgsInt[3],
                        _this.ArgsInt[4]);
                }
                    break;
            }
            return 0;
        }

        public bool ValidateGet(GMEquipGemCommand _this, params string[] args)
        {
            var nIndex = 0;
            _this.ArgsInt = new List<int>();
            foreach (var s in args)
            {
                int TempInt;
                if (!Int32.TryParse(s, out TempInt))
                {
                    return false;
                }
                _this.ArgsInt.Add(TempInt);
                nIndex++;
            }
            if (nIndex != 1)
            {
                return false;
            }
            return true;
        }

        public int ExecuteGet(GMEquipGemCommand _this, ulong characterId)
        {
            var obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (obj == null)
            {
                return -1;
            }
            var pEquip = obj.GetItemByBagByIndex(_this.ArgsInt[0], 0);
            if (pEquip == null)
            {
                return -1;
            }
            _this.ResultGet.Clear();
            for (var i = 0; i < 4; i++)
            {
                _this.ResultGet.Add(pEquip.GetExdata(i + 2));
            }
            return 0;
        }

        public object GetResult(GMEquipGemCommand _this)
        {
            return _this.ResultGet;
        }
    }

    public class GMEquipGemCommand : GmCommand
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private static IGMEquipGemCommand mImpl;

        static GMEquipGemCommand()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (GMEquipGemCommand),
                typeof (GMEquipGemCommandDefaultImpl),
                o => { mImpl = (IGMEquipGemCommand) o; });
        }

        public List<int> ResultGet = new List<int>();

        public bool ValidateAdd(params string[] args)
        {
            return mImpl.ValidateAdd(this, args);
        }

        public int ExecuteAdd(ulong characterId)
        {
            return mImpl.ExecuteAdd(this, characterId);
        }

        public bool ValidateDel(params string[] args)
        {
            return mImpl.ValidateDel(this, args);
        }

        public int ExecuteDel(ulong characterId)
        {
            return mImpl.ExecuteDel(this, characterId);
        }

        public bool ValidateSet(params string[] args)
        {
            return mImpl.ValidateSet(this, args);
        }

        public int ExecuteSet(ulong characterId)
        {
            return mImpl.ExecuteSet(this, characterId);
        }

        public bool ValidateGet(params string[] args)
        {
            return mImpl.ValidateGet(this, args);
        }

        public int ExecuteGet(ulong characterId)
        {
            return mImpl.ExecuteGet(this, characterId);
        }

        public object GetResult()
        {
            return mImpl.GetResult(this);
        }

        public string Name
        {
            get { return "EquipGem"; }
        }

        public List<int> ArgsInt { get; set; }
    }

    #endregion

    #region 基础道具

    public interface IGMItemCommand
    {
        int ExecuteAdd(GMItemCommand _this, ulong characterId);
        int ExecuteDel(GMItemCommand _this, ulong characterId);
        int ExecuteGet(GMItemCommand _this, ulong characterId);
        int ExecuteSet(GMItemCommand _this, ulong characterId);
        object GetResult(GMItemCommand _this);
        bool ValidateAdd(GMItemCommand _this, params string[] args);
        bool ValidateDel(GMItemCommand _this, params string[] args);
        bool ValidateGet(GMItemCommand _this, params string[] args);
        bool ValidateSet(GMItemCommand _this, params string[] args);
    }

    public class GMItemCommandDefaultImpl : IGMItemCommand
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public bool ValidateAdd(GMItemCommand _this, params string[] args)
        {
            var nIndex = 0;
            _this.ArgsInt = new List<int>();
            foreach (var s in args)
            {
                int TempInt;
                if (!Int32.TryParse(s, out TempInt))
                {
                    return false;
                }
                _this.ArgsInt.Add(TempInt);
                nIndex++;
            }
            if (nIndex != 2)
            {
                return false;
            }
            return true;
        }

        public int ExecuteAdd(GMItemCommand _this, ulong characterId)
        {
            var obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (obj == null)
            {
                return -1;
            }
            return (int) obj.mBag.AddItem(_this.ArgsInt[0], _this.ArgsInt[1], eCreateItemType.GMAdd);
        }

        public bool ValidateDel(GMItemCommand _this, params string[] args)
        {
            var nIndex = 0;
            _this.ArgsInt = new List<int>();
            foreach (var s in args)
            {
                int TempInt;
                if (!Int32.TryParse(s, out TempInt))
                {
                    return false;
                }
                _this.ArgsInt.Add(TempInt);
                nIndex++;
            }
            if (nIndex != 2)
            {
                return false;
            }
            return true;
        }

        public int ExecuteDel(GMItemCommand _this, ulong characterId)
        {
            var obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (obj == null)
            {
                return -1;
            }
            return (int) obj.mBag.DeleteItem(_this.ArgsInt[0], _this.ArgsInt[1], eDeleteItemType.GMDel);
        }

        public bool ValidateSet(GMItemCommand _this, params string[] args)
        {
            var nIndex = 0;
            _this.ArgsInt = new List<int>();
            foreach (var s in args)
            {
                int TempInt;
                if (!Int32.TryParse(s, out TempInt))
                {
                    return false;
                }
                _this.ArgsInt.Add(TempInt);
                nIndex++;
            }
            if (nIndex != 4)
            {
                return false;
            }
            return true;
        }

        public int ExecuteSet(GMItemCommand _this, ulong characterId)
        {
            var obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (obj == null)
            {
                return -1;
            }
            var pItem = obj.GetItemByBagByIndex(_this.ArgsInt[0], _this.ArgsInt[1]);
            if (pItem == null)
            {
                return -1;
            }
            if (pItem.CheckExdata(_this.ArgsInt[2]))
            {
                if (pItem.GetExdata(_this.ArgsInt[2]) != _this.ArgsInt[3])
                {
                    pItem.SetExdata(_this.ArgsInt[2], _this.ArgsInt[3]);
                    pItem.MarkDirty();
                    if (EquipExtension.Equips.Contains(_this.ArgsInt[0]))
                    {
                        obj.EquipChange(2, _this.ArgsInt[0], _this.ArgsInt[1], pItem);
                    }
                }
            }
            else
            {
                return -1;
            }
            return 0;
        }

        public bool ValidateGet(GMItemCommand _this, params string[] args)
        {
            var nIndex = 0;
            _this.ArgsInt = new List<int>();
            foreach (var s in args)
            {
                int TempInt;
                if (!Int32.TryParse(s, out TempInt))
                {
                    return false;
                }
                _this.ArgsInt.Add(TempInt);
                nIndex++;
            }
            if (nIndex != 2)
            {
                return false;
            }
            return true;
        }

        public int ExecuteGet(GMItemCommand _this, ulong characterId)
        {
            var obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (obj == null)
            {
                return -1;
            }
            _this.ResultGet = obj.GetItemByBagByIndex(_this.ArgsInt[0], _this.ArgsInt[1]);
            if (_this.ResultGet == null)
            {
                return -1;
            }
            return 0;
        }

        public object GetResult(GMItemCommand _this)
        {
            return _this.ResultGet;
        }
    }

    public class GMItemCommand : GmCommand
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private static IGMItemCommand mImpl;

        static GMItemCommand()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (GMItemCommand), typeof (GMItemCommandDefaultImpl),
                o => { mImpl = (IGMItemCommand) o; });
        }

        public ItemBase ResultGet;

        public bool ValidateAdd(params string[] args)
        {
            return mImpl.ValidateAdd(this, args);
        }

        public int ExecuteAdd(ulong characterId)
        {
            return mImpl.ExecuteAdd(this, characterId);
        }

        public bool ValidateDel(params string[] args)
        {
            return mImpl.ValidateDel(this, args);
        }

        public int ExecuteDel(ulong characterId)
        {
            return mImpl.ExecuteDel(this, characterId);
        }

        public bool ValidateSet(params string[] args)
        {
            return mImpl.ValidateSet(this, args);
        }

        public int ExecuteSet(ulong characterId)
        {
            return mImpl.ExecuteSet(this, characterId);
        }

        public bool ValidateGet(params string[] args)
        {
            return mImpl.ValidateGet(this, args);
        }

        public int ExecuteGet(ulong characterId)
        {
            return mImpl.ExecuteGet(this, characterId);
        }

        public object GetResult()
        {
            return mImpl.GetResult(this);
        }

        public string Name
        {
            get { return "Item"; }
        }

        public List<int> ArgsInt { get; set; }
    }

    #endregion

    #region 扩展数据

    public interface IGMExdataCommand
    {
        int ExecuteAdd(GMExdataCommand _this, ulong characterId);
        int ExecuteDel(GMExdataCommand _this, ulong characterId);
        int ExecuteGet(GMExdataCommand _this, ulong characterId);
        int ExecuteSet(GMExdataCommand _this, ulong characterId);
        object GetResult(GMExdataCommand _this);
        bool ValidateAdd(GMExdataCommand _this, params string[] args);
        bool ValidateDel(GMExdataCommand _this, params string[] args);
        bool ValidateGet(GMExdataCommand _this, params string[] args);
        bool ValidateSet(GMExdataCommand _this, params string[] args);
    }

    public class GMExdataCommandDefaultImpl : IGMExdataCommand
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public bool ValidateAdd(GMExdataCommand _this, params string[] args)
        {
            var nIndex = 0;
            _this.ArgsInt = new List<int>();
            foreach (var s in args)
            {
                int TempInt;
                if (!Int32.TryParse(s, out TempInt))
                {
                    return false;
                }
                _this.ArgsInt.Add(TempInt);
                nIndex++;
            }
            if (nIndex != 2)
            {
                return false;
            }
            return true;
        }

        public int ExecuteAdd(GMExdataCommand _this, ulong characterId)
        {
            var obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (obj == null)
            {
                return -1;
            }
            obj.AddExData(_this.ArgsInt[0], _this.ArgsInt[1]);
            return 0;
        }

        public bool ValidateDel(GMExdataCommand _this, params string[] args)
        {
            var nIndex = 0;
            _this.ArgsInt = new List<int>();
            foreach (var s in args)
            {
                int TempInt;
                if (!Int32.TryParse(s, out TempInt))
                {
                    return false;
                }
                _this.ArgsInt.Add(TempInt);
                nIndex++;
            }
            if (nIndex != 2)
            {
                return false;
            }
            return true;
        }

        public int ExecuteDel(GMExdataCommand _this, ulong characterId)
        {
            var obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (obj == null)
            {
                return -1;
            }
            obj.AddExData(_this.ArgsInt[0], -_this.ArgsInt[1]);
            return 0;
        }

        public bool ValidateSet(GMExdataCommand _this, params string[] args)
        {
            var nIndex = 0;
            _this.ArgsInt = new List<int>();
            foreach (var s in args)
            {
                int TempInt;
                if (!Int32.TryParse(s, out TempInt))
                {
                    return false;
                }
                _this.ArgsInt.Add(TempInt);
                nIndex++;
            }
            if (nIndex != 2)
            {
                return false;
            }
            return true;
        }

        public int ExecuteSet(GMExdataCommand _this, ulong characterId)
        {
            var obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (obj == null)
            {
                return -1;
            }
            obj.SetExData(_this.ArgsInt[0], _this.ArgsInt[1]);
            return 0;
        }

        public bool ValidateGet(GMExdataCommand _this, params string[] args)
        {
            var nIndex = 0;
            _this.ArgsInt = new List<int>();
            foreach (var s in args)
            {
                int TempInt;
                if (!Int32.TryParse(s, out TempInt))
                {
                    return false;
                }
                _this.ArgsInt.Add(TempInt);
                nIndex++;
            }
            if (nIndex != 1)
            {
                return false;
            }
            return true;
        }

        public int ExecuteGet(GMExdataCommand _this, ulong characterId)
        {
            var obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (obj == null)
            {
                return -1;
            }
            _this.ResultGet = obj.GetExData(_this.ArgsInt[0]);
            return 0;
        }

        public object GetResult(GMExdataCommand _this)
        {
            return _this.ResultGet;
        }
    }

    public class GMExdataCommand : GmCommand
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private static IGMExdataCommand mImpl;

        static GMExdataCommand()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (GMExdataCommand),
                typeof (GMExdataCommandDefaultImpl),
                o => { mImpl = (IGMExdataCommand) o; });
        }

        public int ResultGet;

        public bool ValidateAdd(params string[] args)
        {
            return mImpl.ValidateAdd(this, args);
        }

        public int ExecuteAdd(ulong characterId)
        {
            return mImpl.ExecuteAdd(this, characterId);
        }

        public bool ValidateDel(params string[] args)
        {
            return mImpl.ValidateDel(this, args);
        }

        public int ExecuteDel(ulong characterId)
        {
            return mImpl.ExecuteDel(this, characterId);
        }

        public bool ValidateSet(params string[] args)
        {
            return mImpl.ValidateSet(this, args);
        }

        public int ExecuteSet(ulong characterId)
        {
            return mImpl.ExecuteSet(this, characterId);
        }

        public bool ValidateGet(params string[] args)
        {
            return mImpl.ValidateGet(this, args);
        }

        public int ExecuteGet(ulong characterId)
        {
            return mImpl.ExecuteGet(this, characterId);
        }

        public object GetResult()
        {
            return mImpl.GetResult(this);
        }

        public string Name
        {
            get { return "Exdata"; }
        }

        public List<int> ArgsInt { get; set; }
    }

    #endregion

    #region 标记位

    public interface IGMFalgCommand
    {
        int ExecuteAdd(GMFalgCommand _this, ulong characterId);
        int ExecuteDel(GMFalgCommand _this, ulong characterId);
        int ExecuteGet(GMFalgCommand _this, ulong characterId);
        int ExecuteSet(GMFalgCommand _this, ulong characterId);
        object GetResult(GMFalgCommand _this);
        bool ValidateAdd(GMFalgCommand _this, params string[] args);
        bool ValidateDel(GMFalgCommand _this, params string[] args);
        bool ValidateGet(GMFalgCommand _this, params string[] args);
        bool ValidateSet(GMFalgCommand _this, params string[] args);
    }

    public class GMFalgCommandDefaultImpl : IGMFalgCommand
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public bool ValidateAdd(GMFalgCommand _this, params string[] args)
        {
            var nIndex = 0;
            _this.ArgsInt = new List<int>();
            foreach (var s in args)
            {
                int TempInt;
                if (!Int32.TryParse(s, out TempInt))
                {
                    return false;
                }
                _this.ArgsInt.Add(TempInt);
                nIndex++;
            }
            if (nIndex != 1)
            {
                return false;
            }
            return true;
        }

        public int ExecuteAdd(GMFalgCommand _this, ulong characterId)
        {
            var obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (obj == null)
            {
                return -1;
            }
            obj.SetFlag(_this.ArgsInt[0]);
            return 0;
        }

        public bool ValidateDel(GMFalgCommand _this, params string[] args)
        {
            var nIndex = 0;
            _this.ArgsInt = new List<int>();
            foreach (var s in args)
            {
                int TempInt;
                if (!Int32.TryParse(s, out TempInt))
                {
                    return false;
                }
                _this.ArgsInt.Add(TempInt);
                nIndex++;
            }
            if (nIndex != 1)
            {
                return false;
            }
            return true;
        }

        public int ExecuteDel(GMFalgCommand _this, ulong characterId)
        {
            var obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (obj == null)
            {
                return -1;
            }
            obj.SetFlag(_this.ArgsInt[0], false);
            return 0;
        }

        public bool ValidateSet(GMFalgCommand _this, params string[] args)
        {
            var nIndex = 0;
            _this.ArgsInt = new List<int>();
            foreach (var s in args)
            {
                int TempInt;
                if (!Int32.TryParse(s, out TempInt))
                {
                    return false;
                }
                _this.ArgsInt.Add(TempInt);
                nIndex++;
            }
            if (nIndex != 2)
            {
                return false;
            }
            return true;
        }

        public int ExecuteSet(GMFalgCommand _this, ulong characterId)
        {
            var obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (obj == null)
            {
                return -1;
            }
            if (_this.ArgsInt[1] == 1)
            {
                obj.SetFlag(_this.ArgsInt[0]);
            }
            else
            {
                obj.SetFlag(_this.ArgsInt[0], false);
            }
            return 0;
        }

        public bool ValidateGet(GMFalgCommand _this, params string[] args)
        {
            var nIndex = 0;
            _this.ArgsInt = new List<int>();
            foreach (var s in args)
            {
                int TempInt;
                if (!Int32.TryParse(s, out TempInt))
                {
                    return false;
                }
                _this.ArgsInt.Add(TempInt);
                nIndex++;
            }
            if (nIndex != 1)
            {
                return false;
            }
            return true;
        }

        public int ExecuteGet(GMFalgCommand _this, ulong characterId)
        {
            var obj = CharacterManager.Instance.GetCharacterControllerFromMemroy(characterId);
            if (obj == null)
            {
                return -1;
            }
            _this.ResultGet = obj.GetFlag(_this.ArgsInt[0]);
            return 0;
        }

        public object GetResult(GMFalgCommand _this)
        {
            return _this.ResultGet;
        }
    }

    public class GMFalgCommand : GmCommand
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private static IGMFalgCommand mImpl;

        static GMFalgCommand()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (GMFalgCommand), typeof (GMFalgCommandDefaultImpl),
                o => { mImpl = (IGMFalgCommand) o; });
        }

        public bool ResultGet;

        public bool ValidateAdd(params string[] args)
        {
            return mImpl.ValidateAdd(this, args);
        }

        public int ExecuteAdd(ulong characterId)
        {
            return mImpl.ExecuteAdd(this, characterId);
        }

        public bool ValidateDel(params string[] args)
        {
            return mImpl.ValidateDel(this, args);
        }

        public int ExecuteDel(ulong characterId)
        {
            return mImpl.ExecuteDel(this, characterId);
        }

        public bool ValidateSet(params string[] args)
        {
            return mImpl.ValidateSet(this, args);
        }

        public int ExecuteSet(ulong characterId)
        {
            return mImpl.ExecuteSet(this, characterId);
        }

        public bool ValidateGet(params string[] args)
        {
            return mImpl.ValidateGet(this, args);
        }

        public int ExecuteGet(ulong characterId)
        {
            return mImpl.ExecuteGet(this, characterId);
        }

        public object GetResult()
        {
            return mImpl.GetResult(this);
        }

        public string Name
        {
            get { return "Falg"; }
        }

        public List<int> ArgsInt { get; set; }
    }

    #endregion
}