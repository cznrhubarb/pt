
// TODO: Could potentially be a class instead of an enum, and then I could put the range etc in here...?
public enum TargetingMode
{
    // Area of targeting based on distance, hits AoE spread
    StandardArea,
    // Can target anything in a line, hits AoE spread
    StandardLine,
    // Hits everything in a line, no AoE
    WholeLine,
    // Can only target the first blocked thing in a line, hits AoE spread
    BlockedLine,
}
