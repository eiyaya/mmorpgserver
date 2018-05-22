#region using

using DataTable;
using NLog;

#endregion

namespace Logic
{
    public interface IAttributes
    {
        int GetAttrValue(int nCharacterId, int nLevel, int type);
        void Init();
    }

    public class AttributesDefaultImpl : IAttributes
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //

        private AttrRefRecord GetAttrRefTable(int RoleId, int AttrId)
        {
            AttrRefRecord tbAttrRef = null;
            Table.ForeachAttrRef(record =>
            {
                if (record.CharacterId != RoleId || AttrId != record.AttrId)
                {
                    return true;
                }
                tbAttrRef = record;
                return false;
            });
            return tbAttrRef;
        }

        public void Init()
        {
            for (var i = 0; i != Attributes.CharacterCount; ++i)
            {
                var thisCharacter = Table.GetCharacterBase(i);
                if (thisCharacter == null)
                {
                    continue;
                }
                var tbAttrRef = GetAttrRefTable(i, (int) eAttributeType.Level);
                if (tbAttrRef == null)
                {
                    continue;
                }
                for (var k = 0; k != 4; ++k)
                {
                    var nBaseAttr = thisCharacter.Attr[k + 1];
                    for (var j = 0; j != Attributes.CharacterLevelMax; ++j)
                    {
                        Attributes.CharacterAttr[i, j, k] = nBaseAttr + tbAttrRef.Attr[k]*j/100;
                    }
                }
            }
        }

        public int GetAttrValue(int nCharacterId, int nLevel, int type)
        {
            if (nCharacterId >= Attributes.CharacterCount || nCharacterId < 0)
            {
                //Logger.Error("AttrBaseManager::GetAttrValue Error CharacterId={0}", nCharacterId);
                var tbmonster = Table.GetCharacterBase(nCharacterId);
                if (tbmonster == null)
                {
                    Logger.Error("Character ID not find ! CharacterId={0}", nCharacterId);
                    return 0;
                }
                return tbmonster.Attr[type];
            }
            nLevel = nLevel - 1;
            if (nLevel >= Attributes.CharacterLevelMax || nLevel < 0)
            {
                Logger.Error("AttrBaseManager::GetAttrValue Error Level={0}", nLevel);
                return 0;
            }
            if (type >= 4 || type < 0)
            {
                Logger.Error("AttrBaseManager::GetAttrValue Error AttrId={0}", type);
                return 0;
            }
            return Attributes.CharacterAttr[nCharacterId, nLevel, type];
        }
    }

    public static class Attributes
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //
        private static IAttributes mImpl;

        static Attributes()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (Attributes), typeof (AttributesDefaultImpl),
                o => { mImpl = (IAttributes) o; });
        }

        public static int GetAttrValue(int nCharacterId, int nLevel, int type)
        {
            return mImpl.GetAttrValue(nCharacterId, nLevel, type);
        }

        public static void Init()
        {
            mImpl.Init();
        }

        public static readonly int CharacterCount = 3; //职业数量
        public static readonly int CharacterLevelMax = 400; //职业最大等级
        public static readonly int[,,] CharacterAttr = new int[CharacterCount, CharacterLevelMax, 4]; //各角色各等级的各基础属性
    }
}