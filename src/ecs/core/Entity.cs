using Godot;
using System;
using System.Collections.Generic;

namespace Ecs
{
    public class Entity : Node
    {
        public int Id { get; private set; }

        private Dictionary<Type, Component> components;

        public Entity(int id)
        {
            Id = id;
            components = new Dictionary<Type, Component>();
        }

        internal void AddComponent(Component component)
        {
            components[component.GetType()] = component;
        }

        internal void RemoveComponent<T>() where T : Component
        {
            components.Remove(typeof(T));
        }

        public T GetComponent<T>() where T : Component
        {
            return (T)components[typeof(T)];
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