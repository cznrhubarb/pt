using Godot;
using System;

namespace Ecs
{
    public class Component : Resource, ICloneable
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
