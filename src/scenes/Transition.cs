using Godot;
using System;

public class Transition : CanvasLayer
{
	//private static Node currentScene;
	private static Sprite preSceneShot;
	private static AnimationPlayer animPlayer;

    public override void _Ready()
	{
		//var root = GetTree().Root;
		//currentScene = root.GetChild(root.GetChildCount() - 1);
		preSceneShot = FindNode("PreSceneShot") as Sprite;
		animPlayer = FindNode("AnimationPlayer") as AnimationPlayer;
	}

	public static void To(string scenePath)
    {
		// TODO: Pause processing on everything but the animation player until the animation is finished
		preSceneShot.Texture = TakeScreenshot();
		Globals.SceneTree.ChangeScene(scenePath);
		//animPlayer.Connect("animation_finished", this, nameof(OnAnimationFinished));
		animPlayer.PlaybackSpeed = 0.5f;
		animPlayer.Play("Wipe");
	}

	private void OnAnimationFinished(string _animName)
    {
		/*
		func _deferred_scene_change(path: String, params: Dictionary) -> void:
			current_scene.free()

			var next_scene: PackedScene = ResourceLoader.load(path)
			current_scene = next_scene.instance()

			get_tree().get_root().add_child(current_scene)
			get_tree().set_current_scene(current_scene)


			if params.has("data"):
				current_scene.make(params.data)
			if params.has("suspense"):
				yield(get_tree().create_timer(params.suspense), "timeout")
			$AnimationPlayer.play("fade_in")
			yield($AnimationPlayer, "animation_finished")
			$ColorRect.mouse_filter = Control.MOUSE_FILTER_IGNORE
		*/

	}

    private static Texture TakeScreenshot()
    {
        var image = Globals.SceneTree.Root.GetTexture().GetData();
        image.FlipY();

		var imageTexture = new ImageTexture() { Flags = 0 };
        imageTexture.CreateFromImage(image);

		return imageTexture;
    }
}
