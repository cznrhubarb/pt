using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ecs
{
    public class Manager : Node
    {
        private Dictionary<int, Entity> entities;
        private Dictionary<Type, List<System>> stateSystems;
        private List<int> toDelete;

        private IEnumerable<System> AllSystems { get => stateSystems.Values.SelectMany(s => s); }

        public State CurrentState { get; private set; }

        public Manager()
        {
            entities = new Dictionary<int, Entity>();
            stateSystems = new Dictionary<Type, List<System>>();
            stateSystems[typeof(State)] = new List<System>();
            toDelete = new List<int>();
        }

        public override void _Ready()
        {
            base._Ready();

            var hud = FindNode("HUD");
            if (hud == null)
            {
                hud = new CanvasLayer();
                hud.Name = "HUD";
                AddChild(hud);
            }
        }

        public override void _Process(float delta)
        {
            if (CurrentState == null)
            {
                return;
            }

            var processSystems = stateSystems[typeof(State)]
                .Concat(stateSystems[CurrentState.GetType()]);
            foreach (System system in processSystems)
            {
                system.UpdateAll(delta);
            }
            Flush();
        }

        public Entity GetNewEntity()
        {
            Entity entity = new Entity();
            // Throw it off screen and expect someone else will place it properly
            entity.Position = Vector2.Inf;
            AddChild(entity);
            entities[entity.Id] = entity;
            return entity;
        }

        public void RegisterExistingEntity(Entity entity)
        {
            entities[entity.Id] = entity;
        }

        public void DeleteEntity(int id)
        {
            toDelete.Add(id);
        }

        public bool EntityExists(int id) =>
            entities.ContainsKey(id);

        public Entity GetEntityById(int id) =>
            entities[id];

        // Not the fastest or greatest (has to rebuild list each time and only uses one component type)
        public List<Entity> GetEntitiesWithComponent<T>() where T : Component =>
            entities.Values.Where(e => e.HasComponent<T>()).ToList();

        public void AddComponentToEntity(Entity entity, Component component)
        {
            entity.AddComponent(component);
            UpdateEntityRegistration(entity);
        }

        public void AddComponentsToEntity(Entity entity, params Component[] components)
        {
            entity.AddComponents(components);
            UpdateEntityRegistration(entity);
        }

        public void RemoveComponentFromEntity<T>(Entity entity) where T : Component
        {
            entity.RemoveComponent<T>();
            UpdateEntityRegistration(entity);
        }

        public void ApplyState<T>(T newState) where T : State
        {
            CurrentState?.Post(this);

            GD.Print("Entering state " + newState.GetType().Name);

            var stateType = newState.GetType();
            if (!stateSystems.ContainsKey(stateType))
            {
                stateSystems[stateType] = new List<System>();
            }

            newState.Pre(this);
            CurrentState = newState;
        }

        public void AddSystem(System system) =>
            AddSystem<State>(system);

        public void AddSystem<T>(System system)
        {
            var stateType = typeof(T);
            if (!stateSystems.ContainsKey(stateType))
            {
                stateSystems[stateType] = new List<System>();
            }
            stateSystems[stateType].Add(system);
            system.BindManager(this);
            AddChild(system);
        }

        private void UpdateEntityRegistration(Entity entity)
        {
            foreach (var system in AllSystems)
            {
                system.UpdateEntityRegistration(entity);
            }
        }

        private void Flush()
        {
            foreach (int id in toDelete)
            {
                if (!EntityExists(id))
                    continue;

                foreach (var system in AllSystems)
                {
                    system.DeleteEntity(id);
                }

                entities[id].QueueFree();
                entities.Remove(id);
            }
            toDelete.Clear();
        }
    }
}