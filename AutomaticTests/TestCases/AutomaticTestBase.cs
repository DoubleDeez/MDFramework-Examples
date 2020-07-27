using Godot;
using System;
using System.Collections.Generic;
using MD;

/// <summary>
/// Inherit from this class when creating your tests.
/// Use GetTimeoutDurationInSeconds() to set a longer timeout for your test if needed.
/// You can also use GetValidationWaitTime() to set a higher delay before validation happens if needed.
/// </summary>
public abstract class AutomaticTestBase : Node2D, IAutomaticTest
{
    private const string LOG_CAT = "LogAutomaticTestBase";
    protected const int TEST_VALIDATIONS_NEEDED = 2;
    protected const float VALIDATION_WAIT_TIME = 0.5f;
    protected const float TIMEOUT_DURATION = 30f;
    protected bool TestComplete = false;
    protected bool TestSuccess = false;
    protected List<String> ErrorList = new List<String>();

    protected int CurrentTest = 1;
    private int TestValidations = 0;

    private Timer ValidationTimer;


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
        MDLog.Debug(LOG_CAT, $"{this.GetType().ToString()}: Got validation signal for test {CurrentTest}");
        TestValidations++;
        if (TestValidations == TEST_VALIDATIONS_NEEDED)
        {
            MDLog.Debug(LOG_CAT, $"{this.GetType().ToString()}: Validation complete for test {CurrentTest}");
            CurrentTest++;
            TestValidations = 0;
            StartNextTest();
        }
    }

    [Remote]
    protected void DoValidation()
    {
        try
        {
            DebugPrintHeader($"{this.GetType().ToString()}: Invoking ValidateTest{CurrentTest}");
            this.Invoke($"ValidateTest{CurrentTest}");
        }
        catch {}
        TestFinished();
    }

    protected void StartNextTest()
    {
        try
        {
            DebugPrintHeader($"{this.GetType().ToString()}: Attempting to Invoke Test{CurrentTest}");
            if (!this.Invoke($"Test{CurrentTest}"))
            {
                // We failed to invoke test so we are done
                SetTestComplete();
                return;
            }
        } 
        catch (Exception ex)
        {
            // We had an exception
            MDLog.Debug(LOG_CAT, $"{this.GetType().ToString()}: Exception! {ex.Message}");
            Exception root = ex;
            while (root != null)
            {
                DebugPrintHeader(root.Message);
                MDLog.Debug(LOG_CAT, root.StackTrace);
                root = root.InnerException;
            }
        }

        if (ValidationTimer == null)
        {
            ValidationTimer = this.CreateTimer("ValidationTimer", true, GetValidationWaitTime(), false, this, nameof(ServerDoValidation));
        }
        ValidationTimer.Start(GetValidationWaitTime());
    }

    private void ServerDoValidation()
    {
        MDLog.Debug(LOG_CAT, $"{this.GetType().ToString()}: Requesting validation for test {CurrentTest}");
        Rpc(nameof(DoValidation));
        ValidationTimer.Stop();
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
        string Success = TestSuccess ? "Success" : "Failure";
        DebugPrintHeader($"Test unit {this.GetType().ToString()} finished with result: {Success}");
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
        DebugPrintHeader($"Starting test unit {this.GetType().ToString()}");
        StartNextTest();
    }

    protected void DebugPrintHeader(string Text)
    {
        MDLog.Debug(LOG_CAT, $"-------------------------------------------------------------------");
        MDLog.Debug(LOG_CAT, Text);
        MDLog.Debug(LOG_CAT, $"-------------------------------------------------------------------");
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
    /// <returns>Default timeout is 30f</returns>
    public virtual float GetTimeoutDurationInSeconds()
    {
        return TIMEOUT_DURATION;
    }

    public int GetCurrentRunningTest()
    {
        return CurrentTest;
    }

    /// <summary>
    /// Override if you want a longer wait time before validation after server executed it's method
    /// </summary>
    /// <returns>The wait time before we trigger validation</returns>
    public virtual float GetValidationWaitTime()
    {
        return VALIDATION_WAIT_TIME;
    }
}
