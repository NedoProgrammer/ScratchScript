using ScratchScript.Blocks;
using Serilog;

namespace ScratchScript.Core.Visitor;

public partial class ScratchScriptVisitor
{
	public override object? VisitUnaryAddExpression(ScratchScriptParser.UnaryAddExpressionContext context)
	{
		EnterContext(context);
		Log.Debug("Found a unary expression ({Text})", context.GetText());

		var block = Target.AddBlock(Operators.Join(context.addOperators().GetText(), null));
		Target.EnterAttachmentScope(new AttachInfo
		{
			To = block,
			ChildIsNext = false,
			InputIndex = 1
		});

		if (!TryVisit(context.expression(), out var expression)) return null;
		
		AssertType(typeof(decimal), block, expression);

		Target.ExitAttachmentScope();
		ExitContext();
		return block;
	}
}