using System;
using System.Collections.Generic;

namespace GGWP.Ecs
{
    public struct EntitySetting
    {
        public static EntitySetting operator +(EntitySetting c1, EntitySetting c2)
        {
            foreach(var c in c2.componenetSettingMap)
            {
                if (!c1.componenetSettingMap.TryGetValue(c.Key, out var setting))
                {
                    c1.componenetSettingMap[c.Key] = c.Value;
                }
                else
                {
                    setting += c.Value;
                }
            }
            return c1;
        }

        Dictionary<Type, IComponentSetting> componenetSettingMap;

        public IEnumerable<IComponentSetting> ComponentTypes => componenetSettingMap.Values;

        public EntitySetting(params IComponentSetting[] settings)
        {
            componenetSettingMap = new Dictionary<Type, IComponentSetting>();
            foreach(var c in settings)
            {
                componenetSettingMap.Add(c.Type, c);
            }
        }

        public void AddComponentSetting(IComponentSetting _setting)
        {
            if (!componenetSettingMap.TryGetValue(_setting.Type, out var setting))
            {
                componenetSettingMap[_setting.Type] = _setting;
            }
            else
            {
                setting += _setting;
            }
        }
    }
}