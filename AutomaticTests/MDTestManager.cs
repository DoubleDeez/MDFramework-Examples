using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MD;

public enum MDTestPeer
{
    NOT_SET,
    SERVER,
    NOT_CONNECTED_CLIENT,
    CLIENT1,
    CLIENT2,
    ALL
}

public class MDTestErrorInfo
{
    public string TestPeer {get; private set;}
    public string TestConfig {get; private set;}
    public string TestClassName {get; private set;}
    public string TestName {get; private set;}
    public string Message {get; private set;}

    public MDTestErrorInfo(string TestPeer, string TestConfig, string TestClassName, string TestName, string Message)
    {
        this.TestPeer = TestPeer;
        this.TestConfig = TestConfig;
        this.TestClassName = TestClassName;
        this.TestName = TestName;
        this.Message = Message;
    }

    public string AsErrorString()
    {
        return $"{TestConfig} {TestClassName} [{TestName}] [{TestPeer}] {Message}";;
    }
}

public class PeerTestInfo
{
    private const string LOG_CAT = "LogPeerTestInfo";
    
    [MDReplicated]
    public MDTestPeer Mode {get; private set;}
    
    [MDReplicated]
    public int PeerId {get; private set;}
    
    [MDReplicated]
    public bool Ready {get; set;}

    public PeerTestInfo(int PeerId, int PeerNumber)
    {
        this.PeerId = PeerId;
        if (PeerNumber == 0)
        {
            // This is the server
            Mode = MDTestPeer.SERVER;
            Ready = true;
            return;
        }
        try
        {
            Mode = (MDTestPeer)Enum.Parse(typeof(MDTestPeer), $"CLIENT{PeerNumber}" );
        }
        catch
        {
            Mode = MDTestPeer.NOT_CONNECTED_CLIENT;
            MDLog.Fatal(LOG_CAT, $"PeerNumber {PeerNumber} is not valid and does not exist in MDTestPeerMode");
        }
    }
}

[MDAutoRegister]
public class MDTestManager : Node
{
    private const string LOG_CAT = "LogTestManager";
    public const string NODE_NAME = "NodeManager";

    private static MDTestManager _Instance = null;

    public static MDTestManager Instance
    {
        get
        {
            return _Instance;
        }
    }

    private const string CONFIG_CATEGORY = "AutomatedTests";
    private const string CONFIG_KEY_PORT = "Port";
    private const string CONFIG_KEY_HOST = "Host";

    public const string TEST_CONFIG_TIMER_METHOD = nameof(TestConfigTimerTimeout);

    private const float CLIENT_INITIAL_CONNECT_WAIT_TIME = 3f;
    public const int CLIENT_COUNT_INCLUDING_SERVER = 3;
    public const int CLIENT_COUNT = CLIENT_COUNT_INCLUDING_SERVER-1;

    private MDTestPeer _Mode = MDTestPeer.NOT_SET;
    public MDTestPeer Mode
    {
        get
        {
            return _Mode;
        }
        private set
        {
            _Mode = value;
            if (UIManager != null)
            {
                UIManager.SetMode(Mode.ToString());
            }
        }
    }

    public TestConfigManager TestConfigManager;

    public TestManager TestManager;

    private MDGameSession GameSession;

    private bool IsTestsInProgress = false;

    public TestUIManager UIManager = null;

    [MDReplicated]
    private MDList<PeerTestInfo> PeerList;

    private List<MDTestErrorInfo> ErrorList = new List<MDTestErrorInfo>();
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _Instance = this;
        GameSession = MDStatics.GetGameSession();
        GameSession.OnPlayerJoinedEvent += OnPlayerJoin;
        GameSession.OnPlayerLeftEvent += OnPlayerLeft;
        LoadConfiguration();
        CheckPeerMode();
        LoadConfigs();
        LoadTests();
    }

    public override void _ExitTree()
    {
        GameSession.OnPlayerJoinedEvent -= OnPlayerJoin;
        GameSession.OnPlayerLeftEvent -= OnPlayerLeft;
    }
#region TEST METHODS

    private void LoadNextConfig()
    {
        if (!TestConfigManager.LoadNextConfig())
        {
            // We are done with all configs
            UIManager.SetStatus($"Testing complete");
            if (ErrorList.Count == 0)
            {
                UIManager.Log("All tests completed successfully", Colors.Green);
            }
            else
            {
                UIManager.Log($"Tests completed with {ErrorList.Count} errors", Colors.Red);
            }
        }
    }

    protected void TestConfigTimerTimeout(Timer Timer, TestConfigCommands Command)
    {
        TestConfigManager.TimerTimeout(Timer, Command);
    }

    public void StartTests(string GroupName)
    {
        // Start testing
        UIManager.SetStatus($"Running tests with configuration {TestConfigManager.GetCurrentConfig().GetName()} on test group [{GroupName}]");
        TestManager.StartTests(GroupName);
    }

    public void TestsComplete()
    {
        // Tests are done try to load next config
        LoadNextConfig();
    }

#endregion
#region INITIAL NETWORK SETUP

    private void OnPlayerJoin(int PeerId)
    {
        // Do not run on clients or when clients reconnect
        if (!MDStatics.IsServer() || GetPeerTestInfo(PeerId) != null)
        {
            return;
        }

        PeerTestInfo pti = new PeerTestInfo(PeerId, PeerList.Count);
        if (pti.Mode == MDTestPeer.NOT_CONNECTED_CLIENT)
        {
            GetTree().Quit();
            return;
        }
        PeerList.Add(pti);

        if (PeerId != MDStatics.GetServerId())
        {
            this.MDRpcId(PeerId, nameof(ClientSetPeerMode), pti.Mode);
        }
    }

    private void OnPlayerLeft(int PeerId)
    {
        PeerTestInfo TestInfo = GetPeerTestInfo(PeerId);
        if (TestInfo != null)
        {
            PeerList.Remove(TestInfo);
        }
    }

    [Puppet]
    private void ClientSetPeerMode(MDTestPeer PeerMode)
    {
        Mode = PeerMode;
        MDLog.Info(LOG_CAT, $"Setting client mode to: {PeerMode.ToString()}");
        RpcId(this.MDGetRpcSenderId(), nameof(ServerAckClientIsReady));
    }

    [Master]
    private void ServerAckClientIsReady()
    {
        PeerTestInfo PeerTestInfo = GetPeerTestInfo(Multiplayer.GetRpcSenderId());
        PeerTestInfo.Ready = true;
        if (!IsTestsInProgress)
        {
            if (CLIENT_COUNT_INCLUDING_SERVER == PeerList.Count && (!PeerList.Any(pti => !pti.Ready)))
            {
                UIManager.SetStatus($"Running tests");
                IsTestsInProgress = true;
                LoadNextConfig();
            }
            else
            {
                UIManager.SetStatus($"Waiting for {CLIENT_COUNT_INCLUDING_SERVER - PeerList.Count} clients");
            }
        }
    }

    private void Start()
    {
        // This just sets up the initial connection
        switch (Mode)
        {
            case MDTestPeer.SERVER:
                StartServer();
                break;
            case MDTestPeer.NOT_CONNECTED_CLIENT:
                Timer timer = this.CreateUnpausableTimer("ConnectTimer", true, 
                                CLIENT_INITIAL_CONNECT_WAIT_TIME, true, this, nameof(ConnectToServer));
                timer.Start();
                break;
        }
    }

    public void ConnectToServer(Timer timer)
    {
        ConnectToServer();
        timer.Stop();
        timer.RemoveAndFree();
    }

    public void StartServer()
    {
        GameSession.StartServer(GetPort());
    }

    public void ConnectToServer()
    {
        GameSession.StartClient(GetHost(), GetPort());
    }

    public void Disconnect()
    {
        GameSession.Disconnect();
    }
#endregion

#region UTILITY METHODS

    private void LoadConfiguration()
    {
        if (!GameSession.GetConfiguration().LoadConfiguration("AutomaticTestConfig.ini"))
        {
            MDLog.Fatal(LOG_CAT, "Failed to load configuration");
            GetTree().Quit();
        }
    }

    public void SetUIManager(TestUIManager Manager)
    {
        UIManager = Manager;
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
                Mode = MDTestPeer.SERVER;
                break;
            case "client":
                Mode = MDTestPeer.NOT_CONNECTED_CLIENT;
                break;
            default:
                MDLog.Fatal(LOG_CAT, $"Unkown mode {mode}");
                GetTree().Quit();
                return;
        }
        CallDeferred(nameof(Start));
    }

    private void LoadConfigs()
    {
        TestConfigManager = new TestConfigManager(this);
        if (TestConfigManager.GetConfigCount() == 0)
        {
            GetTree().Quit();
            return;
        }
        TestConfigManager.Name = TestConfigManager.NODE_NAME;
        this.AddChild(TestConfigManager);
    }

    private void LoadTests()
    {
        TestManager = new TestManager(this);
        if (TestManager.GetConfigCount() == 0)
        {
            GetTree().Quit();
            return;
        }
        TestManager.Name = TestManager.NODE_NAME;
        this.AddChild(TestManager);
    }

    public PeerTestInfo GetPeerTestInfo(int PeerId)
    {
        try
        {
            return PeerList.Single(pti => pti.PeerId == PeerId);
        }
        catch
        {
            return null;
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

#endregion

#region ERROR LOGGING
    public static void LogError(string Message)
    {
        Instance.MDServerRpc(nameof(LogErrorServer), Message);
    }

    [Master]
    public void LogErrorServer(string Message)
    {
        PeerTestInfo PeerTestInfo = GetPeerTestInfo(this.MDGetRpcSenderId());
        string TestConfig = TestConfigManager.GetCurrentConfig().GetName();
        string TestClassName = TestManager.GetCurrentTestClass().GetName();
        string TestName = TestManager.GetCurrentTest().TestMethod.Name;
        MDTestErrorInfo info = new MDTestErrorInfo(PeerTestInfo.Mode.ToString(), TestConfig, TestClassName, TestName, Message);
        ErrorList.Add(info);
        MDLog.Error(LOG_CAT, info.AsErrorString());
        UIManager.Log(info.AsErrorString(), Colors.Red);
    }

#endregion
}
