using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Object = Godot.Object;

namespace WAT
{
    public class Container: Assertion
	{
		public static Dictionary ListContains<T>(List<T> list, T value, string context)
		{
			var passed = $"List contains {value}";
			var failed = $"List does not contain {value}";
			var success = list.Contains(value);
			var result = success ? passed : failed;
			return Result(success, passed, result, context);
		}

		public static Dictionary ListDoesNotContain<T>(List<T> list, T value, string context)
		{
			var passed = $"List does not contain {value}";
			var failed = $"List contains {value}";
			var success = !list.Contains(value);
			var result = success ? passed : failed;
			return Result(success, passed, result, context);
		}
    }
}
