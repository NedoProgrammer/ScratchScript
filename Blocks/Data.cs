using ScratchScript.Blocks.Builders;
using ScratchScript.Core.Types;
using ScratchScript.Wrapper;

namespace ScratchScript.Blocks;

public class Data
{
	public static Block SetVariableTo(ScratchVariable target, object? to)
	{
		var builder = new BlockBuilder()
			.WithOpcode("data_setvariableto")
			.IsShadow(false)
			.WithId("SetVariableTo")
			.WithField(new FieldBuilder()
				.WithName("VARIABLE")
				.WithVariable(target))
			.WithInput(new InputBuilder()
				.WithName("VALUE")
				.WithObject(to));
		return builder;
	}
}