public class EvolutionPath
{
    public EvolutionPath(int evolvesIntoId, EvolutionMethod evoMethod, object evoParams)
    {
        EvolvesInto = DataLoader.BlueprintData[evolvesIntoId];
        EvolutionMethod = evoMethod;
        EvolutionParams = evoParams;
    }

    public MonsterBlueprint EvolvesInto { get; private set; }

    public EvolutionMethod EvolutionMethod { get; private set; }

    // TODO: This shouldn't be object probably.
    //  Or maybe we get rid of this and subclass EvolutionPath for the various evo methods
    public object EvolutionParams { get; private set; }
}
