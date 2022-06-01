namespace ScratchScript.Wrapper;

public class Block
{
	[NonSerialized] public Dictionary<string, object> AdditionalData;
	public string? comment;
	public Dictionary<string, List<object>> fields = new();

	[NonSerialized] public string Id;
	public Dictionary<string, List<object>> inputs = new();
	public string? next;
	public string opcode;
	public string? parent;
	public bool shadow;
	public int? x;
	public int? y;
	public Mutation? mutation;
	public bool topLevel => parent == null;
}