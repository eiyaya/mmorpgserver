using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTable
{

    #region Test
    #endregion

    public class Introduction
    {
        //服务器物理结构
        public void PhysicalStructure()
        {

            #region Gate：负责连接客户端与Broker的数据传输
            #endregion

            #region AnyBroker：可以分布式（一拖N数据转发）的数据传输服务器

            {
                #region LoginBroker：登录数据中转
                #endregion

                #region SceneBroker：场景数据中转
                #endregion

                #region LogicBroker：逻辑数据中转
                #endregion

                #region ChatBroker：聊天数据中转
                #endregion

                #region RankBroker：排行数据中转
                #endregion
            }


            #endregion

            #region Anys：具体功能服务器

            {
                #region Login：登陆服务器
                #endregion

                #region Scene：场景服务器
                #endregion

                #region Logic：逻辑服务器
                #endregion

                #region Chat：聊天服务器
                #endregion

                #region Rank：排行服务器
                #endregion
            }

            #endregion

            #region ShareMemory：共享缓存
            #endregion

            #region DB：数据库(MySQL)
            #endregion

        }

        //共享结构
        public void SharingStructure()
        {

            #region Protocl(NET)：网络数据结构
            {

                #region Client：客户端
                #endregion

                #region Servers：相应服务器链

                {

                    #region Gate：第1层转发
                    #endregion

                    #region Broker：第2层转发
                    #endregion
                    
                    #region Anys：具体功能逻辑服务器
                    #endregion

                }


                #endregion

            }
            #endregion

            #region Protocl(DB)：存储数据结构
            {

                #region DB：数据库
                #endregion
                
                #region ShareMemory：共享缓存
                #endregion

                #region Servers：相应服务器
                #endregion

            }
            #endregion

            #region DataTable：服务器表格数据
            {

                #region Login：登录服务器
                #endregion

                #region Scene：场景服务器
                #endregion

                #region Logic：逻辑服务器
                #endregion

                #region Chat：聊天服务器
                #endregion

                #region Rank：排行服务器
                #endregion

            }
            #endregion

            #region Shared：共享内容
            {

                #region Client：客户端
                #endregion

                #region Anys：具体功能服务器

                {
                    #region Login：登录服务器
                    #endregion

                    #region Scene：场景服务器
                    #endregion

                    #region Logic：逻辑服务器
                    #endregion

                    #region Chat：聊天服务器
                    #endregion

                    #region Rank：排行服务器
                    #endregion
                }

                #endregion

            }
            #endregion

        }


        //登录服务器
        public void LoginServer()
        {

            #region Login：登录功能（详见登陆流程）
            #endregion
            
            #region Login Assist：登录辅助
            {

                #region Account：账号相关
                {

                    #region State：状态（在线，离线，封停）
                    #endregion
                    
                    #region Data：数据（各服务器角色Id）
                    #endregion

                }
                #endregion

                #region Character：角色相关
                {

                    #region State：状态（在线，离线，封停）
                    #endregion

                    #region Data：数据（ID及相关服务器信息）
                    #endregion

                }
                #endregion

            }
            #endregion
            
            #region  Logout：退出
            {
                

            }
            #endregion
            
            #region  GM：管理员权限
            {

                #region  Kick：踢下线
                #endregion

                #region  Ban：封停
                #endregion

                #region  No talking：禁言
                #endregion

            }
            #endregion

        }

        //场景服务器
        public void SceneServer()
        {

            #region  Scene：场景相关
            {

                #region  Enter Scene：进入场景
                #endregion

                #region  Leave Scene：离开场景
                #endregion

                #region  Swap Scene：切换场景
                #endregion

            }
            #endregion
            
            #region  Character：角色
            {

                #region  NPC：非玩家控制的角色
                {

                }
                #endregion

                #region  Player：玩家控制的角色
                {

                }
                #endregion

            }
            #endregion

            #region  Action：行为相关
            {

                #region  Move：移动
                {
                    #region  Rocker：摇杆移动
                    #endregion

                    #region  Target Point：目标点移动
                    #endregion
                }
                #endregion


                #region  Skill：技能
                {

                    #region  Prompt：瞬发
                    #endregion

                    #region  Sing：吟唱
                    #endregion

                    #region  Guide：引导
                    #endregion

                    #region  Passive：被动
                    #endregion

                }
                #endregion

            }
            #endregion
            
            #region  Fight：战斗相关
            {

                #region  Attribute：属性相关
                {

                    #region  Base：基础属性
                    #endregion

                    #region  Equip：装备属性
                    #endregion

                    #region  Buff：效果属性
                    #endregion

                }
                #endregion
                
                #region  Skill：技能相关
                {

                    #region  Check：检查
                    #endregion

                    #region  Consume：消耗
                    #endregion

                    #region  Cast：释放
                    #endregion

                    #region  Do：生效
                    #endregion

                    #region  Stop：打断
                    #endregion

                }
                #endregion
                
                #region  Buff：Buff相关
                {

                    #region  Add Buff：获得Buff
                    {

                        #region  Mutex：互斥
                        #endregion

                        #region  Priority：优先级
                        #endregion

                        #region  Layer：叠层
                        #endregion

                    }
                    #endregion

                    #region  Delete Buff：删除Buff
                    {

                        #region  Time Over：时间结束
                        #endregion

                        #region  Mutex：互斥
                        #endregion

                        #region  Dispel：驱散
                        #endregion

                        #region  Forget：遗忘（技能或天赋取消）
                        #endregion

                    }
                    #endregion

                }
                #endregion

                #region  Effect：效果相关
                {

                    #region  Damage：伤害
                    #endregion

                    #region  Health：治疗
                    #endregion

                    #region  Attribute：属性修改
                    #endregion

                    #region  Postion：位置变化
                    #endregion

                    #region  NewBuff：触发新Buff
                    #endregion
                    
                    #region  Damage Modify：伤害修正
                    #endregion

                    #region  Health Modify：治疗修正
                    #endregion

                    #region  Damage Immune：伤害免疫
                    #endregion

                    #region  Action Curb ：行动限制
                    #endregion

                    #region  Suck Blood ：吸血
                    #endregion

                }
                #endregion

                #region  Event：事件相关
                {
                    #region  Get ：获得时
                    #endregion

                    #region  MISS ：消失时
                    #endregion
                    
                    #region  WILL_DIE ：将要死亡时
                    #endregion

                    #region  REAL_DIE ：真正死亡时
                    #endregion
                    
                    #region  CAUSE_DIE ：造成死亡时
                    #endregion

                    #region  CRITICAL ：暴击时
                    #endregion

                    #region  DODGE ：闪避时
                    #endregion
                    
                    #region  CAUSE_DAMAGE ：造成伤害时
                    #endregion

                    #region  BEAR_DAMAGE ：受到伤害时
                    #endregion
                    
                    #region  CAUSE_HEALTH ：造成治疗时
                    #endregion

                    #region  BEAR_HEALTH ：受到治疗时
                    #endregion
                    
                    #region  SECOND_ONE ：每1秒
                    #endregion
                    
                    #region  SECOND_THREE ：每3秒
                    #endregion

                    #region  SECOND_FIVE ：每5秒
                    #endregion

                }
                #endregion

            }
            #endregion

        }

        //逻辑服务器
        public void LogicServer()
        {

            #region  Flag：标记位系统
            {

                #region  Mission：邮件标记
                #endregion
                
                #region  Toll Gate：关卡标记
                #endregion

                #region  Achievement：成就标记
                #endregion

            }
            #endregion
            
            #region  Exdata：扩展数据系统
            {
                #region  Toll Gate：关卡数据
                #endregion

                #region  Achievement：成就数据
                #endregion
            }
            #endregion
            
            #region  Bag：包裹系统
            {
                
                #region  Type：包裹類型
                {

                    #region  Resources：资源包裹
                    #endregion

                    #region  Equip：装备包裹
                    #endregion

                    #region  common：普通包裹
                    #endregion

                    #region  Piece：碎片包裹
                    #endregion

                    #region  Gem：宝石包裹
                    #endregion

                    #region  EquipSlot：所有装备部位
                    #endregion

                }
                #endregion
                
                #region  Interface：接口
                {

                    #region  Add：增加
                    #endregion
                    
                    #region  Del：刪除
                    #endregion
                    
                    #region  Reset：修改
                    #endregion

                    #region  Get：查询
                    #endregion
                }
                #endregion

            }
            #endregion

            #region  Items：道具系统
            {

                #region  Equip：装备
                {

                    #region  Use：穿戴
                    #endregion

                    #region  Enchance：强化
                    #endregion

                    #region  Enchant：附魔
                    #endregion

                }
                #endregion

                #region  common：普通道具
                #endregion
                
                #region  Gem：宝石
                {

                    #region  Swap：换洗
                    #endregion

                    #region  compose：合成
                    #endregion

                    #region  On：鑲嵌
                    #endregion

                    #region  Off：摘除
                    #endregion

                }
                #endregion

            }
            #endregion
            
            #region  Event：事件系统
            {

                    #region  Trueflag：标记位改真
                    #endregion

                    #region  Falseflag：标记位改假
                    #endregion

                    #region  ExDataChange：数据改变
                    #endregion

                    #region  ItemChange：物品改变
                    #endregion

                    #region  KillMonster：杀怪
                    #endregion

                    #region  EnterArea：区域事件
                    #endregion

                    #region  Tollgate：关卡
                    #endregion

            }
            #endregion

            #region  Mission：邮件系统
            {

                #region  Accept：接受
                #endregion

                #region  Drop：放弃
                #endregion

                #region  Complete：完成
                #endregion

            }
            #endregion

            #region  Event：天赋系统
            {

                #region  Add：增加
                #endregion

                #region  Delete：删除
                #endregion

                #region  Clean：清空单条
                #endregion

                #region  Reset：重置
                #endregion

            }
            #endregion
            
        }

        //聊天服务器
        public void ChatServer()
        {

            #region Scene：单场景聊天
            #endregion

            #region Friend：好友密语
            #endregion

            #region Alliance：联盟聊天
            #endregion

            #region Near：附近人
            #endregion

            #region Server：服务器广播
            #endregion

            #region GM：全服公告
            #endregion

        }

        //排行服务器
        public void RankServer()
        {

        }
        #region Rank：排行服务器
        #endregion


        //登录流程
        public void LoginProcess()
        {

            #region SelectLoginType：选择登录方式
            {

                #region Direct：直接登录
                #endregion

                #region Account：账号登录
                {

                    #region Phone：手机号
                    #endregion

                    #region Mail：邮箱
                    #endregion

                    #region Custom：自定义账号
                    #endregion
                }
                #endregion

                #region Third：第三方登录
                {

                    #region QQ
                    #endregion

                    #region 新浪微博
                    #endregion

                    #region 微信
                    #endregion

                    #region 360
                    #endregion

                    #region 百度
                    #endregion

                    #region 小米
                    #endregion

                    #region 。。。。。。
                    #endregion

                }
                #endregion

            }
            #endregion

            #region Verify Landing：验证登录
            {

                #region False：失败（断开验证链接，允许重新输入）
                #endregion

                #region True：成功（进入下一步流程）
                #endregion

            }
            #endregion

            #region Login：登录
            {

                #region Gate：随机或按地区链接
                #endregion

                #region LoginBroker：选择分担压力的Login服务器
                #endregion

                #region Login：根据选定的登录方式查询数据
                {

                    #region Servers By Game：获取当前游戏所有服务器列表
                    #endregion

                    #region Characters By Game：获取当前游戏所有角色的所在服务器
                    #endregion

                    #region Recommend Servers：获取当前游戏推荐服务器
                    #endregion

                }
                #endregion

            }
            #endregion

            #region SelectServer：选择服务器
            {

                #region New：创建新角色
                #endregion

                #region Old：登录老角色
                #endregion

            }
            #endregion

            #region EnterGame：进入游戏
            {

                #region Prepare Data：准备数据
                {

                    #region Scene Read From DB：读取角色的场景数据
                    #endregion

                    #region Logic Read From DB：读取角色的逻辑数据
                    #endregion

                    #region Chat Read From DB：读取角色的聊天数据
                    #endregion

                }
                #endregion

                #region Sync Data：同步数据
                {

                    #region Logic to Scene： 逻辑同步到场景
                    {

                        #region Equip：装备数据
                        {

                        }
                        #endregion

                        #region Skill：技能数据
                        {

                        }
                        #endregion

                        #region Level：等级数据
                        {

                        }
                        #endregion

                    }
                    #endregion

                    #region Logic to Chat： 逻辑同步到聊天
                    {

                        #region Friend：好友成员数据
                        {

                        }
                        #endregion

                        #region Alliance：联盟成员数据
                        {

                        }
                        #endregion

                    }
                    #endregion

                    #region Logic to Rank： 逻辑同步到排行榜
                    {

                        #region Level：等级数据
                        {

                        }
                        #endregion

                        #region Power：战斗力
                        {

                        }
                        #endregion

                        #region 。。。。。。：等等
                        {

                        }
                        #endregion
                    }
                    #endregion

                }
                #endregion

                #region EnterScene：进入场景
                {

                    #region Logic Data：逻辑数据初始化
                    #endregion

                    #region Broadcast Other To Me：广播其他玩家给我
                    #endregion

                    #region Broadcast Me To Other：把我广播给其他玩家
                    #endregion

                }
                #endregion

            }
            #endregion

        }

    }
}
