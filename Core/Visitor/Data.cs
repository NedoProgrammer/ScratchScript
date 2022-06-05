using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ScratchScript.Core.Compiler;

namespace ScratchScript.Core.Visitor;

public partial class ScratchScriptVisitor : ScratchScriptBaseVisitor<object?>
{
	private ParserRuleContext _currentContext;
	private TargetCompiler Target => Project.CurrentTarget;
	private ProjectCompiler Project => ProjectCompiler.Current;

	private void Message(string id, bool highlightLine = false, IToken? customToken = null,
		params object[] formatObjects)
	{
		DiagnosticHandler.DefaultHandler(_currentContext, customToken ?? _currentContext.Start, id, highlightLine,
			formatObjects);
	}

	private bool TryVisit(IParseTree tree, out object result)
	{
		var visitResult = Visit(tree);
		if (visitResult is null)
		{
			Message("E2", true);
			result = null!;
			return false;
		}

		result = visitResult;
		return true;
	}
}