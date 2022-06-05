using ScratchScript.Core.Types;

namespace ScratchScript.Blocks.Builders;

public class FieldBuilder
{
	private string _name;
	private List<object?> _objects = new();

	public FieldBuilder WithName(string name)
	{
		_name = name;
		return this;
	}

	public FieldBuilder WithVariable(ScratchVariable variable)
	{
		_objects = new List<object>
		{
			variable.Name,
			variable.Id
		}!;
		return this;
	}

	public FieldBuilder WithObjects(params object?[] objects)
	{
		_objects = objects.ToList();
		return this;
	}

	public KeyValuePair<string, List<object>> Build()
	{
		return new KeyValuePair<string, List<object>>(_name, _objects!);
	}
}