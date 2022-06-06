using Serilog;

namespace ScratchScript.Core.Visitor;

public partial class ScratchScriptVisitor
{
	public override object? VisitAttributeStatement(ScratchScriptParser.AttributeStatementContext context)
	{
		EnterContext(context);
		if (Target.Blocks.Count > 1 || Target.Variables.Count > 0)
		{
			Message("E4", true);
			return null;
		}

		var attribute = context.Identifier().GetText();
		Log.Debug("Found attribute ({Name})", context.GetText());
		switch (attribute)
		{
			case "stage":
				Log.Debug("Switching to stage");
				Project.SetCurrentTarget("Stage");
				break;
			default:
				Message("E20", false, context.Identifier().Symbol, attribute);
				break;
		}

		ExitContext();
		return null;
	}
}