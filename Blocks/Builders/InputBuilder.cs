using ScratchScript.Core.Types;
using ScratchScript.Core.Visitor;
using ScratchScript.Wrapper;

namespace ScratchScript.Blocks.Builders;

public enum ShadowMode
{
	NoShadow = 1,
	Shadow = 2,
	ObscuredShadow = 3,
	Undefined = 0
}

public class InputBuilder
{
	private string _name;
	private List<object> _objects = new();
	private string _shadowId;
	private int _shadowMode;

	private readonly Dictionary<Type, int> _typeToScratchId = new()
	{
		{typeof(decimal), 4},
		{typeof(string), 10},
		{typeof(ScratchColor), 9},
		{typeof(bool), 10}
	};

	public List<Block> Shadows = new();

	public InputBuilder WithName(string name)
	{
		_name = name;
		return this;
	}

	public InputBuilder WithObject(object? obj, ShadowMode mode = ShadowMode.ObscuredShadow)
	{
		switch (obj)
		{
			case Block shadow:
				_shadowMode = (int) mode;
				_shadowId = shadow.Id;
				Shadows.Add(shadow);
				break;
			case ScratchVariable variable:
				_shadowMode = (int) ShadowMode.ObscuredShadow;
				_objects = new List<object>
				{
					12,
					variable.Name,
					variable.Id
				};
				break;
			case null:
				break;
			default:
				_shadowMode = (int) ShadowMode.NoShadow;
				_objects = new List<object>
				{
					_typeToScratchId[ScratchScriptVisitor.GetExpectedType(obj)],
					obj.ToString() == null ? obj: obj.ToString()!.ToLower()
				};
				break;
		}

		return this;
	}

	public KeyValuePair<string, List<object>> Build()
	{
		var objects = new List<object> {_shadowMode};
		if (!string.IsNullOrEmpty(_shadowId))
			objects.Add(_shadowId);
		if (_objects.Count != 0)
			objects.Add(_objects);
		return new KeyValuePair<string, List<object>>(_name, objects);
	}
}