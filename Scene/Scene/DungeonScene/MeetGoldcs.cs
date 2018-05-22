#region using

using System;
using System.Collections.Generic;
using System.Linq;
using DataContract;
using DataTable;
using Mono.GameMath;
using Shared;

#endregion

namespace Scene
{
    public class MeetGoldcs : UniformDungeon
    {
        #region 常量

        private static readonly List<Vector2> pos1 = new List<Vector2>
        {
            new Vector2(6.2f, 12.6f),
            new Vector2(9.4f, 12.6f),
            new Vector2(12.6f, 12.6f),
            new Vector2(15.8f, 12.6f),
            new Vector2(19f, 12.6f),
            new Vector2(6.2f, 8.9f),
            new Vector2(9.4f, 8.9f),
            new Vector2(12.6f, 8.9f),
            new Vector2(15.8f, 8.9f),
            new Vector2(19f, 8.9f),
            new Vector2(6.2f, 5.2f),
            new Vector2(9.4f, 5.2f),
            new Vector2(12.6f, 5.2f),
            new Vector2(15.8f, 5.2f),
            new Vector2(19f, 5.2f),
            new Vector2(7.62f, 16.29f), 
            new Vector2(12.07f, 17.24f), 
            new Vector2(12.69f, 20.05f), 
            new Vector2(15.64f, 19.40f), 
            new Vector2(18.55f, 18.76f), 
            new Vector2(17.85f, 15.57f), 
            new Vector2(14.38f, 15.64f), 
            new Vector2(10.30f, 14.50f),  
            new Vector2(7.33f, 14.58f),  
            new Vector2(6.14f, 13.89f)
        };

        private static readonly List<Vector2> pos2 = new List<Vector2>
        {
            new Vector2(7.66f, 10.9f),
            new Vector2(11.22f, 10.9f),
            new Vector2(14.78f, 10.9f),
            new Vector2(18.34f, 10.9f),
            new Vector2(7.66f, 6.6f),
            new Vector2(11.22f, 6.6f),
            new Vector2(14.78f, 6.6f),
            new Vector2(18.34f, 6.6f),
            new Vector2(7.72f, 17.81f),
            new Vector2(13.86f, 16.37f),
            new Vector2(14.49f, 19.31f),
            new Vector2(19.41f, 17.75f),
            new Vector2(14.28f, 15.27f),
            new Vector2(9.02f, 14.85f),
            new Vector2(14.94f, 20.34f),
            new Vector2(19.55f, 13.21f),
            new Vector2(14.12f, 14.4f),
            new Vector2(8.42f, 13.19f)
        };

        #endregion

        #region 数据

        private int addMoney;
        private int nPickMoney;
        private int seconds;
        private ObjPlayer mPlayer;
        private Trigger mTrigger;
        private static List<Vector2> oldList = new List<Vector2>();

        // 鼓舞
        private int InspireBuffId = 40100;
        private int GoldNumMax = 0;
        private int GoldNumInspireMax = 0;
        private int MaxGold = 5000;
        private int MinGold = 2500;

        #endregion

        #region 重写父类方法

        public override void OnCreate()
        {
            base.OnCreate();
            seconds = 0;

            var startTime = DateTime.Now.AddSeconds(10);
            StartTimer(eDungeonTimerType.WaitStart, startTime, TimeOverStart);
            StartTimer(eDungeonTimerType.WaitEnd, startTime.AddSeconds(90), TimeOverEnd);

            SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddSeconds(5), () =>
            {
                PushActionToAllPlayer(player =>
                {
                    player.Proxy.NotifyBattleReminder(14, Utils.WrapDictionaryId(224205), 1);
                    player.Proxy.NotifyBattleReminder(18, Utils.WrapDictionaryId(224206), 1);
                    player.Proxy.NotifyCountdown((ulong) startTime.ToBinary(), 1);
                });
            });

            InItConfig();
            return;
        }

        private void InItConfig()
        {
            ReadParseConfig(548, ref GoldNumMax);
            ReadParseConfig(549, ref GoldNumInspireMax);
            ReadParseConfig(288, ref MinGold);
            ReadParseConfig(289, ref MaxGold);
        }

        private void ReadParseConfig(int configId, ref int max)
        {
            var tbServer = Table.GetServerConfig(configId);
            if (tbServer == null)
            {
                return;
            }
            max = tbServer.ToInt();
        }

        public override void StartDungeon()
        {
            base.StartDungeon();

            mTrigger = SceneServerControl.Timer.CreateTrigger(DateTime.Now, TimeTick, 1000);
        }

        public override void OnPlayerEnter(ObjPlayer player)
        {
            mPlayer = player;

            base.OnPlayerEnter(player);
        }

        public override void OnPlayerLeave(ObjPlayer player)
        {
            mPlayer = null;
            base.OnPlayerLeave(player);
        }

        public override void ExitDungeon(ObjPlayer player)
        {
            base.ExitDungeon(player);

            Complete(eDungeonCompleteType.Quit);

            if (PlayerCount == 1)
            {
//没人掉线，且强退副本，30s后关闭副本
                CloseTrigger();
                EnterAutoClose(30);
            }
        }

        public override void EndDungeon()
        {
            Complete(eDungeonCompleteType.Failed);
            base.EndDungeon();
        }

        public override void OnPlayerPickItem(ObjPlayer player, ObjDropItem item)
        {
            if (item.ItemId == (int) eResourcesType.GoldRes)
            {
                nPickMoney += item.Count;

                var unit = mFubenInfoMsg.Units[0];
                unit.Params[0] = nPickMoney;
                mIsFubenInfoDirty = true;
            }
        }

        #endregion

        #region 内部逻辑

        private void CloseTrigger()
        {
            if (mTrigger != null)
            {
                SceneServerControl.Timer.DeleteTrigger(mTrigger);
                mTrigger = null;
            }
        }

        private void TimeTick()
        {
            if (mPlayer == null)
            {
                return;
            }
            if (addMoney > 5000000)
            {
                Complete(eDungeonCompleteType.Success);
                return;
            }
            if (seconds%6 == 0)
            {
                oldList = pos1.RandRange(0, 5);
                var index = 0;
                foreach (var v in oldList)
                {
                    PosGuideBefore(mPlayer, mPlayer, v, index*800);
                    index++;
                }
            }
            else if (seconds%6 == 4)
            {
                oldList = pos2.RandRange(0, 3);
                var index = 0;
                foreach (var v in oldList)
                {
                    PosGuideBefore(mPlayer, mPlayer, v, index*600);
                    index++;
                }
            }
            seconds = seconds + 1;
        }

        private void Complete(eDungeonCompleteType type)
        {
            if (State > eDungeonState.Start)
            {
                return;
            }

            CloseTrigger();

            var result = new FubenResult();
            result.CompleteType = (int) type;
            result.Args.Add(nPickMoney);
            CompleteToAll(result);
        }

        //掉落金币
        private void DropItem(int id, int count, Vector2 pos)
        {
            count *= AutoActivityManager.GetActivity(6200);
            addMoney += count;
            CreateDropItem(99, new List<ulong> {mPlayer.ObjId}, mPlayer.GetTeamId(), id, count, pos);
        }

        //火球生效
        private void FireCast(Vector2 pos)
        {
            if (mPlayer == null)
            {
                return;
            }
            var targetlist = new List<ObjCharacter>();

            var shape = new CircleShape(pos, 2);
            SceneShapeAction(shape, obj => { targetlist.Add(obj); });
            if (targetlist.Count < 0)
            {
                return;
            }
            foreach (var character in targetlist)
            {
                character.AddBuff(1059, 1, character);
            }
        }

        //广播地面火球效果
        private void PosGuideBefore(ObjCharacter caster, ObjPlayer target, Vector2 pos, int d)
        {
            //var mTable = Table.GetSkill(5150);
            if (target == null)
            {
                return;
            }
            var buffId = 1060;
            var replyMsg = new BuffResultMsg();
            var temp = new BuffResult
            {
                Type = BuffType.HT_EFFECT,
                BuffTypeId = buffId,
                ViewTime = Extension.AddTimeDiffToNet(d)
            };
            temp.Param.Add(5150);
            temp.TargetObjId = target.ObjId;
            temp.Param.Add((int) (pos.X*100));
            temp.Param.Add((int) (pos.Y*100));
            temp.Param.Add((int) (caster.GetDirection().X*1000));
            temp.Param.Add((int) (caster.GetDirection().Y*1000));
            temp.Param.Add(1500); //mTable.CastParam[1]
            replyMsg.buff.Add(temp);
            target.BroadcastBuffList(replyMsg);
            SceneServerControl.Timer.CreateTrigger(DateTime.Now.AddMilliseconds(1400 + d), () => { FireCast(pos); });
            if (MyRandom.Random(10000) < 5000)
            {
                DropItem(2, MyRandom.Random(MinGold, MaxGold), pos);

                var max = pos2.Count;
                if (IsInspired(target))
                {
                    max = Math.Max(GoldNumInspireMax, 0);
                }
                else
                {
                    max = Math.Max(GoldNumMax, 0);
                }
                // 修改掉落的堆数 走配置  跟是否鼓舞有关系
                oldList = pos2.RandRange(0, max);
                for (int i = 0; i < oldList.Count; ++i)
                {
                    DropItem(2, MyRandom.Random(MinGold, MaxGold), oldList[i]);
                }
            }
        }

        private bool IsInspired(ObjPlayer player)
        {
            if (player == null)
            {
                return false;
            }
            return player.BuffList.IsHaveBuffById(InspireBuffId);
        }

        #endregion
    }
}