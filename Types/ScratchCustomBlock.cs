using ScratchScript.Wrapper;

namespace ScratchScript.Types;

public class ScratchCustomBlock
{
	public Mutation SharedMutation;
	public string Name => SharedMutation.proccode.Split(" ")[0].Trim();
	public Block Definition;
	public Block Prototype;
	public Type ReturnType;
	public string? ReturnVariable;
	public Dictionary<string, Block> Reporters = new();
	public Dictionary<string, string> ArgumentIds = new();
	public Dictionary<string, Type> ArgumentTypes = new();
}