using Godot;

namespace Ecs
{
    public class WrapComponent<T> : Component where T : Node
    {
        protected T wrappedNode;

        public override void GrabReferences(Entity owner)
        {
            foreach (var child in owner.GetChildren())
            {
                if (child is T wrapMe)
                {
                    wrappedNode = wrapMe;
                    break;
                }
            }
        }
    }
}
