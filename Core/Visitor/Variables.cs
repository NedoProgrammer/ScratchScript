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
		Target.TryAssign(expression);
		AssertType(GetExpectedType(expression), Target.Variables[name]);

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

		if (Target.Variables.ContainsKey(name))
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
		Target.TryAssign(expression);
		Target.Variables[name].Type = GetExpectedType(expression);

		Target.ExitAttachmentScope();
		ExitContext();
		return block;
	}
}