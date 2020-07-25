using Godot;
using System;
using System.Collections.Generic;

public class TxtLog : RichTextLabel
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

    public void LogNoLineBreak(string Text, Color Color)
    {
        PushColor(Color);
        AddText(Text);
        Pop();
    }

    public void Log(string Text, Color Color)
    {
        PushColor(Color);
        AddText(Text);
        Pop();
        AddText("\n");
    }

    public void Log(string Text)
    {
        Log(Text, Colors.White);
    }

}
