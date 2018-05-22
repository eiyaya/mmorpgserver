using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Scorpion;
using DataContract;
using ProtoBuf;

using System.CodeDom.Compiler;

#pragma warning disable 0162,0108
namespace WatchDogClientService
{

    public abstract class WatchDogAgent : ClientAgentBase
    {
        public WatchDogAgent(string addr)
            : base(addr)
        {
        }

        public WatchDogAgent(ServerInfo broker, ServerInfo[] directConnect, Func<ulong, int> characterId2ServerId)
            : base(broker, directConnect, characterId2ServerId)
        {
        }

        protected override object GetPublishData(uint p, byte[] list)
        {
            switch (p)
            {
                default:
                    break;
            }

            return null;
        }


        protected override void DispatchPublishMessage(PublishMessageRecievedEvent evt)
        {
        }
    }

    public class AddFunctionNameWatchDog
    {
        public static void AddFunctionName(IDictionary<int, string> dict)
        {
            dict[2] = "Test";
        }
    }
}
