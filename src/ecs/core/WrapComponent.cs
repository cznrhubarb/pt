using Godot;
using System;

namespace Ecs
{
    public class WrapComponent<T> : Component where T : Node
    {
        protected T wrappedNode;

        public override void GrabReferences(Entity owner)
        {
            // Wrap existing if it is there
            foreach (var child in owner.GetChildren())
            {
                if (child is T wrapMe)
                {
                    wrappedNode = wrapMe;
                    return;
                }
            }

            // If there are none, create a new one
            wrappedNode = Activator.CreateInstance<T>();
            owner.AddChild(wrappedNode);
        }
    }
}
