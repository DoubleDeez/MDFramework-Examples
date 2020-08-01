using Godot;
using System;
using System.Collections.Generic;
using MD;

public enum MDTestPeerMode
{
    SERVER,
    NOT_CONNECTED_CLIENT,
    CLIENT1,
    CLIENT2
}

public class MDTestManager : Node
{
    private const string LOG_CAT = "LogTestManager";

    private const string CONFIG_CATEGORY = "AutomatedTests";
    private const string CONFIG_KEY_PORT = "Port";
    private const string CONFIG_KEY_HOST = "Host";

    private const float CLIENT_INITIAL_CONNECT_WAIT_TIME = 3f;
    private const int START_PLAYER_COUNT = 3;

    private MDTestPeerMode Mode;

    private TestConfigManager TestConfigManager;

    private MDGameSession GameSession;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GameSession = MDStatics.GetGameSession();
        CheckPeerMode();
        LoadConfigs();
        LoadTests();
    }

    private void CheckPeerMode()
    {
        String mode = MDArguments.GetArg("mode");
        if (mode == null)
        {
            MDLog.Fatal(LOG_CAT, "Mode not set");
            GetTree().Quit();
            return;
        }
        switch (mode.ToLower())
        {
            case "server":
                Mode = MDTestPeerMode.SERVER;
                break;
            case "client":
                Mode = MDTestPeerMode.NOT_CONNECTED_CLIENT;
                break;
            default:
                MDLog.Fatal(LOG_CAT, $"Unkown mode {mode}");
                GetTree().Quit();
                return;
        }
        CallDeferred(nameof(Start));
    }

    private void Start()
    {
        // This just sets up the initial connection
        switch (Mode)
        {
            case MDTestPeerMode.SERVER:
                GameSession.StartServer(GetPort());
                break;
            case MDTestPeerMode.NOT_CONNECTED_CLIENT:
                Timer timer = this.CreateUnpausableTimer("ConnectTimer", true, 
                                CLIENT_INITIAL_CONNECT_WAIT_TIME, true, this, nameof(ConnectToServer));
                timer.Start();
                break;
        }
    }

    public void ConnectToServer(Timer timer)
    {
        GameSession.StartClient(GetHost(), GetPort());
        timer.Stop();
        timer.RemoveAndFree();
    }

    private void LoadConfigs()
    {
        TestConfigManager = new TestConfigManager();
        if (TestConfigManager.GetConfigCount() == 0)
		{
			GetTree().Quit();
            return;
		}
    }

    private void LoadTests()
    {
        List<Type> TestList = MDStatics.FindAllScriptsImplementingAttribute<MDTestConfig>();
        if (TestList.Count == 0)
		{
			MDLog.Fatal(LOG_CAT, "No tests found");
			GetTree().Quit();
		}
		else
		{
			MDLog.Info(LOG_CAT, $"Found {TestList.Count} tests ({MDStatics.GetParametersAsString(TestList.ToArray())})");
		}
    }

    public static int GetPort()
    {
        return Convert.ToInt32(MDStatics.GetGameInstance().Configuration.GetValue(CONFIG_CATEGORY, CONFIG_KEY_PORT, "7777"));
    }

    public static string GetHost()
    {
        return Convert.ToString(MDStatics.GetGameInstance().Configuration.GetValue(CONFIG_CATEGORY, CONFIG_KEY_HOST, "127.0.0.1"));
    }
}
