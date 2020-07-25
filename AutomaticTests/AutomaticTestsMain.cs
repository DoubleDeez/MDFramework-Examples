using Godot;
using System;
using System.Collections.Generic;
using MD;

[MDAutoRegister]
public class AutomaticTestsMain : Node2D
{
    private const string LOG_CAT = "AutomaticTestMain";
    protected const String TEST_PATH = "res://AutomaticTests/TestCases/";
    private const int START_PLAYER_COUNT = 3;
    private const float CLIENT_CONNECT_WAIT_TIME = 3f;
    private const float CHECK_TEST_STATUS_DELAY = 0.1f;

    [MDBindNode("CanvasLayer/GridContainer/LabelMode")]
    private Label ModeLabel;

    [MDBindNode("CanvasLayer/GridContainer/LabelStatus")]
    private Label StatusLabel;

    [MDBindNode("CanvasLayer/TxtLog")]
    private TxtLog LogText;

    private Queue<PackedScene> Tests = new Queue<PackedScene>();

    private IAutomaticTest RunningTest = null;

    private Timer TestTimer;

    private int FailedTests = 0;

    private float TimeoutTimer = 0f;

    MDGameSession GameSession;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GameSession = MDStatics.GetGameSession();
        GameSession.OnPlayerJoinedEvent += OnPlayerJoin;
        LoadTests();
        LoadConfig();
        String mode = MDArguments.GetArg("mode");
        if (mode == null)
        {
            MDLog.Fatal(LOG_CAT, "Mode not set");
            GetTree().Quit();
        }
        switch (mode.ToLower())
        {
            case "server":
                CallDeferred(nameof(StartHosting));
                break;
            case "client":
                CallDeferred(nameof(StartClient));
                break;
            default:
                MDLog.Fatal(LOG_CAT, $"Unkown mode {mode}");
                GetTree().Quit();
                break;
        }
    }

    public override void _ExitTree()
    {
        GameSession.OnPlayerJoinedEvent -= OnPlayerJoin;
    }

    private void OnPlayerJoin(int PeerId)
    {
        if (!MDStatics.IsServer())
        {
            return;
        }
        this.ChangeNetworkMaster(MDStatics.GetServerId());
        int RemainingPlayerCount = START_PLAYER_COUNT - GameSession.GetAllPlayerInfos().Count;
        if (RemainingPlayerCount > 0)
        {
            SetStatus($"Waiting for {RemainingPlayerCount} clients");
        }
        else
        {
            SetStatus($"Running tests");
            TestTimer = this.CreateTimer("TestTimer", false, CHECK_TEST_STATUS_DELAY, false, this, nameof(StartNextTest));
            TestTimer.Start();
        }
    }

    private void FinishTest()
    {
        LogTestResults(RunningTest);
        ((Node)RunningTest).RemoveAndFree();
        RunningTest = null;
    }

    private void StartNextTest()
    {
        TimeoutTimer += CHECK_TEST_STATUS_DELAY;
        if (RunningTest != null)
        {
            if (RunningTest.IsComplete())
            {
                FinishTest();
            }
            else
            {
                if (TimeoutTimer > RunningTest.GetTimeoutDurationInSeconds())
                {
                    // Test timed out
                    LogNoLineBreak($" - TEST {RunningTest.GetCurrentRunningTest()} TIMED OUT", Colors.Red);
                    FinishTest();
                }
                else
                {
                    return;
                }
            }
        }
        if (Tests.Count == 0)
        {
            SetStatus("All tests complete");
            if (FailedTests > 0)
            {
                Log($"We had {FailedTests} test failures", Colors.Red);
            }
            else
            {
                Log($"All tests passed!", Colors.Green);
            }
            TestTimer.Stop();
            // Quit or something?
            return;
        }

        // Start next test
        PackedScene Scene = Tests.Dequeue();
        RunningTest = (IAutomaticTest)this.SpawnNetworkedNode(Scene, "Test");
        LogNoLineBreak($"Running test: {Scene.ResourcePath}", Colors.DarkGreen);
        RunningTest.StartTest();
        TimeoutTimer = 0f;
    }

    private void LoadConfig()
    {
        if (!GameSession.GetConfiguration().LoadConfiguration("AutomaticTestConfig.ini"))
        {
            MDLog.Fatal(LOG_CAT, "Failed to load configuration");
            GetTree().Quit();
        }
    }

    public void StartClient()
    {
        SetMode("Client");
        SetStatus("Waiting to connect");
        // Wait a bit before starting the client
        Timer timer = this.CreateUnpausableTimer("ConnectTimer", true, CLIENT_CONNECT_WAIT_TIME, true, this,
                nameof(ConnectToServer));
        timer.Start();
    }

    public void ConnectToServer(Timer timer)
    {
        SetStatus("Connecting to server");
        GameSession.StartClient(GetHost(), GetPort());
        timer.Stop();
        timer.RemoveAndFree();
    }

    public void StartHosting()
    {
        SetMode("Server");
        GameSession.StartServer(GetPort());
    }

    private int GetPort()
    {
        return 7777;
    }

    private string GetHost()
    {
        return "127.0.0.1";
    }

    [Remote]
    private void SetStatus(string Status)
    {
        StatusLabel.Text = Status;
        if (MDStatics.IsServer())
        {
            if (MDStatics.IsNetworkActive())
            {
                Rpc(nameof(SetStatus), Status);
            }
            Log($"New Status: {Status}", Colors.Purple);
        }
    }

    private void SetMode(String Mode)
    {
        LogText.Log($"Mode set to: {Mode}", Colors.HotPink);
        ModeLabel.Text = Mode;
    }

    [Remote]
    private void Log(String Text)
    {
        Log(Text, Colors.White);
    }

    [Remote]
    private void Log(String Text, Color Color)
    {
        LogText.Log(Text, Color);
        if (MDStatics.IsNetworkActive() && MDStatics.IsServer())
        {
            Rpc(nameof(Log), Text, Color);
        }
    }

    [Remote]
    private void LogNoLineBreak(String Text, Color Color)
    {
        LogText.LogNoLineBreak(Text, Color);
        if (MDStatics.IsNetworkActive() && MDStatics.IsServer())
        {
            Rpc(nameof(LogNoLineBreak), Text, Color);
        }
    }

    private void LogTestResults(IAutomaticTest Test)
    {
        if (Test.IsSuccess())
        {
            Log(" - OK", Colors.Green);
        }
        else
        {
            Log(" - FAILED", Colors.Red);
            FailedTests++;
            foreach (string error in Test.GetErrors())
            {
                Log(error, Colors.Red);
            }
        }
    }

#region LOADING CODE

    private void LoadTests()
    {
        LoadTests(TEST_PATH);
        if (Tests.Count == 0)
        {
            MDLog.Fatal(LOG_CAT, "No tests found");
            GetTree().Quit();
        }
        else
        {
            Log($"Found {Tests.Count} tests");
        }
    }

    private void LoadTests(string path)
    {
        Directory dir = new Directory();
        dir.Open(path);
        dir.ListDirBegin(true, true);
        while (true)
        {
            String filePath = dir.GetNext();
            if (filePath == "")
            {
                break;
            }
            if (dir.CurrentIsDir())
            {
                // Go into all subfolder except ignore folder
                LoadTests(path + filePath + "/");
            }
            else if (filePath.ToLower().EndsWith(".tscn"))
            {
                PackedScene Scene = LoadPackedScene(path + filePath);
                Node instance = Scene.Instance();
                if (instance.GetType().GetInterface("IAutomaticTest") != null)
                {
                    Tests.Enqueue(Scene);
                }
                instance.QueueFree();
            }
        }
        dir.ListDirEnd();
    }

    private PackedScene LoadPackedScene(String path)
    {
        String full_path = path;
        if (!ResourceLoader.Exists(full_path))
        {
            GD.Print("Can't find: " + full_path);
        }
        return (PackedScene)ResourceLoader.Load(full_path);
    }
#endregion

}
