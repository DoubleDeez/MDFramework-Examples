using Godot;
using System;

// All modes respects MDTestPeer to decide where to run
public enum MDTestMode
{
    Test, // Normal test, executed as a test, looks for validation method after
    SetupClass, // Ran before all tests
    BeforeTest, // Run before every MDTestMode.Test
    AfterTest, // Run after every MDTestMode.Test
    ShutdownClass // Run after all tests are complete
}

public enum MDTestSettings
{
    Validate, // Set which client to validate on
    // Example: [MDTestSetting(MDTestSettings.Validate, MDTestPeer.CLIENT1, nameof(ValidationMethod))]
    DevelopmentMode // If set to true we only run this test
    // Example: [MDTestSetting(MDTestSettings.DevelopmentMode, true)]
}

[AttributeUsage(AttributeTargets.Class)]
public class MDTestClass : Attribute
{
    public const string TEST_GROUP_ALL = "All";
    public bool DevelopmentMode { private set; get; }
    public string TestGroup { private set; get; }
    public bool AlwaysRunInAllMode { private set; get; }

    public MDTestClass(bool DevelopmentMode = false, string TestGroup = TEST_GROUP_ALL, bool AlwaysRunInAllMode = false)
    {
        this.DevelopmentMode = DevelopmentMode;
        this.TestGroup = TestGroup;
        this.AlwaysRunInAllMode = AlwaysRunInAllMode;
    }

    public MDTestClass(bool DevelopmentMode)
    {
        this.DevelopmentMode = DevelopmentMode;
        this.TestGroup = TEST_GROUP_ALL;
        this.AlwaysRunInAllMode = false;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class MDTest : Attribute 
{
    public MDTestPeer MDTestHost { private set; get; }
    public MDTestMode TestMode { private set; get; }

    public MDTest(MDTestPeer MDTestHost = MDTestPeer.SERVER, MDTestMode TestMode = MDTestMode.Test)
    {
        this.MDTestHost = MDTestHost;
        this.TestMode = TestMode;
    }

    public MDTest(MDTestPeer MDTestHost)
    {
        this.MDTestHost = MDTestHost;
        this.TestMode = MDTestMode.Test;
    }

    public MDTest(MDTestMode TestMode)
    {
        this.MDTestHost = MDTestPeer.ALL;
        this.TestMode = TestMode;
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MDTestSetting : Attribute 
{
    public MDTestSettings Setting { private set; get; }
    public object Key { private set; get; }
    public object Value { private set; get; }

    public MDTestSetting(MDTestSettings Setting, object Key, object Value)
    {
        this.Setting = Setting;
        this.Key = Key;
        this.Value = Value;
    }

    public MDTestSetting(MDTestSettings Setting, object Value)
    {
        this.Setting = Setting;
        this.Value = Value;
    }
}