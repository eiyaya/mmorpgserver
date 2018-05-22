#region using

using System.Collections.Generic;
using NLog;

#endregion

namespace Shared
{
    public class FlagTree
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<int, int> IdIndex = new Dictionary<int, int>(); //key(Id),FlagIndex
        private readonly Dictionary<int, FlagTree> IdTree = new Dictionary<int, FlagTree>(); //key(Id),Tree
        private readonly Dictionary<int, FlagTree> IndexTree = new Dictionary<int, FlagTree>(); //FlagIndex,Tree
        private bool mbCleanSendSon = true; //是否需要清除标记时同时清除子的标记
        private bool mbSetSendDad = true; //是否需要设置标记时在父节点记录
        private FlagTree mDad; //树的父节点
        private BitFlag mFlag; //该树的标记位数据
        private int mTreeId; //树ID
        //添加子节点
        public FlagTree AddTree(int nId, int nCount)
        {
            if (IdTree.ContainsKey(nId))
            {
                Logger.Warn("AddTree {0} is faild!", nId);
                return null;
            }

            var result = CreateTree(nId, nCount);
            var nIndex = IdIndex.Count;
            IdTree[nId] = result;
            IdIndex[nId] = nIndex;
            IndexTree[nIndex] = result;
            result.mDad = this;
            return result;
        }

        //清除某个标记
        public void CleanFlag(int nIndex)
        {
            if (mFlag.GetFlag(nIndex) == 1)
            {
                mFlag.CleanFlag(nIndex);
                //是否需要清除该子标记
                if (mbCleanSendSon)
                {
                    FlagTree tree;
                    if (!IndexTree.TryGetValue(nIndex, out tree))
                    {
                        return;
                    }
                    tree.CleanFlagByDad();
                }
            }
        }

        //清除源于父节点的清除脏标记
        public void CleanFlagByDad()
        {
            mFlag.ReSetAllFlag();
        }

        //是否需要清除标记时同时清除子的标记
        public void CleanSendSon(bool b = false)
        {
            mbCleanSendSon = b;
        }

        //创建一棵树
        public static FlagTree CreateTree(int nId, int nCount)
        {
            var result = new FlagTree
            {
                mTreeId = nId,
                mFlag = new BitFlag(nCount)
            };
            return result;
        }

        //获得某个Id的子节点是否脏了
        public bool GetSonIsDirty(int nId)
        {
            FlagTree tree;
            if (!IdTree.TryGetValue(nId, out tree))
            {
                return false;
            }
            return tree.mFlag.IsDirty();
        }

        //设置脏标记
        public void SetFlag(int nIndex)
        {
            if (mFlag.GetFlag(nIndex) == 0)
            {
                mFlag.SetFlag(nIndex);
                //是否需要记录标记
                if (mbSetSendDad)
                {
                    if (mDad != null)
                    {
                        mDad.SetFlagBySon(mTreeId);
                    }
                }
            }
        }

        //某个ID的子设置了脏标记
        public void SetFlagBySon(int treeId)
        {
            int n;
            if (!IdIndex.TryGetValue(treeId, out n))
            {
                Logger.Warn("SetFlagBySon {0} is faild!", treeId);
                return;
            }
            SetFlag(n);
        }

        //是否需要设置标记时在父节点记录
        public void SetSendDad(bool b = false)
        {
            mbSetSendDad = b;
        }
    }
}