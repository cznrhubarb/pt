using Ecs;
using Godot;
using System.Linq;

public class RenderTurnOrderCardsSystem : Ecs.System
{
    private const int CardHorzSpace = 65;

    public RenderTurnOrderCardsSystem()
    {
        AddRequiredComponent<TurnSpeed>();
        AddRequiredComponent<TurnOrderCard>();
    }

    public override void UpdateAll(float deltaTime)
    {
        var sortedActors = EntitiesFor(PrimaryEntityKey)
            .Select(ent => new SortPair() { speed = ent.GetComponent<TurnSpeed>(), card = ent.GetComponent<TurnOrderCard>() })
            .OrderBy(act => act.speed.TimeToAct)
            .ToList();

        var cardStartX = GetViewport().Size.x - sortedActors.Count * CardHorzSpace;
        for (var i = 0; i < sortedActors.Count; i++)
        {
            var actor = sortedActors[i];
            if (actor.card.Card.GetParent() == null)
            {
                // TODO: I wish there was a good way to add this to HUD on creation, but I can't gettree from outside a node.
                var hud = GetTree().Root.FindNode("HUD", true, false);
                hud.AddChild(actor.card.Card);
            }
            actor.card.RemainingTicks = actor.speed.TimeToAct;
            actor.card.Card.Position = new Vector2(cardStartX + CardHorzSpace * i, 3);
        }
    }

    protected override void Update(Entity entity, float deltaTime)
    {
    }

    private struct SortPair
    {
        public TurnOrderCard card;
        public TurnSpeed speed;
    }
}
