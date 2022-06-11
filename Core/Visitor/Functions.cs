using System.Linq.Expressions;
using System.Text;
using ScratchScript.Blocks.Builders;
using ScratchScript.Extensions;
using ScratchScript.Wrapper;
using Serilog;

namespace ScratchScript.Core.Visitor;

public partial class ScratchScriptVisitor
{
	public override object? VisitFunctionCallExpression(ScratchScriptParser.FunctionCallExpressionContext context)
	{
		return Visit(context.functionCallStatement());
	}

	public override object? VisitFunctionCallStatement(ScratchScriptParser.FunctionCallStatementContext context)
	{
		return base.VisitFunctionCallStatement(context);
	}

	public override object? VisitFunctionDeclarationStatement(ScratchScriptParser.FunctionDeclarationStatementContext context)
	{
		EnterContext(context);
		
		var name = context.Identifier(0).GetText();

		if (Target.Variables.ContainsKey(name))
		{
			Message("E12", false, context.Identifier(0).Symbol, name);
			return null;
		}

		if (Target.Functions.ContainsKey(name))
		{
			Message("E13", false, context.Identifier(0).Symbol);
			return null;
		}

		var id = BlockExtensions.RandomId($"Function_{Target.Name}_{name}");
		Log.Debug("Found function declaration {Name}", name);
		
		_currentBuilder = new CustomBlockBuilder()
			.WithName(name)
			.WithId(id);
		
		Log.Debug("Adding parameters");
		var arguments = context.Identifier().Skip(1).ToList();

		foreach (var argument in arguments)
		{
			var argumentName = argument.GetText();
			if (argumentName == name)
			{
				Message("E14", false, argument.Symbol);
				return null;
			}

			if (Target.Variables.ContainsKey(argumentName))
			{
				Message("E15", false, argument.Symbol);
				return null;
			}

			_currentBuilder = _currentBuilder.WithArgument(argumentName, typeof(string), true);
		}
		
		Log.Debug("Parsing function body");
		Target.EnterAttachmentScope(new AttachInfo
		{
			To = null,
			ChildIsNext = true
		});

		Block? first = null;
		foreach (var line in context.block().line())
		{
			if (!TryVisit(line, out var result, true)) return null;
			if (result is not Block resultBlock || first is not null) continue;
			first = resultBlock;
			Target.ExitAttachmentScope();
			Target.EnterAttachmentScope(new AttachInfo
			{
				To = first,
				ChildIsNext = true
			});
		}

		Target.ExitAttachmentScope();
		var function = _currentBuilder.Build();
		if (first != null)
		{
			function.Definition.next = first.Id;
			first.parent = function.Definition.Id;
			Target.UpdateBlock(function.Definition);
			Target.UpdateBlock(first);
		}
		
		ExitContext();
		_currentBuilder = null;

		return null;
	}
	
	private bool HasFunctionArgument(string name) => _currentBuilder.Arguments.Any(a => a.Key == name);
}