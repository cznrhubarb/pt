using Ecs;

public class ProfileDetails : Component
{
    public string Name { get; set; }
    public int MonNumber { get; set; }
    public Affiliation Affiliation { get; set; }
    public int Level { get; set; } = 1;
}
