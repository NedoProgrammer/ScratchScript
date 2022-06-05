using ScratchScript.Blocks;
using ScratchScript.Wrapper;
using Serilog;

namespace ScratchScript.Core.Visitor;

public partial class ScratchScriptVisitor
{
	public override object? VisitBinaryBooleanExpression(ScratchScriptParser.BinaryBooleanExpressionContext context)
	{
		_currentContext = context;
		Log.Debug("Found a binary boolean expression ({Text})", context.GetText());

		var block = Target.AddBlock(context.booleanOperators().GetText() switch
		{
			"&&" => Operators.And(null, null),
			"||" => Operators.Or(null, null)
		});

		Target.EnterAttachmentScope(new AttachInfo
		{
			ChildIsNext = false,
			To = block
		});

		if (!TryVisit(context.expression(0), out var first)) return null;
		if (!TryVisit(context.expression(1), out var second)) return null;

		AssertType(first, typeof(bool));
		AssertType(second, typeof(bool));

		Target.ExitAttachmentScope();

		return block;
	}

	public override object? VisitBinaryCompareExpression(ScratchScriptParser.BinaryCompareExpressionContext context)
	{
		_currentContext = context;
		Log.Debug("Found a binary compare expression ({Text})", context.GetText());

		var op = context.compareOperators().GetText();

		Block block;
		Block attachTo;
		Block? complexOperator = null, equal = null;
		switch (op)
		{
			case "==":
				block = Operators.Equals(null, null);
				attachTo = block;
				break;
			case ">":
				block = Operators.GreaterThan(null, null);
				attachTo = block;
				break;
			case "<":
				block = Operators.LessThan(null, null);
				attachTo = block;
				break;
			case ">=":
			{
				complexOperator = Operators.GreaterThan(null, null);
				equal = Operators.Equals(null, null);
				block = Operators.Or(complexOperator, equal);
				attachTo = complexOperator;
				break;
			}
			case "<=":
			{
				complexOperator = Operators.LessThan(null, null);
				equal = Operators.Equals(null, null);
				block = Operators.Or(complexOperator, equal);
				attachTo = complexOperator;
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
		}

		block = Target.AddBlock(block);

		Target.EnterAttachmentScope(new AttachInfo
		{
			ChildIsNext = false,
			To = attachTo
		});

		if (!TryVisit(context.expression(0), out var first)) return null;
		if (!TryVisit(context.expression(1), out var second)) return null;
		Target.TryAssign(first);
		Target.TryAssign(second);

		if (equal != null && complexOperator != null)
		{
			equal.inputs = complexOperator.inputs;
			Target.UpdateBlock(equal);
		}

		Target.ExitAttachmentScope();

		return block;
	}

	public override object? VisitBinaryMultiplyExpression(ScratchScriptParser.BinaryMultiplyExpressionContext context)
	{
		return base.VisitBinaryMultiplyExpression(context);
	}

	public override object? VisitBinaryAddExpression(ScratchScriptParser.BinaryAddExpressionContext context)
	{
		_currentContext = context;
		Log.Debug("Found a binary add expression ({Text})", context.GetText());

		var block = Target.AddBlock(context.addOperators().GetText() switch
		{
			"+" => Operators.Add(null, null),
			"-" => Operators.Subtract(null, null)
		});

		Target.EnterAttachmentScope(new AttachInfo
		{
			ChildIsNext = false,
			To = block
		});

		if (!TryVisit(context.expression(0), out var first)) return null;
		if (!TryVisit(context.expression(1), out var second)) return null;
		Target.TryAssign(first);
		Target.TryAssign(second);

		AssertType(first, typeof(decimal));
		AssertType(second, typeof(decimal));
		AssertType(block, typeof(decimal));

		Target.ExitAttachmentScope();

		return block;
	}
}