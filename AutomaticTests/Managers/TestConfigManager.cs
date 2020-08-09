using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using MD;

public class TestConfig
{
    List<MethodInfo> BeforeConnect;
    List<MethodInfo> AfterConnect;
    List<MethodInfo> BeforeDisconnect;
    List<MethodInfo> AfterDisconnect;

    public Type Type {get; private set;}
    public object Instance {get; private set;}
    public string TestGroup {get; private set;}
    public bool DevelopmentMode {get; private set;}
    public int Repeat {get; private set;}
    public int RunNumber = 0;

    public TestConfig(Type Type)
    {
        MDTestConfig TestConf = MDStatics.FindClassAttribute<MDTestConfig>(Type);
        this.Type = Type;
        this.Instance = Activator.CreateInstance(Type);
        this.TestGroup = TestConf.TestGroup;
        this.DevelopmentMode = TestConf.DevelopmentMode;
        this.Repeat = TestConf.Repeat;
        LoadAttributedMethods();
    }

    private void LoadAttributedMethods()
    {
        BeforeConnect = Type.GetAllMethodsWithAttribute<MDTestConfigBeforeConnect>();
        AfterConnect = Type.GetAllMethodsWithAttribute<MDTestConfigAfterConnect>();
        BeforeDisconnect = Type.GetAllMethodsWithAttribute<MDTestConfigBeforeDisconnect>();
        AfterDisconnect = Type.GetAllMethodsWithAttribute<MDTestConfigAfterDisconnect>();
    }

    public void InvokeBeforeDisconnect()
    {
        BeforeDisconnect.ForEach(method => method.Invoke(Instance, null));
    }

    public void InvokeAfterDisconnect()
    {
        AfterDisconnect.ForEach(method => method.Invoke(Instance, null));
    }

    public void InvokeBeforeConnect()
    {
        BeforeConnect.ForEach(method => method.Invoke(Instance, null));
    }

    public void InvokeAfterConnect()
    {
        AfterConnect.ForEach(method => method.Invoke(Instance, null));
    }

    public string GetName()
    {
        return $"[{Type.Name}]";
    }

}

public enum TestConfigCommands
{
    INIT_CONFIG,    // Sent when we start a new config run, causes before disconnect to be run on current config.
                    // Then disconnect happens, run after disconnect, before connect, then wait and reconnect. Then after connect is run
    ACK_INIT_CONFIG, // Sent by clients right before they disconnect
    DISCONNECT_TIMER_TIMEOUT, // Used for disconnect timer
    RECONNECT_TIMER_TIMEOUT, // Used for reconnect timer
    RECONNECT_MESSAGE_TIMER_TIMEOUT, // Used to make sure we send reconnect message after we connected
    READY_WITH_NEW_CONFIG // Sent by clients after they have connected and completed all invokes
}

public class TestConfigManager : Node
{
    private const string LOG_CAT = "LogTestConfigManager";

    public const string NODE_NAME = "TestConfigManager";

    private const float DISCONNECT_DELAY = 0.3f;

    private const float RECONNECT_DELAY = 0.5f;

    private const float RECONNECT_MESSAGE_DELAY = 0.1f;

    List<TestConfig> TestConfigs = new List<TestConfig>();

    private int CurrentConfig = -1;

    private MDTestManager TestManager;

    private int NextConfig = -1;

    private int SignalsRecieved = 0;

    public TestConfigManager(MDTestManager TestManager)
    {
        this.TestManager = TestManager;

        // Load our config files
        List<Type> ConfigList = MDStatics.FindAllScriptsImplementingAttribute<MDTestConfig>();
        if (ConfigList.Count == 0)
        {
            MDLog.Fatal(LOG_CAT, "No configs found");
            return;
        }
        
        MDLog.Trace(LOG_CAT, $"Found {ConfigList.Count} configs ({MDStatics.GetParametersAsString(ConfigList.ToArray())})");
        ConfigList.ForEach(config => TestConfigs.Add(new TestConfig(config)));
    }

    public int GetConfigCount()
    {
        return TestConfigs.Count;
    }

    /// <summary>
    /// Loads the next config and starts the reconnect cycle
    /// </summary>
    /// <returns>True if we got a next config, false if we are done</returns>
    public bool LoadNextConfig()
    {
        if (GetCurrentConfig() != null)
        {
            if (GetCurrentConfig().RunNumber < GetCurrentConfig().Repeat)
            {
                // Do another run
                RunConfig(GetCurrentConfig());
                return true;
            }
        }

        if (TestConfigs.Count > CurrentConfig+1)
        {
            RunConfig(TestConfigs[CurrentConfig+1]);
            return true;
        }
        return false;
    }

    public TestConfig GetCurrentConfig()
    {
        if (CurrentConfig < 0 || CurrentConfig >= TestConfigs.Count)
        {
            return null;
        }
        return TestConfigs[CurrentConfig];
    }

    private TestConfig GetNextConfig()
    {
        if (NextConfig < 0 || NextConfig >= TestConfigs.Count)
        {
            return null;
        }
        return TestConfigs[NextConfig];
    }

    private void ShutdownCurrentConfig()
    {
        
    }

    private void InvokeBeforeDisconnect()
    {
         if (GetCurrentConfig() != null)
        {
            MDLog.Info(LOG_CAT, $"Invoking before disconnect on config {GetCurrentConfig().GetName()}");
            GetCurrentConfig().InvokeBeforeDisconnect();
        }
    }

    private void InvokeAfterDisconnect()
    {
         if (GetCurrentConfig() != null)
        {
            MDLog.Info(LOG_CAT, $"Invoking after disconnect on config {GetCurrentConfig().GetName()}");
            GetCurrentConfig().InvokeAfterDisconnect();
        }
    }

    /// <summary>
    /// Called on the server, STEP 1 of the reconnect cycle
    /// </summary>
    /// <param name="Config">The next config to load</param>
    private void RunConfig(TestConfig Config)
    {
        InvokeBeforeDisconnect();

        // Set the current config
        NextConfig = TestConfigs.IndexOf(Config);
        GetNextConfig().RunNumber++;
        LogAndSetStatus($"Telling clients to run config {GetNextConfig().GetName()}, run number {GetNextConfig().RunNumber}/{GetNextConfig().Repeat}");

        // Send other clients signal to invoke before disconnect
        SignalsRecieved = 0;
        this.MDRpc(nameof(SendTestConfigManagerMessage), TestConfigCommands.INIT_CONFIG, NextConfig);
    }

    /// <summary>
    /// Called on the client, STEP 2 of reconnect cycle
    /// </summary>
    /// <param name="ConfigIndex">The index of the config to load</param>
    private void ClientRunConfig(int ConfigIndex)
    {
        InvokeBeforeDisconnect();
        NextConfig = ConfigIndex;
        this.MDServerRpc(nameof(SendTestConfigManagerMessage), TestConfigCommands.ACK_INIT_CONFIG, ConfigIndex);
        Timer timer = TestManager.CreateTimer("DisconnectTimer", true, DISCONNECT_DELAY, 
                        true, TestManager, MDTestManager.TEST_CONFIG_TIMER_METHOD, TestConfigCommands.DISCONNECT_TIMER_TIMEOUT);
        timer.Start();
        LogAndSetStatus($"Preparing to change to configuration {GetNextConfig().GetName()}");
    }

    /// <summary>
    /// Called on client, STEP 3 of disconnect cycle
    /// </summary>
    private void Disconnect()
    {
        // Disconnect from the server
        TestManager.Disconnect();

        InvokeAfterDisconnect();
        CurrentConfig = NextConfig;
        NextConfig = -1;
        GetCurrentConfig().InvokeBeforeConnect();
        Timer timer = TestManager.CreateTimer("ReconnectTimer", true, RECONNECT_DELAY, 
                        true, TestManager, MDTestManager.TEST_CONFIG_TIMER_METHOD, TestConfigCommands.RECONNECT_TIMER_TIMEOUT);
        timer.Start();
        LogAndSetStatus($"Disconnected for config {GetCurrentConfig().GetName()}");
    }

    /// <summary>
    /// Called on client, STEP 3.1 of disconnect cycle
    /// </summary>
    private void Reconnect()
    {
        // Reconnect
        TestManager.ConnectToServer();
        
        GetCurrentConfig().InvokeAfterConnect();
        LogAndSetStatus($"Reconnected for config {GetCurrentConfig().GetName()}");

        Timer timer = TestManager.CreateTimer("ReconnectMessageTimer", false, RECONNECT_MESSAGE_DELAY, 
                        true, TestManager, MDTestManager.TEST_CONFIG_TIMER_METHOD, TestConfigCommands.RECONNECT_MESSAGE_TIMER_TIMEOUT);
        timer.Start();
    }

    /// <summary>
    /// Called on client, STEP 3.2 of reconnect cycle
    /// </summary>
    private void SendReconnectMessage(Timer timer)
    {
        if (MDStatics.IsNetworkActive())
        {
            timer.Stop();
            timer.QueueFree();
            // Notify server that we are ready
            this.MDServerRpc(nameof(SendTestConfigManagerMessage), TestConfigCommands.READY_WITH_NEW_CONFIG, CurrentConfig);
        }
    }

    /// <summary>
    /// Called on Server, STEP 4 of reconnect cycle
    /// </summary>
    private void ServerDisconnect()
    {
        TestManager.Disconnect();
        InvokeAfterDisconnect();
        CurrentConfig = NextConfig;
        NextConfig = -1;
        GetCurrentConfig().InvokeBeforeConnect();

        TestManager.StartServer();
        GetCurrentConfig().InvokeAfterConnect();
        LogAndSetStatus($"Started server again with config {GetCurrentConfig().GetName()}");
        SignalsRecieved = 0;
    }

    /// <summary>
    /// Called on server, FINAL STEP of reconnect cycle
    /// </summary>
    private void StartTests()
    {
        TestManager.StartTests(GetCurrentConfig().TestGroup);
    }

    private void LogAndSetStatus(string Message)
    {
        MDLog.Info(LOG_CAT, Message);
        TestManager.UIManager.SetStatus(Message);
    }

    public void TimerTimeout(Timer timer, TestConfigCommands Command)
    {
        switch (Command)
        {
            case TestConfigCommands.DISCONNECT_TIMER_TIMEOUT:
                timer.Stop();
                timer.RemoveAndFree();
                Disconnect();
                return;
            case TestConfigCommands.RECONNECT_TIMER_TIMEOUT:
                timer.Stop();
                timer.RemoveAndFree();
                Reconnect();
                return;
            case TestConfigCommands.RECONNECT_MESSAGE_TIMER_TIMEOUT:
                SendReconnectMessage(timer);
                return;
        }
    }

    public void MessageRecieved(PeerTestInfo PeerInfo, TestConfigCommands Command, int ConfigNumber)
    {
        SignalsRecieved++;
        switch (Command)
        {
            case TestConfigCommands.INIT_CONFIG:
                ClientRunConfig(ConfigNumber);
                break;
            case TestConfigCommands.ACK_INIT_CONFIG:
                if (SignalsRecieved == MDTestManager.CLIENT_COUNT)
                {
                    ServerDisconnect();
                }
                break;
            case TestConfigCommands.READY_WITH_NEW_CONFIG:
                if (SignalsRecieved == MDTestManager.CLIENT_COUNT)
                {
                    StartTests();
                }
                break;
        }
    }

    [Remote]
    public void SendTestConfigManagerMessage(TestConfigCommands Command, int ConfigNumber)
    {
        MDLog.Info(LOG_CAT, $"[{this.MDGetRpcSenderId()}] sent us the command {Command.ToString()} with index {ConfigNumber}");
        MessageRecieved(TestManager.GetPeerTestInfo(this.MDGetRpcSenderId()), Command, ConfigNumber);
    }
}
