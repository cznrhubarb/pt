using Godot;
using System;
using System.Collections.Generic;

namespace Ecs
{
    public class Manager : Node
    {
        private Dictionary<int, Entity> entities;
        private Dictionary<Type, System> systems;
        private List<int> toDelete;
        private int currentId = 0;

        public Manager()
        {
            entities = new Dictionary<int, Entity>();
            systems = new Dictionary<Type, System>();
            toDelete = new List<int>();
        }

        public override void _Process(float delta)
        {
            foreach (System system in systems.Values)
            {
                system.UpdateAll(delta);
            }
            Flush();
        }

        public Entity GetNewEntity()
        {
            Entity entity = new Entity(currentId++);
            entities[entity.Id] = entity;
            return entity;
        }

        public void DeleteEntity(int id)
        {
            toDelete.Add(id);
        }

        public bool EntityExists(int id)
        {
            return entities.ContainsKey(id);
        }

        public Entity GetEntityById(int id)
        {
            return entities[id];
        }

        public void AddComponentToEntity(Entity entity, Component component)
        {
            entity.AddComponent(component);
            UpdateEntityRegistration(entity);
        }

        public void RemoveComponentFromEntity<T>(Entity entity) where T : Component
        {
            entity.RemoveComponent<T>();
            UpdateEntityRegistration(entity);
        }

        public void AddSystem(System system)
        {
            systems[system.GetType()] = system;
            system.BindManager(this);
        }

        public T GetSystem<T>() where T : System
        {
            return (T)systems[typeof(T)];
        }

        private void UpdateEntityRegistration(Entity entity)
        {
            foreach (System system in systems.Values)
            {
                system.UpdateEntityRegistration(entity);
            }
        }

        private void Flush()
        {
            foreach (int id in toDelete)
            {
                if (!EntityExists(id)) //safeguard against deleting twice
                    continue;

                foreach (System system in systems.Values)
                {
                    system.DeleteEntity(id);
                }

                entities.Remove(id);
            }
            toDelete.Clear();
        }
    }
}