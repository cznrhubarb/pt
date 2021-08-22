using Ecs;
using Godot;
using MonoCustomResourceRegistry;

[RegisteredType(nameof(LabelWrap), "res://editoricons/Component.svg", nameof(Resource))]
public class LabelWrap : WrapComponent<Label>
{
    public Label Label
    {
        get => wrappedNode;
        private set => wrappedNode = value;
    }
}
