using Ecs;
using Godot;

public class SelectActorSystem : Ecs.System
{
    private const string SelectableEntityKey = "selectable";
    private const string SelectedEntityKey = "selected";

    public SelectActorSystem()
    {
        AddRequiredComponent<Reticle>();
        AddRequiredComponent<TileLocation>();
        AddRequiredComponent<Selectable>(SelectableEntityKey);
        AddRequiredComponent<TileLocation>(SelectableEntityKey);
        AddRequiredComponent<Selected>(SelectedEntityKey);
    }

    protected override void Update(Entity entity, float deltaTime)
    {
        var selectableEntities = EntitiesFor(SelectableEntityKey);

        if (Input.IsActionJustPressed("ui_accept"))
        {
            var tileLocationComp = entity.GetComponent<TileLocation>();

            // TODO: Adding and removing Selected should happen in a new State in Pre/Post

            foreach (var selected in EntitiesFor(SelectedEntityKey))
            {
                manager.RemoveComponentFromEntity<Selected>(selected);
            }

            foreach (var target in selectableEntities)
            {
                var targetLocationComp = target.GetComponent<TileLocation>();
                if (targetLocationComp.TilePosition == tileLocationComp.TilePosition)
                {
                    manager.AddComponentToEntity(target, new Selected());
                }
            }
        }
    }
}
