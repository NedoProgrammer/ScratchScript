using System.Linq.Expressions;
using ScratchScript.Compiler;
using ScratchScript.Extensions;
using ScratchScript.Types;
using ScratchScript.Wrapper;

namespace ScratchScript.Blocks.Builders;

public enum ParameterType
{
	Boolean,
	StringOrNumber
}

public class CustomBlockBuilder
{
	private string _name;
	private Dictionary<string, ParameterType> _arguments = new();
	private List<Block> _body = new();

	public CustomBlockBuilder WithName(string name)
	{
		_name = name;
		return this;
	}

	public CustomBlockBuilder WithArgument(string name, ParameterType type)
	{
		_arguments[name] = type;
		return this;
	}

	public CustomBlockBuilder WithBody(IEnumerable<Block> blocks)
	{
		_body = blocks.ToList();
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
		
		var proccode = _name + " ";
		var argumentIds = "[";
		var argumentDefaults = "[";
		var argumentNames = "[";
		
		foreach (var pair in _arguments)
		{
			block.ArgumentIds[pair.Key] = BlockExtensions.RandomId($"Argument_{_name}");
			proccode += pair.Value == ParameterType.Boolean ? "%b " : "%s ";
			argumentIds += $"\"{block.ArgumentIds[pair.Key]}\",";
			argumentDefaults += $"\"{(pair.Value == ParameterType.Boolean ? "false": "")}\",";
			argumentNames += $"\"{pair.Key}\",";
			block.Reporters[pair.Key] = pair.Value == ParameterType.Boolean
				? target.CreateBlock(CustomBlocks.ReporterBoolean(pair.Key), ignoreParent:true, ignoreNext:true)
				: target.CreateBlock(CustomBlocks.ReporterStringNumber(pair.Key), ignoreNext:true, ignoreParent:true);
		}

		proccode = proccode.Trim();
		argumentIds = argumentIds.Remove(argumentIds.LastIndexOf(",")) + "]";
		argumentDefaults = argumentDefaults.Remove(argumentDefaults.LastIndexOf(",")) + "]";
		argumentNames = argumentNames.Remove(argumentNames.LastIndexOf(",")) + "]";
		
		mutation.proccode = proccode;
		mutation.argumentids = argumentIds;
		mutation.argumentdefaults = argumentDefaults;
		mutation.argumentnames = argumentNames;

		block.SharedMutation = mutation;
		block.Prototype = CustomBlocks.Prototype(block);
		block.Definition = CustomBlocks.Definition(block.Name, block.Prototype);
		return block;
	}
}