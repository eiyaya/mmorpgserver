using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Scorpion;
using DataContract;
using ProtoBuf;

#pragma warning disable 0162,0108
namespace DirectoryClientService
{

    public abstract class DirectoryAgent : ClientAgentBase
    {
        public DirectoryAgent(string addr)
            : base(addr)
        {
        }

        public DirectoryAgent(ServerInfo broker, ServerInfo[] directConnect, Func<ulong, int> characterId2ServerId)
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

    public class AddFunctionNameDirectory
    {
        public static void AddFunctionName(IDictionary<int, string> dict)
        {
            dict[8000] = "CheckVersion";
            dict[8001] = "CheckVersion2";
            dict[8002] = "CheckVersion3";
        }
        public static void AddCSFunctionName(IDictionary<int, string> dict)
        {
            dict[8000] = "CheckVersion";
            dict[8001] = "CheckVersion2";
            dict[8002] = "CheckVersion3";
        }
    }
}
