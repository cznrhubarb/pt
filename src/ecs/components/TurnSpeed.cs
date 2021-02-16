using Ecs;

public class TurnSpeed : Component
{
    // TODO: Maybe set timetoact in a pre-game state when an FSM is available

    // Lower == better
    public int Speed { get; set; }

    public int TimeToAct { get; set; }
}
