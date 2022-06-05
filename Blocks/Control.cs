using ScratchScript.Blocks.Builders;
using ScratchScript.Wrapper;

namespace ScratchScript.Blocks;

public class Control
{
	public static Block If(Block condition)
	{
		return new BlockBuilder()
			.WithOpcode("control_if")
			.WithId("ControlIfStatement")
			.WithInput(new InputBuilder()
				.WithName("CONDITION")
				.WithObject(condition, ShadowMode.Shadow));
	}

	public static Block IfElse(Block condition)
	{
		return new BlockBuilder()
			.WithOpcode("control_if_else")
			.WithId("ControlIfElseStatement")
			.WithInput(new InputBuilder()
				.WithName("CONDITION")
				.WithObject(condition, ShadowMode.Shadow));
	}
}