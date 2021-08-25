using Godot;
using System;

namespace Ecs
{
    public class Component : Resource
    {
        public virtual void GrabReferences(Entity owner)
        {
        }
    }
}
