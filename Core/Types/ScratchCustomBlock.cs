using ScratchScript.Wrapper;

namespace ScratchScript.Core.Types;

public class ScratchCustomBlock
{
	public Dictionary<string, string> ArgumentAliases = new();
	public Dictionary<string, string> ArgumentIds = new();
	public Dictionary<string, Type> ArgumentTypes = new();
	public Block Definition;
	public Block Prototype;
	public Dictionary<string, Block> Reporters = new();
	public Type? ReturnType;
	public string? ReturnVariable;
	public Mutation SharedMutation;
	public string Name => SharedMutation.proccode.Split(" ")[0].Trim();
}