#region using

using System;
using System.Collections.Generic;
using System.Linq;
using DataTable;
using Mono.GameMath;
using NLog;

#endregion

namespace Scene
{
    public interface ITriggerArea
    {
        void AdjustPlayer(TriggerArea _this, ObjCharacter player);
        bool Cantains(TriggerArea _this, ObjCharacter character);
        void Destroy(TriggerArea _this);
        void InitTriggerArea(TriggerArea _this, int id, Scene scene);
        bool IsInArea(TriggerArea _this, Vector2 pos);
        void Remove(TriggerArea _this, ObjCharacter character);
        void Tick(TriggerArea _this, float delta);
    }

    public class TriggerAreaDefaultImpl : ITriggerArea
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private void Add(TriggerArea _this, ObjCharacter character)
        {
            if (null == character)
            {
                return;
            }

            if (!_this.mCharacterList.Contains(character))
            {
                _this.mCharacterList.Add(character);

                try
                {
                    _this.Scene.OnCharacterEnterArea(_this.Id, character);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Scene.OnCharacterEnterArea");
                }
            }
            else
            {
                Logger.Warn("TriggerArea mObjDict has this");
            }

            if (character.GetObjType() == ObjType.PLAYER)
            {
                if (!_this.mPlayer.Contains(character))
                {
                    _this.mPlayer.Add(character as ObjPlayer);
                }
            }
        }

        public void InitTriggerArea(TriggerArea _this, int id, Scene scene)
        {
            _this.Id = id;
            _this.Scene = scene;

            _this.TableTriggerArea = Table.GetTriggerArea(_this.Id);
            if (null != _this.TableTriggerArea)
            {
                if (0 == _this.TableTriggerArea.AreaType)
                {
//0是陷阱那种(会重复发生)，1是开门那种(只发生一次，触发条件在这个区域里描述不了)
                    _this.mBuffId = _this.TableTriggerArea.Param[0]; //buff id
                    _this.mBuffLevel = _this.TableTriggerArea.Param[1]; //buff 等级
                    _this.mBuffIntervalMilliseconds = Math.Max(_this.mBuffIntervalMilliseconds,
                        _this.TableTriggerArea.Param[2]); //最小时间间隔
                    var tableId = _this.TableTriggerArea.Param[3];
                    if (tableId != -1)
                    {
                        var skillUpgrade = Table.GetSkillUpgrading(tableId);
                        if (null != skillUpgrade)
                        {
                            _this.mCamps = new List<int>(skillUpgrade.Values);
                        }
                    }
                }
                _this.TirggerRadiusSqrt = _this.TableTriggerArea.Radius*_this.TableTriggerArea.Radius;
            }
        }

        public void Remove(TriggerArea _this, ObjCharacter character)
        {
            if (character.GetObjType() == ObjType.PLAYER)
            {
                _this.mPlayer.Remove(character as ObjPlayer);
            }

            if (_this.mCharacterList.Contains(character))
            {
                _this.mCharacterList.Remove(character);
                try
                {
                    _this.Scene.OnCharacterLeaveArea(_this.Id, character);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Scene.OnCharacterLeaveArea");
                }
            }
            else
            {
                Logger.Warn("TriggerArea mObjDict doesn't has this");
            }
        }

        public void AdjustPlayer(TriggerArea _this, ObjCharacter player)
        {
            if (IsInArea(_this, player.GetPosition()))
            {
                if (!_this.mCharacterList.Contains(player))
                {
                    Add(_this, player);
                }
            }
            else
            {
                if (_this.mCharacterList.Contains(player))
                {
                    Remove(_this, player);
                }
            }
        }

        public void Tick(TriggerArea _this, float delta)
        {
            try
            {
                _this.Scene.OnAreaTick(_this.Id, _this.mCharacterList.GetEnumerator());
            }
            catch (Exception e)
            {
                Logger.Error(e, "Scene.OnAreaTick");
            }

            if (_this.mPlayer.Count > 0 && _this.mTriggerTime < DateTime.Now)
            {
                if (-1 != _this.mBuffId && _this.mBuffLevel >= 0)
                {
                    _this.mTriggerTime = DateTime.Now.AddMilliseconds(_this.mBuffIntervalMilliseconds);
                    foreach (var player in _this.mPlayer)
                    {
                        if (null != _this.mCamps)
                        {
                            if (!_this.mCamps.Contains(player.GetCamp()))
                            {
                                continue;
                            }
                        }
                        player.AddBuff(_this.mBuffId, _this.mBuffLevel, player);
                    }

                    if (null != _this.TableTriggerArea && null != _this.Scene &&
                        -1 != _this.TableTriggerArea.ClientAnimation)
                    {
                        SceneServer.Instance.ServerControl.NotifySceneAction(_this.Scene.EnumAllPlayerId(),
                            _this.TableTriggerArea.ClientAnimation);
                    }
                }
            }
        }

        public bool IsInArea(TriggerArea _this, Vector2 pos)
        {
            var distance =
                new Vector2(pos.X - _this.TableTriggerArea.PosX, pos.Y - _this.TableTriggerArea.PosZ).LengthSquared();
            return (distance <= _this.TirggerRadiusSqrt);
        }

        public bool Cantains(TriggerArea _this, ObjCharacter character)
        {
            return _this.mCharacterList.Contains(character);
        }

        public void Destroy(TriggerArea _this)
        {
        }
    }

    public class TriggerArea
    {
        private static ITriggerArea mImpl;

        static TriggerArea()
        {
            SceneServer.Instance.UpdateManager.InitStaticImpl(typeof (TriggerArea), typeof (TriggerAreaDefaultImpl),
                o => { mImpl = (ITriggerArea) o; });
        }

        public TriggerArea(int id, Scene scene)
        {
            mImpl.InitTriggerArea(this, id, scene);
        }

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public int mBuffId = -1;
        public int mBuffIntervalMilliseconds = 50;
        public int mBuffLevel = -1;
        public List<int> mCamps;
        public List<ObjCharacter> mCharacterList = new List<ObjCharacter>();
        public List<ObjPlayer> mPlayer = new List<ObjPlayer>();
        public DateTime mTriggerTime = DateTime.Now;
        public float TirggerRadiusSqrt;
        public int Id { get; set; }
        public Scene Scene { get; set; }
        public TriggerAreaRecord TableTriggerArea { get; set; }

        public void AdjustPlayer(ObjCharacter player)
        {
            mImpl.AdjustPlayer(this, player);
        }

        public bool Cantains(ObjCharacter character)
        {
            return mImpl.Cantains(this, character);
        }

        public void Destroy()
        {
            mImpl.Destroy(this);
        }

        public bool IsInArea(Vector2 pos)
        {
            return mImpl.IsInArea(this, pos);
        }

        public void Remove(ObjCharacter character)
        {
            mImpl.Remove(this, character);
        }

        public void Tick(float delta)
        {
            mImpl.Tick(this, delta);
        }
    }
}