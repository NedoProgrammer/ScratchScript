using Serilog;

namespace ScratchScript.Core.Visitor;

public partial class ScratchScriptVisitor
{
	public override object? VisitParenthesizedExpression(ScratchScriptParser.ParenthesizedExpressionContext context)
	{
		Log.Debug("Found parenthesized expression ({Text})", context.GetText());
		return Visit(context.expression());
	}

	public override object? VisitConstantExpression(ScratchScriptParser.ConstantExpressionContext context)
	{
		return Visit(context.constant());
	}

	public override object? VisitConstant(ScratchScriptParser.ConstantContext context)
	{
		Log.Debug("Found constant ({Text})", context.GetText());

		if (context.String() is { } s)
			return s.GetText()[1..^1];

		if (context.Number() is { } n)
			return decimal.Parse(n.GetText());

		if (context.boolean() is { } b)
			return b.GetText() == "true";

		return null;
	}

	public override object? VisitIdentifierExpression(ScratchScriptParser.IdentifierExpressionContext context)
	{
		var identifier = context.GetText();
		Log.Debug("Found identifier ({Text})", identifier);
		
		if(Target.Variables.ContainsKey(identifier) && Target.Variables[identifier].Built)
			return Target.Variables[identifier];
		
		Message("E9", false, null, identifier);
		return null;
	}

	public override object? VisitLine(ScratchScriptParser.LineContext context)
	{
		if (context.statement() != null)
			return Visit(context.statement());

		if (context.ifStatement() != null)
			return Visit(context.ifStatement());

		if (context.whileStatement() != null)
			return Visit(context.whileStatement());

		if (context.functionDeclarationStatement() != null)
			return Visit(context.functionDeclarationStatement());

		if (context.attributeStatement() != null)
			return Visit(context.attributeStatement());

		if (context.comment() != null)
			return Visit(context.comment());

		return null;
	}

	public override object? VisitStatement(ScratchScriptParser.StatementContext context)
	{
		if (context.assignmentStatement() != null)
			return Visit(context.assignmentStatement());

		if (context.importStatement() != null)
			return Visit(context.importStatement());

		if (context.returnStatement() != null)
			return Visit(context.returnStatement());

		if (context.functionCallStatement() != null)
			return Visit(context.functionCallStatement());

		if (context.variableDeclarationStatement() != null)
			return Visit(context.variableDeclarationStatement());

		return null;
	}

	public override object? VisitBlock(ScratchScriptParser.BlockContext context)
	{
		return context.children.Select(Visit).ToList();
	}
}