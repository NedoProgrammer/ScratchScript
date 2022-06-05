namespace ScratchScript.Wrapper;

public class Target
{
	public Dictionary<string, Block> blocks = new();
	public Dictionary<string, string> broadcasts = new();
	public Dictionary<string, Comment> comments = new();
	public List<Asset> costumes = new();
	public int currentCostume;
	public float? direction;
	public bool? draggable;
	public bool isStage;
	public int layerOrder;
	public Dictionary<string, List<object>> lists = new();
	public string name;
	public string? rotationStyle;
	public float? size;
	public List<Asset> sounds = new();

	public int? tempo;
	public string? textToSpeechLanguage;
	public Dictionary<string, List<object>> variables = new();
	public string? videoState;
	public int? videoTransparency;

	public bool? visible;
	public float volume;
	public int? x;
	public int? y;
}