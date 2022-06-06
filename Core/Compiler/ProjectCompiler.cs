using System.IO.Compression;
using Antlr4.Runtime;
using Newtonsoft.Json;
using RandomUserAgent;
using ScratchScript.Core.Visitor;
using ScratchScript.Helpers;
using ScratchScript.Wrapper;
using Serilog;
using Spectre.Console;

namespace ScratchScript.Core.Compiler;

public class ProjectCompiler
{
	public const string SupportedVmVersion = "0.2.0-prerelease.20220222132735";
	public static ProjectCompiler Current;
	private readonly List<TargetCompiler> _targetCompilers = new();

	private Asset _emptyCostume;
	public TargetCompiler CurrentTarget;

	public Project Project = new();
	public bool Success = true;
	public bool IgnoreErrors = false;

	public ProjectCompiler(string sourcePath, string outputPath)
	{
		SourcePath = sourcePath;
		OutputPath = outputPath;
		Current = this;
	}

	public string SourcePath { get; }
	public string FileName => Path.GetFileName(SourcePath);
	public string OutputPath { get; }
	public string TempDirectory { get; private set; }
	public int TargetCompilerCount => _targetCompilers.Count;

	public void Initialize()
	{
		Log.Debug("Creating temp folder");

		TempDirectory = Path.Join(Path.GetTempPath(), $"ScratchScript_{Guid.NewGuid():N}");
		Directory.CreateDirectory(TempDirectory);
		Log.Information("Temp directory is {TempDirectory}", TempDirectory);

		Log.Information("Initializing project with default data");
		Log.Debug("Setting metadata");
		Project.meta.agent = RandomUa.RandomUserAgent;
		Project.meta.vm = SupportedVmVersion;

		Log.Debug("Adding default sprites (Stage)");
		var emptyPath = EmptySpriteDrawer.Generate(TempDirectory);
		_emptyCostume = new Asset
		{
			assetId = Path.GetFileNameWithoutExtension(emptyPath),
			name = "ScratchScript_Empty",
			md5ext = Path.GetFileName(emptyPath),
			dataFormat = "png",
			rotationCenterX = 240,
			rotationCenterY = 180
		};

		//Stage
		CreateAndSwitchTargetCompiler(null);
	}

	public void CreateAndSwitchTargetCompiler(string? path)
	{
		var name = Path.GetFileNameWithoutExtension(path) ?? "Stage";
		_targetCompilers.Add(new TargetCompiler {Name = name});
		CurrentTarget = _targetCompilers.Last();
		CurrentTarget.AddEntryPoint();
	}

	public void SetCurrentTarget(string name)
	{
		CurrentTarget = _targetCompilers.First(x => x.Name == name);
	}

	public void Compile()
	{
		Log.Information("Compiling {Path}..", Path.GetFileName(SourcePath));

		Log.Debug("Preparing target compiler");
		CreateAndSwitchTargetCompiler(SourcePath);

		Log.Debug("Calling the lexer");
		var inputStream = new AntlrInputStream(File.ReadAllText(SourcePath));
		var lexer = new ScratchScriptLexer(inputStream);
		var tokenStream = new CommonTokenStream(lexer);
		if (!Success) Cancel();
		Log.Debug("Calling the parser");
		var parser = new ScratchScriptParser(tokenStream);
		parser.AddErrorListener(new ErrorListener());
		var program = parser.program();
		if (!Success) Cancel();
		Log.Debug("Calling the visitor");
		var visitor = new ScratchScriptVisitor();
		visitor.Visit(program);
		if (!Success) Cancel();

		Log.Debug("Combining targets");
		Project.targets = _targetCompilers.Select(x => x.WrappedTarget).ToList();

		Log.Debug("Fixing targets with no sprites");
		foreach (var target in Project.targets.Where(target => target.costumes.Count == 0))
			target.costumes.Add(_emptyCostume);
	}

	private void Cancel()
	{
		AnsiConsole.MarkupLine("[red]One or more errors have occured, aborting.[/]");
		Directory.Delete(TempDirectory, true);
		Environment.Exit(1);
	}

	public void Finish()
	{
		Log.Information("Finishing..");

		Log.Debug("Serializing project");
		File.WriteAllText(Path.Join(TempDirectory, "project.json"), JsonConvert.SerializeObject(Project,
			Formatting.Indented, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			}));

		File.WriteAllText("testing.json", JsonConvert.SerializeObject(Project, Formatting.Indented,
			new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			}));

		Log.Debug("Zipping project files");
		ZipFile.CreateFromDirectory(TempDirectory, OutputPath);

		Directory.Delete(TempDirectory, true);
	}
}