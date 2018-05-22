namespace Scene
{

    #region     //伤害类型

    public enum eDamageType
    {
        Physical = 0, //物理伤害（受物理防御影响）
        Magic = 1, //法术伤害（受法术防御影响）
        IgnoreArmor = 2, //无视防御
        Ice = 3, //不播放被击动作的伤害（固定伤害：不加成 不减免) (不反弹)
        Blood = 4, //流血伤害（固定伤害：不加成 不减免) (不反弹)
        Rebound = 5, //反弹伤害
        FireAttr = 6, // 火属性
        IceAttr = 7, //  冰属性
        PoisonAttr = 8 // 毒属性
    }

    #endregion

    #region     //Buff类型

    public enum eBuffType
    {
        Other = 0, //其他
        DeBuff = 1, //负面
        Buff = 2, //正面
        ReduceMoveSpeed = 3, //减速
        NoMove = 4, //定身
        Blind = 5, //致盲
        Stun = 6, //眩晕
        Fear = 7, //恐惧
        DurationHealth = 8, //HOT
        DurationDamage = 9, //DOT
        KnockDown = 10, //击倒
        KnockFly = 11 //击飞
    }

    #endregion

    #region     //Buff消失类型

    public enum eCleanBuffType
    {
        TimeOver = 0, //时间结束
        Huchi = 1, //被互斥掉了（互斥，替换,优先级)
        Clear = 2, //驱散
        EffectOver = 3, //效果结束
        LayerZero = 4, //层数为0
        ForgetSkill = 5, //被动技能遗忘了
        ForgetTalent = 6, //天赋遗忘了
        EquipTie = 7, //套装更换
        Die = 8, //死亡
        RetinueDie = 9, //召唤生物死亡
        DeleteEquip = 10, //删除装备带着的buf
        RemoveElf = 11, // 删除精灵
        AbsorbOver = 12,    // 吸收伤害完了
        GoHome = 13, //怪物回家了
    }

    #endregion

    #region     //治疗类型

    public enum eHealthType
    {
        Item = 0, //道具治疗
        Skill = 1, //技能治疗
        Vampire = 2, //吸血治疗
        Set = 3 //强制治疗
    }

    #endregion

    #region     //控制权

    public enum eControlType
    {
        Move = 0, //移动
        Attack = 1, //攻击
        PhysicsSkill = 2, //物理技能
        MagicSkill = 3, //法术技能
        ForceSkill = 4 //强制技能
    }

    #endregion

    #region     //目标类型

    public enum eTargetType
    {
        Self = 0, //Buff释放者(自己)
        Target = 1, //Buff承受者(目标)    
        Around = 2, //周围                0半径
        Fan = 3, //扇形                0 半径   1 度数    
        Rect = 4, //长方形              0 宽度   1 长度      
        TargetAround = 5, //到目标周围的圆       0 半径  1施法距离    
        TargetRect = 6, //到目标的长方形      0 宽度  1长度-1    2施法距离
        TargetFan = 7, //到目标的扇形        0半径   1度数     2施法距离      
        Ejection = 8 //弹射               0=施法距离  1=传递半径  2=传递次数 3=是否弹射  4=单目标承受次数  5=参数修正
    }

    #endregion

    #region     //阵营类型

    public enum eCampType
    {
        Enemy = 0, //敌人
        Neutral = 1, //中立
        Friend = 2, //友好
        Team = 3, //队伍
        All = 4 //所有
    }

    #endregion

    #region     //朝向类型

    public enum eDirectionType
    {
        Caster = 0, //释放者的当前朝向
        ToCaster = 1, //朝向施法者
        Bear = 2, //承受者的当前朝向
        ToBear = 3 //朝向承受者
    }

    #endregion

    #region     //效果列表

    public enum eEffectType
    {
        DoDamage = 0, //伤害    1、伤害类型   2、目标选择    3、属性ID   4、属性比例   5、固定修正最小值   6、固定修正最大值
        DoHealth = 1, //治疗    1、治疗类型   2、目标选择    3、属性ID   4、属性比例   5、固定修正最小值   6、固定修正最大值
        RefAttr = 2, //修改属性  1、修改的属性ID   2、按属性ID修改     3、属性比例    4、固定修正 
        PositionChange = 3, //位置修正  1、方向    2、距离类型   3、距离修正   4、位移后朝向
        ProAddBuff = 4, //释放Buff  1、概率    2、目标类型   3、TrueBuffId   4、FalseBuffId
        CasterRefHealth = 5, //释放方治疗万分比修正    1、治疗类型   2、修改比例
        BearRefHealth = 6, //承受方治疗万分比修正    1、治疗类型   2、修改比例
        CasterRefDamage = 7, //释放方伤害万分比修正    1、伤害类型   2、修改比例
        BearRefDamage = 8, //承受方伤害万分比修正    1、伤害类型   2、修改比例
        //嘲讽
        SpecialState = 9, //特殊状态  1、特定状态  2、技能限制
        NoBuffType = 10, //免疫Buff类型  1、Buff类型   2、Buff效果    3、伤害类型
        DispelBuff = 11, //驱散Buff类型
        //控制权限制
        AbsorbInjury = 12, //吸收伤害                  //吸收完伤害会消失Buff
        //隐身
        //陷阱
        //修正技能参数
        //降低Buff层数
        //NoDamage = 13,             //免疫伤害    1、伤害类型
        CreateMonsterType1 = 13, //召唤怪物   1、怪物ID   2、召唤数量  3、最小距离   4、最大距离   5、夹角度数
        ModifySkill = 14, //修改Skill  1、修改SkillID  2、Skill参数类型  3、影响类型 4、影响参数
        ModifyBuff = 15, //修改Buff    1、修改BUFFID  2、BUFF参数类型  3、影响类型 4、影响参数
        ModifyModel = 16, //修改模型     1、新的CharModelId
        CreateMonster = 17, //召唤怪物    1、怪物ID
        DamageHealth = 18, //吸血        1、治疗类型  2、目标类型  3、万分比影响
        ExpModify = 23, //经验修正        1、万分比修正  
        DoMana = 24, //回蓝    1、治疗类型   2、目标选择    3、属性ID   4、属性比例   5、固定修正最小值   6、固定修正最大值
        KillSelf = 25, //自杀
        ExpModify2 = 26, //经验修正        1、万分比修正  
        DoSkill = 27,    // 施放技能 1、技能id
		HpInRangeTriggerBuff = 28,    // 档血量在某个区间内给目标上个buff, 1、血量下线万分比  2、血量上线万分比  3、目标类型  4、buffid
        AddExp = 29     //最外层经验
    }

    #endregion

    #region     //触发点列表

    public enum eEffectEventType
    {
        GetBuff = 0, //获得时
        MissBuff = 1, //消失时
        WillDie = 2, //将要死亡时
        RealDie = 3, //真正死亡时
        CauseDie = 4, //造成死亡时
        Critical = 5, //暴击时
        WasCrit = 6, //被暴击时
        CauseDamage = 7, //造成伤害时
        BearDamage = 8, //受到伤害时
        CauseHealth = 9, //造成治疗时
        BearHealth = 10, //受到治疗时
        SecondOne = 11, //每1秒
        SecondThree = 12, //每3秒
        SecondFive = 13, //每5秒
        HpLessPercent = 14, // 血量低于多少
		OnTrapped = 15, // 档被限制移动时
        EVENT_COUNT
    }

    #endregion

    #region     //命中类型

    public enum eHitType
    {
        Miss = 0, //Miss
        Excellent = 1, //卓越一击
        Lucky = 2, //幸运一击
        Hit = 3, //命中
        Count
    }

    #endregion

    public enum eAddskillType
    {
        ResetSkill = 0,
        CheckDoSkill = 1,
        DoSkill = 2,
        DoSkill2 = 3,
        InitSkill = 4,
        EquipSkill = 5,
        InitSkillObjRetinue = 6,
        InitByRobot = 7,
        InitByRobot2 = 8,
        AutoPlayer = 9,
        AutoPlayer2 = 10,
        ApplySkill = 11,
        SkillChange = 12,
        EquipAddSkill = 13
    }

    #region //技能命中类型

    public enum eSkillHitType
    {
        Normal = 0, //普通
        Health = 1, //治疗
        Hit = 2, //必然命中（无视闪避）
        Critical = 3 //必然暴击（暴击率最大）
    }

    #endregion

    #region     //Buff参数类型

    public enum eModifyBuffType
    {
        Duration = 0, //BUFF持续时间（毫秒）
        Type = 1, //BUFF类型
        LayerMax = 2, //叠加层数上限
        Effect1Id = 3, //效果1ID
        Effect1Point = 4, //效果1触发点
        Effect1Param1 = 5, //效果1参数1
        Effect1Param2 = 6, //效果1参数2
        Effect1Param3 = 7, //效果1参数3
        Effect1Param4 = 8, //效果1参数4
        Effect1Param5 = 9, //效果1参数5
        Effect1Param6 = 10, //效果1参数6
        Effect2Id = 11, //效果2ID
        Effect2Point = 12, //效果2触发点
        Effect2Param1 = 13, //效果2参数1
        Effect2Param2 = 14, //效果2参数2
        Effect2Param3 = 15, //效果2参数3
        Effect2Param4 = 16, //效果2参数4
        Effect2Param5 = 17, //效果2参数5
        Effect2Param6 = 18, //效果2参数6
        Effect3Id = 19, //效果3ID
        Effect3Point = 20, //效果3触发点
        Effect3Param1 = 21, //效果3参数1
        Effect3Param2 = 22, //效果3参数2
        Effect3Param3 = 23, //效果3参数3
        Effect3Param4 = 24, //效果3参数4
        Effect3Param5 = 25, //效果3参数5
        Effect3Param6 = 26, //效果3参数6
        Effect4Id = 27, //效果4ID
        Effect4Point = 28, //效果4触发点
        Effect4Param1 = 29, //效果4参数1
        Effect4Param2 = 30, //效果4参数2
        Effect4Param3 = 31, //效果4参数3
        Effect4Param4 = 32, //效果4参数4
        Effect4Param5 = 33, //效果4参数5
        Effect4Param6 = 34, //效果4参数6
        Effect1PointParam = 35, // 触发点1参数
        Effect2PointParam = 36, // 触发点2参数
        Effect3PointParam = 37, // 触发点3参数
        Effect4PointParam = 38, // 触发点4参数
    }

    #endregion

    #region     //通知技能的事件类型

    public enum eSkillEventType
    {
        ForceStop = 0, //强制打断
        Move = 1, //移动了
        Silence = 2, //沉默了
        Stun = 3 //眩晕了
    }

    #endregion

    #region     //技能施放类型
    public enum eDoSkillType
    {
        None = -1, // 无
        Normal = 0, // 
        Character = 1, // 角色技能
        Other = 2, // 其它
        ForceSkill = 3 // 强制技能(不判断公共cd,不打断当前技能)
    }

    #endregion
}