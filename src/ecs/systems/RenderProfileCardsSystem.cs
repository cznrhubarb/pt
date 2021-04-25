using Ecs;

public class RenderProfileCardsSystem : Ecs.System
{
    public RenderProfileCardsSystem()
    {
        AddRequiredComponent<ProfileCard>();
        //AddRequiredComponent<ProfileDetails>();
        AddRequiredComponent<Health>();
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var card = entity.GetComponent<ProfileCard>();
        var healthComp = entity.GetComponent<Health>();
        card.HealthDisplay = healthComp.Current + " / " + healthComp.Max;
    }
}
