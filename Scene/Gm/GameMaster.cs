#region using

using System;
using System.Collections;
using DataContract;
using Scorpion;
using Mono.GameMath;
using NLog;
using Shared;

#endregion

namespace Scene
{
    public interface IGameMaster
    {
		void AddBuff(ObjCharacter Character, int buffId, int buffLevel, ulong targetId = ulong.MaxValue);
        void CreateNpc(ObjCharacter Character, int dataId, bool canRelive = false,int level=-1);
        void DelBuff(ObjCharacter Character, int buffId);

        IEnumerator GmGoto(Coroutine coroutine,
                           ObjCharacter Character,
                           int targetSceneId,
                           int x,
                           int y,
                           AsyncReturnValue<ErrorCodes> error);

        void LookSceneManager();

        IEnumerator NotifyCreateChangeSceneCoroutine(Coroutine co,
                                                     ObjPlayer Character,
                                                     int scneneId,
                                                     int x,
                                                     int y,
                                                     AsyncReturnValue<ErrorCodes> error);

        void ReloadTable(string tableName);
        IEnumerator ReloadTableCoroutine(Coroutine coroutine, string tableName);
        void SetHp(ObjCharacter Character, int Hp);
        void SetMp(ObjCharacter Character, int Mp);
        void SetSpeed(ObjCharacter Character, int Speed);
    }

    public class GameMasterDefaultImpl : IGameMaster
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //调玩家位置
        public IEnumerator GmGoto(Coroutine coroutine,
                                  ObjCharacter Character,
                                  int targetSceneId,
                                  int x,
                                  int y,
                                  AsyncReturnValue<ErrorCodes> error)
        {
            var co = CoroutineFactory.NewSubroutine(SceneServer.Instance.ServerControl.CreateAndEnterScene, coroutine,
                Character.ObjId,
                Character.ServerId,
                targetSceneId,
                0ul,
                eScnenChangeType.Normal,
                new SceneParam());
            if (co.MoveNext())
            {
                yield return co;
            }
        }

        //开始创建副本
        public IEnumerator NotifyCreateChangeSceneCoroutine(Coroutine co,
                                                            ObjPlayer Character,
                                                            int scneneId,
                                                            int x,
                                                            int y,
                                                            AsyncReturnValue<ErrorCodes> error)
        {
            //GM命令切换场景，根据合服ID进行
            var serverLogicId = SceneExtension.GetServerLogicId(Character.ServerId);
            var sceneInfo = new ChangeSceneInfo
            {
                SceneId = scneneId,
                ServerId = serverLogicId,
                SceneGuid = 0,
                Type = (int) eScnenChangeType.Position
            };
            sceneInfo.Guids.Add(Character.ObjId);
            sceneInfo.Pos = new SceneParam();
            sceneInfo.Pos.Param.Add(x);
            sceneInfo.Pos.Param.Add(y);
            Character.BeginChangeScene();
            var msgChgScene = SceneServer.Instance.SceneAgent.SBChangeSceneByTeam(Character.ObjId, sceneInfo);
            yield return msgChgScene.SendAndWaitUntilDone(co, TimeSpan.FromSeconds(30));
        }

        //场景管理的Log
        public void LookSceneManager()
        {
            SceneManager.Instance.Log();
        }

        //调整移动速度
        public void SetSpeed(ObjCharacter Character, int Speed)
        {
            Character.Attr.GMMoveSpeedModify = Speed;
            Character.Attr.SetFlag(eAttributeType.MoveSpeed);
        }

        public void SetHp(ObjCharacter Character, int Hp)
        {
            Character.Attr.SetDataValue(eAttributeType.HpNow, Hp);
        }

        public void SetMp(ObjCharacter Character, int Mp)
        {
            Character.Attr.SetDataValue(eAttributeType.MpNow, Mp);
        }

        //增加Buff
		public void AddBuff(ObjCharacter Character, int buffId, int buffLevel, ulong targetId = ulong.MaxValue)
        {
			if (targetId == ulong.MaxValue)
			{
				Character.AddBuff(buffId, buffLevel, Character);
			}
			else
			{
				var target = Character.Scene.FindCharacter(targetId);
				if (null!=target)
				{
					target.AddBuff(buffId, buffLevel, Character);	
				}
				
			}
        }

        //删除Buff
        public void DelBuff(ObjCharacter character, int buffId)
        {
            var buffs = character.BuffList.GetBuffById(buffId);
            foreach (var buff in buffs)
            {
                MissBuff.DoEffect(character.Scene, character, buff);
                character.DeleteBuff(buff, eCleanBuffType.Clear);
            }
        }

        public void ReloadTable(string tableName)
        {
            //Table.ReloadTable(tableName);
            CoroutineFactory.NewCoroutine(ReloadTableCoroutine, tableName).MoveNext();
        }

        public IEnumerator ReloadTableCoroutine(Coroutine coroutine, string tableName)
        {
			var Reloadtable = SceneServer.Instance.SceneAgent.ServerGMCommand("ReloadTable",tableName);
            yield return Reloadtable.SendAndWaitUntilDone(coroutine);

            var sbReloadtable = SceneServer.Instance.SceneAgent.SBReloadTable(0, tableName);
            yield return sbReloadtable.SendAndWaitUntilDone(coroutine);
        }

        public void CreateNpc(ObjCharacter Character, int dataId, bool canRelive = false,int level = -1)
        {
            var scene = Character.Scene;
            if (null == scene)
            {
                return;
            }

			var npc = scene.CreateNpc(null, dataId, Character.GetPosition(), Vector2.UnitX, "GM_Create_" + dataId, level);
            if (null != npc)
            {
                npc.CanRelive = canRelive;
            }
        }
    }

    //Logic所有GM命令
    public static class GameMaster
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IGameMaster mImpl;

        static GameMaster()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (GameMaster), typeof (GameMasterDefaultImpl),
                o => { mImpl = (IGameMaster) o; });
        }

        //增加Buff
        public static void AddBuff(ObjCharacter Character, int buffId, int buffLevel,ulong targetId = ulong.MaxValue)
        {
			mImpl.AddBuff(Character, buffId, buffLevel, targetId);
        }

        public static void CreateNpc(ObjCharacter Character, int dataId, bool canRelive = false,int level=-1)
        {
			mImpl.CreateNpc(Character, dataId, canRelive, level);
        }

        //删除Buff
        public static void DelBuff(ObjCharacter Character, int buffId)
        {
            mImpl.DelBuff(Character, buffId);
        }

        //调玩家位置
        public static IEnumerator GmGoto(Coroutine coroutine,
                                         ObjCharacter Character,
                                         int targetSceneId,
                                         int x,
                                         int y,
                                         AsyncReturnValue<ErrorCodes> error)
        {
            return mImpl.GmGoto(coroutine, Character, targetSceneId, x, y, error);
        }

        //场景管理的Log
        public static void LookSceneManager()
        {
            mImpl.LookSceneManager();
        }

        //开始创建副本
        public static IEnumerator NotifyCreateChangeSceneCoroutine(Coroutine co,
                                                                   ObjPlayer Character,
                                                                   int scneneId,
                                                                   int x,
                                                                   int y,
                                                                   AsyncReturnValue<ErrorCodes> error)
        {
            return mImpl.NotifyCreateChangeSceneCoroutine(co, Character, scneneId, x, y, error);
        }

        public static void ReloadTable(string tableName)
        {
            mImpl.ReloadTable(tableName);
        }

        public static IEnumerator ReloadTableCoroutine(Coroutine coroutine, string tableName)
        {
            return mImpl.ReloadTableCoroutine(coroutine, tableName);
        }

        public static void SetHp(ObjCharacter Character, int Hp)
        {
            mImpl.SetHp(Character, Hp);
        }

        public static void SetMp(ObjCharacter Character, int Mp)
        {
            mImpl.SetMp(Character, Mp);
        }

        //调整移动速度
        public static void SetSpeed(ObjCharacter Character, int Speed)
        {
            mImpl.SetSpeed(Character, Speed);
        }
    }
}