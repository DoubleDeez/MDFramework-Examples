using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MD;

public enum MDTestPeerMode
{
    NOT_SET,
    SERVER,
    NOT_CONNECTED_CLIENT,
    CLIENT1,
    CLIENT2
}

public class PeerTestInfo
{
    private const string LOG_CAT = "LogPeerTestInfo";
    public MDTestPeerMode Mode {get; private set;}
    public int PeerId {get; private set;}
    public bool Ready {get; set;}

    public PeerTestInfo(int PeerId, int PeerNumber)
    {
        this.PeerId = PeerId;
        try
        {
            Mode = (MDTestPeerMode)Enum.Parse(typeof(MDTestPeerMode), $"CLIENT{PeerNumber}" );
        }
        catch
        {
            Mode = MDTestPeerMode.NOT_CONNECTED_CLIENT;
            MDLog.Fatal(LOG_CAT, $"PeerNumber {PeerNumber} is not valid and does not exist in MDTestPeerMode");
        }
    }
}

public class MDTestManager : Node
{
    private const string LOG_CAT = "LogTestManager";

    private const string CONFIG_CATEGORY = "AutomatedTests";
    private const string CONFIG_KEY_PORT = "Port";
    private const string CONFIG_KEY_HOST = "Host";

    private const float CLIENT_INITIAL_CONNECT_WAIT_TIME = 3f;
    private const int START_CLIENT_COUNT = 2;

    private MDTestPeerMode _Mode = MDTestPeerMode.NOT_SET;
    private MDTestPeerMode Mode
    {
        get
        {
            return _Mode;
        }
        set
        {
            _Mode = value;
            if (UIManager != null)
            {
                UIManager.SetMode(Mode.ToString());
            }
        }
    }

    private TestConfigManager TestConfigManager;

    private MDGameSession GameSession;

    private TestUIManager UIManager = null;

    private List<PeerTestInfo> PeerList = new List<PeerTestInfo>();
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GameSession = MDStatics.GetGameSession();
        GameSession.OnPlayerJoinedEvent += OnPlayerJoin;
        CheckPeerMode();
        LoadConfigs();
        LoadTests();
    }

    public override void _ExitTree()
    {
        GameSession.OnPlayerJoinedEvent -= OnPlayerJoin;
    }

#region NETWORK CODE

    private void OnPlayerJoin(int PeerId)
    {
        this.ChangeNetworkMaster(MDStatics.GetServerId());
        if (!MDStatics.IsServer() || PeerId == MDStatics.GetServerId())
        {
            return;
        }

        PeerTestInfo pti = new PeerTestInfo(PeerId, PeerList.Count+1);
        if (pti.Mode == MDTestPeerMode.NOT_CONNECTED_CLIENT)
        {
            GetTree().Quit();
            return;
        }
        PeerList.Add(pti);
        this.MDRpcId(PeerId, nameof(ClientSetPeerMode), pti.Mode);
    }

    [Puppet]
    private void ClientSetPeerMode(MDTestPeerMode PeerMode)
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
        if (START_CLIENT_COUNT == PeerList.Count && (!PeerList.Any(pti => !pti.Ready)))
        {
            UIManager.SetStatus($"Running tests");
            // TODO: actually start tests
        }
        else
        {
            UIManager.SetStatus($"Waiting for {START_CLIENT_COUNT - PeerList.Count} clients");
        }
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
#endregion

#region UTILITY METHODS

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
            MDLog.Trace(LOG_CAT, $"Found {TestList.Count} tests ({MDStatics.GetParametersAsString(TestList.ToArray())})");
        }
    }

    public PeerTestInfo GetPeerTestInfo(int PeerId)
    {
        PeerTestInfo PeerTestInfo = PeerList.Single(pti => pti.PeerId == PeerId);
        if (PeerTestInfo == null)
        {
            MDLog.Fatal(LOG_CAT, $"Could not resolve PeerTestInfo for peer {PeerId}");
            GetTree().Quit();
        }
        return PeerTestInfo;
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
}
