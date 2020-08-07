using Godot;
using System;

[MDTestClass(false, "CustomGroup", false)]
public class TestFrameworkTest02 : Node
{
    public TestFrameworkTest02()
    {
        // Tests should have empty constructor for reflection
    }

    [MDTest]
    public void Test1()
    {
        String TestGroup = MDTestManager.Instance.TestConfigManager.GetCurrentConfig().TestGroup;
        if (TestGroup != "CustomGroup")
        {
            MDTestManager.LogError($"This test should only run for test group CustomGroup not for {TestGroup}");
        }
    }

    public void Test1Validate()
    {
        String TestGroup = MDTestManager.Instance.TestConfigManager.GetCurrentConfig().TestGroup;
        if (TestGroup != "CustomGroup")
        {
            MDTestManager.LogError($"This test should only run for test group CustomGroup not for {TestGroup}");
        }
    }
}