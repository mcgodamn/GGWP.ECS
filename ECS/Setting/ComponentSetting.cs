using System;
using System.Collections.Generic;
 
namespace GGWP.Ecs
{
    public interface IComponentSetting
    {
        Type Type {get;}
        void Apply(EcsComponent c);

        IComponentSetting Combine(IComponentSetting b);
        
        public static IComponentSetting operator +(IComponentSetting c1, IComponentSetting c2)
        {
            return c1.Combine(c2);
        }
    }
    
    public struct ComponentSetting<T> : IComponentSetting where T : EcsComponent 
    {
        public static ComponentSetting<T> operator +(ComponentSetting<T> c1, ComponentSetting<T> c2)
        {
            return (ComponentSetting<T>)(c1.Combine(c2));
        }

        public IComponentSetting Combine(IComponentSetting c2)
        {
            applyAction += ((ComponentSetting<T>)c2).applyAction;
            return this;
        }

        public Type Type => typeof(T);
        private Action<T> applyAction;
        
        public ComponentSetting(Action<T> _applyAction = null)
        {
            applyAction = _applyAction;
        }

        public void Apply(EcsComponent _c)
        {
            applyAction?.Invoke((T)_c);
        }
    }
}