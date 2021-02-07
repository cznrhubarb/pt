using Ecs;

public class Pulse : Component
{
    public float squishSpeed { get; set; }

    public float squishAmountX { get; set; }

    public float squishAmountY { get; set; }

    public float squishAccumulator { get; set; } = 0;
}
