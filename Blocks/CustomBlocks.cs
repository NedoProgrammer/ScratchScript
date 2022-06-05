using ScratchScript.Blocks.Builders;
using ScratchScript.Core.Compiler;
using ScratchScript.Core.Types;
using ScratchScript.Wrapper;

namespace ScratchScript.Blocks;

public class CustomBlocks
{
	public static Block Definition(string name, Block prototype)
	{
		return new BlockBuilder()
			.WithOpcode("procedures_definition")
			.WithId($"FunctionDefinition_{name}")
			.IsTopLevel()
			.WithInput(new InputBuilder()
				.WithName("custom_block")
				.WithObject(prototype, ShadowMode.NoShadow));
	}

	public static Block ReporterBoolean(string argumentName)
	{
		return new BlockBuilder()
			.WithOpcode("argument_reporter_boolean")
			.WithId("ArgumentBoolean")
			.WithField(new FieldBuilder()
				.WithName("VALUE")
				.WithObjects(argumentName, null))
			.IsShadow();
	}

	public static Block ReporterStringNumber(string argumentName)
	{
		return new BlockBuilder()
			.WithOpcode("argument_reporter_string_number")
			.WithId("ArgumentStringNumber")
			.WithField(new FieldBuilder()
				.WithName("VALUE")
				.WithObjects(argumentName, null))
			.IsShadow();
	}

	public static Block Prototype(ScratchCustomBlock block)
	{
		var builder = new BlockBuilder()
			.WithOpcode("procedures_prototype")
			.WithId($"FunctionPrototype_{block.Name}")
			.IsShadow()
			.WithMutation(block.SharedMutation);

		return block.ArgumentIds.Aggregate(builder, (current, pair) => current.WithInput(new InputBuilder()
			.WithName(pair.Value)
			.WithObject(block.Reporters[pair.Key], ShadowMode.NoShadow)));
	}

	public static Block Call(ScratchCustomBlock block, params object[] parameters)
	{
		var builder = new BlockBuilder()
			.WithOpcode("procedures_call")
			.WithId($"MethodCall_{block.Name}")
			.WithMutation(block.SharedMutation);

		for (var i = 0; i < parameters.Length; i++)
		{
			var input = new InputBuilder()
				.WithName(block.ArgumentIds.ElementAt(i).Value)
				.WithObject(parameters[i]);

			if (parameters[i] is Block shadow)
			{
				shadow.parent = builder.Id;
				ProjectCompiler.Current.CurrentTarget.UpdateBlock(shadow);
			}

			builder = builder.WithInput(input);
		}

		return builder;
	}
}