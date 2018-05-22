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
    public class MaYaWeaponDungeon : UniformDungeon
    {
        #region 刷新表格
        static MaYaWeaponDungeon()
        {

        }

        #endregion

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private int DanageNpcNum = 0;
        private int DeadNpcNum = 0;
        private static int startNum = 3; //击杀多少只怪物开始引导
        private int XuanYunBuffId = 1400;
        private float timeDelta = 2;
        private DateTime startTime = DateTime.MinValue;
        private bool DelBuffFailed = false;
        private bool HasDeletedBuff = false;
	    private bool FirstEnter = true;
        #region 重写父类方法
        public override void OnCreate()
        {
            base.OnCreate();
            DeadNpcNum = 0;
            timeDelta = 2;
            DelBuffFailed = false;
            HasDeletedBuff = false;
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

        public override void OnNpcDamage(ObjNPC npc, int damage, ObjBase enemy)
        {
            if (npc == null) return;
            base.OnNpcDamage(npc, damage, enemy);
            try
            {
                if (npc.GetObjType() != ObjType.NPC)
                {
                    return;
                }

                var killer = FindCharacter(enemy.ObjId);
                if (killer == null)
                {
                    return;
                }

                var tmpNpc = killer as ObjNPC;
                if (tmpNpc != null)
                {
                    return;
                }


                var player = killer.GetRewardOwner() as ObjPlayer;
                if (null == player)
                {
                    return;
                }

                DanageNpcNum++;

                if ((DanageNpcNum > 0 && startNum <= DeadNpcNum) || DelBuffFailed)
                {
                    if (HasDeletedBuff)
                    {
                        return;
                    }
                    if ((DateTime.Now - startTime).TotalSeconds < timeDelta)
                    {
                        DelBuffFailed = true;
                        return;
                    }
                    DelBuffFailed = false;

                    player.Proxy.NotifyBattleReminder(26, Utils.WrapDictionaryId(589), 1);
                    // 给主角加buff
//                     var buff1 = -1;
//                     var buff2 = -1;
//                     if (player.GetRole()== 0) //战士
//                     {
//                         buff1 = 513;
//                         buff2 = 510;
//                     }
// 					else if (player.GetRole() == 1) //法师
//                     {
//                         buff1 = 514;
//                         buff2 = 511;
//                     }
// 					else if (player.GetRole() == 2) // 弓手
//                     {
//                         buff1 = 515;
//                         buff2 = 512;
//                     }
// 
//                     if (buff1 == -1 || buff2 == -1)
//                     {
//                         return;
//                     }
// 
//                     player.AddBuff(buff1, 1, player);
//                     player.AddBuff(buff2, 1, player);
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

                        tempNpc.DeleteBuff(XuanYunBuffId, eCleanBuffType.EffectOver);
                    });

                    HasDeletedBuff = true;
                    return;
                }
            }
            catch (Exception)
            {
                Logger.Error("MaYaWeaponDungeon err on OnNpcDamage");
            }
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

                DeadNpcNum++;

                if (DeadNpcNum == startNum)
                {
                    DanageNpcNum = 0;
                    startTime = DateTime.Now;
                    DoAction(player);
                }
            }
            catch (Exception)
            {
                Logger.Error("MaYaWeaponDungeon err on OnNpcDie");
            }
        }

        protected void DoAction(ObjPlayer player)
        {
            CoroutineFactory.NewCoroutine(DoAction, player).MoveNext();
        }

        protected IEnumerator DoAction(Coroutine co, ObjPlayer player)
        {
            // 给怪物加眩晕buff
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

                tempNpc.AddBuff(XuanYunBuffId, 1, tempNpc);
            });

            //给玛雅武器
            var item = -1;
            if (player.GetRole() == 0) // 战士
            {
                item = 600100;
            }
            else if (player.GetRole() == 1) // 法师
            {
                item = 600101;
            }
            else if (player.GetRole() == 2) // 弓手
            {
                item = 600102;
            }
            if (item == -1)
            {
                yield break;
            }

            CoroutineFactory.NewCoroutine(GiveItemCoroutine, player.ObjId, item, 1, player).MoveNext();

            yield break;
        }
        private IEnumerator GiveItemCoroutine(Coroutine coroutine, ulong characterId, int itemId, int itemCount, ObjPlayer player)
	    {
			var result = SceneServer.Instance.LogicAgent.GiveItem(characterId, itemId, itemCount,-1);
			yield return result.SendAndWaitUntilDone(coroutine);
			if (result.State != MessageState.Reply)
			{
				Logger.Warn("GiveItemCoroutine time out");
				yield break;
			}
            if (result.ErrorCode == (int)ErrorCodes.OK)
            {
                //发消息包触发引导
                if (player != null && player.Proxy != null)
                {
                    player.Proxy.NotifyStartMaYaFuBenGuide(0); 
                }
            }
	    }

        public override void OnPlayerEnterOver(ObjPlayer player)
        {
            if (player == null) return;

            base.OnPlayerEnterOver(player);

	        
	        if (!FirstEnter)
	        {
				// 给主角加buff
// 				var buff1 = -1;
// 				var buff2 = -1;
// 				if (player.GetRole()== 0) //战士
// 				{
// 					buff1 = 513;
// 					buff2 = 510;
// 				}
// 				else if (player.GetRole()== 1) //法师
// 				{
// 					buff1 = 514;
// 					buff2 = 511;
// 				}
// 				else if (player.GetRole() == 2) // 弓手
// 				{
// 					buff1 = 515;
// 					buff2 = 512;
// 				}
// 
// 				if (buff1 != -1 )
// 				{
// 					player.AddBuff(buff1, 1, player);
// 				}
// 		        if (buff2 != -1)
// 		        {
// 					player.AddBuff(buff2, 1, player);    
// 		        }
// 
// 				player.Proxy.NotifyStartMaYaFuBenGuide(0);
	        }

			FirstEnter = false;	
        }

        public override void OnPlayerLeave(ObjPlayer player)
        {
            if (player == null) return;

            base.OnPlayerLeave(player);

            player.Proxy.NotifyStartMaYaFuBenGuide(1);

            // 给主角删除buff
            var buff1 = -1;
            var buff2 = -1;
			if (player.GetRole() == 0) //战士
            {
                buff1 = 513;
                buff2 = 510;
            }
			else if (player.GetRole() == 1) //法师
            {
                buff1 = 514;
                buff2 = 511;
            }
			else if (player.GetRole() == 2) // 弓手
            {
                buff1 = 515;
                buff2 = 512;
            }

            if (buff1 == -1 || buff2 == -1)
            {
                return;
            }
            player.DeleteBuff(buff1, eCleanBuffType.EffectOver);
            player.DeleteBuff(buff2, eCleanBuffType.EffectOver);
        }

        #endregion
    }
}