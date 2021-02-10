using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Ecs
{
    public abstract class System : Node
    {
        protected const string PrimaryEntityKey = "primary";

        protected Dictionary<string, EntitySelection> entitySelections;
        protected Manager manager;

        protected System()
        {
            Name = GetType().Name;
            entitySelections = new Dictionary<string, EntitySelection>();
        }

        public void BindManager(Manager manager)
        {
            this.manager = manager;
        }

        protected void AddRequiredComponent<T>() where T : Component =>
            AddRequiredComponent<T>(PrimaryEntityKey);


        protected void AddRequiredComponent<T>(string selectionKey) where T : Component
        {
            if (!entitySelections.ContainsKey(selectionKey))
            {
                entitySelections.Add(selectionKey, new EntitySelection());
            }

            entitySelections[selectionKey].AddRequiredComponent<T>();
        }

        protected List<Entity> EntitiesFor(string selectionKey) =>
            entitySelections[selectionKey].Entities;

        protected Entity SingleEntityFor(string selectionKey) =>
            entitySelections[selectionKey].Entities.First();

        public virtual void UpdateEntityRegistration(Entity entity)
        {
            foreach (var selection in entitySelections.Values)
            {
                selection.UpdateEntityRegistration(entity);
            }
        }

        public virtual void DeleteEntity(int id)
        {
            foreach (var selection in entitySelections.Values)
            {
                selection.DeleteEntity(id);
            }
        }

        public virtual void UpdateAll(float deltaTime)
        {
            var primarySelection = entitySelections[PrimaryEntityKey];
            foreach (Entity entity in primarySelection.Entities)
            {
                Update(entity, deltaTime);
            }
        }

        protected abstract void Update(Entity entity, float deltaTime);
    }
}