using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using MD;

public class TestClassInfo
{
    public class TestPeerAndMethodInfo
    {
        public MDTestPeer TestPeer = MDTestPeer.NOT_SET;
        public MethodInfo TestMethod = null;

        public TestPeerAndMethodInfo(MDTestPeer TestPeer, MethodInfo TestMethod)
        {
            this.TestPeer = TestPeer;
            this.TestMethod = TestMethod;
        }
    }
    public class TestMethodInfo
    {
        public MDTestPeer TestPeer = MDTestPeer.NOT_SET;
        public MDTestMode TestMode = MDTestMode.Test;
        public MethodInfo TestMethod = null;
        public List<TestPeerAndMethodInfo> ValidationMethods = new List<TestPeerAndMethodInfo>();
        public bool DevelopmentMode = false;
    }
    private const string LOG_CAT = "LogTestClassInfo";
    public Type Type {get; private set;}
    private Node _Instance = null;
    public Node Instance {
        get
        {
            if (_Instance == null || !Godot.Object.IsInstanceValid(_Instance))
            {
                _Instance = null;
                foreach (Node child in TestManager.Instance.GetChildren())
                {
                    if (child.Name.StartsWith(GetNameNoBrackets()))
                    {
                        _Instance = child;
                        break;
                    }
                }
            }
            if (_Instance == null)
            {
                MDLog.Fatal(LOG_CAT, $"Could not resolve instance of {GetName()}");
            }
            return _Instance;
        }
        private set
        {
            _Instance = value;
        }}
    public string TestGroup {get; private set;}
    public bool DevelopmentMode {get; private set;}
    public bool AlwaysRunInAllMode {get; private set;}
    public List<TestMethodInfo> TestMethods = new List<TestMethodInfo>();
    public List<TestMethodInfo> BeforeTest = new List<TestMethodInfo>();
    public List<TestMethodInfo> AfterTest = new List<TestMethodInfo>();
    public List<TestMethodInfo> SetupClass = new List<TestMethodInfo>();
    public List<TestMethodInfo> ShutdownClass = new List<TestMethodInfo>();

    public TestClassInfo(Type Type)
    {
        MDTestClass TestClass = MDStatics.FindClassAttribute<MDTestClass>(Type);
        this.Type = Type;
        this.DevelopmentMode = TestClass.DevelopmentMode;
        this.TestGroup = TestClass.TestGroup;
        this.AlwaysRunInAllMode = TestClass.AlwaysRunInAllMode;
        LoadTests();
    }

    public void CreateNewInstance()
    {
        if (Instance != null)
        {
            Instance.RemoveAndFree();
        }
        this.Instance = MDStatics.GetGameSession().SpawnNetworkedNode(Type, TestManager.Instance, GetNameNoBrackets());
    }

    public bool ShouldRunTestClass(string GroupName)
    {
        if (this.TestGroup.Equals(GroupName))
        {
            return true;
        } 
        else if (GroupName.Equals(MDTestClass.TEST_GROUP_ALL) && AlwaysRunInAllMode)
        {
            return true;
        }

        return false;
    }

    private void LoadTests()
    {
        // Load our config files
        List<MethodInfo> Methods = Type.GetMethodInfos();
        foreach (MethodInfo Method in Methods)
        {
            MDTest TestAttribute = Method.GetCustomAttribute(typeof(MDTest)) as MDTest;
            if (TestAttribute == null)
            {
                continue;
            }

            TestMethodInfo TestMethodInfo = new TestMethodInfo();
            TestMethodInfo.TestMethod = Method;
            TestMethodInfo.TestPeer = TestAttribute.MDTestHost;
            TestMethodInfo.TestMode = TestAttribute.TestMode;
            if (TestAttribute.TestMode != MDTestMode.Test)
            {
                // This is not a test
                AddTestClassMethod(TestMethodInfo);
            }
            else
            {
                ParseSettings(Method, TestMethodInfo, Methods);

                // If no validation method is in the tmi then parse it
                if (TestMethodInfo.ValidationMethods.Count == 0)
                {
                    // Resolve validation method
                    String ValidationMethodName = $"{Method.Name}Validate";
                    try
                    {
                        MethodInfo MethodInfo = Methods.Single(methodInfo => methodInfo.Name.Equals(ValidationMethodName));
                        TestMethodInfo.ValidationMethods.Add(new TestPeerAndMethodInfo(MDTestPeer.ALL, MethodInfo));
                    }
                    catch(Exception ex)
                    {
                        MDLog.Fatal(LOG_CAT, $"{GetName()} Could not found validation method [{ValidationMethodName}] for test method [{Method.Name}]");
                        throw ex;
                    }
                }

                TestMethods.Add(TestMethodInfo);
            }

            MDLog.Trace(LOG_CAT, $"{GetName()} Loaded [{TestMethodInfo.TestMode.ToString()}] [{TestMethodInfo.TestMethod.Name}] runs on peer [{TestMethodInfo.TestPeer.ToString()}]");
            foreach (TestPeerAndMethodInfo ValidationMethod in TestMethodInfo.ValidationMethods)
            {
                MDLog.Trace(LOG_CAT, $"{GetName()} Found validation method [{ValidationMethod.TestMethod.Name}] for peer [{ValidationMethod.TestPeer.ToString()}]");
            }
        }
    }

    private void ParseSettings(MethodInfo Method, TestMethodInfo TestMethodInfo, List<MethodInfo> Methods)
    {
        object[] TempArray = Method.GetCustomAttributes(typeof(MDTestSetting), true);
        MDTestSetting[] Settings = Array.ConvertAll(TempArray, item => (MDTestSetting) item);

        foreach (MDTestSetting Setting in Settings)
        {
            switch (Setting.Setting)
            {
                case MDTestSettings.Validate:
                    MDTestPeer TestPeer = (MDTestPeer)Enum.Parse(typeof(MDTestPeer), Setting.Key.ToString());
                    String MethodName = Setting.Value.ToString();
                    MethodInfo MethodInfo = Methods.Single(methodInfo => methodInfo.Name.Equals(MethodName));
                    TestMethodInfo.ValidationMethods.Add(new TestPeerAndMethodInfo(TestPeer, MethodInfo));
                    break;
                case MDTestSettings.DevelopmentMode:
                    TestMethodInfo.DevelopmentMode = (bool) Setting.Value;
                    break;
            }
        }
    }

    private void AddTestClassMethod(TestMethodInfo TestMethodInfo)
    {
        switch (TestMethodInfo.TestMode)
        {
            case MDTestMode.BeforeTest:
                BeforeTest.Add(TestMethodInfo);
                break;
            case MDTestMode.AfterTest:
                AfterTest.Add(TestMethodInfo);
                break;
            case MDTestMode.SetupClass:
                SetupClass.Add(TestMethodInfo);
                break;
            case MDTestMode.ShutdownClass:
                ShutdownClass.Add(TestMethodInfo);
                break;
        }
    }

    public string GetName()
    {
        return $"[{Type.Name}]";
    }

    public string GetNameNoBrackets()
    {
        return Type.Name;
    }

}

public class TestManager : Node
{
    public static TestManager Instance;
    private const string LOG_CAT = "LogTestManager";

    public const string NODE_NAME = "TestManager";

    List<TestClassInfo> TestClassList = new List<TestClassInfo>();

    private MDTestManager MDTestMan;

    private int CurrentTestClass = 0;

    private int CurrentTest = 0;

    private int CurrentResponses = 0;

    private string GroupName = "";

    public TestManager(MDTestManager MDTestManager)
    {
        this.MDTestMan = MDTestManager;
        Instance = this;

        // Load our config files
        List<Type> FoundTests = MDStatics.FindAllScriptsImplementingAttribute<MDTestClass>();
        if (FoundTests.Count == 0)
		{
			MDLog.Fatal(LOG_CAT, "No tests found");
            return;
		}
		
        MDLog.Trace(LOG_CAT, $"Found {FoundTests.Count} tests ({MDStatics.GetParametersAsString(FoundTests.ToArray())})");
        FoundTests.ForEach(test => TestClassList.Add(new TestClassInfo(test)));
    }

    public int GetConfigCount()
    {
        return TestClassList.Count;
    }

    public void StartTests(String GroupName)
    {
        CurrentTestClass = -1;
        this.GroupName = GroupName;
        StartNextTestClass();
    }
#region TEST LOOP

    /// <summary>
    /// STEP1. New test is started on the server
    /// </summary>
    private void StartNextTestClass()
    {
        CurrentTestClass++;
        if (CurrentTestClass < TestClassList.Count)
        {
            if (GetCurrentTestClass().ShouldRunTestClass(GroupName))
            {
                CurrentResponses = 0;
                CurrentTest = -1;
                GetCurrentTestClass().CreateNewInstance();
                this.MDRpc(nameof(RunSetupClass), CurrentTestClass);
            }
            else
            {
                StartNextTestClass();
            }
        }
        else
        {
            // We completed all tests
            MDTestMan.TestsComplete();
        }
    }

    [PuppetSync]
    private void RunSetupClass(int TestClassNumber)
    {
        if (!MDStatics.IsServer())
        {
            CurrentTestClass = TestClassNumber;
        }
        MDTestMan.UIManager.SetStatus($"{GetCurrentTestClass().GetName()} Running", Colors.Gold);
        RunMyMethods(GetCurrentTestClass().SetupClass);
        if (!MDStatics.IsServer())
        {
            this.MDServerRpc(nameof(RunSetupClassComplete));
        }
    }

    [Master]
    private void RunSetupClassComplete()
    {
        if (GotAllRespones())
        {
            CurrentTest++;
            this.MDRpc(nameof(RunBeforeTest), CurrentTest);
        }
    }

    [PuppetSync]
    private void RunBeforeTest(int TestNumber)
    {
        if (!MDStatics.IsServer())
        {
            CurrentTest = TestNumber;
        }
        MDTestMan.UIManager.SetStatus($"{GetCurrentTestClass().GetName()} Running test [{GetCurrentTestClass().TestMethods[CurrentTest].TestMethod.Name}]", Colors.Goldenrod);
        RunMyMethods(GetCurrentTestClass().BeforeTest);
        if (!MDStatics.IsServer())
        {
            this.MDServerRpc(nameof(BeforeTestComplete));
        }
    }

    [Master]
    private void BeforeTestComplete()
    {
        if (GotAllRespones())
        {
            this.MDRpc(nameof(RunTest));
        }
    }

    [PuppetSync]
    private void RunTest()
    {
        InvokeIfMine(GetCurrentTestClass().TestMethods[CurrentTest]);
        if (!MDStatics.IsServer())
        {
            this.MDServerRpc(nameof(TestComplete));
        }
    }

    [Master]
    private void TestComplete()
    {
        if (GotAllRespones())
        {
            this.MDRpc(nameof(RunValidateTest));
        }
    }

    [PuppetSync]
    private void RunValidateTest()
    {
        // Only do validation on clients that didn't run the test
        if (GetCurrentTestClass().TestMethods[CurrentTest].TestPeer != MDTestMan.Mode)
        {
            RunMyMethods(GetCurrentTestClass().TestMethods[CurrentTest].ValidationMethods);
        }
        if (!MDStatics.IsServer())
        {
            this.MDServerRpc(nameof(ValidateTestComplete));
        }
    }

    [Master]
    private void ValidateTestComplete()
    {
        if (GotAllRespones())
        {
            this.MDRpc(nameof(RunAfterTest));
        }
    }

    [PuppetSync]
    private void RunAfterTest()
    {
        RunMyMethods(GetCurrentTestClass().AfterTest);
        if (!MDStatics.IsServer())
        {
            this.MDServerRpc(nameof(AfterTestComplete));
        }
    }

    [Master]
    private void AfterTestComplete()
    {
        if (GotAllRespones())
        {
            CurrentTest++;
            if (CurrentTest < GetCurrentTestClass().TestMethods.Count)
            {
                // Run next test
                this.MDRpc(nameof(RunBeforeTest), CurrentTest);
            }
            else
            {
                // Tests complete move to shutdown
                this.MDRpc(nameof(RunShutdownClass));
            }
        }
    }

    [PuppetSync]
    private void RunShutdownClass()
    {
        MDTestMan.UIManager.SetStatus($"{GetCurrentTestClass().GetName()} Complete", Colors.Gold);
        RunMyMethods(GetCurrentTestClass().ShutdownClass);
        if (!MDStatics.IsServer())
        {
            this.MDServerRpc(nameof(AfterShutdownClass));
        }
    }

    [Master]
    private void AfterShutdownClass()
    {
        if (GotAllRespones())
        {
            StartNextTestClass();
        }
    }

#endregion

    private bool GotAllRespones()
    {
        CurrentResponses++;
        if (CurrentResponses == MDTestManager.CLIENT_COUNT)
        {
            CurrentResponses = 0;
            return true;
        }
        return false;
    }
    private void RunMyMethods(List<TestClassInfo.TestPeerAndMethodInfo> TestPeerAndMethodInfoList)
    {
        TestPeerAndMethodInfoList.ForEach(tpmi => 
        {
            if (tpmi.TestPeer == MDTestMan.Mode || tpmi.TestPeer == MDTestPeer.ALL)
            {
                LogToTrace($"{GetCurrentTestClass().GetName()} Invoking {tpmi.TestMethod.Name}");
                tpmi.TestMethod.Invoke(GetCurrentTestClass().Instance, null);
            }
        });
    }

    private void RunMyMethods(List<TestClassInfo.TestMethodInfo> MethodInfoList)
    {
        MethodInfoList.ForEach(tmi => InvokeIfMine(tmi));
    }

    private void InvokeIfMine(TestClassInfo.TestMethodInfo testMethodInfo)
    {
        if (testMethodInfo.TestPeer == MDTestMan.Mode || testMethodInfo.TestPeer == MDTestPeer.ALL)
        {
            LogToTrace($"{GetCurrentTestClass().GetName()} Invoking {testMethodInfo.TestMethod.Name}");
            testMethodInfo.TestMethod.Invoke(GetCurrentTestClass().Instance, null);
        }
    }
    public TestClassInfo GetCurrentTestClass()
    {
        if (CurrentTestClass < 0 || CurrentTestClass >= TestClassList.Count)
        {
            return null;
        }
        return TestClassList[CurrentTestClass];
    }

    public TestClassInfo.TestMethodInfo GetCurrentTest()
    {
        TestClassInfo tci = GetCurrentTestClass();
        if (tci == null)
        {
            return null;
        }
        if (CurrentTest < 0 || CurrentTest >= tci.TestMethods.Count)
        {
            return null;
        }
        return tci.TestMethods[CurrentTest];
    }

    private void LogToTrace(string Message)
    {
        MDLog.Trace(LOG_CAT, Message);
    }
}
