using Ecs;
using Godot;

public class LabelWrap : WrapComponent<Label>
{
    public Label Label
    {
        get => wrappedNode;
        private set => wrappedNode = value;
    }
}
