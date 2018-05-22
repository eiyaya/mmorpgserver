#region using

using System.Collections.Generic;
using DataTable;

#endregion

namespace Chat
{
    public enum ChannelType
    {
        System = 0, //系统频道
        Create = 1 //自建频道
    }

    public enum CityType
    {
        Son = 0, //子节点城市
        Dad = 1 //父节点城市
    }

    public class CityChannel
    {
        public CityChannel(ulong guid, string Name, ChannelType type)
        {
            ChannelName = Name;
            mType = type;
            mGuid = guid;
        }

        public string ChannelName; //房间名称

        public Dictionary<ulong, ChatCharacterController> Characters = new Dictionary<ulong, ChatCharacterController>();
            //房间ID

        public ulong mGuid;
        public ChannelType mType; //房间类型

        public int GetMaxCount()
        {
            return 100;
        }

        public void PushCharacter(ChatCharacterController c)
        {
            Characters[c.mGuid] = c;
        }

        public void RemoveCharacter(ChatCharacterController c)
        {
            Characters.Remove(c.mGuid);
        }
    }

    public class CityManager
    {
        public Dictionary<ulong, CityChannel> cChannels = new Dictionary<ulong, CityChannel>();
        public string CityName; //北京  天津。。。。。。
        public List<CityManager> Citys = new List<CityManager>(); //子城市管理
        public int mCityId;
        public CityType mType;
    }

    public static class CityChatManager
    {
        public static List<CityManager> CityDads = new List<CityManager>(); //一级城市管理
        public static Dictionary<int, CityManager> Citys = new Dictionary<int, CityManager>(); //所有子城市管理
        public static Dictionary<ulong, CityChannel> mChannels = new Dictionary<ulong, CityChannel>(); //当前频道管理
        public static ulong NextCreateId = 1000000;
        public static ulong NextSystemId;
        //读取DB
        //存储DB

        //创建频道
        public static CityChannel CreateChannel(int cityId, ChannelType type, string name)
        {
            var c = GetSonCity(cityId);
            if (c == null)
            {
                return null; //没找到城市
            }
            if (c.mType != CityType.Son)
            {
                return null; //不是子频道，不能创建频道
            }
            var nextId = GetNextCreateId(type);
            var cc = new CityChannel(nextId, name, type);
            mChannels[nextId] = cc;
            return cc;
        }

        //创建某个城市
        public static CityManager CreateCity(CityTalkRecord tbCity, CityType type, string name, bool isDad = true)
        {
            var cityId = tbCity.Id;
            var cm = new CityManager();
            cm.mCityId = cityId;
            cm.mType = type;
            cm.CityName = name;
            if (isDad)
            {
                CityDads.Add(cm);
            }
            Citys[cityId] = cm;
            if (type == CityType.Son)
            {
                for (var i = 1; i <= tbCity.Param; i++)
                {
                    var cc = CreateChannel(cityId, ChannelType.System, string.Format("{0}{1}", name, i));
                    cm.cChannels.Add(cc.mGuid, cc);
                }
            }
            else
            {
                var tbSkillup = Table.GetSkillUpgrading(tbCity.Param);
                if (tbSkillup != null)
                {
                    foreach (var i in tbSkillup.Values)
                    {
                        var tbCT = Table.GetCityTalk(i);
                        if (tbCT == null)
                        {
                            continue;
                        }
                        cm.Citys.Add(CreateCity(tbCT, (CityType) tbCT.IsParent, tbCT.Name));
                    }
                }
                //var newCity = CreateCity(cityId * 100 + 1, CityType.Son, "测试1");
                //cm.Citys.Add(newCity);
                //newCity = CreateCity(cityId * 100 + 2, CityType.Son, "测试2");
                //cm.Citys.Add(newCity);
            }
            return cm;
        }

        //进入频道
        public static ErrorCodes EnterChannel(ulong channelId, ChatCharacterController character)
        {
            var cc = GetChannel(channelId);
            if (cc == null)
            {
                return ErrorCodes.Unknow;
            }
            if (cc.Characters.Count >= cc.GetMaxCount())
            {
                return ErrorCodes.Unknow;
            }
            if (character.ChannelGuid != 0)
            {
                LeaveChannel(character.ChannelGuid, character);
            }
            character.ChannelGuid = channelId;
            cc.PushCharacter(character);
            return ErrorCodes.OK;
        }

        //进入城市
        public static ErrorCodes EnterCity(int cityId, ChatCharacterController character)
        {
            var cm = GetSonCity(cityId);
            if (cm == null)
            {
                return ErrorCodes.Unknow;
            }
            foreach (var channel in cm.cChannels)
            {
                if (channel.Value.Characters.Count >= channel.Value.GetMaxCount())
                {
                    continue;
                }
                if (character.ChannelGuid != 0)
                {
                    LeaveChannel(character.ChannelGuid, character);
                }
                character.ChannelGuid = channel.Key;
                channel.Value.PushCharacter(character);
                return ErrorCodes.OK;
            }
            return ErrorCodes.Unknow;
        }

        //查询某个频道
        public static CityChannel GetChannel(ulong channelId)
        {
            CityChannel cc;
            if (mChannels.TryGetValue(channelId, out cc))
            {
                return cc;
            }
            return null;
        }

        private static ulong GetNextCreateId(ChannelType type)
        {
            if (type == ChannelType.Create)
            {
                return NextCreateId++;
            }
            return NextSystemId++;
        }

        //获取某个城市
        public static CityManager GetSonCity(int cityId)
        {
            CityManager cm;
            if (Citys.TryGetValue(cityId, out cm))
            {
                return cm;
            }
            return null;
        }

        //初始化
        public static void Init()
        {
            ////直辖市
            //CreateCity(1, CityType.Son, "北京");
            //CreateCity(2, CityType.Son, "伤害");
            //CreateCity(3, CityType.Son, "天津");
            //CreateCity(4, CityType.Son, "重庆");
            //CreateCity(5, CityType.Son, "香港");
            //CreateCity(6, CityType.Son, "澳门");

            ////省
            //CreateCity(10000, CityType.Dad, "河北");
            //CreateCity(10001, CityType.Dad, "湖南");
            //CreateCity(10002, CityType.Dad, "山西");
            //CreateCity(10003, CityType.Dad, "河南");

            Table.ForeachCityTalk(record =>
            {
                if (record.Id > 9999)
                {
                    return true;
                }

                CreateCity(record, (CityType) record.IsParent, record.Name);
                return true;
            });
        }

        //离开频道
        public static ErrorCodes LeaveChannel(ulong channelId, ChatCharacterController character)
        {
            var cc = GetChannel(channelId);
            if (cc == null)
            {
                return ErrorCodes.Unknow;
            }
            character.ChannelGuid = 0;
            cc.RemoveCharacter(character);
            return ErrorCodes.OK;
        }
    }
}