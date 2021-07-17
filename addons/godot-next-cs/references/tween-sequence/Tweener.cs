using Godot;
/**
 * author: cznrhubarb, adapted from KoBeWi's gdscript version
 * license: MIT
 * description:
 * Abstract class for all Tweeners.
 */

public abstract class Tweener : Reference
{
	internal abstract void Start(Tween tween);
}