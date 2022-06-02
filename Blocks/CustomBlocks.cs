using ScratchScript.Blocks.Builders;
using ScratchScript.Compiler;
using ScratchScript.Types;
using ScratchScript.Wrapper;

namespace ScratchScript.Blocks;

public static class CustomBlocks
{
	public static Block Definition(string name, Block prototype)
	{
		return new BlockBuilder()
			.WithOpcode("procedures_definition")
			.WithId($"FunctionDefinition_{name}")
			.IsTopLevel()
			.WithInput(new InputBuilder()
				.WithName("custom_block")
				.WithShadow(prototype, ShadowMode.NoShadow));
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

		return block.ArgumentIds.Aggregate(builder, (current, pair) => current.WithInput(new InputBuilder().WithName(pair.Value)
			.WithShadow(block.Reporters[pair.Key], ShadowMode.NoShadow)));
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
				.WithName(block.ArgumentIds.ElementAt(i).Value);

			switch (parameters[i])
			{
				case ScratchVariable variable:
					input = input.WithVariable(variable);
					break;
				case Block shadow:
					input = input.WithShadow(shadow);
					shadow.parent = builder.Id;
					ProjectCompiler.Current.CurrentTarget.ReplaceBlock(shadow);
					break;
				default:
					input = input.WithRawObject(parameters[i]);
					break;
			}

			builder = builder.WithInput(input);
		}

		return builder;
	}
}