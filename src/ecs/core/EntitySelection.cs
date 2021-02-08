using System;
using System.Collections.Generic;
using System.Linq;

namespace Ecs
{
    public class EntitySelection
    {
        private Dictionary<int, Entity> registeredEntities;
        private List<Type> requiredComponents;

        public EntitySelection()
        {
            registeredEntities = new Dictionary<int, Entity>();
            requiredComponents = new List<Type>();
        }

        public List<Entity> Entities
        {
            get => registeredEntities.Values.ToList();
        }

        public void UpdateEntityRegistration(Entity entity)
        {
            bool matches = Matches(entity);
            if (registeredEntities.ContainsKey(entity.Id))
            {
                if (!matches)
                {
                    registeredEntities.Remove(entity.Id);
                }
            }
            else
            {
                if (matches)
                {
                    registeredEntities.Add(entity.Id, entity);
                }
            }
        }

        public void DeleteEntity(int id)
        {
            if (registeredEntities.ContainsKey(id))
            {
                registeredEntities.Remove(id);
            }
        }

        public void AddRequiredComponent<T>() where T : Component
        {
            requiredComponents.Add(typeof(T));
        }

        private bool Matches(Entity entity)
        {
            foreach (Type required in requiredComponents)
            {
                if (!entity.HasComponent(required))
                    return false;
            }
            return true;
        }
    }
}