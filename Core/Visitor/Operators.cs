namespace ScratchScript.Core.Visitor;

public partial class ScratchScriptVisitor
{
	public override object? VisitMultiplyOperators(ScratchScriptParser.MultiplyOperatorsContext context)
	{
		return context.GetText();
	}

	public override object? VisitAddOperators(ScratchScriptParser.AddOperatorsContext context)
	{
		return context.GetText();
	}

	public override object? VisitCompareOperators(ScratchScriptParser.CompareOperatorsContext context)
	{
		return context.GetText();
	}

	public override object? VisitBooleanOperators(ScratchScriptParser.BooleanOperatorsContext context)
	{
		return context.GetText();
	}

	public override object? VisitAssignmentOperators(ScratchScriptParser.AssignmentOperatorsContext context)
	{
		return context.GetText();
	}
}