using System.Diagnostics;
using ScratchScript.Extensions;
using ScratchScript.Helpers;
using ScratchScript.Wrapper;

namespace ScratchScript.Blocks.Builders;

public class BlockBuilder
{
    private string _opcode = "";
    private string? _parent = null;
    private int? _x = 0;
    private int? _y = 0;
    private string _id = "";
    private bool _shadow;
    private Dictionary<string, List<object>> _inputs = new();
    private Dictionary<string, List<object>> _fields = new();

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
    }
    
    public BlockBuilder() {}

    public BlockBuilder WithId(string id)
    {
        _id = id;
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
            shadow = _shadow
        }.WithPurposeId(_id);
        return block;
    }

    public static implicit operator Block(BlockBuilder builder) => builder.Build();
}