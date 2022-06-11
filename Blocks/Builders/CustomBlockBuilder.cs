using System.Reflection.Emit;
using ScratchScript.Core.Compiler;
using ScratchScript.Core.Types;
using ScratchScript.Extensions;
using ScratchScript.Wrapper;

namespace ScratchScript.Blocks.Builders;

public enum ParameterType
{
	Boolean,
	StringOrNumber,
	Undefined
}

public class CustomBlockBuilder
{
	private string _id;
	public readonly Dictionary<string, Block> Reporters = new();
	public string ReturnVariable;

	public string Name { get; private set; }

	public Type? ReturnType { get; private set; }

	public Dictionary<string, ScratchCustomBlock.ScratchArgument> Arguments { get; } = new();

	public CustomBlockBuilder WithName(string name)
	{
		Name = name;
		return this;
	}

	public CustomBlockBuilder WithReturnType(Type type)
	{
		ReturnType = type;
		return this;
	}

	public CustomBlockBuilder WithId(string id)
	{
		_id = id;
		return this;
	}

	public CustomBlockBuilder WithArgument(string name, Type type, bool fake = false)
	{
		Arguments[name] = new ScratchCustomBlock.ScratchArgument
		{
			Type = type,
			Fake = fake
		};
		Reporters[name] = ProjectCompiler.Current.CurrentTarget.AddBlock(type == typeof(bool)
			? CustomBlocks.ReporterBoolean(name)
			: CustomBlocks.ReporterStringNumber(name), true);
		return this;
	}

	private ScratchCustomBlock? _block;

	public ScratchCustomBlock Build()
	{
		var target = ProjectCompiler.Current.CurrentTarget;
		var block = new ScratchCustomBlock();
		var mutation = new Mutation
		{
			warp = false
		};

		var proccode = Name + " ";
		var argumentIds = "[";
		var argumentDefaults = "[";
		var argumentNames = "[";

		foreach (var pair in Arguments)
		{
			var type = pair.Value.Type;
			block.ArgumentTypes[pair.Key] = pair.Value;
			block.ArgumentIds[pair.Key] = _block == null ? BlockExtensions.RandomId($"FunctionArgument_{Name}"): _block.ArgumentIds[pair.Key];
			proccode += type == typeof(bool) ? "%b " : "%s ";
			argumentIds += $"\"{block.ArgumentIds[pair.Key]}\",";
			argumentDefaults += $"\"{(type == typeof(bool) ? "false" : "")}\",";
			argumentNames += $"\"{pair.Key}\",";
		}

		proccode = proccode.Trim();
		FinishArrayString(ref argumentIds);
		FinishArrayString(ref argumentNames);
		FinishArrayString(ref argumentDefaults);

		mutation.proccode = proccode;
		mutation.argumentids = argumentIds;
		mutation.argumentdefaults = argumentDefaults;
		mutation.argumentnames = argumentNames;

		block.Reporters = Reporters;
		block.ReturnVariable = ReturnVariable;
		block.ReturnType = ReturnType;
		block.SharedMutation = mutation;
		block.Prototype = target.AddBlock(CustomBlocks.Prototype(block), true);
		block.Definition = target.AddBlock(CustomBlocks.Definition(block.Name, block.Prototype));

		block.Prototype.parent = block.Definition.Id;
		target.UpdateBlock(block.Prototype);
		foreach (var pair in block.Reporters)
		{
			pair.Value.parent = block.Prototype.Id;
			target.UpdateBlock(pair.Value);
		}

		return block;
	}

	private static void FinishArrayString(ref string array)
	{
		var index = array.LastIndexOf(",", StringComparison.Ordinal);
		if (index != -1)
			array = array.Remove(index);
		array += ']';
	}

	public void UpdateArgumentType(string name, Type type)
	{
		var project = ProjectCompiler.Current;
		Arguments[name] = new ScratchCustomBlock.ScratchArgument
		{
			Fake = false,
			Type = type
		};
		Reporters[name].ExpectedType = type;
		if (type == typeof(bool))
		{
			Reporters[name].opcode = "argument_reporter_boolean";
			project.AddPostReplace(Reporters[name].Id, BlockExtensions.RandomId("ArgumentBoolean"));
		}

		project.CurrentTarget.UpdateBlock(Reporters[name]);
	}
}