#region using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using Scorpion;
using Mono.GameMath;
using Shared;

#endregion

namespace Scene
{
    public class PathManager
    {
        public static bool AddCollider(Coroutine co,
                                       SceneObstacle obstacle,
                                       ObjCharacter obj,
                                       float radius,
                                       AsyncReturnValue<uint> result)
        {
            if (!mWaitingPathToFind.IsAddingCompleted)
            {
                mWaitingPathToFind.Add(() =>
                {
                    var b = obstacle.AddCollider(obj.GetPosition(), radius);
                    result.Value = b;

                    if (!SceneServer.Instance.ServerControl.mWaitingEvents.IsAddingCompleted)
                    {
                        SceneServer.Instance.ServerControl.mWaitingEvents.Add(new ContinueEvent(co));
                    }
                });
            }

            return true;
        }

        public static void DisposeObstacle(SceneObstacle obstacle)
        {
            if (!mWaitingPathToFind.IsAddingCompleted)
            {
                mWaitingPathToFind.Add(() => { obstacle.Close(); });
            }
        }

        public static bool FindPath(Coroutine co,
                                    SceneObstacle obstacle,
                                    ObjCharacter obj,
                                    Vector2 start,
                                    Vector2 end,
                                    AsyncReturnValue<List<Vector2>> result)
        {
            if (mWaitingPathToFind.Count > 100)
            {
                if (obj.GetObjType() == ObjType.NPC)
                {
                    var npc = obj as ObjNPC;
                    if (npc.CurrentState != BehaviorState.Combat)
                    {
                        result.Value = SceneObstacle.EmptyPath;
                        if (!SceneServer.Instance.ServerControl.mWaitingEvents.IsAddingCompleted)
                        {
                            SceneServer.Instance.ServerControl.mWaitingEvents.Add(new ContinueEvent(co));
                        }

                        return false;
                    }
                }
            }

            if (!mWaitingPathToFind.IsAddingCompleted)
            {
                mWaitingPathToFind.Add(() =>
                {
                    var path = obstacle.FindPath(start, end);
                    result.Value = path;

                    if (path.Count == 0)
                    {
                        // Count为0时，我们认为start点是无效的
                        // 当前点的格子中心点是有效的。。存在无效的可能？？
                        var ValidStart = new Vector2(((int) (start.X * 2)) / 2.0f, ((int) (start.Y * 2)) / 2.0f);

                        path = obstacle.FindPath(ValidStart, end);
                        result.Value = path;
                    }

                    if (!SceneServer.Instance.ServerControl.mWaitingEvents.IsAddingCompleted)
                    {
                        SceneServer.Instance.ServerControl.mWaitingEvents.Add(new ContinueEvent(co));
                    }
                });
            }

            return true;
        }

        public static bool Raycast(Coroutine co,
                                   SceneObstacle obstacle,
                                   ObjCharacter obj,
                                   Vector2 start,
                                   Vector2 end,
                                   AsyncReturnValue<Vector2> result)
        {
            if (!mWaitingPathToFind.IsAddingCompleted)
            {
                mWaitingPathToFind.Add(() =>
                {
                    Vector2 hit;
                    var b = obstacle.Raycast(start, end, out hit);
                    result.Value = hit;

                    if (!SceneServer.Instance.ServerControl.mWaitingEvents.IsAddingCompleted)
                    {
                        SceneServer.Instance.ServerControl.mWaitingEvents.Add(new ContinueEvent(co));
                    }
                });
            }

            return true;
        }

        public static bool RemoveCollider(SceneObstacle obstacle, uint id)
        {
            if (!mWaitingPathToFind.IsAddingCompleted)
            {
                mWaitingPathToFind.Add(() => { obstacle.RemoveCollider(id); });
            }

            return true;
        }

        public static void Start()
        {
            try
            {
                mPathFindingThread.Start();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void Stop()
        {
            mWaitingPathToFind.CompleteAdding();
            mPathFindingThread.Join();
        }

        private static readonly BlockingCollection<Action> mWaitingPathToFind = new BlockingCollection<Action>();

        private static readonly Thread mPathFindingThread = new Thread(() =>
        {
            mWaitingPathToFind.GetConsumingEnumerable().ToObservable().Subscribe(act =>
            {
                if (act != null)
                {
                    try
                    {
                        act();
                        act = null;
                    }
                    catch
                    {
                        // do nothing...
                    }
                }
            });
        });
    }
}