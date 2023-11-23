
using Ecs;
using Godot;

public class FactoryUtils
{
    public static readonly float TextEffectDelay = 0.5f;

    public static void BuildTextEffect(Manager manager, Vector3 tilePos, string text, Color color)
    {
        var effect = manager.GetNewEntity();
        var tweenSeq = new TweenSequence(manager.GetTree());
        manager.AddComponentsToEntity(effect,
            new LabelWrap(),
            new Tweening() { TweenSequence = tweenSeq, DeleteEntityOnComplete = true },
            new TileLocation() { TilePosition = tilePos, ZLayer = 1000 }
        );

        var label = effect.GetComponent<LabelWrap>().Label;
        label.Align = Label.AlignEnum.Center;
        label.GrowHorizontal = Control.GrowDirection.Both;
        label.Text = text;
        label.Modulate = color;
        label.Set("custom_fonts/font", GD.Load<DynamicFont>("res://res/BungeeRegular_20_outline.tres"));
        label.MarginTop = -80;

        tweenSeq.Append(label, "margin_top", -100, 0.5f);
        tweenSeq.Append(label, "margin_top", -140, 1f);
        tweenSeq.Join();
        tweenSeq.Append(label, "modulate:a", 0, 1f);
    }

    public static void BuildTextEffect(Manager manager, Vector3 tilePos, string text, Color color, float delay)
    {
        manager.AddComponentToEntity(manager.GetNewEntity(), new DeferredEvent()
        {
            Callback = () =>
            {
                BuildTextEffect(manager, tilePos, text, color);
            },
            Delay = delay
        });
    }

    public static void BuildTextEffect(Manager manager, Entity centeredOn, string text, Color color)
    {
        var tilePos = centeredOn.GetComponent<TileLocation>().TilePosition;
        BuildTextEffect(manager, tilePos, text, color);
    }

    public static void BuildTextEffect(Manager manager, Entity centeredOn, string text, Color color, float delay)
    {
        manager.AddComponentToEntity(manager.GetNewEntity(), new DeferredEvent()
        {
            Callback = () =>
            {
                var tilePos = centeredOn.GetComponent<TileLocation>().TilePosition;
                BuildTextEffect(manager, tilePos, text, color);
            },
            Delay = delay
        });
    }
}