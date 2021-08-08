using Ecs;
using System;

public class DeferredEvent : Component
{
    public Action Callback { get; set; }
    public float Delay { get; set; } = 0;
}
