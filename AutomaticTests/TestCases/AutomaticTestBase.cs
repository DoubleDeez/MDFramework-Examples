using Godot;
using System;
using System.Collections.Generic;
using MD;

public abstract class AutomaticTestBase : Node2D, IAutomaticTest
{
    private const string LOG_CAT = "LogAutomaticTestBase";
    protected const int TEST_VALIDATIONS_NEEDED = 2;
    protected bool TestComplete = false;
    protected bool TestSuccess = false;
    protected List<String> ErrorList = new List<String>();

    protected int CurrentTest = 1;
    private int TestValidations = 0;


    protected void TestFinished()
    {
        RpcId(MDStatics.GetServerId(), nameof(ServerTestFinished), CurrentTest);
        CurrentTest++;
    }

    [Master]
    protected void ServerTestFinished(int Number)
    {
        if (Number != CurrentTest)
        {
            AddError($"Got test finished signal for wrong test, should be {CurrentTest} but it was {Number}");
            return;
        }
        TestValidations++;
        if (TestValidations == TEST_VALIDATIONS_NEEDED)
        {
            CurrentTest++;
            TestValidations = 0;
            StartNextTest();
        }
    }

    protected void DoValidation()
    {
        try
        {
            MDLog.Debug(LOG_CAT, $"---------------------------------------------------------------");
            MDLog.Debug(LOG_CAT, $"{this.GetType().ToString()}: Invoking ValidateTest{CurrentTest}");
            MDLog.Debug(LOG_CAT, $"---------------------------------------------------------------");
            this.Invoke($"ValidateTest{CurrentTest}");
        }
        catch {}
    }

    protected void StartNextTest()
    {
        try
        {
            MDLog.Debug(LOG_CAT, $"-------------------------------------------------------------------");
            MDLog.Debug(LOG_CAT, $"{this.GetType().ToString()}: Attempting to Invoke Test{CurrentTest}");
            MDLog.Debug(LOG_CAT, $"-------------------------------------------------------------------");
            if (!this.Invoke($"Test{CurrentTest}"))
            {
                // We failed to invoke test so we are done
                SetTestComplete();
            }
        } 
        catch
        {
            // Any exception means we are done
            SetTestComplete();
        }
    }

    protected void LogError(string text)
    {
        RpcId(MDStatics.GetServerId(), nameof(ServerLogError), $"Test{CurrentTest} - {text}");
    }

    [Master]
    protected void ServerLogError(string text)
    {
        AddError($"{Multiplayer.GetRpcSenderId()} - {text}");
    }

    protected void SetTestComplete()
    {
        TestComplete = true;
        TestSuccess = ErrorList.Count == 0;
    }

    protected void SetTestComplete(bool Success)
    {
        TestComplete = true;
        TestSuccess = Success;
    }

    protected void AddError(string text)
    {
        ErrorList.Add(text);
    }

    public void StartTest()
    {
        StartNextTest();
    }

    public bool IsComplete()
    {
        return TestComplete;
    }

    public bool IsSuccess()
    {
        return TestSuccess;
    }

    public List<String> GetErrors()
    {
        return ErrorList;
    }

    /// <summary>
    ///  You can override this in your test to give it a different timeout
    /// </summary>
    /// <returns>Default timeout is 10f</returns>
    public virtual float GetTimeoutDurationInSeconds()
    {
        return 10f;
    }

    public int GetCurrentRunningTest()
    {
        return CurrentTest;
    }
}
