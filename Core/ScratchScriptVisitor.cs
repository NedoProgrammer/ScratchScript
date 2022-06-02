using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using ScratchScript.Blocks;
using ScratchScript.Blocks.Builders;
using ScratchScript.Compiler;
using ScratchScript.Extensions;
using ScratchScript.Types;
using ScratchScript.Wrapper;
using Serilog;
using Serilog.Configuration;

namespace ScratchScript.Core;

public class ScratchScriptVisitor : ScratchScriptBaseVisitor<object?>
{
	private readonly Dictionary<string, Type> _expectedType = new();
	private bool _isStage;
	private string _context;
	private bool _conditional;
	private CustomBlockBuilder? _currentBuilder;
	private ScratchCustomBlock? _currentFunction;

	private bool HasFunctionArgument(string name) => _currentBuilder != null && _currentBuilder.Arguments.ContainsKey(name);

	public override object? VisitAttributeStatement(ScratchScriptParser.AttributeStatementContext context)
	{
		Log.Debug("Found an attribute");

		var target = ProjectCompiler.Current.CurrentTarget;
		if (target.WrappedTarget.blocks.Count > 1 || target.Variables.Count != 0)
		{
			DiagnosticReporter.ReportError(context.Start, "E4", -1, -1, context.GetText());
			return null;
		}

		var attribute = context.Identifier().GetText();
		switch (attribute)
		{
			case "stage":
				Log.Debug("Switching to stage sprite");
				_isStage = true;
				ProjectCompiler.Current.SetCurrentTarget("Stage");
				break;
		}

		return null;
	}

	public override object? VisitVariableDeclarationStatement(
		ScratchScriptParser.VariableDeclarationStatementContext context)
	{
		var name = context.Identifier().GetText();
		var expression = context.expression();
		Log.Debug("Found variable declaration ({VariableName}, Expression ID {ExpressionNumber})", name,
			expression.RuleIndex);
		var target = ProjectCompiler.Current.CurrentTarget;

		if (target.Variables.ContainsKey(name) && _context != "Conditional")
		{
			DiagnosticReporter.ReportError(context.Identifier().Symbol, "E3", -1, -1, "", name);
			return null;
		}

		var last = target.WrappedTarget.blocks.Last(x => !x.Value.shadow).Key;
		var expressionResult = Visit(expression);
		if (expressionResult == null)
		{
			DiagnosticReporter.ReportError(context.Start, "E2", -1, -1, context.GetText());
			return null;
		}

		if (expressionResult is not Block expressionBlock)
		{
			target.CreateVariable(name, GetDefaultValue(expressionResult.GetType()));
			var block = target.CreateBlock(Data.SetVariableTo(target.Variables[name], expressionResult));
			block.parent = last;
			target.ReplaceBlock(block);
			return block;
		}

		Log.Debug("Found shadow in variable declaration ({ShadowId}). Creating SetTo block", expressionBlock.Id);
		if (!_expectedType.ContainsKey(expressionBlock.Id))
		{
			DiagnosticReporter.ReportWarning(expression.Start, "W5", -1, -1, "", name);
			target.CreateVariable(name, "");
		}

		var type = _expectedType[expressionBlock.Id];
		target.CreateVariable(name, GetDefaultValue(type));

		var setBlock = target.CreateBlock(Data.SetVariableTo(target.Variables[name], expressionBlock), true, true);
		AttachShadow(setBlock, expressionBlock, last);
		return setBlock;
	}

	private object GetDefaultValue(Type type)
	{
		object? defaultValue;
		if (type == typeof(string)) defaultValue = "";
		else if (type == typeof(bool)) defaultValue = "false";
		else defaultValue = Activator.CreateInstance(type);
		if (defaultValue == null)
			throw new Exception($"Cannot get default value for type {type.Name}.");
		Log.Debug("Default value for type {Type} is {Value}", type.Name, defaultValue);
		return defaultValue;
	}

	private Type GetExpectedType(object obj)
	{
		var type = obj switch
		{
			ScratchVariable variable => variable.Type,
			Block block => _expectedType[block.Id],
			_ => null
		};
		
		return type ?? GetExpectedInternalType(obj);
	}

	public static Type GetExpectedInternalType(object obj)
	{
		var type = decimal.TryParse(obj.ToString(), out _) ? typeof(decimal) : obj.GetType();
		return type;
	}

	public static Type TypeFromString(string name)
	{
		return name switch
		{
			"bool" => typeof(bool),
			"number" => typeof(decimal),
			"color" => typeof(ScratchColor),
			"string" => typeof(string),
			_ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
		};
	}


	public override object? VisitParenthesizedExpression(ScratchScriptParser.ParenthesizedExpressionContext context)
	{
		Log.Debug("Found parenthesized expression ({Text})", context.GetText());
		return Visit(context.expression());
	}

	private Dictionary<string, Type> _contextToType = new()
	{
		{"Unary", typeof(decimal)},
		{"BinaryPlus", typeof(decimal)},
		{"BinaryPlusString", typeof(string)},
		{"BinaryMultiply", typeof(decimal)},
		{"BinaryBoolean", typeof(bool)},
	};

	public override object? VisitIdentifierExpression(ScratchScriptParser.IdentifierExpressionContext context)
	{
		var identifier = context.GetText();
		Log.Debug("Found identifier ({Text})", identifier);
		var target = ProjectCompiler.Current.CurrentTarget;
		if (target.Variables.ContainsKey(identifier))
			return target.Variables[identifier];
		if (_currentBuilder != null && HasFunctionArgument(identifier))
		{
			if (_currentBuilder.Arguments[identifier] == typeof(void))
			{
				if (_contextToType.ContainsKey(_context))
					_currentBuilder.Arguments[identifier] = _contextToType[_context];
				else if (_conditional)
					_currentBuilder.Arguments[identifier] = typeof(bool);
				else
				{
					DiagnosticReporter.ReportWarning(context.Start, "W19", -1, -1, "", identifier);
					_currentBuilder.Arguments[identifier] = typeof(string);		
				}
			}
			var arg = _currentBuilder.Arguments[identifier];
			var block = target.CreateBlock(arg == typeof(bool) ?
				CustomBlocks.ReporterBoolean(identifier)
				: CustomBlocks.ReporterStringNumber(identifier));
			_expectedType[block.Id] = arg;
			return block;
		}

		DiagnosticReporter.ReportError(context.Start, "E9", -1, -1, "", identifier);
		return null;
	}

	public override object? VisitConstant(ScratchScriptParser.ConstantContext context)
	{
		Log.Debug("Found constant ({Text})", context.GetText());

		if (context.String() is { } s)
			return s.GetText()[1..^1];
		
		if (context.Number() is { } d)
			return decimal.Parse(d.GetText());

		if (context.Boolean() is { } b)
			return b.GetText() == "true";

		return null;
	}

	public override object? VisitConstantExpression([NotNull] ScratchScriptParser.ConstantExpressionContext context)
	{
		return VisitConstant(context.constant());
	}

	public override object? VisitBinaryBooleanExpression(
		[NotNull] ScratchScriptParser.BinaryBooleanExpressionContext context)
	{
		Log.Debug("Found a &&/|| expression");
		_context = "BinaryBoolean";

		var first = Visit(context.expression(0));
		var second = Visit(context.expression(1));

		if (first is not null && second is not null)
			return context.booleanOperators().GetText() switch
			{
				"&&" => HandleBinaryOperation(context.Start, first, second, Operators.And(first, second), typeof(bool)),
				"||" => HandleBinaryOperation(context.Start, first, second, Operators.Or(first, second), typeof(bool)),
				_ => null
			};
		DiagnosticReporter.ReportError(context.Start, "E2", -1, -1, context.GetText());
		return null;
	}

	public override object? VisitFunctionCallExpression(
		[NotNull] ScratchScriptParser.FunctionCallExpressionContext context)
	{
		return base.VisitFunctionCallExpression(context);
	}

	public override object? VisitNotExpression([NotNull] ScratchScriptParser.NotExpressionContext context)
	{
		return base.VisitNotExpression(context);
	}

	public override object? VisitUnaryAddExpression(ScratchScriptParser.UnaryAddExpressionContext context)
	{
		Log.Debug("Found a unary +/- expression");
		_context = "Unary";
		var expressionResult = Visit(context.expression());
		if (expressionResult is null)
		{
			DiagnosticReporter.ReportError(context.Start, "E2", -1, -1, context.GetText());
			return null;
		}

		var op = context.addOperators().GetText();
		var type = GetExpectedType(expressionResult);
		return HandleBinaryOperation(context.addOperators().Start, op, expressionResult,
			Operators.Join(op, expressionResult), type);
	}

	public override object? VisitBinaryCompareExpression(
		[NotNull] ScratchScriptParser.BinaryCompareExpressionContext context)
	{
		Log.Debug("Found a > / < / == / >= / <= expression");
		_context = "BinaryBoolean";
		
		var first = Visit(context.expression(0));
		var second = Visit(context.expression(1));
		if (first is null || second is null)
		{
			DiagnosticReporter.ReportError(context.Start, "E2", -1, -1, context.GetText());
			return null;
		}

		var position = context.compareOperators().Start;
		return context.compareOperators().GetText() switch
		{
			"==" => HandleBinaryOperation(position, first, second, Operators.Equals(first, second), typeof(bool)),
			">" => HandleBinaryOperation(position, first, second, Operators.GreaterThan(first, second), typeof(bool)),
			"<" => HandleBinaryOperation(position, first, second, Operators.LessThan(first, second), typeof(bool)),
			">=" => throw new NotImplementedException("Currently not implemented."),
			"<=" => throw new NotImplementedException("Currently not implemented."),
			_ => null
		};
	}

	public override object? VisitFunctionCallStatement(
		[NotNull] ScratchScriptParser.FunctionCallStatementContext context)
	{
		return base.VisitFunctionCallStatement(context);
	}

	public override object? VisitIfStatement([NotNull] ScratchScriptParser.IfStatementContext context)
	{
		Log.Debug("Found an if statement (Expression ID {Id})", context.expression().RuleIndex);
		_conditional = true;
		var target = ProjectCompiler.Current.CurrentTarget;
		var last = target.WrappedTarget.blocks.Last().Key;
		var expressionResult = Visit(context.expression());
		if (expressionResult is null)
		{
			DiagnosticReporter.ReportError(context.Start, "E2", -1, -1, context.GetText());
			return null;
		}

		if (expressionResult is not Block expressionBlock)
		{
			DiagnosticReporter.ReportError(context.Start, "E11", -1, -1, context.GetText(), "Block",
				expressionResult.GetType().Name);
			return null;
		}
		
		var ifBlock = target.CreateBlock(context.elseIfStatement() == null
			? Control.If(expressionBlock)
			: Control.IfElse(expressionBlock));

		ifBlock.parent = last;
		expressionBlock.parent = ifBlock.Id;
		target.ReplaceBlock(expressionBlock);
		target.ReplaceBlock(ifBlock);

		var lines = context.block().line().Select(Visit).Where(x => x != null).ToList();
		var mainBlocks = new List<Block>();
		for (var i = 0; i < lines.Count; i++)
		{
			var line = lines[i];
			ifBlock.next = null;
			if (line is null) continue;
			if (line is not Block)
			{
				DiagnosticReporter.ReportError(context.Start, "E11", context.block().line(i).Start.Line,
					context.block().line(i).Start.Column, context.block().line(i).GetText(), "Block",
					line.GetType().Name);
				return null;
			}
			
			var block = (line as Block)!;
			mainBlocks.Add(block);
			if (i == 0)
			{
				ifBlock = new BlockBuilder(ifBlock)
					.WithInput(new InputBuilder()
						.WithName("SUBSTACK")
						.WithShadow(block, ShadowMode.Shadow));
				block.parent = ifBlock.Id;
				target.ReplaceBlock(block);
				target.ReplaceBlock(ifBlock);
			}
			else
			{
				block.parent = mainBlocks[i - 1].Id;
				mainBlocks[i - 1].next = block.Id;
				target.ReplaceBlock(block);
				target.ReplaceBlock(mainBlocks[i - 1]);
			}

			if (i == lines.Count - 1)
			{
				block.next = null;
				target.ReplaceBlock(block);
			}
		}

		if (context.elseIfStatement() != null)
		{
			var blocksResult = Visit(context.elseIfStatement());
			if (blocksResult is not List<Block> blocks)
			{
				DiagnosticReporter.ReportError(context.Start, "E11", -1, -1, context.GetText(), "List<Block>",
					blocksResult == null ? "null" : blocksResult.GetType().Name);
				return null;
			}

			for (var i = 0; i < blocks.Count; i++)
			{
				var block = blocks[i];
				if (i == 0)
				{
					ifBlock = new BlockBuilder(ifBlock)
						.WithInput(new InputBuilder()
							.WithName("SUBSTACK2")
							.WithShadow(block, ShadowMode.Shadow));
					block.parent = ifBlock.Id;
					target.ReplaceBlock(block);
					target.ReplaceBlock(ifBlock);
				}
				else
				{
					block.parent = blocks[i - 1].Id;
					blocks[i - 1].next = block.Id;
					target.ReplaceBlock(block);
					target.ReplaceBlock(blocks[i - 1]);
				}

				if (i == lines.Count - 1)
				{
					block.next = null;
					target.ReplaceBlock(block);
				}
			}
		}

		_conditional = false;
		return ifBlock;
	}

	public override object? VisitWhileStatement([NotNull] ScratchScriptParser.WhileStatementContext context)
	{
		return base.VisitWhileStatement(context);
	}

	public override object? VisitElseIfStatement([NotNull] ScratchScriptParser.ElseIfStatementContext context)
	{
		Log.Debug("Found an else/if statement");
		if (context.block() != null)
		{
			var lines = context.block().line().Select(Visit).Where(x => x != null).ToList();
			for (var i = 0; i < lines.Count; i++)
			{
				var line = lines[i];
				if (line is null or Block) continue;
				DiagnosticReporter.ReportError(context.Start, "E11", context.block().line(i).Start.Line,
					context.block().line(i).Start.Column, context.block().line(i).GetText(), "Block",
					line.GetType().Name);
				return null;
			}

			return lines.Select(x => (Block) x!).ToList();
		}

		if (context.ifStatement() != null)
		{
			var ifResult = Visit(context.ifStatement());
			if (ifResult is not Block or null)
			{
				DiagnosticReporter.ReportError(context.Start, "E11", context.ifStatement().Start.Line,
					context.ifStatement().Start.Column, context.ifStatement().GetText(), "Block",
					ifResult == null ? "null" : ifResult.GetType().Name);
				return null;
			}

			return new List<Block> {(ifResult as Block)!};
		}

		return null;
	}

	public override object? VisitImportStatement([NotNull] ScratchScriptParser.ImportStatementContext context)
	{
		return base.VisitImportStatement(context);
	}

	public override object? VisitBinaryMultiplyExpression(
		[NotNull] ScratchScriptParser.BinaryMultiplyExpressionContext context)
	{
		Log.Debug("Found */(**)% binary expression");
		_context = "BinaryMultiply";
		var first = Visit(context.expression(0));
		var second = Visit(context.expression(1));

		if (first is null || second is null)
		{
			DiagnosticReporter.ReportError(context.Start, "E2", -1, -1, context.GetText());
			return null;
		}

		var position = context.multiplyOperators().Start;
		switch (context.multiplyOperators().GetText())
		{
			case "*":
				return HandleBinaryOperation(position, first, second, Operators.Multiply(first, second), typeof(decimal));
			case "/":
				if (first is 0 || second is 0)
					DiagnosticReporter.ReportWarning(
						first is 0 ? context.expression(0).Start : context.expression(1).Start, "W1");
				return HandleBinaryOperation(position, first, second, Operators.Divide(first, second), typeof(decimal));
			case "**":
				throw new NotImplementedException("Currently not implemented.");
			case "%":
				return HandleBinaryOperation(position, first, second, Operators.Modulo(first, second), typeof(decimal));
		}

		return null;
	}


	public override object? VisitBinaryAddExpression([NotNull] ScratchScriptParser.BinaryAddExpressionContext context)
	{
		Log.Debug("Found +- binary expression");
		_context = "BinaryPlus";
		var first = Visit(context.expression(0));
		var second = Visit(context.expression(1));

		if (first is null || second is null)
		{
			DiagnosticReporter.ReportError(context.Start, "E2", -1, -1, context.GetText());
			return null;
		}

		var position = context.addOperators().Start;
		switch (context.addOperators().GetText())
		{
			case "+":
				//TODO: join strings (which is a separate block)
				return HandleBinaryOperation(position, first, second, Operators.Add(first, second), typeof(decimal));
			case "-":
				return HandleBinaryOperation(position, first, second, Operators.Subtract(first, second), typeof(decimal));
		}

		return null;
	}

	private object HandleBinaryOperation(IToken position, object? first, object? second, Block operatorBlock,
		Type? expectedType = null)
	{
		var target = ProjectCompiler.Current.CurrentTarget;
		var block = target.CreateBlock(operatorBlock, true, true);
		block.next = null;
		if (expectedType == null)
			DiagnosticReporter.ReportWarning(position, "W6");
		else _expectedType[block.Id] = expectedType;
		if (first is Block firstBlock)
		{
			firstBlock.parent = block.Id;
			target.ReplaceBlock(block);
			target.ReplaceBlock(firstBlock);
		}
		if (second is Block secondBlock)
		{
			secondBlock.parent = block.Id;
			target.ReplaceBlock(block);
			target.ReplaceBlock(secondBlock);
		}

		return block;
	}

	public override object? VisitAssignmentStatement([NotNull] ScratchScriptParser.AssignmentStatementContext context)
	{
		var name = context.Identifier().GetText();
		Log.Debug("Found variable assignment ({Name}, Expression ID {Value})", name, context.expression().RuleIndex);
		var target = ProjectCompiler.Current.CurrentTarget;

		if (!target.Variables.ContainsKey(name))
		{
			DiagnosticReporter.ReportError(context.Identifier().Symbol, "E7");
			return null;
		}

		var variable = target.Variables[name];
		var value = Visit(context.expression());
		var last = target.WrappedTarget.blocks.Last(x => !x.Value.shadow).Key;
		if (value == null)
		{
			DiagnosticReporter.ReportError(context.Start, "E2", -1, -1, context.GetText());
			return null;
		}

		if (GetExpectedType(variable) != GetExpectedType(value))
		{
			DiagnosticReporter.ReportError(context.assignmentOperators().Start, "E8", -1, -1, "",
				GetExpectedType(value).Name,
				GetExpectedType(variable).Name);
			return null;
		}

		var op = context.assignmentOperators().GetText();

		var shadow = op switch
		{
			"=" => null,
			"+=" => Operators.Add(variable, value),
			"-=" => Operators.Subtract(variable, value),
			"*=" => Operators.Multiply(variable, value),
			"/=" => Operators.Divide(variable, value),
			"%=" => Operators.Modulo(variable, value),
			_ => null!
		};

		if (shadow is null && value is Block block)
			shadow = block;
		if (shadow is not null)
		{
			var setBlock = target.CreateBlock(Data.SetVariableTo(variable, shadow));
			AttachShadow(setBlock, shadow, last);
			return setBlock;
		}
		else
		{
			var setBlock = target.CreateBlock(Data.SetVariableTo(variable, value));
			return setBlock;
		}
	}

	private void AttachShadow(Block main, Block shadow, string blockBeforeMain)
	{
		var target = ProjectCompiler.Current.CurrentTarget;
		shadow.parent = main.Id;
		main.parent = blockBeforeMain;
		target.WrappedTarget.blocks[blockBeforeMain].next = main.Id;
		target.ReplaceBlock(main);
		target.ReplaceBlock(shadow);
	}

	public override object? VisitMultiplyOperators([NotNull] ScratchScriptParser.MultiplyOperatorsContext context)
	{
		Log.Debug("Found */(**)% arithmetic operator");
		return context.GetText();
	}

	public override object? VisitAddOperators([NotNull] ScratchScriptParser.AddOperatorsContext context)
	{
		Log.Debug("Found +- arithmetic operator");
		return context.GetText();
	}

	public override object? VisitCompareOperators([NotNull] ScratchScriptParser.CompareOperatorsContext context)
	{
		Log.Debug("Found == != > >= < <= boolean operator");
		return context.GetText();
	}

	public override object? VisitBooleanOperators([NotNull] ScratchScriptParser.BooleanOperatorsContext context)
	{
		Log.Debug("Found && || ^ boolean operator");
		return context.GetText();
	}

	public override object? VisitComment(ScratchScriptParser.CommentContext context)
	{
		var target = ProjectCompiler.Current.CurrentTarget;
		var last = target.WrappedTarget.blocks.Last(x => !x.Value.shadow).Value;
		Log.Debug("Found a comment. Attaching to next block after {LastBlockId}", last.Id);
		var text = context.GetText().EndsWith("*/") ? context.GetText()[2..^2] : context.GetText()[2..];
		var comment = new Comment
		{
			minimized = false,
			text = text,
			width = text.Length * 15,
			height = 75,
			x = -100,
			y = -100
		};
		var id = BlockExtensions.RandomId("Comment");
		target.PendingComment = id;
		target.WrappedTarget.comments[id] = comment;
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

	public override object? VisitAssignmentOperators(ScratchScriptParser.AssignmentOperatorsContext context)
	{
		Log.Debug("Found an = += -= *= /= operator");
		return context.GetText();
	}

	public override object? VisitBlock(ScratchScriptParser.BlockContext context)
	{
		return context.children.Select(Visit).ToList();
	}

	public override object? VisitFunctionDeclarationStatement(ScratchScriptParser.FunctionDeclarationStatementContext context)
	{
		var target = ProjectCompiler.Current.CurrentTarget;

		if (!string.IsNullOrEmpty(_context))
		{
			DiagnosticReporter.ReportError(context.FirstParentOfType<ScratchScriptParser.LineContext>().Start, "E16", -1, -1, context.GetText());
			return null;
		}

		var name = context.Identifier(0).GetText();
		if (target.Variables.ContainsKey(name))
		{
			DiagnosticReporter.ReportError(context.Identifier(0).Symbol, "E12", -1, -1, "", name);
			return null;
		}

		if (target.Functions.ContainsKey(name))
		{
			DiagnosticReporter.ReportError(context.Identifier(0).Symbol, "E13");
			return null;
		}

		var last = target.WrappedTarget.blocks.Last(x => !x.Value.shadow).Value;
		var id = BlockExtensions.RandomId($"Function_{name}_{target.Name}");
		Log.Debug("Found function declaration ({Name}, {Id})", name, id);
		
		_currentBuilder = new CustomBlockBuilder()
			.WithName(name)
			.WithId(id);
		
		_context = "Function " + id;


		Log.Debug("Adding arguments");
		var arguments = context.Identifier().Skip(1).ToList();

		foreach (var argument in arguments)
		{
			if (argument.GetText() == name)
			{
				DiagnosticReporter.ReportError(argument.Symbol, "E14");
				return null;
			}

			if (target.Variables.ContainsKey(argument.GetText()))
			{
				DiagnosticReporter.ReportError(argument.Symbol, "E15");
				return null;
			}

			_currentBuilder = _currentBuilder.WithArgument(argument.GetText(), typeof(void));
		}

		Log.Debug("Parsing function body");
		var lines = context.block().line().Select(Visit).Where(x => x != null).ToList();
		if (_currentBuilder.ReturnType == null)
			_currentBuilder = _currentBuilder.WithReturnType(typeof(void));
		_currentFunction = _currentBuilder.Build();

		for (var i = 0; i < lines.Count; i++)
		{
			var line = lines[i];
			if (line is not Block)
			{
				DiagnosticReporter.ReportError(context.Start, "E11", context.block().line(i).Start.Line,
					context.block().line(i).Start.Column, context.block().line(i).GetText(), "Block",
					line!.GetType().Name);
				return null;
			}

			var block = (line as Block)!;
			if (i == 0)
			{
				_currentFunction.Definition.next = block.Id;
				block.parent = _currentFunction.Definition.Id;
				
				target.ReplaceBlock(_currentFunction.Definition);
				target.ReplaceBlock(block);
			}
			else
			{
				var previous = (lines[i - 1] as Block)!;
				block.parent = previous.Id;
				previous.next = block.Id;
				target.ReplaceBlock(block);
				target.ReplaceBlock(previous);
			}

			if (i == lines.Count - 1)
			{
				block.next = null;
				target.ReplaceBlock(block);
			}
		}

		last.next = null;
		target.ReplaceBlock(last);


		target.Functions[name] = _currentFunction;
		_currentBuilder = null;
		_currentFunction = null;
		return null;
	}

	public override object? VisitReturnStatement(ScratchScriptParser.ReturnStatementContext context)
	{
		Log.Debug("Found a return statement");
		var target = ProjectCompiler.Current.CurrentTarget;

		if (_currentBuilder == null)
		{
			var line = context.FirstParentOfType<ScratchScriptParser.LineContext>();
			DiagnosticReporter.ReportError(line.Start, "E17", -1, -1, line.GetText());
			return null;
		}
		
		var expressionResult = Visit(context.expression());

		if (expressionResult is null)
		{
			DiagnosticReporter.ReportError(context.Start, "E2", -1, -1, context.GetText());
			return null;
		}

		if (_currentBuilder.ReturnType == null)
		{
			var expectedType = typeof(void);
			switch (expressionResult)
			{
				case Block shadow:
					if (!_expectedType.ContainsKey(shadow.Id))
						DiagnosticReporter.ReportWarning(context.Start, "W18", -1, -1, context.GetText());
					else expectedType = _expectedType[shadow.Id];
					break;
				case ScratchVariable variable:
					expectedType = variable.Type;
					break;
				default:
					expectedType = GetExpectedInternalType(expressionResult);
					break;
			}

			if (expectedType == typeof(void))
				DiagnosticReporter.ReportWarning(context.Start, "W18", -1, -1, context.GetText());

			_currentBuilder = _currentBuilder.WithReturnType(expectedType);
		}

		var name = $"{_currentBuilder.Name}_{target.Name}_ReturnValue";
		if (_currentBuilder.ReturnType != typeof(void) && string.IsNullOrEmpty(_currentBuilder.ReturnVariable))
		{
			target.CreateVariable(name, GetDefaultValue(_currentBuilder.ReturnType!));
			_currentBuilder.ReturnVariable = name;
		}

		var setValue = target.CreateBlock(Data.SetVariableTo(target.Variables[name], expressionResult));
		if (expressionResult is Block expressionShadow)
		{
			setValue.next = null;
			expressionShadow.parent = setValue.Id;
			expressionShadow.next = null;
			target.ReplaceBlock(setValue);
			target.ReplaceBlock(expressionShadow);
		}

		return setValue;
	}
}