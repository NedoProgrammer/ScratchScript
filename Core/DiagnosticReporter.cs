using Antlr4.Runtime;
using ScratchScript.Compiler;
using Serilog;
using Spectre.Console;

namespace ScratchScript.Core;

public static class DiagnosticReporter
{
	public static void ReportError(IToken position, string id, int line = -1, int column = -1, string text = "", params object[] objects)
	{
		Log.Error("An error has been reported (Line {Line}, Position {Position}). Error ID: {ErrorId}", position.Line,
			position.Column, id);
		DefaultHandler("Error", "red", position, id, line, column, text, objects);
		ProjectCompiler.Current.Success = false;
	}

	public static void ReportWarning(IToken position, string id, int line = -1, int column = -1, string text = "", params object[] objects)
	{
		Log.Warning("A warning has been reported (Line {Line}, Position {Position}). Warning ID: {ErrorId}",
			position.Line, position.Column, id);
		DefaultHandler("Warning", "yellow", position, id, line, column, text, objects);
	}

	private static void DefaultHandler(string name, string color, IToken position, string id, int line, int column, string text, params object[] objects)
	{
		var project = ProjectCompiler.Current;
		var fileData = File.ReadAllLines(project.SourcePath);

		line = line == -1 ? position.Line : line;
		column = column == -1 ? position.Column : column;
		text = string.IsNullOrEmpty(text) ? position.Text : text;
		
		AnsiConsole.MarkupLine($"[{color}]{name}[/]: {string.Format(Strings.Messages[id], objects)}");
		AnsiConsole.WriteLine($" --> {project.FileName}:{line}:{column + 1}");
		var lineFormatted = $"{line} | ";
		AnsiConsole.WriteLine($"{lineFormatted}{fileData[line - 1].Trim()}");
		var underline = new string(' ', column + lineFormatted.Length) + new string('~', text.Length);
		AnsiConsole.MarkupLine($"[{color}]{underline}[/]");
		if (Strings.Notes.ContainsKey(id))
			AnsiConsole.WriteLine("note: " + Strings.Notes[id]);
		AnsiConsole.MarkupLine($"\nFor more information, try `[yellow bold]ScratchScript explain {id}[/]`.");
	}
}