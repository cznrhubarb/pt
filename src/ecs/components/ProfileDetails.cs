using Ecs;

public class ProfileDetails : Component
{
    public string Name { get; set; }
    public string MonNumber { get; set; }
    public Affiliation affiliation { get; set; }
}
