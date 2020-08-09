using Godot;
using System;
using MD;

[MDTestConfig(false, "CustomGroup", 1)]
public class CustomGroupTestConfig
{
    private const string LOG_CAT = "LogDefaultTestConfig";

    [MDTestConfigBeforeConnect]
    public void DoBeforeConnect()
    {
        MDLog.Info(LOG_CAT, "Before connect");
    }

    [MDTestConfigBeforeConnect]
    public void SecondBeforeConnect()
    {
        MDLog.Info(LOG_CAT, "A second before connect");
    }

    [MDTestConfigAfterConnect]
    public void DoAfterConnect()
    {
        MDLog.Info(LOG_CAT, "After connect");
    }

    [MDTestConfigBeforeDisconnect]
    public void DoBeforeDisconnect()
    {
        MDLog.Info(LOG_CAT, "Before disconnect");
    }

    [MDTestConfigAfterDisconnect]
    public void DoAfterDisconnect()
    {
        MDLog.Info(LOG_CAT, "After disconnect");
    }
}