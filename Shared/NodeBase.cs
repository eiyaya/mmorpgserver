#region using

using System.Collections.Generic;

#endregion

namespace Shared
{
    public abstract class NodeBase
    {
        private bool mDbDirty;
        public abstract IEnumerable<NodeBase> Children { get; }

        public bool DbDirty
        {
            get { return mDbDirty; }
            set
            {
                mDbDirty = value;
                if (value)
                {
                    if (Parent != null)
                    {
                        Parent.DbDirty = true;
                    }
                }
                else
                {
                    var children = Children;
                    if (children != null)
                    {
                        foreach (var child in Children)
                        {
                            child.DbDirty = false;
                        }
                    }
                }
            }
        }

        public bool NetDirty { get; private set; }
        private NodeBase Parent { get; set; }

        public virtual void AddChild(NodeBase node)
        {
            node.Parent = this;
        }

        public void CleanDbDirty()
        {
            mDbDirty = false;
            var children = Children;
            if (children != null)
            {
                foreach (var child in Children)
                {
                    child.CleanDbDirty();
                }
            }
        }

        public void CleanNetDirty()
        {
            NetDirty = false;
            var children = Children;
            if (children != null)
            {
                foreach (var child in Children)
                {
                    child.CleanNetDirty();
                }
            }
        }

        public void MarkDbDirty()
        {
            mDbDirty = true;
            if (Parent != null)
            {
                Parent.MarkDbDirty();
            }
        }

        public void MarkDirty()
        {
            mDbDirty = true;
            NetDirty = true;

            if (Parent != null)
            {
                Parent.MarkDirty();
            }
        }

        public void MarkNetDirty()
        {
            NetDirty = true;

            if (Parent != null)
            {
                Parent.MarkNetDirty();
            }
        }

        public virtual void NetDirtyHandle()
        {
        }
    }
}