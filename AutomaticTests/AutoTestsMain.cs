using Godot;
using System;

public class AutoTestsMain : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CallDeferred(nameof(CreateTestManager));
	}

	private void CreateTestManager()
	{
		MDTestManager TestManager = Activator.CreateInstance(typeof(MDTestManager)) as MDTestManager;
		TestManager.Name = "TestManager";
		GetTree().Root.AddChild(TestManager);
	}
}
