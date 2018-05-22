using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActivityClientService;
using ChatClientService;
using LogicClientService;
using LoginClientService;
using Scorpion;
using Protocol;
using RankClientService;
using SceneClientService;
using TeamClientService;
using GameMasterClientService;

namespace Protocol
{
    public class ServerBase
    {
        public bool IsReadyToEnter = false;

        private AgentControl mAgetntControl;
        public SceneAgent SceneAgent
        {
            get { return mAgetntControl.SceneAgent; }
        }
        public LoginAgent LoginAgent
        {
            get { return mAgetntControl.LoginAgent; }
        }
        public ActivityAgent ActivityAgent
        {
            get { return mAgetntControl.ActivityAgent; }
        }
        public RankAgent RankAgent
        {
            get { return mAgetntControl.RankAgent; }
        }
        public TeamAgent TeamAgent
        {
            get { return mAgetntControl.TeamAgent; }
        }
        public LogicAgent LogicAgent
        {
            get { return mAgetntControl.LogicAgent; }
        }
        public ChatAgent ChatAgent
        {
            get { return mAgetntControl.ChatAgent; }
        }
        public GameMasterAgent GameMasterAgent
        {
            get { return mAgetntControl.GameMasterAgent; }
        }

        public Dictionary<string, ClientAgentBase> Agents
        {
            get { return mAgetntControl.Agents; }
        }

        public ServerBase()
        {
            mAgetntControl = new AgentControl();
        }

        public void Init(string[] args)
        {
            mAgetntControl.Init(args[3], args[5]);
        }

        public void Start(ServerAgentBase serverAgent)
        {
            mAgetntControl.Start(serverAgent);
        }

        public bool AllAgentConnected()
        {
            return mAgetntControl.Agents.Values.All(agent => agent.Connected);
        }

        public void AreAllServersReady(Action<bool> act)
        {
            if (act == null)
            {
                return;
            }

            var co = CoroutineFactory.NewCoroutine(ReadyToEnterImpl, act);
            co.MoveNext();
        }

        private IEnumerator ReadyToEnterImpl(Coroutine co, Action<bool> act)
        {
            var msg1 = mAgetntControl.LoginAgent.ReadyToEnter(0);
            yield return msg1.SendAndWaitUntilDone(co);

            if (msg1.State != MessageState.Reply)
            {
                act(false);
                yield break;
            }

            foreach (var i in msg1.Response)
            {
                if (i == 0)
                {
                    act(false);
                    yield break;
                }
            }

            var msg2 = mAgetntControl.LogicAgent.ReadyToEnter(0);
            yield return msg2.SendAndWaitUntilDone(co);

            if (msg2.State != MessageState.Reply)
            {
                act(false);
                yield break;
            }

            foreach (var i in msg2.Response)
            {
                if (i == 0)
                {
                    act(false);
                    yield break;
                }
            }

            var msg3 = mAgetntControl.ChatAgent.ReadyToEnter(0);
            yield return msg3.SendAndWaitUntilDone(co);

            if (msg3.State != MessageState.Reply)
            {
                act(false);
                yield break;
            }

            foreach (var i in msg3.Response)
            {
                if (i == 0)
                {
                    act(false);
                    yield break;
                }
            }


            var msg4 = mAgetntControl.TeamAgent.ReadyToEnter(0);
            yield return msg4.SendAndWaitUntilDone(co);

            if (msg4.State != MessageState.Reply)
            {
                act(false);
                yield break;
            }

            foreach (var i in msg4.Response)
            {
                if (i == 0)
                {
                    act(false);
                    yield break;
                }
            }


            var msg5 = mAgetntControl.ActivityAgent.ReadyToEnter(0);
            yield return msg5.SendAndWaitUntilDone(co);

            if (msg5.State != MessageState.Reply)
            {
                act(false);
                yield break;
            }

            foreach (var i in msg5.Response)
            {
                if (i == 0)
                {
                    act(false);
                    yield break;
                }
            }


            var msg6 = mAgetntControl.SceneAgent.ReadyToEnter(0);
            yield return msg6.SendAndWaitUntilDone(co);

            if (msg6.State != MessageState.Reply)
            {
                act(false);
                yield break;
            }

            foreach (var i in msg6.Response)
            {
                if (i == 0)
                {
                    act(false);
                    yield break;
                }
            }

            act(true);
        }

        public void Status(ConcurrentDictionary<string, string> dict)
        {
            if (mAgetntControl != null)
            {
                mAgetntControl.ConnetedInfo(dict);
            }
        }

        public void Stop()
        {
            mAgetntControl.Stop();
        }
    }
}