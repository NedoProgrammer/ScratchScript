using ScratchScript.Core;
using ScratchScript.Helpers;
using ScratchScript.Types;
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

	public InputBuilder WithName(string name)
	{
		_name = name;
		return this;
	}

	public InputBuilder WithShadow(Block shadow, ShadowMode mode = ShadowMode.ObscuredShadow)
	{
		_shadowMode = (int) mode;
		_shadowId = shadow.Id;
		return this;
	}

	private Dictionary<Type, int> _typeToScratchId = new()
	{
		{typeof(decimal), 4},
		{typeof(string), 10},
		{typeof(ScratchColor), 9}
	};

	public InputBuilder WithRawObject(object? obj)
	{
		if (obj != null)
		{
			_shadowMode = (int) ShadowMode.NoShadow;
			_objects = new List<object>
			{
				_typeToScratchId[ScratchScriptVisitor.GetExpectedInternalType(obj)],
				obj
			};
		}

		return this;
	}

	public InputBuilder WithVariable(ScratchVariable variable)
	{
		_shadowMode = (int) ShadowMode.ObscuredShadow;
		_objects = new List<object>
		{
			12,
			variable.Name,
			variable.Id
		};
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