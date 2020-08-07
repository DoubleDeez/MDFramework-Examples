using Godot;
using System;
using System.Collections.Generic;
using MD;

/// <summary>
/// Inherit from this class when creating your tests.
/// </summary>
public abstract class AutomaticTestBase : Node2D
{
    public AutomaticTestBase()
    {

    }

    protected void AddError(string text)
    {
        MDTestManager.LogError(text);
    }

    protected void LogError(string text)
    {
        MDTestManager.LogError(text);
    }
}
