using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameMaster
{
   
    public class GMCommandResultBaseMsg
    {
        public int Ret;
        public string RetStr;
    }

    public class GMSendMailRetMsg : GMCommandResultBaseMsg
    {
        public ulong MailId;
    }

    public class GMSendBroadCastRetMsg : GMCommandResultBaseMsg
    {
        public ulong BroadCastId;
    }

    public class GMCharacterInfo
    {
        public GMCharacterInfo()
        {
            Attr = new List<int>();
        }
        public ulong CharacterId;
        public bool IsOnline;
        public string Name;
        public int Type;
        public int Level;
        public int VipLevel;
        public long Diamond;
        public long Money;
        public int SceneId;
        public float PosX;
        public float PosY;
        public uint SilenceMask;
        public List<int> Attr;
        public int fight_value;
    }
    public class GMCharacterServers
    {
        public GMCharacterServers()
        {
            Infos = new List<GMCharacterInfo>();
        }
        public int ServerId;
        public List<GMCharacterInfo> Infos;
    }

    public class GMPlayerInfoMsg : GMCommandResultBaseMsg
    {
        public GMPlayerInfoMsg()
        {
            CharacterServers = new List<GMCharacterServers>();
        }
        public ulong PlayerId;
        public string Account;
        public int Type;
        public string FoundTime;
        public uint LoginDay;
        public uint LoginTotal;
        public string LastTime;
        public string BindPhone;
        public string BindEmail;
        public string LockTime;

        public List<GMCharacterServers> CharacterServers;
    }

    public class GMMailData
    {
        public GMMailData()
        {
            items = new Dictionary<int, int>();
        }
        public ulong mailId;
        public string time;
        public string title;
        public string content;
        public string OverTime;
        public int IsNew;
        public Dictionary<int, int> items;
    }
    public class GMMailInfoMsg : GMCommandResultBaseMsg
    {
        public GMMailInfoMsg()
        {
            mailList = new List<GMMailData>();
        }
        public List<GMMailData> mailList;
    }

    public class GMBroadCastData
    {
        public ulong BroadCastId;
        public string time;
    }
    public class GMGMBroadCastDataMsg : GMCommandResultBaseMsg
    {
        public GMGMBroadCastDataMsg()
        {
            BroadCastList = new List<GMBroadCastData>();
        }
        public List<GMBroadCastData> BroadCastList;
    }

    public class GMGetFanKui
    {
        public uint Id;
        public long CharacterId;
        public string Name;
        public string Title;
        public string Content;
        public string CreateTime;
        public int State;
    }

    public class GMGetFanKuiList : GMCommandResultBaseMsg
    {
        public GMGetFanKuiList()
        {
            FanKuiList = new List<GMGetFanKui>();
        }
        public List<GMGetFanKui> FanKuiList;
    }

    public class GMAlianceMember
    {
        public GMAlianceMember()
        {
            
        }

        public ulong Id;
        public string Name;
        public int Level;
        public int RoleId;
    }
    public class GMGetAlianceInfo
    {
        public GMGetAlianceInfo()
        {
            Member = new List<GMAlianceMember>();
        }

        public int Id;
        public string Name;
        public ulong Leader;
        public string LeaderName;
        public int ServerId;
        public int State;
        public string Notice;
        public int Level;
        public string CreateTime;
        public List<GMAlianceMember> Member;
    }

    public class GMGetAlianceInfoList : GMCommandResultBaseMsg
    {
        public GMGetAlianceInfoList()
        {
            Infos = new List<GMGetAlianceInfo>();
        }

        public List<GMGetAlianceInfo> Infos;
    }

	public class GMMissionCommitRequest
	{
		public ulong CharacterId = ulong.MaxValue;
		public ulong MissionId = ulong.MaxValue;
	}


    ///////////////////////////////////////////////////////////////接受到的结构///////////////////////////////////////////////////////////////////////
    public class GMRequestCommand
    {
        public string CharacterId;
        public string Command;
    }

    public class GMRequestReloadTable
    {
        public string TableName;
    }
}
