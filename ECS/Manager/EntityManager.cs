using System;
using System.Reflection;
using System.Collections.Generic;

namespace GGWP.Ecs
{
    //TODO Performance analyze
    //TODO: 平行化
    public class EntityManager : Singleton<EntityManager>
    {
        public event Action<ulong> OnEntityAdd = (id)=>{
            Logger.TraceLog($"Entity {id} spawned.");
        };
        public event Action<ulong> OnEntityRemove = (id)=>{
            Logger.TraceLog($"Entity {id} removed.");
        };

        private MethodInfo addComponentInfo;
        private Delegate componentAddedDelegate;

        private ulong nowEntityID = 0;
        private EntitySetting nowConfig;
        private Action<ulong> onComplete = null;
        private IEnumerator<IComponentSetting> configEnumerator = null;
        
        public EntityManager()
        {
            addComponentInfo = typeof(ComponentManager).GetMethod("AddComponent");

            var mi1 = typeof(EntityManager).GetMethod("OnComponentAdded",
                BindingFlags.NonPublic | BindingFlags.Instance);
            componentAddedDelegate = Delegate.CreateDelegate(typeof(Action<EcsComponent>), this, mi1, false);
        }

        private void OnComponentAdded(EcsComponent c)
        {
            SpawnEntity();
        }

        private void SpawnEntity()
        {
            if(configEnumerator.MoveNext())
            {
                var gm = addComponentInfo.MakeGenericMethod(configEnumerator.Current.Type);
                gm.Invoke(
                    ComponentManager.Instance,
                    new object[] {nowEntityID, configEnumerator.Current, componentAddedDelegate});
            }
            else
            {
                OnEntityAdd?.Invoke(nowEntityID);
                onComplete?.Invoke(nowEntityID);
            }
        }

        public void SpawnEntity(EntitySetting _config, Action<ulong> _onComplete = null)
        {
            nowEntityID = IDGenerator.Get();
            nowConfig = _config;
            onComplete = _onComplete;
            configEnumerator = _config.ComponentTypes.GetEnumerator();
            SpawnEntity();
        }

        public void DestroyEntity(ulong entityID)
        {
            var allComponent = ComponentManager.Instance.GetAllComponents(entityID);
            foreach(var c in allComponent)
            {
                ComponentManager.Instance.RemoveComponent(entityID, c);
            }
            OnEntityRemove.Invoke(entityID);
        }
    }
}