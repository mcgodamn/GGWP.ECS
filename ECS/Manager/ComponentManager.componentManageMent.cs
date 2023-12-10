using GGWP.Memory;
using System;
using System.Collections.Generic;

namespace GGWP.Ecs
{
    public partial class ComponentManager
    {
        private readonly Dictionary<Type, ObjectPool<EcsComponent>> componentPools = new();

        private T GetComponent<T>() where T : EcsComponent
        {
            var type = typeof(T);
            if (!componentPools.TryGetValue(type, out var objectPool))
            {
                objectPool = new ObjectPool<EcsComponent>(() => (EcsComponent)Activator.CreateInstance(type));
                componentPools[type] = objectPool;
            }
            return (T)objectPool.ForcePop();
        }
        
        private void RecycleComponent(EcsComponent component)
        {
            var type = component.GetType();
            if (componentPools.TryGetValue(type, out var objectPool))
            {
                objectPool.ForcePush(component);
            }
            else
            {
                throw new Exception($"Can't find Component object pool of Type {type}.");
            }
        }
    }
}