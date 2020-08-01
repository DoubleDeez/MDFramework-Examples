using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using MD;

internal class TestConfig
{
    List<MethodInfo> BeforeConnect;
    List<MethodInfo> AfterConnect;
    List<MethodInfo> BeforeDisconnect;
    List<MethodInfo> AfterDisconnect;

    public Type Type {get; private set;}

    public TestConfig(Type Type)
    {
        this.Type = Type;
        LoadAttributedMethods();
    }

    private void LoadAttributedMethods()
    {
        BeforeConnect = Type.GetAllMethodsWithAttribute<MDTestConfigBeforeConnect>();
        AfterConnect = Type.GetAllMethodsWithAttribute<MDTestConfigAfterConnect>();
        BeforeDisconnect = Type.GetAllMethodsWithAttribute<MDTestConfigBeforeDisconnect>();
        AfterDisconnect = Type.GetAllMethodsWithAttribute<MDTestConfigAfterDisconnect>();
    }

}

public class TestConfigManager
{
    private const string LOG_CAT = "LogTestConfigManager";

    Queue<TestConfig> TestConfigs = new Queue<TestConfig>();

    public TestConfigManager()
    {
        // Load our config files
        List<Type> ConfigList = MDStatics.FindAllScriptsImplementingAttribute<MDTestConfig>();
        if (ConfigList.Count == 0)
		{
			MDLog.Fatal(LOG_CAT, "No configs found");
            return;
		}
		
        MDLog.Info(LOG_CAT, $"Found {ConfigList.Count} configs ({MDStatics.GetParametersAsString(ConfigList.ToArray())})");
        ConfigList.ForEach(config => TestConfigs.Enqueue(new TestConfig(config)));
    }

    public int GetConfigCount()
    {
        return TestConfigs.Count;
    }
}
