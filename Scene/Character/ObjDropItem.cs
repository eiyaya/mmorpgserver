#region using

using System;
using System.Collections;
using System.Collections.Generic;
using DataContract;
using DataTable;
using EventSystem;
using Scorpion;
using Mono.GameMath;
using Shared;

#endregion

namespace Scene
{
    public interface IObjDropItem
    {
        void Destroy(ObjDropItem _this);
        ObjData DumpObjData(ObjDropItem _this, ReasonType reason);
        void EnterScene(ObjDropItem _this, Scene scene, int delayTime = 0);
        void InitObjDropItem(ObjDropItem _this, int type, List<ulong> ownerList, ulong teamId, int itemId, int count);
        bool IsOwnerList(ObjDropItem _this, ulong pickerId);
        bool Pickup(ObjDropItem _this, ObjPlayer picker);
        void Remove(ObjDropItem _this);
        void TimeOver(ObjDropItem _this);
    }

    public class ObjDropItemDefaultImpl : IObjDropItem
    {
        //推送拾取列表
        private void PushPick(ObjDropItem _this, ObjPlayer picker)
        {


            if (_this.TableDrop != null && _this.TableDrop.Type == 300)
            {//buff类

                if (picker.Scene != null)
                {
                    picker.Scene.OnPlayerPickItem(picker, _this);
                }
                _this.Remove();
                return;
            }
            if (_this.mPickList.Count > 0)
            {
                _this.mPickList[picker.ObjId] = 1;
                return;
            }
            CoroutineFactory.NewCoroutine(TryPick, _this, picker).MoveNext();
        }

        private IEnumerator TryPick(Coroutine coroutine, ObjDropItem objDrop, ObjPlayer character)
        {
            //增加道具
            PlayerLog.WriteLog((int) LogType.DropItem, "TryPickDropItem  Id ={0}  ObjPlayer={1}", objDrop.ObjId,
                character.ObjId);
            var result = SceneServer.Instance.LogicAgent.GiveItem(character.ObjId, objDrop.ItemId, objDrop.Count,-1);
            yield return result.SendAndWaitUntilDone(coroutine);
            if (result.State != MessageState.Reply)
            {
                PlayerLog.WriteLog((int) LogType.DropItem, "PickUpItemSuccess not reply  Id ={0}  ObjPlayer={1}",
                    objDrop.ObjId, character.ObjId);
                objDrop.Remove();
                yield break;
            }
            if (result.Response == (int) ErrorCodes.OK)
            {
//Logic增加道具成功
                //告诉客户端我拾取了
                if (character.Proxy != null)
                {
                    character.Proxy.PickUpItemSuccess(objDrop.ObjId);
                    character.Proxy.Wait(coroutine, TimeSpan.FromSeconds(3));
                }
                PlayerLog.WriteLog((int) LogType.DropItem, "PickUpItemSuccess  Id ={0}  ObjPlayer={1}", objDrop.ObjId,
                    character.ObjId);
                //删除自己
                objDrop.Remove();
                if (character.Scene != null)
                {
                    character.Scene.OnPlayerPickItem(character, objDrop);
                }
            }
            else
            {
//Logic增加道具失败
                if (character.Proxy != null)
                {
                    character.Proxy.BagisFull(objDrop.ObjId, objDrop.ItemId, objDrop.Count);
                }
                objDrop.mPickList.Remove(character.ObjId);
                if (objDrop.mOverTrigger == null)
                {
                    yield break;
                }
                if (objDrop.mPickList.Count > 0)
                {
                    foreach (var i in objDrop.mPickList)
                    {
                        var uId = i.Key;
                        var cl = CharacterManager.Instance.GetCharacterControllerFromMemroy(uId);
                        if (cl == null)
                        {
                            continue;
                        }
                        CoroutineFactory.NewCoroutine(TryPick, objDrop, cl).MoveNext();
                        //TryPick(coroutine, ObjDropItem objDrop, ObjPlayer character)
                        yield break;
                    }
                }
            }
        }

        //是否在优先拾取列表
        public bool IsOwnerList(ObjDropItem _this, ulong pickerId)
        {
            return _this.OwnerList.Contains(pickerId);
        }

        //创建
        public void InitObjDropItem(ObjDropItem _this,
                                    int type,
                                    List<ulong> ownerList,
                                    ulong teamId,
                                    int itemId,
                                    int count)
        {
            _this.BelongType = type;
            _this.OwnerList = ownerList;
            _this.TeamId = teamId;
            _this.ItemId = itemId;
            _this.Count = count;
            _this.mDropTime = DateTime.Now;
            _this.TableDrop = Table.GetItemBase(itemId);
            var stayseconds = 180;
            switch (type)
            {
                case 0: //队内自由拾取
                {
                    stayseconds = ObjDropItem.StaySecondsByTeamFree;
                }
                    break;
                case 1: //队内伤害拾取
                {
                    stayseconds = ObjDropItem.StaySecondsByTeamDamage;
                }
                    break;
                case 2: //队内分别拾取
                {
                    stayseconds = ObjDropItem.StaySecondsByTeamAll;
                }
                    break;
                case 3: //所有人分别拾取
                {
                    stayseconds = ObjDropItem.StaySecondsByAll;
                }
                    break;
                case 99: //潜规则的类型，5秒消失
                {
                    stayseconds = 5;
                    _this.BelongType = 0;
                }
                    break;
            }
            _this.mOverTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(stayseconds), () =>
            {
                _this.TimeOver();
                _this.mOverTrigger = null;
            });

			SceneServerMonitor.CreatedDropItemRate.Mark();
        }

        public void TimeOver(ObjDropItem _this)
        {
            PlayerLog.WriteLog((int) LogType.DropItem, "TimeOver  Id ={0}", _this.ObjId);
            _this.Remove();
        }

        //移除
        public void Remove(ObjDropItem _this)
        {
            if (_this.mOverTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(_this.mOverTrigger);
                _this.mOverTrigger = null;
                if (null != _this.Scene)
                {
                    _this.Scene.LeaveScene(_this);
                }
            }
        }

        //拾取
        public bool Pickup(ObjDropItem _this, ObjPlayer picker)
        {
            if (picker.mIsDead)//死亡玩家尸体无法拾取
                return false;
            var IsCan = false;
            if ((picker.GetPosition() - _this.GetPosition()).LengthSquared() > ObjDropItem.PickDistanceSquared)
            {
                return false;
            }
            if (_this.IsOwnerList(picker.ObjId))
            {
                IsCan = true;
            }
            else
            {
                switch (_this.BelongType)
                {
                    case 0: //队内自由拾取
                    {
                        if (DateTime.Now >= _this.mDropTime.AddSeconds(ObjDropItem.SafeSecondsByTeamFree))
                        {
                            IsCan = true;
                        }
                    }
                        break;
                    case 1: //队内伤害拾取
                    {
                        if (_this.TeamId == 0)
                        {
//没有队
                            if (DateTime.Now >= _this.mDropTime.AddSeconds(ObjDropItem.TeamSecondsByTeamDamage))
                            {
                                IsCan = true;
                            }
                        }
                        else if (picker.GetTeamId() == _this.TeamId)
                        {
//本队成员
                            if (DateTime.Now >= _this.mDropTime.AddSeconds(ObjDropItem.SafeSecondsByTeamDamage))
                            {
                                IsCan = true;
                            }
                        }
                        else
                        {
//非本队成员
                            if (DateTime.Now >= _this.mDropTime.AddSeconds(ObjDropItem.TeamSecondsByTeamDamage))
                            {
                                IsCan = true;
                            }
                        }
                    }
                        break;
                    case 2: //队内分别拾取
                    {
                    }
                        break;
                    case 3: //所有人分别拾取
                    {
                    }
                        break;
                    case 4:
                    {
                        IsCan = true;
                    }
                        break;
                }
            }
            if (IsCan)
            {
                PushPick(_this, picker);
            }
            return IsCan;
        }

        //public override void Init(ulong characterId, int dataId)
        //{
        //    base.Init(characterId, dataId);

        //}

        //输出成一个objdata用于客户端创建
        public ObjData DumpObjData(ObjDropItem _this, ReasonType reason)
        {
            var data = ObjBase.GetImpl().DumpObjData(_this, reason);
            data.Pos = Utility.MakePositionDataByPosAndDir(_this.OrginPos, _this.GetDirection());
            var span = DateTime.Now - _this.mDropTime;
            data.Owner = new Uint64Array();
            data.Owner.Items.AddRange(_this.OwnerList);
            data.ExtData.Add(60 - span.Seconds); //temp
            data.TargetPos.Add(new Vector2Int32
            {
                x = Utility.MultiplyPrecision(_this.GetPosition().X),
                y = Utility.MultiplyPrecision(_this.GetPosition().Y)
            });
            if (DateTime.Now.GetDiffSeconds(_this.mDropTime) > ObjDropItem.DROP_TIME)
            {
                data.ExtData.Add(0);
            }
            else
            {
                data.ExtData.Add(1);
            }
            return data;
        }

        //进入场景
        public void EnterScene(ObjDropItem _this, Scene scene, int delayTime = 0)
        {
            if (delayTime < 50)
            {
                scene.EnterScene(_this);
                return;
            }
            _this.mEnterSceneTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(delayTime),
                () =>
                {
                    scene.EnterScene(_this);
                    _this.mEnterSceneTrigger = null;
                });
        }

        public void Destroy(ObjDropItem _this)
        {
            if (null != _this.mEnterSceneTrigger)
            {
                SceneServerControl.Timer.DeleteTrigger(_this.mEnterSceneTrigger);
                _this.mEnterSceneTrigger = null;
            }
            if (null != _this.mOverTrigger)
            {
                SceneServerControl.Timer.DeleteTrigger(_this.mOverTrigger);
                _this.mOverTrigger = null;
            }
        }
    }

    public class ObjDropItem : ObjBase
    {
        public const float DROP_TIME = 0.5f;
        private static IObjDropItem mImpl;

        public static int PickDistanceSquared = (Table.GetDropConfig(2).Param[0] + 3)*
                                                (3 + Table.GetDropConfig(2).Param[0]) + 2;
            //因为客户端发的移动包会有时间延迟，导致位置和服务端会有些差距，这里增加容错

        public static int SafeSecondsByTeamDamage = Table.GetDropConfig(2).Param[3]; //60
        public static int SafeSecondsByTeamFree = Table.GetDropConfig(2).Param[1]; //60
        public static int StaySecondsByAll = Table.GetDropConfig(2).Param[7]; //180
        public static int StaySecondsByTeamAll = Table.GetDropConfig(2).Param[6]; //180
        public static int StaySecondsByTeamDamage = Table.GetDropConfig(2).Param[5]; //240
        public static int StaySecondsByTeamFree = Table.GetDropConfig(2).Param[2]; //180
        public static int TeamSecondsByTeamDamage = Table.GetDropConfig(2).Param[4]; //120

        static ObjDropItem()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (ObjDropItem), typeof (ObjDropItemDefaultImpl),
                o => { mImpl = (IObjDropItem) o; });
            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);
        }

        //创建
        public ObjDropItem(int type, List<ulong> ownerList, ulong teamId, int itemId, int count)
        {
            mImpl.InitObjDropItem(this, type, ownerList, teamId, itemId, count);
        }

        public Dictionary<ulong, int> mPickList = new Dictionary<ulong, int>();
        public List<ulong> OwnerList = new List<ulong>(); //优先拾取列表
        public int BelongType { get; set; }
        public int Count { get; set; }
        //public static readonly int StaySecondsByTeamFree = Table.GetDropConfig(2).Param[2];
        public int ItemId { get; set; }
        public DateTime mDropTime { get; set; }
        public Trigger mEnterSceneTrigger { get; set; }
        public Trigger mOverTrigger { get; set; }
        //从哪掉落的（掉落的时候会有个抛物的效果）
        public Vector2 OrginPos { get; set; }
        public ItemBaseRecord TableDrop { get; set; }
        public ulong TeamId { get; set; } //优先队伍ID

        public override void Destroy()
        {
            mImpl.Destroy(this);
        }

        public override ObjData DumpObjData(ReasonType reason)
        {
            return mImpl.DumpObjData(this, reason);
        }

        //进入场景
        public void EnterScene(Scene scene, int delayTime = 0)
        {
            mImpl.EnterScene(this, scene, delayTime);
        }

        //Obj类型
        public override ObjType GetObjType()
        {
            return ObjType.DROPITEM;
        }

        //是否在优先拾取列表
        public bool IsOwnerList(ulong pickerId)
        {
            return OwnerList.Contains(pickerId);
        }

        //拾取
        public bool Pickup(ObjPlayer picker)
        {
            return mImpl.Pickup(this, picker);
        }

        private static void ReloadTable(IEvent ievent)
        {
            var v = ievent as ReloadTableEvent;
            if (v.tableName == "DropConfig")
            {
                PickDistanceSquared = (Table.GetDropConfig(2).Param[0] + 3)*(3 + Table.GetDropConfig(2).Param[0]) + 2;
                    //因为客户端发的移动包会有时间延迟，导致位置和服务端会有些差距，这里增加容错
                SafeSecondsByTeamFree = Table.GetDropConfig(2).Param[1]; //60
                StaySecondsByTeamFree = Table.GetDropConfig(2).Param[2]; //180
                SafeSecondsByTeamDamage = Table.GetDropConfig(2).Param[3]; //60
                TeamSecondsByTeamDamage = Table.GetDropConfig(2).Param[4]; //120
                StaySecondsByTeamDamage = Table.GetDropConfig(2).Param[5]; //240
                StaySecondsByTeamAll = Table.GetDropConfig(2).Param[6]; //180
                StaySecondsByAll = Table.GetDropConfig(2).Param[7]; //180
            }
        }

        //移除
        public void Remove()
        {
            mImpl.Remove(this);
        }

        public void TimeOver()
        {
            mImpl.TimeOver(this);
        }
    }
}