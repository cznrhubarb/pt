
public enum Affiliation
{
    Friendly,
    Enemy,
    Neutral,
}

static class AffiliationExtensions
{
    public static bool IsOpposedTo(this Affiliation myAffiliation, Affiliation yourAffiliation)
    {
        return (myAffiliation == Affiliation.Enemy && yourAffiliation == Affiliation.Friendly) ||
                (myAffiliation == Affiliation.Friendly && yourAffiliation == Affiliation.Enemy);
    }
}