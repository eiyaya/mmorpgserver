#region using

using System.Collections.Generic;
using NLog;

#endregion

namespace Scene
{
    public class BaseState
    {
        public virtual string GetStateName()
        {
            return "";
        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnExit()
        {
        }

        public virtual void OnTick(float delta)
        {
        }
    }

    public interface IStateMachine
    {
        bool AddState(StateMachine _this, BaseState state);
        void EnterState(StateMachine _this, string stateName);
        BaseState GetCurrentState(StateMachine _this);
        void Tick(StateMachine _this, float delta);
    }

    public class StateMachineDefaultImpl : IStateMachine
    {
        protected static Logger Logger = LogManager.GetCurrentClassLogger();

        public bool AddState(StateMachine _this, BaseState state)
        {
            if (_this.mStateDict.ContainsKey(state.GetStateName()))
            {
                Logger.Fatal("duplicate state[{0}]", state.GetStateName());
                return false;
            }
            _this.mStateDict.Add(state.GetStateName(), state);
            return true;
        }

        public void EnterState(StateMachine _this, string stateName)
        {
            if (null != _this.mCurrentState && _this.mCurrentState.GetStateName() == stateName)
            {
                Logger.Info("current state is [{0}]", stateName);
                return;
            }

            BaseState state = null;
            if (!_this.mStateDict.TryGetValue(stateName, out state))
            {
                Logger.Fatal("can't find state[{0}]", stateName);
                return;
            }

            if (null != _this.mCurrentState)
            {
                _this.mCurrentState.OnExit();
            }

            _this.mCurrentState = state;
            _this.mCurrentState.OnEnter();
        }

        public void Tick(StateMachine _this, float delta)
        {
            if (null != _this.mCurrentState)
            {
                _this.mCurrentState.OnTick(delta);
            }
        }

        public BaseState GetCurrentState(StateMachine _this)
        {
            return _this.mCurrentState;
        }
    }

    public class StateMachine
    {
        protected static Logger Logger = LogManager.GetCurrentClassLogger();
        private static IStateMachine mImpl;

        static StateMachine()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (StateMachine),
                typeof (StateMachineDefaultImpl),
                o => { mImpl = (IStateMachine) o; });
        }

        public BaseState mCurrentState;
        public Dictionary<string, BaseState> mStateDict = new Dictionary<string, BaseState>();

        public bool AddState(BaseState state)
        {
            return mImpl.AddState(this, state);
        }

        public void EnterState(string stateName)
        {
            mImpl.EnterState(this, stateName);
        }

        public BaseState GetCurrentState()
        {
            return mImpl.GetCurrentState(this);
        }

        public void Tick(float delta)
        {
            mImpl.Tick(this, delta);
        }
    }
}