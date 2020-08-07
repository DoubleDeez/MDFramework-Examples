using Godot;
using System;
using MD;

public class TestUIManager
{
    private const string LOG_CAT = "LogTestUIManager";
	private Label ModeLabel = null;

	private Label StatusLabel = null;

	private TxtLog LogText = null;

    public TestUIManager(TxtLog LogText, Label ModeLabel, Label StatusLabel)
    {
        this.LogText = LogText;
        this.ModeLabel = ModeLabel;
        this.StatusLabel = StatusLabel;
        LogText.ScrollFollowing = true;
    }

	public void SetStatus(string Status)
	{
		StatusLabel.Text = Status;
		Log($"{Status}", Colors.Purple);
	}

    public void SetStatus(string Status, Color Color)
	{
		StatusLabel.Text = Status;
		Log($"{Status}", Color);
	}

	public void SetMode(String Mode)
	{
		Log($"Mode set to: {Mode}", Colors.HotPink);
		ModeLabel.Text = Mode;
	}

	public void Log(String Text)
	{
		Log(Text, Colors.White);
	}

	public void Log(String Text, Color Color)
	{
		LogText.Log(Text, Color);
	}

	public void LogNoLineBreak(String Text, Color Color)
	{
		LogText.LogNoLineBreak(Text, Color);
	}
}
