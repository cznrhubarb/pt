using Godot;
using System;
using System.Collections.Generic;

namespace Ecs
{
    public class Entity : Node2D
    {
        private static int nextAvailableIndex = 0;

        public int Id { get; private set; }

        private Dictionary<Type, Component> components;

        public Entity()
        {
            Id = nextAvailableIndex++;
            components = new Dictionary<Type, Component>();
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
    }
}