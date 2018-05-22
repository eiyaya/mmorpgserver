#region using

using EventSystem;

#endregion

namespace Logic
{
    public class ChacacterFlagTrue : EventBase
    {
        public const string EVENT_TYPE = "ChacacterFlagTrue";

        public ChacacterFlagTrue(CharacterController c, int Id)
            : base(EVENT_TYPE)
        {
            character = c;
            FlagId = Id;
        }

        public CharacterController character;
        public int FlagId { get; set; }
    }

    public class ChacacterFlagFalse : EventBase
    {
        public const string EVENT_TYPE = "ChacacterFlagFalse";

        public ChacacterFlagFalse(CharacterController c, int Id)
            : base(EVENT_TYPE)
        {
            character = c;
            FlagId = Id;
        }

        public CharacterController character;
        public int FlagId { get; set; }
    }

    public class CharacterExdataChange : EventBase
    {
        public const string EVENT_TYPE = "CharacterExdataChange";

        public CharacterExdataChange(CharacterController c, int Id, int Value)
            : base(EVENT_TYPE)
        {
            character = c;
            ExdataId = Id;
            ExdataValue = Value;
        }

        public CharacterController character;
        public int ExdataId;
        public int ExdataValue;
    }


    public class ItemChange : EventBase
    {
        public const string EVENT_TYPE = "ItemChange";

        public ItemChange(CharacterController c, int Id, int Count)
            : base(EVENT_TYPE)
        {
            character = c;
            mItemId = Id;
            mItemCount = Count;
        }

        public CharacterController character;
        public int mItemCount;
        public int mItemId;
    }

    public class KillMonster : EventBase
    {
        public const string EVENT_TYPE = "KillMonster";

        public KillMonster(CharacterController c, int Id)
            : base(EVENT_TYPE)
        {
            character = c;
            mMonsterId = Id;
        }

        public CharacterController character;
        public int mMonsterId;
    }

    public class EnterArea : EventBase
    {
        public const string EVENT_TYPE = "EnterArea";

        public EnterArea(CharacterController c, int Id, bool Enter)
            : base(EVENT_TYPE)
        {
            character = c;
            mAreaId = Id;
            mIsEnter = Enter;
        }

        public CharacterController character;
        public int mAreaId;
        public bool mIsEnter;
    }

    public class TollgateFinish : EventBase
    {
        public const string EVENT_TYPE = "Tollgate";

        public TollgateFinish(CharacterController c, int Id)
            : base(EVENT_TYPE)
        {
            character = c;
            TollgateId = Id;
        }

        public CharacterController character;
        public int TollgateId { get; set; }
    }

    public class BuyItemEvent : EventBase
    {
        public const string EVENT_TYPE = "BuyItemEvent";

        public BuyItemEvent(CharacterController c, int id, int count)
            : base(EVENT_TYPE)
        {
            character = c;
            ItemId = id;
            ItemCount = count;
        }

        public CharacterController character;
        public int ItemCount { get; set; }
        public int ItemId { get; set; }
    }

    public class EquipItemEvent : EventBase
    {
        public const string EVENT_TYPE = "EquipItemEvent";

        public EquipItemEvent(CharacterController c, int part)
            : base(EVENT_TYPE)
        {
            character = c;
            EquipPart = part;
        }

        public CharacterController character;
        public int EquipPart { get; set; }
    }

    public class EnhanceEquipEvent : EventBase
    {
        public const string EVENT_TYPE = "EnhanceEquipEvent";

        public EnhanceEquipEvent(CharacterController c, int part)
            : base(EVENT_TYPE)
        {
            character = c;
            EquipPart = part;
        }

        public CharacterController character;
        public int EquipPart { get; set; }
    }

    public class AdditionalEquipEvent : EventBase
    {
        public const string EVENT_TYPE = "AdditionalEquipEvent";

        public AdditionalEquipEvent(CharacterController c, int part)
            : base(EVENT_TYPE)
        {
            character = c;
            EquipPart = part;
        }

        public CharacterController character;
        public int EquipPart { get; set; }
    }

    public class UpgradeSkillEvent : EventBase
    {
        public const string EVENT_TYPE = "UpgradeSkillEvent";

        public UpgradeSkillEvent(CharacterController c, int id)
            : base(EVENT_TYPE)
        {
            character = c;
            SkillID = id;
        }

        public CharacterController character;
        public int SkillID { get; set; }
    }

    public class NpcServeEvent : EventBase
    {
        public const string EVENT_TYPE = "NpcServeEvent";

        public NpcServeEvent(CharacterController c, int id)
            : base(EVENT_TYPE)
        {
            character = c;
            NpcServeID = id;
        }

        public CharacterController character;
        public int NpcServeID { get; set; }
    }

    public class ArenaEvent : EventBase
    {
        public const string EVENT_TYPE = "ArenaEvent";

        public ArenaEvent(CharacterController c)
            : base(EVENT_TYPE)
        {
            character = c;
        }

        public CharacterController character;
    }

    public class AddFriendEvent : EventBase
    {
        public const string EVENT_TYPE = "AddFriendEvent";

        public AddFriendEvent(CharacterController c)
            : base(EVENT_TYPE)
        {
            character = c;
        }

        public CharacterController character;
    }

    public class ComposeItemEvent : EventBase
    {
        public const string EVENT_TYPE = "ComposeItemEvent";

        public ComposeItemEvent(CharacterController c, int composeId, int itemId)
            : base(EVENT_TYPE)
        {
            character = c;
            ComposeId = composeId;
            ItemId = itemId;
        }

        public CharacterController character;
        public int ComposeId;
        public int ItemId;
    }


    public class CharacterExdataAddEvent : EventBase
    {
        public const string EVENT_TYPE = "CharacterExdataAddEvent";

        public CharacterExdataAddEvent(CharacterController c, int Id, int addValue)
            : base(EVENT_TYPE)
        {
            character = c;
            ExdataId = Id;
            AddValue = addValue;
        }

        public int AddValue;
        public CharacterController character;
        public int ExdataId;
    }

    public class SkillPointChangeEvent : EventBase
    {
        public const string EVENT_TYPE = "SkillPointChangeEvent";

        public SkillPointChangeEvent(CharacterController c, int Id, int value)
            : base(EVENT_TYPE)
        {
            character = c;
            SkillId = Id;
            Value = value;
        }

        public CharacterController character;
        public int SkillId;
        public int Value;
    }

	public class AddTalentEvent : EventBase
	{
		public const string EVENT_TYPE = "AddTalentEvent";

		public AddTalentEvent(CharacterController c, int id)
			: base(EVENT_TYPE)
		{
			character = c;
			Id = id;
		}

		public CharacterController character;
		public int Id;
	}

    public class CharacterDepotTakeOutEvent : EventBase
    {
        public const string EVENT_TYPE = "CharacterDepotTakeOutEvent";

        public CharacterDepotTakeOutEvent(CharacterController c, int Id, int cnt)
            : base(EVENT_TYPE)
        {
            character = c;
            itemID = Id;
            count = cnt;
        }

        public CharacterController character;
        public int count;
        public int itemID;
    }

	public class CharacterRechargeSuccessEvent : EventBase
	{
		public const string EVENT_TYPE = "CharacterRechargeSuccessEvent";

		public CharacterRechargeSuccessEvent(CharacterController c, int num)
			: base(EVENT_TYPE)
		{
			character = c;
			Num = num;
		}

		public CharacterController character;
		public int Num;
	}

	public class UseDiamondEvent : EventBase
	{
		public const string EVENT_TYPE = "UseDiamondEvent";

		public UseDiamondEvent(CharacterController c, int num)
			: base(EVENT_TYPE)
		{
			character = c;
			Num = num;
		}

		public CharacterController character;
		public int Num;
	}
    public class TollgateNextFinish : EventBase
    {
        public const string EVENT_TYPE = "TollgateNextFinish";

        public TollgateNextFinish(CharacterController c, int Id)
            : base(EVENT_TYPE)
        {
            character = c;
            TollgateId = Id;
        }

        public CharacterController character;
        public int TollgateId { get; set; }
    }
}