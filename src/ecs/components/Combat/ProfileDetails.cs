using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(ProfileDetails), "res://editoricons/Component.svg", nameof(Resource))]
public class ProfileDetails : Component
{
    // Doing a little bit of a hack because I already wrote so much
    //  and I'm too lazy to think about how to refactor this all
    //  to look like a regular component...
    // The reason this isn't great is because a lot of the monsterstate
    //  is already represented in a bunch of other components.
    //  Maybe those components could be removed and they could all reference this????
    public MonsterState MonsterState { get; set; }

    // Essentially just helper reference props now
    public string Name { get => MonsterState.CustomName; }
    public Texture ProfilePicture { get => MonsterState.Blueprint.ProfilePicture; }
    public int Level { get => MonsterState.Level; }
}
