using Godot;
using System;
using MD;

[MDAutoRegister]
public class AutoTestsMain : Node2D
{
	[MDBindNode("CanvasLayer/GridContainer/LabelMode")]
	private Label ModeLabel = null;

	[MDBindNode("CanvasLayer/GridContainer/LabelStatus")]
	private Label StatusLabel = null;

	[MDBindNode("CanvasLayer/TxtLog")]
	private TxtLog LogText = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CallDeferred(nameof(CreateTestManager));
	}

	private void CreateTestManager()
	{
		TestUIManager manager = new TestUIManager(LogText, ModeLabel, StatusLabel);
		MDTestManager TestManager = Activator.CreateInstance(typeof(MDTestManager)) as MDTestManager;
		TestManager.Name = "TestManager";
		TestManager.SetUIManager(manager);
		GetTree().Root.AddChild(TestManager);
	}
}
