using Godot;
using PokemonTactics.src.ecs.core.exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using MonoCustomResourceRegistry;

namespace Ecs
{
    [RegisteredType(nameof(Entity), "res://editoricons/Entity.svg", nameof(Node2D))]
    public class Entity : Node2D
    {
        private static int nextAvailableIndex = 0;

        public int Id { get; private set; }

        [Export]
        private IEnumerable<Component> editorComponents = new List<Component>();

        private IDictionary<Type, Component> components = new Dictionary<Type, Component>();

        public Entity()
        {
            Id = nextAvailableIndex++;
        }

        public void RegisterExistingComponents(Manager manager)
        {
            foreach (var component in editorComponents)
            {
                if (component is TileLocation tileLoc)
                {
                    tileLoc.TilePosition = manager.TilePositionFromActor(this);
                }
                manager.AddComponentToEntity(this, component);
            }
        }

        internal void AddComponent(Component component)
        {
            components[component.GetType()] = component;
            component.GrabReferences(this);
        }

        internal void AddComponents(params Component[] components)
        {
            foreach (var comp in components)
            {
                AddComponent(comp);
            }
        }

        internal void RemoveComponent<T>() where T : Component
        {
            components.Remove(typeof(T));
        }

        public T GetComponent<T>() where T : Component
        {
            return (T)components[typeof(T)];
        }

        public T GetComponentOrNull<T>() where T : Component
        {
            return HasComponent<T>() 
                ? (T)components[typeof(T)] 
                : null;
        }

        public bool HasComponent(Type componentType)
        {
            return components.ContainsKey(componentType);
        }

        public bool HasComponent<T>() where T : Component
        {
            return components.ContainsKey(typeof(T));
        }

        public void AssertComponentExists<T>() where T : Component
        {
            if (!HasComponent<T>())
            {
                throw new MissingRequiredComponentException(typeof(T));
            }
        }

        // HACK: Can't interpolate the vectors directly, so this is some janky way of doing it using method callback interpolation
        //  Very shitty coupling
        private void SetTilePositionXY(Vector3 vec)
        {
            var tileLocation = this.GetComponent<TileLocation>();
            tileLocation.TilePosition = new Vector3(vec.x, vec.y, tileLocation.TilePosition.z);
        }

        private void SetTilePositionZ(float z)
        {
            var tileLocation = this.GetComponent<TileLocation>();
            tileLocation.TilePosition = new Vector3(tileLocation.TilePosition.x, tileLocation.TilePosition.y, z);
        }
    }
}