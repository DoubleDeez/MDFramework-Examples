using Godot;
using System;
using System.Collections.Generic;

public interface IAutomaticTest
{
    /// <summary>
    /// Start the test (should be called on the server)
    /// </summary>
    void StartTest();

    /// <summary>
    /// Checks if the test is complete
    /// </summary>
    /// <returns>True if complete, false if not</returns>
    bool IsComplete();

    /// <summary>
    /// Check if the test was a success
    /// </summary>
    /// <returns>Returns true if it was, false if not</returns>
    bool IsSuccess();

    /// <summary>
    /// Get a list of erros that occured during testing
    /// </summary>
    /// <returns>List of errors</returns>
    List<String> GetErrors();

    /// <summary>
    /// Get the timeout for this test
    /// </summary>
    /// <returns>The maximum time this test is allowed to run</returns>
    float GetTimeoutDurationInSeconds();

    /// <summary>
    /// The number of the currently running test
    /// </summary>
    int GetCurrentRunningTest();
}
