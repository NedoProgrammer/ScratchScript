using ScratchScript.Blocks;
using ScratchScript.Blocks.Builders;
using ScratchScript.Core.Types;
using ScratchScript.Wrapper;

namespace ScratchScript.Core.Compiler;

public class TargetCompiler
{
	private readonly List<AttachInfo> AttachInformationList = new();
	public Dictionary<string, ScratchCustomBlock> Functions = new();
	public Comment? PendingComment;
	public Dictionary<string, ScratchVariable> Variables = new();
	public Target WrappedTarget = new();


	public TargetCompiler()
	{
		WrappedTarget = new Target
		{
			isStage = ProjectCompiler.Current.TargetCompilerCount == 0,
			currentCostume = 0,
			volume = 100,
			tempo = 60,
			videoTransparency = 50,
			videoState = "on",
			textToSpeechLanguage = null,
			x = 0,
			y = 0,
			size = 100,
			direction = 90,
			draggable = false,
			rotationStyle = "all around"
		};
		LayerOrder = ProjectCompiler.Current.TargetCompilerCount;
	}

	public Dictionary<string, Block> Blocks => WrappedTarget.blocks;

	public string Name
	{
		get => WrappedTarget.name;
		set => WrappedTarget.name = value;
	}

	public int LayerOrder
	{
		get => WrappedTarget.layerOrder;
		set => WrappedTarget.layerOrder = value;
	}

	private AttachInfo? AttachInformation => AttachInformationList.LastOrDefault();

	public void AddEntryPoint()
	{
		var entryPoint = AddBlock(Event.WhenFlagClicked());

		EnterAttachmentScope(new AttachInfo
		{
			ChildIsNext = true,
			To = entryPoint
		});
	}

	public void CreateVariable(string name, object value)
	{
		if (Variables.ContainsKey(name)) return;
		var id = "ScratchScript_Variable_" + Guid.NewGuid().ToString("N");
		Variables[name] = new ScratchVariable
		{
			Id = id,
			Name = name,
			Type = value.GetType()
		};

		WrappedTarget.variables[id] = new List<object>
		{
			name,
			value
		};
	}

	public Block AddBlock(Block block)
	{
		if (AttachInformation == null)
		{
			block.parent = null;
			block.topLevel = true;
		}
		else
		{
			block.parent = AttachInformation.To.Id;
			if (AttachInformation.ChildIsNext)
			{
				AttachInformation.To.next = block.Id;
				UpdateBlock(AttachInformation.To);
				AttachInformation.To = block;
			}
			else if (AttachInformation.InputIndex != AttachInformation.To.inputs.Count)
			{
				var lastInput =
					AttachInformation.To.inputs.ElementAt(AttachInformation.InputIndex);
				AttachInformation.To = new BlockBuilder(AttachInformation.To)
					.WithInput(new InputBuilder()
						.WithName(lastInput.Key)
						.WithObject(block));
				AttachInformation.InputIndex++;
			}

			UpdateBlock(AttachInformation.To);
		}

		UpdateBlock(block);
		return block;
	}

	public void TryAssign(object value)
	{
		if (AttachInformation == null) return;
		if (AttachInformation.InputIndex == AttachInformation.To.inputs.Count) return;
		if (value is Block) return;

		var lastInput =
			AttachInformation.To.inputs.ElementAt(AttachInformation.InputIndex);
		AttachInformation.To = new BlockBuilder(AttachInformation.To)
			.WithInput(new InputBuilder()
				.WithName(lastInput.Key)
				.WithObject(value));
		AttachInformation.InputIndex++;
	}

	public void EnterAttachmentScope(AttachInfo info)
	{
		AttachInformationList.Add(info);
	}

	public void ExitAttachmentScope()
	{
		AttachInformationList.RemoveAt(AttachInformationList.Count - 1);
	}

	public void UpdateBlock(Block block)
	{
		if (block.parent != null)
			block.topLevel = false;
		Blocks[block.Id] = block;
	}
}