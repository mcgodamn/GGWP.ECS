using GGWP.Memory;
using System;
using System.Collections.Generic;

namespace GGWP.Ecs
{
    public partial class NodeManager
    {
        private Dictionary<Type, ObjectPool<EcsNode>> nodePools = new();
        private Dictionary<ulong, Dictionary<Type, EcsNode>> entityNodeDict = new();

        private void AddNode2System(Type type, ulong entityID)
        {
            var node = GetNode(type);
            node.Init(entityID);
            SystemManager.Instance.AddNode(node);

            if (!entityNodeDict.TryGetValue(entityID, out var nodeDict))
            {
                entityNodeDict[entityID] = nodeDict = new Dictionary<Type, EcsNode>();
            }
            nodeDict.Add(type, node);

            Logger.TraceLog($"Entity {entityID} Add Node {type}");
        }

        private void RemoveNodeFromSystem(Type type, ulong entityID)
        {
            if (!entityNodeDict.TryGetValue(entityID, out var nodeDict))
            {
                throw new Exception($"entityNodeDict doesn't contain the Entity {entityID}.");
            }

            if (!nodeDict.TryGetValue(type, out var node))
            {
                throw new Exception($"Entity {entityID} doesn't have the Type {type}.");
            }
            SystemManager.Instance.RemoveNode(node);
            node.Dispose();
            RecycleNode(type, node);

            nodeDict.Remove(type);
            if (nodeDict.Count == 0)
            {
                entityNodeDict.Remove(entityID);
            }
            Logger.TraceLog($"Entity {entityID} Remove Node {type}");
        }

        private EcsNode GetNode(Type type)
        {
            if (!nodePools.TryGetValue(type, out var objectPool))
            {
                objectPool = new ObjectPool<EcsNode>(() => (EcsNode)Activator.CreateInstance(type));
                nodePools[type] = objectPool;
            }
            return objectPool.ForcePop();
        }

        private void RecycleNode(Type type, EcsNode node)
        {
            if (nodePools.TryGetValue(type, out var objectPool))
            {
                objectPool.ForcePush(node);
            }
            else
            {
                throw new Exception($"Can't find Node object pool of Type {type}.");
            }
        }
    }
}