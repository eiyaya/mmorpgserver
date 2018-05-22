#region using

using System.Collections.Generic;
using DataContract;
using ProtoBuf;

#endregion

namespace Shared
{
    public enum CharacterState
    {
        Created = 0,
        LoadData = 1,
        EnterGame = 2,
        PrepareData = 3,
        Connected = 4
    }

    public interface ICharacterController
    {
        /// <summary>
        ///     这个角色是否在线
        /// </summary>
        bool Online { get; }

        CharacterState State { get; set; }

        /// <summary>
        ///     响应事件, count 表示当前缓存的总次数，不受表格控制
        /// </summary>
        void ApplyEvent(int eventId, string evt, int count);

        /// <summary>
        ///     每个角色的定式事件
        /// </summary>
        /// <returns></returns>
        List<TimedTaskItem> GetTimedTasks();
    }

    public interface ICharacterControllerBase<T, ST> : ICharacterController
        where T : IExtensible
        where ST : IExtensible
    {
        T GetData();
        ST GetSimpleData();

        /// <summary>
        ///     使用CharacterId初始化DB的数据, 并初始化自己的内容
        /// </summary>
        /// <param name="characterId"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        T InitByBase(ulong characterId, object[] args = null);

        /// <summary>
        ///     使用DB的数据初始化
        /// </summary>
        /// <param name="characterId"></param>
        /// <param name="dbData"></param>
        /// <returns></returns>
        bool InitByDb(ulong characterId, T dbData);

        /// <summary>
        ///     当玩家的实例要被从服务器中删除的时候调用
        ///     需要把当前所有引用这个实例的引用都断开
        /// </summary>
        void OnDestroy();

        void OnSaveData(T data, ST simpleData);

        /// <summary>
        ///     心跳update
        /// </summary>
        void Tick();
    }

    public interface ICharacterControllerBase<T, ST, VT> : ICharacterController
        where T : IExtensible
        where ST : IExtensible
        where VT : IExtensible
    {
        /// <summary>
        ///     使积累的修改对CharacterController生效
        /// </summary>
        /// <param name="data">积累的修改</param>
        void ApplyVolatileData(VT data);

        T GetData();
        ST GetSimpleData();

        /// <summary>
        ///     使用CharacterId初始化DB的数据, 并初始化自己的内容
        /// </summary>
        /// <param name="characterId"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        T InitByBase(ulong characterId, object[] args = null);

        /// <summary>
        ///     使用DB的数据初始化
        /// </summary>
        /// <param name="characterId"></param>
        /// <param name="dbData"></param>
        /// <returns></returns>
        bool InitByDb(ulong characterId, T dbData);

        void LoadFinished();

        /// <summary>
        ///     当玩家的实例要被从服务器中删除的时候调用
        ///     需要把当前所有引用这个实例的引用都断开
        /// </summary>
        void OnDestroy();

        void OnSaveData(T data, ST simpleData);

        /// <summary>
        ///     心跳update
        /// </summary>
        void Tick();
    }


    public interface ICharacterSimpleController
    {
        
    }


    public interface ICharacterControllerSimpleBase<SDT, VDT> : ICharacterController
        where SDT : IExtensible
        where VDT : IExtensible
    {
    }
}