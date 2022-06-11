using ScratchScript.Blocks;
using Serilog;

namespace ScratchScript.Core.Visitor;

public partial class ScratchScriptVisitor
{
	public override object? VisitAssignmentStatement(ScratchScriptParser.AssignmentStatementContext context)
	{
		EnterContext(context);
		var name = context.Identifier().GetText();
		Log.Debug("Found variable assignment ({Variable}, {Text})", name, context.GetText());

		if (!Target.Variables.ContainsKey(name))
		{
			Message("E7", false, null, name);
			return null;
		}

		var block = Target.AddBlock(Data.SetVariableTo(Target.Variables[name], null));

		Target.EnterAttachmentScope(new AttachInfo
		{
			ChildIsNext = false,
			To = block
		});

		if (!TryVisit(context.expression(), out var expression)) return null;
		AssertType(Target.Variables[name].Type, expression);

		Target.ExitAttachmentScope();
		ExitContext();
		return block;
	}

	public override object? VisitVariableDeclarationStatement(
		ScratchScriptParser.VariableDeclarationStatementContext context)
	{
		EnterContext(context);
		var name = context.Identifier().GetText();
		Log.Debug("Found variable declaration ({Name}, {Text})", name, context.GetText());

		if (Target.Variables.ContainsKey(name) && _contextStack.SkipLast(1).Last() is not ScratchScriptParser.IfStatementContext or ScratchScriptParser.ElseIfStatementContext)
		{
			Message("E3", false, null, name);
			return null;
		}

		Target.CreateVariable(name, "");
		var block = Target.AddBlock(Data.SetVariableTo(Target.Variables[name], null));
		Target.EnterAttachmentScope(new AttachInfo
		{
			ChildIsNext = false,
			To = block
		});
		if (!TryVisit(context.expression(), out var expression)) return null;

		Target.Variables[name].Type = GetExpectedType(expression);
		Target.Variables[name].Built = true;

		Target.ExitAttachmentScope();
		ExitContext();
		return block;
	}
}