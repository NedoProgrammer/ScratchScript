using ScratchScript.Compiler;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(x =>
{
	x.PropagateExceptions();
	x.AddCommand<BuildCommand>("build")
		.WithAlias("compile")
		.WithDescription("Converts a .scrs file into a .sb3 project.")
		.WithExample(new[] {"build", "test.scrs"})
		.WithExample(new[] {"build", "test.scrs", "--output", "project.sb3"});
});

try
{
	app.Run(args);
}
catch (CommandParseException exception)
{
	AnsiConsole.MarkupLine($"[bold red]{exception.Message}[/]");
	AnsiConsole.MarkupLine("Run ScratchScript --help for usage.");
	Environment.Exit(1);
}

internal class BuildCommand : Command<BuildCommand.BuildCommandSettings>
{
	public override int Execute(CommandContext context, BuildCommandSettings settings)
	{
		if (string.IsNullOrEmpty(settings.Path))
		{
			AnsiConsole.MarkupLine("[bold red]Please specify the source file![/]");
			return 1;
		}

		if (!File.Exists(settings.Path))
		{
			AnsiConsole.MarkupLine($"[bold red]Input file \"{settings.Path}\" does not exist![/]");
			return 1;
		}

		if (string.IsNullOrEmpty(settings.Output))
			settings.Output = Path.GetFileNameWithoutExtension(settings.Path) + ".sb3";

		if (File.Exists(settings.Output))
			File.Delete(settings.Output);

		Log.Logger = new LoggerConfiguration()
			.WriteTo.File($"log_{DateTime.Now:ddMMyyyyHHmm}.txt")
			.WriteTo.Conditional(_ => settings.LogToConsole, c => c.Console())
			#if DEBUG
			.MinimumLevel.Debug()
			#else
			.MinimumLevel.Information()
			#endif
			.CreateLogger();
		Log.Information("Logger initialized");
		Log.Information("Preparing to build \"{SourceFile}\"..", settings.Path);

		var compiler = new ProjectCompiler(settings.Path, settings.Output);

		compiler.Initialize();
		compiler.Compile();
		compiler.Finish();

		AnsiConsole.MarkupLine("[green]Done![/]");
		return 0;
	}

	public class BuildCommandSettings : CommandSettings
	{
		[CommandArgument(0, "[path]")] public string? Path { get; set; }

		[CommandOption("-o|--output")] public string? Output { get; set; }

		[CommandOption("-c|--console")] public bool LogToConsole { get; set; } = false;
	}
}
//https://www.cs.uic.edu/~i109/Notes/COperatorPrecedenceTable.pdf