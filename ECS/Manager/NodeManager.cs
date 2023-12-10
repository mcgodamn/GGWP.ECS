using GGWP.Collection;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace GGWP.Ecs
{
    public partial class NodeManager
    {
        private ComponentTypeInNodeDic componentTypeDic;
        private OrderedDictionary<Type, HashSet<Type>> nodeToComponentDict = new();
        private Dictionary<Type, BigInteger> nodeHashes = new();
        private Dictionary<Type, BigInteger> componentHashes = new();
        private Dictionary<ulong, BigInteger> entityHashes = new();

        public NodeManager()
        {
            componentTypeDic = ComponentTypeInNodeDic.GetInstanceByAssembly();
            RefreshMaps();
        }

        private void RefreshMaps()
        {
            nodeToComponentDict.Clear();
            componentHashes.Clear();
            nodeHashes.Clear();

            foreach (var eseentials in componentTypeDic)
            {
                foreach (var com in eseentials.Value)
                {
                    if (!nodeToComponentDict.TryGetValue(com, out var nodes))
                    {
                        nodes = new HashSet<Type>();
                        nodeToComponentDict.Add(com, nodes);
                    }
                    nodes.Add(eseentials.Key);
                }
            }

            BigInteger i = 1;
            foreach (var comPair in nodeToComponentDict)
            {
                componentHashes.Add(comPair.Key, i);
                i <<= 1;
            }

            foreach (var eseentials in componentTypeDic)
            {
                nodeHashes.Add(eseentials.Key, 0);
                foreach (var com in eseentials.Value)
                {
                    nodeHashes[eseentials.Key] += componentHashes[com];
                }
            }
        }

        public void AddComponentSet(ComponentTypeInNodeDic set)
        {
            componentTypeDic += set;
            RefreshMaps();
        }

        public void OnComponentAdd(EcsComponent component)
        {
            if (!entityHashes.ContainsKey(component.EntityID))
            {
                entityHashes.Add(component.EntityID, 0);
            }
            
            if (!componentHashes.TryGetValue(component.GetType(), out var componentHash)) return;

            var oldValue = entityHashes[component.EntityID];
            var newValue = entityHashes[component.EntityID] |= componentHash;

            foreach (var nodeType in nodeToComponentDict[component.GetType()])
            {
                if ((nodeHashes[nodeType] & oldValue) != nodeHashes[nodeType]
                    && (nodeHashes[nodeType] & newValue) == nodeHashes[nodeType])
                {
                    AddNode2System(nodeType, component.EntityID);
                }
            }
        }

        public void OnComponentRemove(EcsComponent component)
        {
            if (!entityHashes.ContainsKey(component.EntityID))
            {
                throw new Exception($"Entity {component.EntityID} doesn't exist.");
            }

            // Could be optional component.
            if (!componentHashes.TryGetValue(component.GetType(), out var componentHash)) return;

            var oldValue = entityHashes[component.EntityID];
            var newValue = entityHashes[component.EntityID] &= ~componentHash;

            foreach (var nodeType in nodeToComponentDict[component.GetType()])
            {
                if ((nodeHashes[nodeType] & oldValue) == nodeHashes[nodeType]
                    && (nodeHashes[nodeType] & newValue) != nodeHashes[nodeType])
                {
                    RemoveNodeFromSystem(nodeType, component.EntityID);
                }
            }
        }
    }
}