namespace ScratchScript.Core;

public class Strings
{
	//could not find suitable type for variable {Variable} and shadow {ShadowId} using _expectedType. Defaulting to string
	public static Dictionary<string, string> Messages = new()
	{
		{"W1", "Division by zero is not recommended."},
		{"E2", "ICE: compiler failed to parse an expression."},
		{"E3", "Variable \"{0}\" was already defined."},
		{"E4", "Attributes must be defined at the beginning of the file."},
		{"W5", "ICW: compiler could not find a suitable type for variable \"{0}\" using _expectedType."},
		{"W6", "ICW: ExpectedType is not recommended to be null."},
		{"E7", "Variable \"{0}\" is not defined."},
		{"E8", "Cannot assign a value of type \"{0}\" to a variable of type \"{1}\"."},
		{"E9", "Unexpected identifier \"{0}\"."},
		{"E10", "Syntax error: {0}"},
		{"E11", "ICE: type mismatch, expected \"{0}\", received \"{1}\"."},
		{"E12", "Cannot define method with name \"{0}\"; a variable with such name already exists."},
		{"E13", "Method with name \"{0}\" is already defined."}
	};

	public static Dictionary<string, string> Notes = new()
	{
		{"E3", "Did you mean to assign a value?"},
		{"W5", "Defaulting to string."}
	};
}