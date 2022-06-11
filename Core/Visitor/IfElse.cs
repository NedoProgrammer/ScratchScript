using System.Data;
using Antlr4.Runtime;
using ScratchScript.Blocks;
using ScratchScript.Blocks.Builders;
using ScratchScript.Wrapper;
using Serilog;

namespace ScratchScript.Core.Visitor;

public partial class ScratchScriptVisitor
{
	public override object? VisitIfStatement(ScratchScriptParser.IfStatementContext context)
	{
		EnterContext(context);
		Log.Debug("Found if statement ({Condition})", context.expression().GetText());

		var block = Target.AddBlock(context.elseIfStatement() == null ? Control.If(null): Control.IfElse(null));
		
		Target.EnterAttachmentScope(new AttachInfo
		{
			To = block,
			ChildIsNext = false
		});
		if (!TryVisit(context.expression(), out var condition)) return null;
		
		AssertType(typeof(bool), condition);
		
		Target.ExitAttachmentScope();
		
		Target.EnterAttachmentScope(new AttachInfo
		{
			To = block,
			ChildIsNext = true
		});
		
		var lines = context.block().line() as RuleContext[];
		var lastMain = AppendLines(block, lines);

		if (context.elseIfStatement() != null)
		{
			if (!TryVisit(context.elseIfStatement(), out var elseLines)) return null;
			lines = (elseLines as RuleContext[])!;
			var lastElse = AppendLines(block, lines, "SUBSTACK2");
			if (lastElse != null)
			{
				lastElse.next = null;
				Target.UpdateBlock(lastElse);
			}
		}
		
		//refresh parents and next blocks
		block.next = null;
		Target.UpdateBlock(block);
		if(lastMain != null)
		{
			lastMain.next = null;
			Target.UpdateBlock(lastMain);
		}

		Target.ExitAttachmentScope();
		ExitContext();
		return block;
	}

	public override object? VisitElseIfStatement(ScratchScriptParser.ElseIfStatementContext context)
	{
		EnterContext(context);
		Log.Debug("Found an else if statement");

		ExitContext();
		if (context.ifStatement() != null)
			return new RuleContext[] {context.ifStatement()};
		if(context.block() != null)
			return context.block().line();

		return Array.Empty<RuleContext>();
	}

	private void AddSubstack(Block ifBlock, Block first, string name)
	{
		//i hate how scratch handles if conditions...
		Target.UpdateBlock(new BlockBuilder(ifBlock)
			.WithInput(new InputBuilder()
				.WithName(name)
				.WithObject(first, ShadowMode.NoShadow)));
		first.parent = ifBlock.parent;
		Target.UpdateBlock(first);
	}

	private Block? AppendLines(Block attachTo, IEnumerable<RuleContext> lines, string substackName = "SUBSTACK")
	{
		var blocks = new List<Block>();
		foreach (var line in lines)
		{
			if (!TryVisit(line, out var result)) return null;
			if (result is Block resultBlock)
			{
				if (blocks.Count == 0)
					AddSubstack(attachTo, resultBlock, substackName);
				blocks.Add(resultBlock);
			}
		}

		return blocks.LastOrDefault();
	}
}