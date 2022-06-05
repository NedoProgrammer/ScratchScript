using Antlr4.Runtime;
using ScratchScript.Core.Compiler;
using ScratchScript.Extensions;
using Spectre.Console;

namespace ScratchScript.Core;

public static class DiagnosticHandler
{
	public static void DefaultHandler(ParserRuleContext? context, IToken offendingSymbol, string id,
		bool highlightLine = false, params object[] formatParameters)
	{
		var project = ProjectCompiler.Current;
		var fileData = File.ReadAllLines(project.SourcePath);
		var line = offendingSymbol.Line;
		var column = highlightLine ? 0 : offendingSymbol.Column;
		var text = highlightLine && context != null
			? context.FirstParentOfType<ScratchScriptParser.LineContext>().GetText()
			: offendingSymbol.Text;
		var name = id.StartsWith("E") ? "Error" : "Warning";
		var color = id.StartsWith("E") ? "red" : "yellow";

		if (name == "Error")
			project.Success = false;

		AnsiConsole.MarkupLine($"[{color}]{name}[/]: {string.Format(Strings.Messages[id], formatParameters)}");
		AnsiConsole.WriteLine($" --> {project.FileName}:{line}:{column + 1}");
		var lineFormatted = $"{line} | ";
		AnsiConsole.WriteLine($"{lineFormatted}{fileData[line - 1].Trim()}");
		var underline = new string(' ', lineFormatted.Length + column) + new string('~', text.Length);
		AnsiConsole.MarkupLine($"[{color}]{underline}[/]");
		if (Strings.Notes.ContainsKey(id))
			AnsiConsole.WriteLine($"note: {Strings.Notes[id]}");
		AnsiConsole.MarkupLine($"For more information, try `[yellow bold]ScratchScript explain {id}[/]`");
	}
}