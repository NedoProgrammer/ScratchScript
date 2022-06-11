using ScratchScript.Wrapper;
using Serilog;

namespace ScratchScript.Core.Visitor;

public partial class ScratchScriptVisitor
{
	public override object VisitComment(ScratchScriptParser.CommentContext context)
	{
		EnterContext(context);
		Log.Debug("Found a comment");
		
		var text = context.GetText().EndsWith("*/") ? context.GetText()[2..^2] : context.GetText()[2..];
		var comment = new Comment
		{
			minimized = false,
			text = text,
			height = 200,
			width = 200
		};
		Target.AddComment(comment);
		
		ExitContext();
		return comment;
	}
}