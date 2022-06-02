using System.Collections.Concurrent;
using System.Linq.Expressions;
using ScratchScript.Compiler;
using ScratchScript.Extensions;
using ScratchScript.Types;
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
	private string _name;
	private string _id;
	private Type? _returnType;
	private Dictionary<string, Block> _reporters = new();
	public Type? ReturnType => _returnType;
	public Dictionary<string, Type> Arguments => _arguments;
	private Dictionary<string, Type> _arguments = new();

	public CustomBlockBuilder WithName(string name)
	{
		_name = name;
		return this;
	}
	
	public CustomBlockBuilder WithReturnType(Type type)
	{
		_returnType = type;
		return this;
	}

	public CustomBlockBuilder WithId(string id)
	{
		_id = id;
		return this;
	}

	public CustomBlockBuilder WithArgument(string name, Type type)
	{
		_arguments[name] = type;
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
		
		var proccode = _name + " ";
		var argumentIds = "[";
		var argumentDefaults = "[";
		var argumentNames = "[";
		
		foreach (var pair in _arguments)
		{
			block.ArgumentTypes[pair.Key] = pair.Value;
			block.ArgumentIds[pair.Key] = BlockExtensions.RandomId($"FunctionArgument_{_name}");
			proccode += pair.Value == typeof(bool) ? "%b " : "%s ";
			argumentIds += $"\"{block.ArgumentIds[pair.Key]}\",";
			argumentDefaults += $"\"{(pair.Value == typeof(bool) ? "false": "")}\",";
			argumentNames += $"\"{pair.Key}\",";
			block.Reporters[pair.Key] = pair.Value == typeof(bool)
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

		block.ReturnType = _returnType;
		block.SharedMutation = mutation;
		block.Prototype = target.CreateBlock(CustomBlocks.Prototype(block), true, true);
		block.Definition = target.CreateBlock(CustomBlocks.Definition(block.Name, block.Prototype), true, true);

		block.Prototype.parent = block.Definition.Id;
		target.ReplaceBlock(block.Prototype);
		foreach (var pair in block.Reporters)
		{
			pair.Value.parent = block.Prototype.Id;
			target.ReplaceBlock(pair.Value);
		}
		return block;
	}
}