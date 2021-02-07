using System;
using System.Collections.Generic;
using System.Linq;

namespace Ecs
{
    public abstract class DyadicSystem : System
    {
        private HashSet<int> registeredSecondaryEntityIds;
        private List<Type> requiredSecondaryComponents;

        protected DyadicSystem() : base()
        {
            registeredSecondaryEntityIds = new HashSet<int>();
            requiredSecondaryComponents = new List<Type>();
        }

        protected List<Entity> SecondaryEntities
        {
            get
            {
                var result = from id in registeredSecondaryEntityIds
                             where Manager.EntityExists(id)
                             select Manager.GetEntityById(id);

                return result.ToList();
            }
        }

        public override void UpdateEntityRegistration(Entity entity)
        {
            base.UpdateEntityRegistration(entity);

            bool matches = MatchesSecondary(entity);
            if (registeredSecondaryEntityIds.Contains(entity.Id))
            {
                if (!matches)
                {
                    registeredSecondaryEntityIds.Remove(entity.Id);
                }
            }
            else
            {
                if (matches)
                {
                    registeredSecondaryEntityIds.Add(entity.Id);
                }
            }
        }

        public override void DeleteEntity(int id)
        {
            base.DeleteEntity(id);

            if (registeredSecondaryEntityIds.Contains(id))
            {
                registeredSecondaryEntityIds.Remove(id);
            }
        }

        protected void AddRequiredSecondaryComponent<T>() where T : Component
        {
            requiredSecondaryComponents.Add(typeof(T));
        }

        private bool MatchesSecondary(Entity entity)
        {
            foreach (Type required in requiredSecondaryComponents)
            {
                if (!entity.HasComponent(required))
                    return false;
            }
            return true;
        }
    }
}