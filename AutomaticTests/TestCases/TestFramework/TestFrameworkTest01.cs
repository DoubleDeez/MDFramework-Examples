using Godot;
using System;
using MD;

[MDTestClass]
public class TestFrameworkTest01 : Node
{
    private const string LOG_CAT = "LogTestFrameworkTest01";

    private bool SetupClassAllDone = false;
    private bool SetupClassServerDone = false;
    private bool SetupClassClient1Done = false;
    private bool SetupClassClient2Done = false;
    private int BeforeTestCounter = 0;
    private int AfterTestCounter = 0;

    public TestFrameworkTest01()
    {
        // Tests should have empty constructor for reflection
    }

    [MDTest(MDTestMode.SetupClass)]
    public void SetupTestClassAll()
    {
        SetupClassAllDone = true;
    }

    [MDTest(MDTestPeer.SERVER, MDTestMode.SetupClass)]
    public void SetupTestClassServer()
    {
        SetupClassServerDone = true;
    }

    [MDTest(MDTestPeer.CLIENT1, MDTestMode.SetupClass)]
    public void SetupTestClassClient1()
    {
        SetupClassClient1Done = true;
    }

    [MDTest(MDTestPeer.CLIENT2, MDTestMode.SetupClass)]
    public void SetupTestClassClient2()
    {
        SetupClassClient2Done = true;
    }

    [MDTest(MDTestMode.BeforeTest)]
    public void BeforeTestAll()
    {
        BeforeTestCounter++;
    }

    [MDTest(MDTestMode.AfterTest)]
    public void AfterTestAll()
    {
        AfterTestCounter++;
    }

    [MDTest(MDTestMode.ShutdownClass)]
    public void ShutdownClassAll()
    {
        MDLog.Info(LOG_CAT, "Executed shutdown class");
        if (BeforeTestCounter == 0)
        {
            MDTestManager.LogError($"BeforeTestAll not done on {MDTestManager.Instance.Mode.ToString()}");
        }
        if (AfterTestCounter == 0)
        {
            MDTestManager.LogError($"AfterTestAll not done on {MDTestManager.Instance.Mode.ToString()}");
        }
    }

    [MDTest]
    public void TestASimpleTest()
    {
        if (MDTestManager.Instance.Mode != MDTestPeer.SERVER)
        {
            MDTestManager.LogError($"We are not SERVER, we are {MDTestManager.Instance.Mode.ToString()}");
        }
        if (!SetupClassAllDone)
        {
            MDTestManager.LogError($"SetupClassAll not done on {MDTestManager.Instance.Mode.ToString()}");
        }
        if (!SetupClassServerDone)
        {
            MDTestManager.LogError($"SetupClassServer not done on {MDTestManager.Instance.Mode.ToString()}");
        }
    }

    public void TestASimpleTestValidate()
    {
        if (!SetupClassAllDone)
        {
            MDTestManager.LogError($"SetupClassAll not done on {MDTestManager.Instance.Mode.ToString()}");
        }
        switch (MDTestManager.Instance.Mode)
        {
            case MDTestPeer.CLIENT1:
                if (!SetupClassClient1Done)
                {
                    MDTestManager.LogError($"SetupClassClient1 not done on {MDTestManager.Instance.Mode.ToString()}");
                }
                break;
            case MDTestPeer.CLIENT2:
            if (!SetupClassClient2Done)
                {
                    MDTestManager.LogError($"SetupClassClient2 not done on {MDTestManager.Instance.Mode.ToString()}");
                }
                break;
        }
    }

    [MDTest]
    [MDTestSetting(MDTestSettings.Validate, MDTestPeer.CLIENT1, nameof(ValidateOnClient1))]
    [MDTestSetting(MDTestSettings.Validate, MDTestPeer.CLIENT2, nameof(ValidateOnClient2))]
    public void TestCustomClientValidation()
    {
        if (MDTestManager.Instance.Mode != MDTestPeer.SERVER)
        {
            MDTestManager.LogError($"We are not SERVER, we are {MDTestManager.Instance.Mode.ToString()}");
        }
    }

    [MDTest(MDTestPeer.CLIENT1)]
    [MDTestSetting(MDTestSettings.Validate, MDTestPeer.SERVER, nameof(ValidateOnServer))]
    [MDTestSetting(MDTestSettings.Validate, MDTestPeer.CLIENT2, nameof(ValidateOnClient2))]
    public void TestClient1RunningTest()
    {
        if (MDTestManager.Instance.Mode != MDTestPeer.CLIENT1)
        {
            MDTestManager.LogError($"We are not CLIENT1, we are {MDTestManager.Instance.Mode.ToString()}");
        }
    }

    [MDTest(MDTestPeer.CLIENT2)]
    [MDTestSetting(MDTestSettings.Validate, MDTestPeer.SERVER, nameof(ValidateOnServer))]
    [MDTestSetting(MDTestSettings.Validate, MDTestPeer.CLIENT1, nameof(ValidateOnClient1))]
    public void TestClient2RunningTest()
    {
        if (MDTestManager.Instance.Mode != MDTestPeer.CLIENT2)
        {
            MDTestManager.LogError($"We are not CLIENT2, we are {MDTestManager.Instance.Mode.ToString()}");
        }
    }

    public void ValidateOnClient1()
    {
        if (MDTestManager.Instance.Mode != MDTestPeer.CLIENT1)
        {
            MDTestManager.LogError($"We are not CLIENT1, we are {MDTestManager.Instance.Mode.ToString()}");
        }
    }

    public void ValidateOnClient2()
    {
        if (MDTestManager.Instance.Mode != MDTestPeer.CLIENT2)
        {
            MDTestManager.LogError($"We are not CLIENT2, we are {MDTestManager.Instance.Mode.ToString()}");
        }
    }

    public void ValidateOnServer()
    {
        if (MDTestManager.Instance.Mode != MDTestPeer.SERVER)
        {
            MDTestManager.LogError($"We are not SERVER, we are {MDTestManager.Instance.Mode.ToString()}");
        }
    }
}
