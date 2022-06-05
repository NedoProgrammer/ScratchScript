namespace ScratchScript.Wrapper;

public class Block
{
	public string? comment;
	[NonSerialized] public Type? ExpectedType;
	public Dictionary<string, List<object>> fields = new();
	[NonSerialized] public string Id;
	public Dictionary<string, List<object>> inputs = new();
	public Mutation? mutation;
	public string? next;
	public string opcode;
	public string? parent;
	public bool shadow;
	public bool topLevel;
	public int? x;
	public int? y;
}