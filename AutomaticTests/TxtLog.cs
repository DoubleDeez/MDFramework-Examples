using Godot;
using MD;

public class TxtLog : RichTextLabel
{
    private const string LOG_CAT = "LogAutomaticTest";

    public override void _Ready()
    {
        MDLog.AddLogCategoryProperties(LOG_CAT, new MDLogProperties(MDLogLevel.Trace));
    }

    public void LogNoLineBreak(string Text, Color Color)
    {
        PushColor(Color);
        AddText(Text);
        Pop();

        if (Color == Colors.Red)
        {
            MDLog.Error(LOG_CAT, Text);
        }
        else
        {
            MDLog.Info(LOG_CAT, Text);
        }
    }

    public void Log(string Text, Color Color)
    {
        PushColor(Color);
        AddText(Text);
        Pop();
        AddText("\n");

        if (Color == Colors.Red)
        {
            MDLog.Error(LOG_CAT, Text);
        }
        else
        {
            MDLog.Info(LOG_CAT, Text);
        }
    }

    public void Log(string Text)
    {
        Log(Text, Colors.White);
    }

}
