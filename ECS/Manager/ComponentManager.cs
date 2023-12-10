using System;
using System.Collections.Generic;
using System.Linq;

namespace GGWP.Ecs
{
    public partial class ComponentManager : Singleton<ComponentManager>
    {
        public event Action<EcsComponent> OnComponentAdd;
        public event Action<EcsComponent> OnComponentRemove;

        private Dictionary<ulong, Dictionary<Type, EcsComponent>> entityToComponentsDict = new();

        private NodeManager nodeManager;
        
        public ComponentManager()
        {
            nodeManager = new NodeManager();
            OnComponentAdd += nodeManager.OnComponentAdd;
            OnComponentRemove += nodeManager.OnComponentRemove;
        }

        public override void Dispose()
        {
            base.Dispose();
            OnComponentAdd -= nodeManager.OnComponentAdd;
            OnComponentRemove -= nodeManager.OnComponentRemove;
        }

        public T GetComponent<T>(ulong entityID) where T : EcsComponent
        {
            return !entityToComponentsDict.TryGetValue(entityID, out var componentDict)
                || !componentDict.TryGetValue(typeof(T), out var result)
                ? null
                : result as T;
        }

        internal IEnumerable<EcsComponent> GetAllComponents(ulong entityID)
        {
            if (!entityToComponentsDict.TryGetValue(entityID, out var componentDict))
            {
                return null;
            }

            return componentDict.Values.ToArray();
        }

        public void AddComponent<T>(ulong entityID,
            IComponentSetting config = null,
            Action<T> onComplete = null) where T : EcsComponent
        {
            var component = GetComponent<T>();
            if (entityToComponentsDict.TryGetValue(entityID, out var components))
            {
                components.Add(component.GetType(), component);
            }
            else
            {
                entityToComponentsDict[entityID] = new Dictionary<Type, EcsComponent>()
                {
                    {component.GetType(), component}
                };
            }
            
            config?.Apply(component);
            component.Use(entityID);

            Logger.TraceLog($"Entity {entityID} Added Component {typeof(T)}");
            
            OnComponentAdd.Invoke(component);
            onComplete?.Invoke(component);
        }
        
        public void RemoveComponent(ulong entityID, EcsComponent c)
        {
            if (entityToComponentsDict.TryGetValue(entityID, out var components))
            {
                components.Remove(c.GetType());
            }

            Logger.TraceLog($"Entity {entityID} Removed Component {c.GetType()}");

            OnComponentRemove.Invoke(c);

            c.Dispose();
            RecycleComponent(c);

            if (components.Count == 0)
            {
                entityToComponentsDict.Remove(entityID);
            }
        }

        public void RemoveComponent<T>(ulong entityID) where T : EcsComponent
        {
            T component = null;
            if (entityToComponentsDict.TryGetValue(entityID, out var components))
            {
                component = (T)components[typeof(T)];
                components.Remove(typeof(T));
            }

            Logger.TraceLog($"Entity {entityID} Remove Component {typeof(T)}");

            OnComponentRemove.Invoke(component);

            component.Dispose();
            RecycleComponent(component);

            if (components.Count == 0)
            {
                entityToComponentsDict.Remove(entityID);
            }
        }
    }
}