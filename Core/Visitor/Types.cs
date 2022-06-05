using ScratchScript.Core.Types;
using ScratchScript.Wrapper;

namespace ScratchScript.Core.Visitor;

public partial class ScratchScriptVisitor
{
	public static Type GetExpectedType(object obj)
	{
		return obj switch
		{
			ScratchVariable variable => variable.Type,
			ScratchCustomBlock function => function.ReturnType ?? typeof(void),
			Block block => block.ExpectedType ?? typeof(void),
			_ => decimal.TryParse(obj.ToString(), out _) ? typeof(decimal) : obj.GetType()
		};
	}

	private object GetDefaultValue(Type type)
	{
		if (type == typeof(string))
			return "";
		if (type == typeof(bool))
			return "false";
		return Activator.CreateInstance(type) ??
		       throw new Exception($"The type \"{type.Name}\" is not supported by ScratchScript.");
	}

	private void AssertType(object obj, Type type)
	{
		switch (obj)
		{
			case ScratchVariable variable:
				if (variable.Type != type)
					Message("E11", true, null, type.Name, variable.Type.Name);
				break;
			case Block shadow:
				if (shadow.ExpectedType != null && shadow.ExpectedType != type)
					Message("E11", true, null, type.Name, shadow.ExpectedType.Name);
				else if (shadow.ExpectedType == null)
					shadow.ExpectedType = type;
				break;
			case ScratchCustomBlock function:
				if (function.ReturnType != type)
					Message("E11", true, null, type.Name, function.ReturnType.Name);
				else if (function.ReturnType == null)
					function.ReturnType = type;
				break;
		}
	}
}