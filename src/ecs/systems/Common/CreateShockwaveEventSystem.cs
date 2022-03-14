using Ecs;
using Godot;

public class CreateShockwaveEventSystem : Ecs.System
{
    // How much spring is present in the animation
    const float Elasticity = 0;
    // How much energy (in tile lengths) is needed to bother displaying an effect
    const float Cutoff = 0.2f;
    // How much energy is lost when going from one tile to the next
    const float TransferDamp = 0.5f;

    const string MapEntityKey = "map";

    public CreateShockwaveEventSystem()
    {
        AddRequiredComponent<CreateShockwaveEvent>();
        AddRequiredComponent<Map>(MapEntityKey);
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var map = SingleEntityFor(MapEntityKey).GetComponent<Map>();
        var shockwaveEvent = entity.GetComponent<CreateShockwaveEvent>();

        var mag = shockwaveEvent.Magnitude;
        if (mag < Cutoff)
        {
            return;
        }

        var maxRange = -1;
        // TODO: Probably a better way of calculating this so we don't have to loop twice
        while (mag > Cutoff)
        {
            mag *= TransferDamp;
            maxRange++;
        }

        var startPos = shockwaveEvent.InitialTilePosition;
        // TODO: This doesn't get tiles that aren't walkable spaces, even though we want it to
        var points = map.AStar.GetPointsBetweenRange(startPos, 0, maxRange);
        foreach (var point in points)
        {
            var delta = point - startPos;
            // Aligned with the shockwave?
            if (delta == Vector3.Zero || shockwaveEvent.Direction.Dot(delta.Normalized()) >= 0)
            {
                var distance = delta.Length();
                mag = shockwaveEvent.Magnitude * Mathf.Pow(TransferDamp, distance);

                var tile = map.IsoMap.GetTileAt(point);
                var offset = new Offset();

                // TODO: MAYBE add a little bit of shimmy here?
                var transformAmount = map.IsoMap.MapToWorld(shockwaveEvent.Direction) * mag;

                var tweenSeq = new TweenSequence(manager.GetTree());
                // TODO: Time should probably be based on magnitude somehow
                // TODO: We should probably also append a delay depending on distance at the start so that there is an actual ripple
                // TODO: The transitions and eases probably need a little bit of playing with to find what is ideal
                tweenSeq.AppendMethod(offset, "SetAmount", Vector2.Zero, transformAmount, 0.5f).SetTransition(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
                tweenSeq.AppendMethod(offset, "SetAmount", transformAmount, Vector2.Zero, 1f).SetTransition(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);

                manager.AddComponentsToEntity(tile,
                    offset,
                    new Tweening() { TweenSequence = tweenSeq });
            }
        }

        manager.DeleteEntity(entity.Id);
    }
}
