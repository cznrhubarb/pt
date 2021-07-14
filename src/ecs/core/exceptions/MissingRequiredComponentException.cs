using Ecs;
using System;

namespace PokemonTactics.src.ecs.core.exceptions
{
    class MissingRequiredComponentException : Exception
    {
        public MissingRequiredComponentException(Type t) : base($"Missing required component: {t}") { }
    }
}
