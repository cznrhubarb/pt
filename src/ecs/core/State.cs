namespace Ecs
{
    public abstract class State
    {
        public virtual void Pre(Manager manager) { }

        public virtual void Post(Manager manager) { }
    }
}
