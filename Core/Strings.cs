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
		{"E12", "Cannot define function with name \"{0}\"; a variable with such name already exists."},
		{"E13", "Function with name \"{0}\" is already defined."},
		{"E14", "Argument names cannot match the function name."},
		{"E15", "Argument names cannot match already defined variables."},
		{"E16", "Functions must be defined as top-level statements."},
		{"E17", "Cannot return in a non-function context."},
		{"W18", "Return statement cannot determine the return type of function using _expectedType."}
	};

	public static Dictionary<string, string> Notes = new()
	{
		{"E3", "Did you mean to assign a value?"},
		{"W5", "Defaulting to string."}
	};
}