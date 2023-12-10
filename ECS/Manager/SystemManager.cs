using GGWP.Collection;
using System;
using System.Collections.Generic;

namespace GGWP.Ecs
{
    public interface ISystemSetting
    {
        Type[] UpdateSystems {get;}
        Type[] LateUpdateSystems {get;}
        Type[] FixedUpdateSystems {get;}
    }

    public class SystemManager : Singleton<SystemManager>
    {
        private Dictionary<Type, EcsSystem> systemWithBelongNodeDict = new();
        private OrderedSet<EcsSystem> updateSystems = new();
        private OrderedSet<EcsSystem> lateUpdateSystems = new();
        private OrderedSet<EcsSystem> fixedUpdateSystems = new();

        public void Initialize(ISystemSetting config)
        {
            void SetSystems(Type[] systemTypes, OrderedSet<EcsSystem> systems)
            {
                if (systemTypes == null || systemTypes.Length == 0)
                {
                    return;
                }

                systems.Clear();
                foreach (var type in systemTypes)
                {
                    var genericArguments = type.BaseType.GenericTypeArguments;
                    if (genericArguments != Type.EmptyTypes)
                    {
                        if (systemWithBelongNodeDict.ContainsKey(genericArguments[0]))
                        {
                            throw new Exception($"Duplicated system {type} add to the Manager.Need to check the config.");
                        }

                        var system = (EcsSystem)Activator.CreateInstance(type);
                        systemWithBelongNodeDict.Add(
                            genericArguments[0],
                            system);
                        systems.Add(system);
                    }
                    else
                    {
                        throw new Exception($"System type {type} don't have node genericArguments");
                    }
                }
            }

            systemWithBelongNodeDict.Clear();
            SetSystems(config.UpdateSystems, updateSystems);
            SetSystems(config.LateUpdateSystems, lateUpdateSystems);
            SetSystems(config.FixedUpdateSystems, fixedUpdateSystems);
        }
        
        public void Start()
        {
            GameKernel.Instance.UpdateEvent += Update;
            GameKernel.Instance.FixedUpdateEvent += FixedUpdate;
            GameKernel.Instance.LateUpdateEvent += LateUpdate;
        }

        public void Update()
        {
            foreach (var system in updateSystems)
            {
                system.Update(UnityEngine.Time.deltaTime);
            }
        }

        public void FixedUpdate()
        {
            foreach (var system in fixedUpdateSystems)
            {
                system.Update(UnityEngine.Time.fixedDeltaTime);
            }
        }

        public void LateUpdate()
        {
            foreach (var system in lateUpdateSystems)
            {
                system.Update(UnityEngine.Time.deltaTime);
            }
        }

        public void AddNode(EcsNode node)
        {
            var nodeType = node.GetType();
            if (systemWithBelongNodeDict.ContainsKey(nodeType))
            {
                systemWithBelongNodeDict[nodeType].AddNode(node);
            }
            else
            {
                throw new Exception($"No system for {nodeType}");
            }
        }

        public void RemoveNode(EcsNode node)
        {
            var nodeType = node.GetType();
            if (systemWithBelongNodeDict.ContainsKey(nodeType))
            {
                systemWithBelongNodeDict[nodeType].RemoveNode(node);
            }
            else
            {
                throw new Exception($"No system for {nodeType}");
            }
        }
    }
}