using System;

namespace GGWP.Ecs
{
    public abstract class EcsSystem
    {
        public abstract void Update(float delta);
        public abstract void AddNode(EcsNode _node);
        public abstract void RemoveNode(EcsNode _node);
    }

    public struct SystemArgu<T> where T : EcsNode
    {
        public T Node;
        public float Delta;
        
        public SystemArgu(T _node, float _delta)
        {
            Node = _node;
            Delta = _delta;
        }
    }

    /// Node系統是個環型系統，為了方便增減
    public abstract class EcsSystem<T> : EcsSystem where T : EcsNode
    {
        private T headNode;

        public override void Update(float delta)
        {
            if (headNode != null)
            {
                UpdateNode(headNode, delta);
            }
        }

        public sealed override void AddNode(EcsNode _node)
        {
            AddNode((T)_node);
        }

        public virtual void AddNode(T _node)
        {
            if (headNode == null)
            {
                headNode = _node;
                headNode.PrevNode = headNode.NextNode = headNode;
            }
            else if (headNode.PrevNode == headNode)
            {
                // if there's no headNode.LastNode
                headNode.PrevNode = headNode.NextNode = _node;
                _node.PrevNode = _node.NextNode = headNode;
            }
            else
            {
                // Add to last
                _node.PrevNode = headNode.PrevNode;
                _node.NextNode = headNode;

                headNode.PrevNode.NextNode = _node;
                headNode.PrevNode = _node;
            }
        }
        
        public sealed override void RemoveNode(EcsNode _node)
        {
            RemoveNode((T)_node);
        }

        public virtual void RemoveNode(T _node)
        {
            // if node.LastNode is null, which means it's the only one.
            // And it's the headNode.
            if (_node == headNode)
            {
                if (_node.NextNode == headNode)
                {
                    headNode = null;
                    return;
                }
                else
                {
                    headNode = (T)_node.NextNode;
                }
            }

            _node.PrevNode.NextNode = _node.NextNode;
            _node.NextNode.PrevNode = _node.PrevNode;
        }

        private void UpdateNode(T node, float delta)
        {
            Process(new SystemArgu<T>(node, delta));

            if (node.NextNode != headNode)
            {
                UpdateNode((T)node.NextNode, delta);
            }
        }

        protected abstract void Process(SystemArgu<T> argu);
    }
}