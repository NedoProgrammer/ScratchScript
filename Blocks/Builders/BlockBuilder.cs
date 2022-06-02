using ScratchScript.Extensions;
using ScratchScript.Wrapper;

namespace ScratchScript.Blocks.Builders;

public class BlockBuilder
{
	private readonly Dictionary<string, List<object>> _fields = new();
	private string _id = "";
	private readonly Dictionary<string, List<object>> _inputs = new();
	private string _opcode = "";
	private string? _parent;
	private bool _shadow;
	private int? _x = 0;
	private int? _y = 0;
	private Mutation? _mutation;
	private bool _topLevel;

	public BlockBuilder(Block block)
	{
		_opcode = block.opcode;
		_parent = block.parent;
		_x = block.x;
		_y = block.y;
		_id = block.Id;
		_shadow = block.shadow;
		_inputs = block.inputs;
		_fields = block.fields;
		_mutation = block.mutation;
		_topLevel = block.topLevel;
	}
	
	public BlockBuilder()
	{
	}

	public BlockBuilder IsTopLevel(bool topLevel = true)
	{
		_topLevel = topLevel;
		return this;
	}

	public BlockBuilder WithId(string id)
	{
		_id = id;
		return this;
	}
	
	public string Id => _id;

	public BlockBuilder WithMutation(Mutation mutation)
	{
		_mutation = mutation;
		return this;
	}

	public BlockBuilder WithInput(InputBuilder builder)
	{
		var pair = builder.Build();
		_inputs[pair.Key] = pair.Value;
		return this;
	}

	public BlockBuilder WithField(FieldBuilder builder)
	{
		var pair = builder.Build();
		_fields[pair.Key] = pair.Value;
		return this;
	}

	public BlockBuilder WithOpcode(string opcode)
	{
		_opcode = opcode;
		return this;
	}

	public BlockBuilder WithParent(string? parent)
	{
		_parent = parent;
		return this;
	}

	public BlockBuilder WithPosition(int x, int y)
	{
		_x = x;
		_y = y;
		return this;
	}

	public BlockBuilder IsShadow(bool shadow = true)
	{
		_shadow = shadow;
		return this;
	}

	public Block Build()
	{
		var block = new Block
		{
			opcode = _opcode,
			parent = _parent,
			inputs = _inputs,
			fields = _fields,
			x = _x,
			y = _y,
			shadow = _shadow,
			mutation = _mutation,
			topLevel = _topLevel
		}.WithPurposeId(_id);
		return block;
	}

	public static implicit operator Block(BlockBuilder builder)
	{
		return builder.Build();
	}
}