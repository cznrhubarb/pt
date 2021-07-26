using Ecs;
using System;

public class DeferredEvent : Component
{
    public Action Callback { get; set; }
}
