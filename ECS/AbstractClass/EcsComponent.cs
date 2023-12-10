using System;

namespace GGWP.Ecs
{
    public abstract class EcsComponent : IDisposable
    {
        public ulong EntityID { get; private set; }

        public virtual void Use(ulong id)
        {
            EntityID = id;
        }

        public virtual void Dispose()
        {
            EntityID = 0;
        }

        //TODO: Be careful
        public override int GetHashCode()
        {
            return (int)EntityID;
        }

        public override string ToString() => $"[{EntityID}]{GetType().ToString()}";
    }
}