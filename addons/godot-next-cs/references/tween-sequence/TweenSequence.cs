using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using static Godot.Tween;
/**
* # author: cznrhubarb, adapted from KoBeWi's gdscript version
* license: MIT
* description:
* 	A helper class for easier management and chaining of Tweens.
* 	dynamically from code.
* 
* 	Example usage:
* 		var seq := TweenSequence.new(get_tree())
* 		seq.append($Sprite, "modulate", Color.red, 1)
* 		seq.append($Sprite, "modulate", Color(1, 0, 0, 0), 1).set_delay(1)
* 	This will create a Tween and automatically start it,
* 	changing the Sprite to red color in one second
* 	and then making it transparent after a delay.
*/

public class TweenSequence : Reference
{
	// Emitted when one step of the sequence is finished.
	[Signal]
	public delegate void step_finished(int index);

	// Emitted when a loop of the sequence is finished.
	[Signal]
	public delegate void loop_finished();

	// Emitted when whole sequence is finished. Doesn't happen with inifnite loops.
	[Signal]
	public delegate void finished();

	private SceneTree tree;
	private Tween tween;
	private List<List<Tweener>> tweeners = new List<List<Tweener>>();

	private int currentStep = 0;
	private bool started = false;
	private bool parallel = false;

	private int Loops { get; set; } = 0;
	public bool Running { get; private set; } = false;
	public bool KillWhenFinished { get; set; } = true;
	public float Speed
	{
		get => this.tween.PlaybackSpeed;
		set => this.tween.PlaybackSpeed = value;
	}
	private bool _autoStart = true;
	public bool AutoStart
	{
		get => _autoStart;
		set 
		{
			if (this._autoStart && !value)
			{
				this.tree.Disconnect("idle_frame", this, nameof(Start));
			}
			else if (!this._autoStart && value)
			{
				this.tree.Connect("idle_frame", this, nameof(Start), null, (int)ConnectFlags.Oneshot);
			}

			this._autoStart = value;
		}
	}
	public bool Complete { get; private set; } = false;

	public TweenSequence(SceneTree tree)
	{
		this.tree = tree;
		this.tween = new Tween();
		this.tween.SetMeta("sequence", this);
		tree.Root.CallDeferred("add_child", this.tween);

		tree.Connect("idle_frame", this, nameof(Start), null, (int)ConnectFlags.Oneshot);
		this.tween.Connect("tween_all_completed", this, nameof(StepComplete));
	}

	public PropertyTweener Append(Object target, NodePath property, object toVal, float duration)
	{
		var tweener = new PropertyTweener(target, property, toVal, duration);
		AddTweener(tweener);
		return tweener;
	}

	public IntervalTweener AppendInterval(float time)
	{
		var tweener = new IntervalTweener(time);
		AddTweener(tweener);
		return tweener;
	}

	public CallbackTweener AppendCallback(Object target, string method, object[] args)
	{
		var tweener = new CallbackTweener(target, method, args);
		AddTweener(tweener);
		return tweener;
	}

	public MethodTweener AppendMethod(Object target, string method, object fromValue, object toValue, float duration)
	{
		var tweener = new MethodTweener(target, method, fromValue, toValue, duration);
		AddTweener(tweener);
		return tweener;
	}

	// When used, next Tweener will be added as a parallel to previous one.
	// Example: sequence.parallel().append(...)
	public Reference Parallel()
	{
		if (this.tweeners.Count == 0)
		{
			this.tweeners.Add(new List<Tweener>());
		}

		this.parallel = true;
		return this;
	}

	// Alias to parallel(), except it won't work without first tweener.
	public Reference Join()
	{
		Debug.Assert(this.tweeners.Count != 0, "Can't join with empty sequence");
		
		this.parallel = true;
		return this;
	}

	public void Start()
	{
		Debug.Assert(this.tween != null, "Tween was removed");
		Debug.Assert(!this.started, "Sequence already started");

		this.started = true;
		this.Running = true;
		RunNextStep();
	}

	public void Pause()
	{
		Debug.Assert(this.tween != null, "Tween was removed");
		Debug.Assert(this.Running, "Sequence not running");
		
		this.tween.StopAll();
		this.Running = false;
	}

	public void Resume()
	{
		Debug.Assert(this.tween != null, "Tween was removed");
		Debug.Assert(!this.Running, "Sequence already running");

		this.tween.ResumeAll();
		this.Running = true;
	}

	public void Reset()
	{
		Debug.Assert(this.tween != null, "Tween was removed");

		if (this.Running)
		{
			Pause();
		}
		this.started = false;
		this.currentStep = 0;
		this.tween.ResetAll();
	}

	public void Kill()
	{
		Debug.Assert(this.tween != null, "Tween was already removed");

		if (this.Running)
		{
			Pause();
		}
		this.tween.QueueFree();
		this.Complete = true;
	}

	private void RunNextStep()
	{
		Debug.Assert(this.tweeners.Count != 0, "Sequence has no steps");

		var group = this.tweeners[this.currentStep];
		foreach (var tweener in group)
		{
			tweener.Start(this.tween);
		}
		this.tween.Start();
	}

	private void StepComplete()
	{
		EmitSignal("step_finished", this.currentStep);
		this.currentStep += 1;

		if (this.currentStep == this.tweeners.Count)
		{
			this.Loops -= 1;
			if (this.Loops == -1)
			{
				EmitSignal("finished");
				if (KillWhenFinished)
				{
					Kill();
				}
				else
				{
					Reset();
				}
			}
			else
			{
				EmitSignal("loop_finished");
				this.currentStep = 0;
				RunNextStep();
			}
		}
		else
		{
			RunNextStep();
		}
	}

	private void AddTweener(Tweener tweener)
	{
		if (!this.parallel)
		{
			this.tweeners.Add(new List<Tweener>());
		}

		this.tweeners[this.tweeners.Count - 1].Add(tweener);
		this.parallel = false;
	}
}

/// <summary>
/// Tweener for tweening properties.
/// </summary>
public class PropertyTweener : Tweener
{
	private Object target;
	private NodePath property;
	private object fromVal;
	private object toVal;
	private float duration;
	private TransitionType transition;
	private EaseType ease;

	private float delay;
	private bool fromCurrent = true;

	internal PropertyTweener(Object target, NodePath property, object to_value, float duration)
	{
		Debug.Assert(target != null, "Invalid target Object.");

		this.target = target;
		this.property = property;
		this.fromVal = target.GetIndexed(property);
		this.toVal = to_value;
		this.duration = duration;
		this.transition = TransitionType.Linear;
		this.ease = EaseType.InOut;
	}

	// Sets custom starting value for the tweener.
	// By default, it starts from value at the start of this tweener.
	public PropertyTweener From(object val)
	{
		this.fromVal = val;
		this.fromCurrent = false;
		return this;
	}

	// Sets the starting value to the current value,
	// i.e. value at the time of creating sequence.
	public PropertyTweener FromCurrent()
	{
		this.fromCurrent = true;
		return this;
	}

	public PropertyTweener SetTransition(TransitionType transition)
	{
		this.transition = transition;
		return this;
	}

	public PropertyTweener SetEase(EaseType ease)
	{
		this.ease = ease;
		return this;
	}

	public PropertyTweener SetDelay(float delay)
	{
		this.delay = delay;
		return this;
	}

	internal override void Start(Tween tween)
	{
		if (!IsInstanceValid(this.target))
		{
			GD.PushWarning("Target object freed, aborting Tweener.");
			return;
		}

		if (this.fromCurrent)
		{
			this.fromVal = this.target.GetIndexed(this.property);
		}

		tween.InterpolateProperty(this.target, this.property, this.fromVal, this.toVal, this.duration, this.transition, this.ease, this.delay);
	}
}

/// <summary>
/// Generic tweener for creating delays in sequence.
/// </summary>
public class IntervalTweener : Tweener
{
	private float time;

	internal IntervalTweener(float time)
	{
		this.time = time;
	}

	internal override void Start(Tween tween)
	{
		tween.InterpolateCallback(this, this.time, "_");
	}

	private void _() { }
}

/// <summary>
/// Tweener for calling methods.
/// </summary>
public class CallbackTweener : Tweener
{
	private Object target;
	private float delay;
	private string method;
	private object[] args;

	internal CallbackTweener(Object target, string method, object[] args)
	{
		Debug.Assert(target != null, "Invalid target Object.");

		this.target = target;
		this.method = method;
		this.args = args;
	}

	public CallbackTweener SetDelay(float delay)
	{
		this.delay = delay;
		return this;
	}

	internal override void Start(Tween tween)
	{
		if (!IsInstanceValid(this.target))
		{
			GD.PushWarning("Target object freed, aborting Tweener.");
			return;
		}

		tween.InterpolateCallback(this.target, this.delay, this.method, GetArgument(0), GetArgument(1), GetArgument(2), GetArgument(3), GetArgument(4));
	}

	private object GetArgument(int index)
	{
		return args.Length > index ? args[index] : null;
	}
}

/// <summary>
/// Tweener for tweening arbitrary values using getter/setter method.
/// </summary>
public class MethodTweener : Tweener
{
	private Object target;
	private string method;
	private object fromVal;
	private object toVal;
	private float duration;
	private TransitionType transition;
	private EaseType ease;

	private float delay;

	internal MethodTweener(Object target, string method, object fromValue, object toValue, float duration)
	{
		Debug.Assert(target != null, "Invalid target Object.");

		this.target = target;
		this.method = method;
		this.fromVal = fromValue;
		this.toVal = toValue;
		this.duration = duration;
		this.transition = TransitionType.Linear;
		this.ease = EaseType.InOut;
	}

	public MethodTweener SetTransition(TransitionType transition)
	{
		this.transition = transition;
		return this;
	}

	public MethodTweener SetEase(EaseType ease)
	{
		this.ease = ease;
		return this;
	}

	public MethodTweener SetDelay(float delay)
	{
		this.delay = delay;
		return this;
	}

	internal override void Start(Tween tween)
	{
		if (!IsInstanceValid(this.target))
		{
			GD.PushWarning("Target object freed, aborting Tweener.");
			return;
		}

		tween.InterpolateMethod(this.target, this.method, this.fromVal, this.toVal, this.duration, this.transition, this.ease, this.delay);
	}
}
