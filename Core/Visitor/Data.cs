using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ScratchScript.Blocks.Builders;
using ScratchScript.Core.Compiler;
using ScratchScript.Core.Types;

namespace ScratchScript.Core.Visitor;

public partial class ScratchScriptVisitor : ScratchScriptBaseVisitor<object?>
{
	private ParserRuleContext CurrentContext => _contextStack.LastOrDefault()!;
	private readonly List<ParserRuleContext> _contextStack = new();
	public void EnterContext(ParserRuleContext context) => _contextStack.Add(context);
	public void ExitContext() => _contextStack.RemoveAt(_contextStack.Count - 1);
	
	private TargetCompiler Target => Project.CurrentTarget;
	private ProjectCompiler Project => ProjectCompiler.Current;

	private CustomBlockBuilder _currentBuilder;

	private void Message(string id, bool highlightLine = false, IToken? customToken = null,
		params object[] formatObjects)
	{
		DiagnosticHandler.DefaultHandler(CurrentContext, customToken ?? CurrentContext.Start, id, highlightLine,
			formatObjects);
	}

	private bool TryVisit(IParseTree tree, out object result, bool ignoreNull = false)
	{
		result = null!;
		
		var visitResult = Visit(tree);
		if (visitResult is null && !ignoreNull)
		{
			Message("E2", true);
			return false;
		}

		if (ignoreNull) return true;

		result = visitResult!;
		Target.TryAssign(visitResult!);
		return true;
	}
}