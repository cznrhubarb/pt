using Godot;

namespace Ecs
{
    // States are Godot Objects as well, just so that we can use them as signal callback targets
    public abstract class State : Object
    {
        public virtual void Pre(Manager manager) { }

        public virtual void Post(Manager manager) { }
    }
}
