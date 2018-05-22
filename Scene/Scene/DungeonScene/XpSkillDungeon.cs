#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using DataContract;
using DataTable;
using Scorpion;
using Mono.GameMath;
using NLog;
using Shared;

#endregion

namespace Scene
{
    public class XpSkillDungeon : UniformDungeon
    {
        #region 刷新表格
        static XpSkillDungeon()
        {

        }

        #endregion

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private int DeadNpcNum = 0;
        private static int startNum = 5;
        private int buffId = 1400;
        private float timeDelta = 5;
        private DateTime startTime = DateTime.MinValue;
        private bool DelBuffFailed = false;
        #region 重写父类方法
        public override void OnCreate()
        {
            base.OnCreate();
            DeadNpcNum = 0;
            timeDelta = 5;
            DelBuffFailed = false;
        }
        public override void ExitDungeon(ObjPlayer player)
        {
            if (player == null) return;

            player.ExitDungeon();
        }

        public override void OnPlayerDie(ObjPlayer player, ulong characterId = 0)
        {
            if (player == null) return;
        }

        public override void OnNpcDie(ObjNPC npc, ulong characterId = 0)
        {
            if (npc == null) return;
            base.OnNpcDie(npc, characterId);
            try
            {
                if (npc.GetObjType() != ObjType.NPC)
                {
                    return;
                }

                if (npc.mTypeId != 3014)
                {
                    return;
                }

                DeadNpcNum++;

                if (startNum + 1 == DeadNpcNum || DelBuffFailed)
                {
                    if ((DateTime.Now - startTime).TotalSeconds < timeDelta)
                    {
                        DelBuffFailed = true;
                        return;
                    }
                    DelBuffFailed = false;
                    // 解除buff
                    PushActionToAllObj(npc2 =>
                    {
                        var tempNpc = npc2 as ObjNPC;
                        if (tempNpc == null)
                        {
                            return;
                        }
                        if (!tempNpc.IsMonster())
                        {
                            return;
                        }
                        
                        
                        tempNpc.DeleteBuff(buffId, eCleanBuffType.EffectOver);
                    });
                    return;
                }

                var killer = FindCharacter(characterId);
                if (killer == null)
                {
                    return;
                }

                var player = killer.GetRewardOwner() as ObjPlayer;
                if (null == player)
                {
                    return;
                }

                if (DeadNpcNum == startNum)
                {
                    startTime = DateTime.Now;
                    LearnSkill(player);
                }
            }
            catch (Exception)
            {
                Logger.Error("XpSkillDungeon err on OnNpcDie");
            }
        }

        protected void LearnSkill(ObjPlayer player)
        {
            CoroutineFactory.NewCoroutine(LearnSkill, player).MoveNext();
        }

        protected IEnumerator LearnSkill(Coroutine co, ObjPlayer player)
        {
            var skillId = -1;
            if (player.GetSimpleData().TypeId == 0) //战士
            {
                skillId = 30;
            }
            else if (player.GetSimpleData().TypeId == 1) //法师
            {
                skillId = 133;
            }
            else if (player.GetSimpleData().TypeId == 2) // 弓手
            {
                skillId = 231;
            }

            if (skillId == -1)
            {
                yield break;
            }


            var msg = SceneServer.Instance.LogicAgent.SSLearnSkill(player.ObjId, skillId, 1);
            yield return msg.SendAndWaitUntilDone(co);
            if (msg.State != MessageState.Reply)
            {
                Logger.Error("SSLearnSkill Xp Failed 1");
                yield break;
            }
            if (msg.ErrorCode != (int)ErrorCodes.OK)
            {
                Logger.Error("SSLearnSkill Xp Failed 2");
                yield break;
            }

            // 加buff  发消息包触发引导
            player.Proxy.NotifyStartXpSkillGuide(0);

            PushActionToAllObj(obj =>
            {
                var tempNpc = obj as ObjNPC;
                if (tempNpc == null)
                {
                    return;
                }
                if (!tempNpc.IsMonster())
                {
                    return;
                }
                        
                tempNpc.AddBuff(buffId, 1, tempNpc);
            });

            yield break;
        }

        public override void OnPlayerEnterOver(ObjPlayer player)
        {
            if (player == null) return;

            base.OnPlayerEnterOver(player);
        }

        public override void OnPlayerLeave(ObjPlayer player)
        {
            if (player == null) return;

            base.OnPlayerLeave(player);

            // 解除buff
            PushActionToAllObj(npc2 =>
            {
                var tempNpc = npc2 as ObjNPC;
                if (tempNpc == null)
                {
                    return;
                }
                if (!tempNpc.IsMonster())
                {
                    return;
                }


                tempNpc.DeleteBuff(buffId, eCleanBuffType.EffectOver);
            });
        }

        public override ReasonType GetBornVisibleType()
        {
            return ReasonType.Born; 
        }
        #endregion
    }
}