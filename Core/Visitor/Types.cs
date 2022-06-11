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


	private void AssertType(Type type, params object[] objects)
	{
		foreach(var obj in objects)
		{
			switch (obj)
			{
				case ScratchVariable variable:
					if (variable.Type != type)
						Message("E11", true, null, variable.Type.Name, type.Name);
					break;
				case Block shadow:
					if (!string.IsNullOrEmpty(shadow.FunctionArgument) && HasFunctionArgument(shadow.FunctionArgument) && _currentBuilder.Arguments[shadow.FunctionArgument].Fake)
						_currentBuilder.UpdateArgumentType(shadow.FunctionArgument, type);
					else
					{
						if (shadow.ExpectedType != null && shadow.ExpectedType != type)
							Message("E11", true, null, shadow.ExpectedType.Name, type.Name);
						else if (shadow.ExpectedType == null)
							shadow.ExpectedType = type;
					}
					break;
				case ScratchCustomBlock function:
					if (function.ReturnType != type)
						Message("E11", true, null, function.ReturnType.Name, type.Name);
					else if (function.ReturnType == null)
						function.ReturnType = type;
					break;
			}
		}
	}
}