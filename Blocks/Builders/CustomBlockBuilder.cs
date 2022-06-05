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
	private readonly Dictionary<string, Block> _reporters = new();
	public string ReturnVariable;

	public string Name { get; private set; }

	public Type? ReturnType { get; private set; }

	public Dictionary<string, Type> Arguments { get; } = new();

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

	public CustomBlockBuilder WithArgument(string name, Type type)
	{
		Arguments[name] = type;
		_reporters[name] = type == typeof(bool)
			? CustomBlocks.ReporterBoolean(name)
			: CustomBlocks.ReporterStringNumber(name);
		return this;
	}

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
			block.ArgumentTypes[pair.Key] = pair.Value;
			block.ArgumentIds[pair.Key] = BlockExtensions.RandomId($"FunctionArgument_{Name}");
			proccode += pair.Value == typeof(bool) ? "%b " : "%s ";
			argumentIds += $"\"{block.ArgumentIds[pair.Key]}\",";
			argumentDefaults += $"\"{(pair.Value == typeof(bool) ? "false" : "")}\",";
			argumentNames += $"\"{pair.Key}\",";
			//block.Reporters[pair.Key] = pair.Value == typeof(bool)
			//	? target.AddBlock(CustomBlocks.ReporterBoolean(pair.Key))
			//	: target.AddBlock(CustomBlocks.ReporterStringNumber(pair.Key));
		}

		proccode = proccode.Trim();
		argumentIds = argumentIds.Remove(argumentIds.LastIndexOf(",")) + "]";
		argumentDefaults = argumentDefaults.Remove(argumentDefaults.LastIndexOf(",")) + "]";
		argumentNames = argumentNames.Remove(argumentNames.LastIndexOf(",")) + "]";

		mutation.proccode = proccode;
		mutation.argumentids = argumentIds;
		mutation.argumentdefaults = argumentDefaults;
		mutation.argumentnames = argumentNames;

		block.ReturnVariable = ReturnVariable;
		block.ReturnType = ReturnType;
		block.SharedMutation = mutation;
		//block.Prototype = target.AddBlock(CustomBlocks.Prototype(block));
		//block.Definition = target.AddBlock(CustomBlocks.Definition(block.Name, block.Prototype));

		block.Prototype.parent = block.Definition.Id;
		target.UpdateBlock(block.Prototype);
		foreach (var pair in block.Reporters)
		{
			pair.Value.parent = block.Prototype.Id;
			target.UpdateBlock(pair.Value);
		}

		return block;
	}
}