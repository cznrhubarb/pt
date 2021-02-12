using System;

namespace Ecs
{
    public class Component : ICloneable
    {
        public virtual void GrabReferences(Entity owner)
        {
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
