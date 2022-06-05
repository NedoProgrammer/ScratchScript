using ScratchScript.Core.Compiler;
using ScratchScript.Extensions;
using ScratchScript.Wrapper;

namespace ScratchScript.Blocks.Builders;

public class BlockBuilder
{
	private readonly Dictionary<string, List<object>> _fields = new();
	private readonly Dictionary<string, List<object>> _inputs = new();
	private Mutation? _mutation;
	private string _opcode = "";
	private string? _parent;
	private bool _shadow;
	private bool _topLevel;
	private int? _x = 0;
	private int? _y = 0;

	public BlockBuilder(Block block)
	{
		_opcode = block.opcode;
		_parent = block.parent;
		_x = block.x;
		_y = block.y;
		Id = block.Id;
		_shadow = block.shadow;
		_inputs = block.inputs;
		_fields = block.fields;
		_mutation = block.mutation;
		_topLevel = block.topLevel;
	}

	public BlockBuilder()
	{
	}

	public string Id { get; private set; } = "";

	public BlockBuilder IsTopLevel(bool topLevel = true)
	{
		_topLevel = topLevel;
		return this;
	}

	public BlockBuilder WithId(string id)
	{
		Id = BlockExtensions.RandomId(id);
		return this;
	}

	public BlockBuilder WithMutation(Mutation mutation)
	{
		_mutation = mutation;
		return this;
	}

	public BlockBuilder WithInput(InputBuilder builder)
	{
		var pair = builder.Build();
		_inputs[pair.Key] = pair.Value;
		foreach (var shadow in builder.Shadows)
		{
			shadow.parent = Id;
			ProjectCompiler.Current.CurrentTarget.UpdateBlock(shadow);
		}

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
			topLevel = _topLevel,
			Id = Id
		};
		return block;
	}

	public static implicit operator Block(BlockBuilder builder)
	{
		return builder.Build();
	}
}