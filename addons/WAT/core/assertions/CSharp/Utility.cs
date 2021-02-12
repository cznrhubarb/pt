using System;
using Godot;
using Godot.Collections;
using Object = Godot.Object;

namespace WAT
{
    public class Utility: Assertion
	{
		public static Dictionary IsNull(object value, string context)
		{
			var passed = "value is null";
			var success = value == null;
			var result = success ? passed : $"value is |{value.GetType()}| {value}, expected null";
			return Result(success, passed, result, context);
		}

		public static Dictionary IsNotNull(object value, string context)
		{
			var passed = $"{value} is not null";
			var failed = "value is null, expected not null";
			var success = value != null;
			var result = success ? passed : failed;
			return Result(success, passed, result, context);
		}

		public static Dictionary Fail(string context)
        {
            return Result(false, "N/A", "N/A", context);
        }
    }
}
