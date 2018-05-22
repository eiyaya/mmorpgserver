#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Database;
using DataContract;
using DataTable;
using EventSystem;
using Scorpion;
using NLog;
using Shared;

#endregion

namespace Logic
{
    public class Mail : NodeBase
    {
        //构造邮件
        public Mail(CharacterController character, DBMail_One dbdata)
        {
            mCharacter = character;
            mDbData = dbdata;
        }

        public Mail(CharacterController character, ulong nId)
        {
            mCharacter = character;
            mDbData = new DBMail_One();
            mDbData.Guid = nId;
            mDbData.StartTime = DateTime.Now.ToBinary();
            mDbData.State = 0;
        }

        public CharacterController mCharacter; //所在角色
        public DBMail_One mDbData;

        public override IEnumerable<NodeBase> Children
        {
            get { return null; }
        }

        public ulong Guid
        {
            get { return mDbData.Guid; }
        }

        public string Name
        {
            get { return mDbData.Name; }
            set { mDbData.Name = value; }
        }

        public long OverTime
        {
            get { return mDbData.OverTime; }
            set { mDbData.OverTime = value; }
        }

        public List<ItemBaseData> Reward
        {
            get { return mDbData.Items; }
            set
            {
                mDbData.Items.Clear();
                foreach (var i in value)
                {
                    mDbData.Items.Add(i);
                }
            }
        }

        public string Send
        {
            get { return mDbData.Send; }
            set { mDbData.Send = value; }
        }

        public long StartTime
        {
            get { return mDbData.StartTime; }
            set { mDbData.StartTime = value; }
        }

        public int Type
        {
            get { return mDbData.Type; }
            set { mDbData.Type = value; }
        }
        public int State
        {
            get { return mDbData.State; }
            set
            {
                if (mDbData.State == value)
                {
                    return;
                }
                mDbData.State = value;
                MarkDbDirty();
            }
        }

        public string Text
        {
            get { return mDbData.Text; }
            set { mDbData.Text = value; }
        }
        public int ExtendType
        {
            get { return mDbData.ExtendType; }
            set { mDbData.ExtendType = value; }
        }

        public string ExtendPara0
        {
            get { return mDbData.ExtendPara0; }
            set { mDbData.ExtendPara0 = value; }
        }
        public string ExtendPara1
        {
            get { return mDbData.ExtendPara1; }
            set { mDbData.ExtendPara1 = value; }
        }
    }

    public interface IMailManager
    {
        ErrorCodes DeleteMail(MailManager _this, ulong uId);
        Mail GetMail(MailManager _this, ulong uId);
        Mail GetTimeMin(MailManager _this);
        void InitByDB(MailManager _this, CharacterController character, DBMail_List mails);

        Mail PushMail(MailManager _this,
                      string name,
                      string text,
                      Dictionary<int, int> reward,
                      string sender = "",
                      List<ItemBaseData> datas = null,int type=0);

        Mail PushMail(MailManager _this, string name, string text, List<ItemBaseData> datas,int type,string sender);
        void PushMail(MailManager _this, int tableId, int ExtendType, string ExtendPara0, string ExtendPara1);
        void PushMail(MailManager _this, int tableId);
        ErrorCodes ReceiveMail(MailManager _this, ulong uId);
    }

    public class MailManagerDefaultImpl : IMailManager
    {
       
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //

        public void InitByDB(MailManager _this, CharacterController character, DBMail_List mails)
        {
            List<DBMail_One> deleteList = new List<DBMail_One>();
            _this.mCharacter = character;
            _this.mDbData = mails;
            var count = 0;
            foreach (var dbmail in _this.mDbData.mData)
            {
                if (DateTime.FromBinary(dbmail.OverTime) < DateTime.Now)
                {
                    deleteList.Add(dbmail);
                    continue;
                }
                var mail = new Mail(character, dbmail);
                _this.Mails.Add(dbmail.Guid, mail);
                _this.AddChild(mail);
                count++;
            }
            foreach (var dbmail in deleteList)
            {
                _this.mDbData.mData.Remove(dbmail);
            }
            _this.GetCanAcceptMail();
        }

        #region 表格相关

        public void PushMail(MailManager _this, int tableId)
        {
            var tbMail = Table.GetMail(tableId);
            if (tbMail == null)
            {
				Logger.Fatal("null==Table.GetMail({0})", tableId);
                return;
            }
            if (tbMail.Flag < 0)
            {
                //Logger.Warn("PushMail tbMail[{0}] Flag is -1", tbMail.Id);

                PlayerLog.WriteLog((int) LogType.MailConfigError, "PushMail tbMail[{0}] Flag is -1", tbMail.Id);
                return;
            }
            if (_this.mCharacter.GetFlag(tbMail.Flag))
            {
                return;
            }
            if (_this.mCharacter.CheckCondition(tbMail.Condition) != -2)
            {
                return;
            }
            var nextId = _this.GetNextId();
            var tempmail = new Mail(_this.mCharacter, nextId);
            tempmail.Name = tbMail.Title;
            tempmail.Text = tbMail.Text;
            tempmail.Send = tbMail.Sender;
            for (var i = 0; i != 5; ++i)
            {
                if (tbMail.ItemId[i] < 0)
                {
                    continue;
                }
                if (tbMail.ItemCount[i] < 1)
                {
                    continue;
                }
                var itemDb = new ItemBaseData();
                ShareItemFactory.Create(tbMail.ItemId[i], itemDb);
                itemDb.Count = tbMail.ItemCount[i];
                tempmail.Reward.Add(itemDb);
                //tempmail.Reward.Add(tbMail.ItemId[i],tbMail.ItemCount[i]);
            }
            tempmail.OverTime = DateTime.Now.AddDays(15).ToBinary();
            _this.Mails.Add(tempmail.Guid, tempmail);
            _this.AddChild(tempmail);
            _this.mDbData.mData.Add(tempmail.mDbData);
            tempmail.MarkDirty();
            _this.mCharacter.SetFlag(tbMail.Flag);
        }
        //通知玩家被谁杀死
        public void PushMail(MailManager _this, int tableId, int ExtendType, string ExtendPara0, string ExtendPara1)
        {
            var tbMail = Table.GetMail(tableId);
            if (tbMail == null)
            {
                Logger.Fatal("null==Table.GetMail({0})", tableId);
                return;
            }
          //  if (tbMail.Flag < 0)
          //  {
          //      PlayerLog.WriteLog((int)LogType.MailConfigError, "PushMail tbMail[{0}] Flag is -1", tbMail.Id);
         //       return;
          //  }
           // if (_this.mCharacter.GetFlag(tbMail.Flag))
          //  {
           //     return;
          //  }
            if (_this.mCharacter.CheckCondition(tbMail.Condition) != -2)
            {
                return;
            }
            var nextId = _this.GetNextId();
            var tempmail = new Mail(_this.mCharacter, nextId);
            tempmail.Name = tbMail.Title;
            tempmail.Text = tbMail.Text;
            tempmail.Send = tbMail.Sender;
            tempmail.ExtendType = ExtendType;
            tempmail.ExtendPara0 = ExtendPara0;
            tempmail.ExtendPara1 = ExtendPara1;

            for (var i = 0; i != 5; ++i)
            {
                if (tbMail.ItemId[i] < 0)
                {
                    continue;
                }
                if (tbMail.ItemCount[i] < 1)
                {
                    continue;
                }
                var itemDb = new ItemBaseData();
                ShareItemFactory.Create(tbMail.ItemId[i], itemDb);
                itemDb.Count = tbMail.ItemCount[i];
                tempmail.Reward.Add(itemDb);
                //tempmail.Reward.Add(tbMail.ItemId[i],tbMail.ItemCount[i]);
            }
            tempmail.OverTime = DateTime.Now.AddDays(15).ToBinary();
            _this.Mails.Add(tempmail.Guid, tempmail);
            _this.AddChild(tempmail);
            _this.mDbData.mData.Add(tempmail.mDbData);
            tempmail.MarkDirty();
          //  _this.mCharacter.SetFlag(tbMail.Flag);


        }

        #endregion

        #region   逻辑接口：接收、领取、删除

        //接收邮件
        public Mail PushMail(MailManager _this,
                             string name,
                             string text,
                             Dictionary<int, int> reward,
                             string sender = "",
                             List<ItemBaseData> datas = null,int type=0)
        {
            // pushMailCount 记录邮件的发送次数
            var pushMailCount = 0;
            Mail mail = null;
            var items = new List<KeyValuePair<int, int>>();
            var index = 1;

            // 拆分邮件中的附件
            foreach (var i in reward)
            {
                var tbItem = Table.GetItemBase(i.Key);
                if (tbItem == null)
                {
                    Logger.Error("Get a error item ID="+i.Key.ToString());                    
                    continue;
                }
                    
                var count = i.Value;
                if (count < 1)
                {
                    continue;
                }
                if (tbItem.MaxCount == -1 || tbItem.MaxCount >= count)
                {
                    if (items.Count >= 5)
                    {
                        mail = PushMail(_this, string.Format("{0}{1}", name, string.Format("({0})", index)), text, items,
                            sender, datas,type);
                        index++;
                        items = new List<KeyValuePair<int, int>>();
                        pushMailCount++;
                    }
                    items.Add(new KeyValuePair<int, int>(i.Key, count));
                    continue;
                }
                do
                {
                    if (items.Count >= 5)
                    {
                        mail = PushMail(_this, string.Format("{0}{1}", name, string.Format("({0})", index)), text, items,
                            sender, datas,type);
                        index++;
                        items = new List<KeyValuePair<int, int>>();
                        pushMailCount++;
                    }
                    if (tbItem.MaxCount < count)
                    {
                        items.Add(new KeyValuePair<int, int>(i.Key, tbItem.MaxCount));
                        count -= tbItem.MaxCount;
                    }
                    else
                    {
                        items.Add(new KeyValuePair<int, int>(i.Key, count));
                        count = 0;
                    }
                } while (count > 0);
            }

            // 如果邮件的发送次数为0，应该发一封
            if (items.Count > 0 || pushMailCount == 0)
            {
                mail = PushMail(_this, string.Format("{0}{1}", name, index == 1 ? "" : string.Format("({0})", index)),
                    text, items, sender, datas,type);
            }
            return mail;
            //if (Mails.Count >= StaticParam.MaxMailCount)
            //{
            //    Mail minMail = GetTimeMin();
            //    if (minMail == null) return null;
            //    minMail.Name = name;
            //    minMail.Text = text;
            //    minMail.StartTime = DateTime.Now.ToBinary();
            //    minMail.OverTime = DateTime.Now.AddDays(15).ToBinary();
            //    minMail.State = 0;
            //    if (reward == null)
            //    {
            //        minMail.Reward.Clear();
            //    }
            //    else
            //    {
            //        foreach (KeyValuePair<int, int> i in reward)
            //        {
            //            ItemBaseData itemDb = new ItemBaseData();
            //            ItemFactory.Create(i.Key, itemDb);
            //            itemDb.Count = i.Value;
            //            minMail.Reward.Clear();
            //            minMail.Reward.Add(itemDb);
            //        }
            //        //minMail.Reward = reward;
            //    }
            //    minMail.MarkDirty();
            //    return minMail;
            //}
            //ulong nextId = GetNextId();
            //Mail tempmail = new Mail(mCharacter, nextId);
            //tempmail.Name = name;
            //tempmail.Send = sender;
            //tempmail.Text = text;
            //if (reward != null)
            //{
            //    //tempmail.Reward = reward;
            //    foreach (KeyValuePair<int, int> i in reward)
            //    {
            //        ItemBaseData itemDb = new ItemBaseData();
            //        ItemFactory.Create(i.Key, itemDb);
            //        itemDb.Count = i.Value;
            //        tempmail.Reward.Add(itemDb);
            //        if (datas != null)
            //        {
            //            datas.Add(itemDb);
            //        }
            //    }
            //}
            //tempmail.OverTime = DateTime.Now.AddDays(15).ToBinary();
            ////tempmail.OverTime = DateTime.Now.AddHours(15).ToBinary();
            //Mails.Add(tempmail.Guid, tempmail);
            //AddChild(tempmail);
            //mDbData.mData.Add(tempmail.mDbData);
            //tempmail.MarkDirty();
            //return tempmail;
        }

        //接收邮件
        public Mail PushMail(MailManager _this, string name, string text, List<ItemBaseData> datas,int type,string sender)
        {
            if (_this.Mails.Count >= StaticParam.MaxMailCount)
            {
                var minMail = GetTimeMin(_this);
                if (minMail == null)
                {
                    return null;
                }
                minMail.Name = name;
                minMail.Text = text;
                minMail.StartTime = DateTime.Now.ToBinary();
                minMail.OverTime = DateTime.Now.AddDays(15).ToBinary();
                minMail.State = 0;
                minMail.Type = type;
                minMail.Send = sender;
                if (datas == null)
                {
                    minMail.Reward.Clear();
                }
                else
                {
                    minMail.Reward.Clear();
                    foreach (var data in datas)
                    {
                        minMail.Reward.Add(data);
                    }
                }
                minMail.MarkDirty();
                return minMail;
            }
            var nextId = _this.GetNextId();
            var tempmail = new Mail(_this.mCharacter, nextId);
            tempmail.Name = name;
            tempmail.Text = text;
            tempmail.Type = type;
            foreach (var data in datas)
            {
                tempmail.Reward.Add(data);
            }
            tempmail.OverTime = DateTime.Now.AddDays(15).ToBinary();
            _this.Mails.Add(tempmail.Guid, tempmail);
            _this.AddChild(tempmail);
            _this.mDbData.mData.Add(tempmail.mDbData);
            tempmail.MarkDirty();
            return tempmail;
        }

        //分发邮件
        private Mail PushMail(MailManager _this,
                              string name,
                              string text,
                              List<KeyValuePair<int, int>> reward,
                              string sender = "",
                              List<ItemBaseData> datas = null,int type=0)
        {
            if (_this.Mails.Count >= StaticParam.MaxMailCount)
            {
                var minMail = GetTimeMin(_this);
                if (minMail == null)
                {
                    return null;
                }
                minMail.Name = name;
                minMail.Text = text;
                minMail.Send = sender;
                minMail.StartTime = DateTime.Now.ToBinary();
                minMail.OverTime = DateTime.Now.AddDays(15).ToBinary();
                minMail.State = 0;
                minMail.Type = type;
                if (reward == null)
                {
                    minMail.Reward.Clear();
                }
                else
                {
                    minMail.Reward.Clear();
                    foreach (var i in reward)
                    {
                        var itemDb = new ItemBaseData();
                        ShareItemFactory.Create(i.Key, itemDb);
                        itemDb.Count = i.Value;
                        minMail.Reward.Add(itemDb);
                        if (datas != null)
                        {
                            datas.Add(itemDb);
                        }
                    }
                    //foreach (KeyValuePair<int, int> i in reward)
                    //{
                    //    ItemBaseData itemDb = new ItemBaseData();
                    //    ItemFactory.Create(i.Key, itemDb);
                    //    itemDb.Count = i.Value;
                    //    minMail.Reward.Clear();
                    //    minMail.Reward.Add(itemDb);
                    //}
                }
                minMail.MarkDirty();
                return minMail;
            }
            var nextId = _this.GetNextId();
            var tempmail = new Mail(_this.mCharacter, nextId);
            tempmail.Name = name;
            tempmail.Send = sender;
            tempmail.Text = text;
            tempmail.Type = type;
            if (reward != null)
            {
                foreach (var i in reward)
                {
                    var itemDb = new ItemBaseData();
                    ShareItemFactory.Create(i.Key, itemDb);
                    itemDb.Count = i.Value;
                    tempmail.Reward.Add(itemDb);
                    if (datas != null)
                    {
                        datas.Add(itemDb);
                    }
                }
                //foreach (KeyValuePair<int, int> i in reward)
                //{
                //    ItemBaseData itemDb = new ItemBaseData();
                //    ItemFactory.Create(i.Key, itemDb);
                //    itemDb.Count = i.Value;
                //    tempmail.Reward.Add(itemDb);
                //    if (datas != null)
                //    {
                //        datas.Add(itemDb);
                //    }
                //}
            }
            tempmail.OverTime = DateTime.Now.AddDays(15).ToBinary();
            //tempmail.OverTime = DateTime.Now.AddHours(15).ToBinary();
            _this.Mails.Add(tempmail.Guid, tempmail);
            _this.AddChild(tempmail);
            _this.mDbData.mData.Add(tempmail.mDbData);
            tempmail.MarkDirty();
            return tempmail;
        }

        //获取邮件
        public Mail GetMail(MailManager _this, ulong uId)
        {
            Mail mail;
            if (_this.Mails.TryGetValue(uId, out mail))
            {
                return mail;
            }
            Logger.Warn("ReceiveMail not find uId={0}", uId);
            return null;
        }

        //获取时间最早的一份邮件
        public Mail GetTimeMin(MailManager _this)
        {
            Mail minOne = null;
            var overTimeMin = DateTime.Now.AddMonths(1);
            foreach (var mail in _this.Mails)
            {
                var ot = DateTime.FromBinary(mail.Value.OverTime);
                if (ot <= overTimeMin)
                {
                    overTimeMin = ot;
                    minOne = mail.Value;
                }
            }
            return minOne;
        }

        //领取邮件
        public ErrorCodes ReceiveMail(MailManager _this, ulong uId)
        {
            var mail = GetMail(_this, uId);
            if (mail == null)
            {
                return ErrorCodes.Error_MailNotFind;
            }
            if (mail.State == (int) MailStateType.Receive)
            {
                return ErrorCodes.Error_MailReceiveOver;
            }
            if (mail.Reward.Count > 0)
            {
                //有物品需要奖励
                ErrorCodes result;
                if (mail.Reward.Count == 1)
                {
                    //result = mCharacter.mBag.AddItem(mail.Reward[0].ItemId, mail.Reward[0].Count, eCreateItemType.Mail);
                    result = _this.mCharacter.mBag.AddItem(mail.Reward[0], eCreateItemType.Mail);
                    if (result == ErrorCodes.OK)
                    {
                        mail.State = (int) MailStateType.Receive;
                    }
                    return result;
                }
                //多个物品需要奖励时
                var items = new Dictionary<int, int>();
                foreach (var i in mail.Reward)
                {
                    if (i.Count > 0)
                        items.modifyValue(i.ItemId, i.Count);
                }
                result = BagManager.CheckAddItemList(_this.mCharacter.mBag, items);
                if (result == ErrorCodes.OK)
                {
                    foreach (var i in mail.Reward)
                    {
                        if (i.Count > 0)
                            _this.mCharacter.mBag.AddItem(i, eCreateItemType.Mail);
                    }
                    mail.State = (int) MailStateType.Receive;
                }
                else
                {
                    return result;
                }
            }
            else
            {
                mail.State = (int)MailStateType.OldMail;
            }
            return ErrorCodes.OK;
        }

        //删除邮件
        public ErrorCodes DeleteMail(MailManager _this, ulong uId)
        {
            Mail mail;
            if (!_this.Mails.TryGetValue(uId, out mail))
            {
                Logger.Warn("DeleteMail not find uId={0}", uId);
                return ErrorCodes.Unknow;
            }
            _this.Mails.Remove(uId);
            _this.mDbData.mData.Remove(mail.mDbData);
            return ErrorCodes.OK;
        }

        #endregion
    }


    public class MailManager : NodeBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //
        private static IMailManager mStaticImpl;

        public static Dictionary<int, List<int>> TriggerMail = new Dictionary<int, List<int>>();
            //Key=条件ID      Value=影响的邮件列表

        static MailManager()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (MailManager), typeof (MailManagerDefaultImpl),
                o => { mStaticImpl = (IMailManager) o; });
        }

        public Dictionary<ulong, Mail> Mails = new Dictionary<ulong, Mail>();
        public CharacterController mCharacter; //所在角色
        public DBMail_List mDbData;

        public override IEnumerable<NodeBase> Children
        {
            get { return Mails.Values; }
        }

        public ulong GmGuid
        {
            get { return mDbData.GmGuid; }
            set { mDbData.GmGuid = value; }
        }

        public override void NetDirtyHandle()
        {
            var msg = new MailList();
            foreach (var mail in Children)
            {
                if (mail.NetDirty) //脏邮件
                {
                    var thisMail = (Mail) mail;
                    var tempMail = new MailCell
                    {
                        Guid = thisMail.Guid,
                        StartTime = thisMail.OverTime,
                        Name = thisMail.Name,
                        Type = thisMail.Type
                    };

                    if (thisMail.Reward.Count > 0)
                    {
                        tempMail.State = thisMail.State + 100;
                    }
                    else
                    {
                        tempMail.State = thisMail.State + 200;
                    }
                    msg.Mails.Add(tempMail);
                }
            }
            mCharacter.Proxy.SyncMails(msg);
        }

        #region 表格相关

        public void PushMail(int tableId)
        {
            mStaticImpl.PushMail(this, tableId);
        }
        public void PushMail(int tableId, int ExtendType, string ExtendPara0, string ExtendPara1)
        {
            mStaticImpl.PushMail(this, tableId, ExtendType, ExtendPara0, ExtendPara1);
        }
        #endregion

        #region   初始化

        //初始化静态数据
        public static void Init()
        {
            var str = Table.GetServerConfig(2005).Value.Trim();
            if (string.IsNullOrEmpty(str) == false)
            {
                var Ids = str.Split('|');
                foreach (var id in Ids)
                {
                    var tb = Table.GetMail(int.Parse(id));
                    if(tb != null)
                        InitOneMail(tb.Id, tb.Condition);
                }
            }

            EventDispatcher.Instance.AddEventListener(ChacacterFlagTrue.EVENT_TYPE, FlagTrueEvent);
            EventDispatcher.Instance.AddEventListener(ChacacterFlagFalse.EVENT_TYPE, FlagFalseEvent);
            EventDispatcher.Instance.AddEventListener(CharacterExdataChange.EVENT_TYPE, ExDataChangeEvent);

            CoroutineFactory.NewCoroutine(GetGmMailId).MoveNext();
        }

        private static IEnumerator GetGmMailId(Coroutine co)
        {
            for (var serverId = 0; serverId != 15; ++serverId)
            {
                var mailId = LogicServer.Instance.DB.GetCurrentId(co, (int) DataCategory.GameMasterMail,
                    serverId.ToString());
                yield return mailId;
                if (mailId.Status != DataStatus.Ok)
                {
                    yield break;
                }
                SetNowGmMailGuid(serverId, mailId.Data);
            }
        }

        //构造所有条件影响的邮件(当某个条件被触发时，可以快速知道哪个邮件可以接了)
        private static void InitOneMail(int mailId, int conditionid)
        {
            List<int> tempList;
            if (TriggerMail.TryGetValue(conditionid, out tempList))
            {
                tempList.Add(mailId);
            }
            else
            {
                tempList = new List<int> {mailId};
                TriggerMail[conditionid] = tempList;
            }
        }


        //创建时的初始化
        public DBMail_List InitByBase(CharacterController character)
        {
            mDbData = new DBMail_List();
            mDbData.NextGuid = 1;
            mCharacter = character;
            MarkDirty();
            return mDbData;
        }

        public void InitByDB(CharacterController character, DBMail_List mails)
        {
            mStaticImpl.InitByDB(this, character, mails);
        }

        public void PushNewMailByDb(DBMail_One dbmail)
        {
            if (DateTime.FromBinary(dbmail.OverTime) < DateTime.Now)
            {
                return;
            }
            var nextId = GetNextId();
            dbmail.Guid = nextId;
            var mail = new Mail(mCharacter, dbmail);
            Mails.Add(dbmail.Guid, mail);
            mDbData.mData.Add(dbmail);
            AddChild(mail);
        }

        public void GetCanAcceptMail()
        {
            var str = Table.GetServerConfig(2005).Value.Trim();
            if (string.IsNullOrEmpty(str) == false)
            {
                var Ids = str.Split('|');
                foreach (var id in Ids)
                {
                    var tb = Table.GetMail(int.Parse(id));
                    if (tb != null)
                    {
                        if (mCharacter.GetFlag(tb.Flag))
                        {
                            continue ;
                        }
                        if (mCharacter.CheckCondition(tb.Condition) == -2)
                        {
                            PushMail(tb.Id);
                        }
                    }
                }
            }

            //Table.ForeachMail(record =>
            //{
            //    if (record.Flag < 0 && record.Condition != -1)
            //    {
            //        Logger.Warn("Mail[{0}] Flag is -1", record.Flag);
            //        return true;
            //    }
            //    if (mCharacter.GetFlag(record.Flag))
            //    {
            //        return true;
            //    }
            //    if (mCharacter.CheckCondition(record.Condition) == -2)
            //    {
            //        PushMail(record.Id);
            //    }
            //    //mCharacter.
            //    return true;
            //});
        }

        public ulong GetNextId()
        {
            return mDbData.NextGuid++;
        }

        #endregion

        #region   逻辑接口：接收、领取、删除

        //接收邮件
        public Mail PushMail(string name,
                             string text,
                             Dictionary<int, int> reward,
                             string sender = "",
                             List<ItemBaseData> datas = null,int type=0)
        {
            return mStaticImpl.PushMail(this, name, text, reward, sender, datas,type);
        }

        //接收邮件
        public Mail PushMail(string name, string text, List<ItemBaseData> datas,int type=0,string sender = "")
        {
            return mStaticImpl.PushMail(this, name, text, datas,type,sender);
        }
  
        //获取邮件
        public Mail GetMail(ulong uId)
        {
            return mStaticImpl.GetMail(this, uId);
        }

        //领取邮件
        public ErrorCodes ReceiveMail(ulong uId)
        {
            return mStaticImpl.ReceiveMail(this, uId);
        }

        //删除邮件
        public ErrorCodes DeleteMail(ulong uId)
        {
            return mStaticImpl.DeleteMail(this, uId);
        }

        #endregion

        #region   事件相关

        private static void ExDataChangeEvent(IEvent ievent)
        {
            var ee = ievent as CharacterExdataChange;
            TriggerMailByEvent(ee.character, eEventType.ExDataChange, ee.ExdataId);
        }

        private static void FlagFalseEvent(IEvent ievent)
        {
            var ee = ievent as ChacacterFlagFalse;
            TriggerMailByEvent(ee.character, eEventType.Falseflag, ee.FlagId);
        }

        private static void FlagTrueEvent(IEvent ievent)
        {
            var ee = ievent as ChacacterFlagTrue;
            TriggerMailByEvent(ee.character, eEventType.Trueflag, ee.FlagId);
        }

        //触发邮件靠事件
        private static readonly Dictionary<int, int> maillist = new Dictionary<int, int>();

        public static void TriggerMailByEvent(CharacterController character,
                                              eEventType type,
                                              int param0 = 0,
                                              int param1 = 0)
        {
            //获得该事件影响的条件
            var conlist = ConditionManager.EventTriggerCondition(type, param0);
            if (conlist == null)
            {
                return;
            }
            //整理这些条件影响了哪些邮件
            maillist.Clear();
            foreach (var i in conlist)
            {
                ConditionMail(character, maillist, i.Key);
            }
            //尝试接受这些邮件
            foreach (var i in maillist)
            {
                //character.mTask.TryAccept(character, i.Key);
                character.mMail.PushMail(i.Key);
            }
        }

        //查看条件是否完成，然后影响到邮件的
        private static void ConditionMail(CharacterController character, Dictionary<int, int> maillist, int nConId)
        {
            //看这个条件是否有影响的邮件
            List<int> tempList;
            if (!TriggerMail.TryGetValue(nConId, out tempList))
            {
                return; //这个条件没有影响邮件
            }
            //条件是否完成了
            if (character.CheckCondition(nConId) != -2)
            {
                return; //没有完成
            }
            //整理这些邮件
            foreach (var mailid in tempList)
            {
                maillist[mailid] = 1;
            }
        }

        #endregion

        #region GM

        private static ulong GetNowGmMailGuid(int serverId)
        {
            ulong guid;
            if (NowGmMailGuid.TryGetValue(serverId, out guid))
            {
                return guid;
            }
            return 0;
        }

        public static void SetNowGmMailGuid(int serverId, ulong MaxGuid)
        {
            var old = GetNowGmMailGuid(serverId);
            if (old >= MaxGuid)
            {
                Logger.Warn("NowGmMailGuid !! old={0} , new={1}", old, MaxGuid);
                return;
            }
            NowGmMailGuid[serverId] = MaxGuid;
        }

        private static readonly Dictionary<int, ulong> NowGmMailGuid = new Dictionary<int, ulong>();

        private static readonly Dictionary<int, Dictionary<ulong, DBMail_One>> GmMailList =
            new Dictionary<int, Dictionary<ulong, DBMail_One>>();

        public static void PushMail(CharacterController character, ulong mailId)
        {
            var server = GetServerMail(character.serverId);
            var mail = GetMail(server, mailId);
            if (mail == null)
            {
                return;
            }
            var overTime = DateTime.FromBinary(mail.OverTime);
            var startTime = DateTime.FromBinary(mail.StartTime);
            if (overTime < DateTime.Now)
            {//已经结束的不可见
                return;
            }
            var createTime = DateTime.FromBinary(character.GetExData64((int) Exdata64TimeType.CreateTime));
            if (createTime > startTime && mail.IsNew == 0)
            {//对新人不可见
                return;
            }

            var items = new Dictionary<int, int>();
            foreach (var i in mail.Items)
            {
                items[i.ItemId] = i.Count;
            }
            var mailone = character.mMail.PushMail(mail.Name, mail.Text, items, mail.Send);
            mailone.OverTime = mail.OverTime;
            mailone.StartTime = mail.StartTime;
        }

        public void GmMail()
        {
            CoroutineFactory.NewCoroutine(GmSyncMail, mCharacter).MoveNext();
        }

        public static IEnumerator GmSyncMail(Coroutine coroutine, CharacterController character)
        {
            var characterGmMailId = character.mMail.GmGuid;
            var GmMailGuid = GetNowGmMailGuid(character.serverId);
            if (characterGmMailId == 0)
            {
                characterGmMailId = 100000000;
         //       character.mMail.GmGuid = GmMailGuid;
         //       yield break;
            }
            Dictionary<ulong, DBMail_One> serverMail = null;
            for (var i = characterGmMailId + 1; i <= GmMailGuid; i++)
            {
                if (serverMail == null)
                {
                    serverMail = GetServerMail(character.serverId);
                }
                if (GetMail(serverMail, i) != null)
                {
                    PushMail(character, i);
                    continue;
                }
                var result = CoroutineFactory.NewSubroutine(ReadMail, coroutine, character.serverId, i);
                if (result.MoveNext())
                {
                    yield return result;
                }
                PushMail(character, i);
            }
            character.mMail.GmGuid = GmMailGuid;
        }

        public static IEnumerator ReadMail(Coroutine coroutine, int serverId, ulong mailId)
        {
            var tasks = LogicServer.Instance.DB.Get<DBMail_One>(coroutine, DataCategory.GameMasterMail,
                serverId + ":" + mailId);
            yield return tasks;
            if (tasks.Data == null)
            {
                yield break;
            }
            GetServerMail(serverId)[mailId] = tasks.Data;
        }

        //获取邮件
        public static DBMail_One GetMail(Dictionary<ulong, DBMail_One> mails, ulong mailId)
        {
            DBMail_One mail;
            if (mails.TryGetValue(mailId, out mail))
            {
                return mail;
            }
            return null;
        }

        //私有方法：获取服务器的邮件
        private static Dictionary<ulong, DBMail_One> GetServerMail(int serverId)
        {
            if (serverId < 0)
            {
                Logger.Error("GetServerMail Error serverId={0}", serverId);
            }
            Dictionary<ulong, DBMail_One> mails;
            if (!GmMailList.TryGetValue(serverId, out mails))
            {
                mails = new Dictionary<ulong, DBMail_One>();
                GmMailList[serverId] = mails;
            }
            return mails;
        }

        #endregion
    }
}