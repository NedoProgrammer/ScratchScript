using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;

namespace ScratchScript.Core;

public class ErrorListener : BaseErrorListener
{
	public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
		string msg,
		RecognitionException e)
	{
		DiagnosticHandler.DefaultHandler(null, offendingSymbol, "E10", false, msg);
	}

	public override void ReportAmbiguity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, bool exact,
		BitSet ambigAlts,
		ATNConfigSet configs)
	{
		Console.WriteLine("Ambiguity");
	}

	public override void ReportAttemptingFullContext(Parser recognizer, DFA dfa, int startIndex, int stopIndex,
		BitSet conflictingAlts,
		SimulatorState conflictState)
	{
		Console.WriteLine("Attempting full context");
	}

	public override void ReportContextSensitivity(Parser recognizer, DFA dfa, int startIndex, int stopIndex,
		int prediction,
		SimulatorState acceptState)
	{
		Console.WriteLine("Context sensivity");
	}
}