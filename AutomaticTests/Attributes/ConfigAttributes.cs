using Godot;
using System;

[AttributeUsage(AttributeTargets.Class)]
public class MDTestConfig : Attribute
{
    public bool DevelopmentMode { private set; get; }
    public string TestGroup { private set; get; }
    public int Repeat { private set; get; }

    public MDTestConfig(bool DevelopmentMode = false, string TestGroup = "All", int Repeat = 1)
    {
        this.DevelopmentMode = DevelopmentMode;
        this.TestGroup = TestGroup;
        this.Repeat = Repeat;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class MDTestConfigBeforeConnect : Attribute {}

[AttributeUsage(AttributeTargets.Method)]
public class MDTestConfigAfterConnect : Attribute {}

[AttributeUsage(AttributeTargets.Method)]
public class MDTestConfigBeforeDisconnect : Attribute {}

[AttributeUsage(AttributeTargets.Method)]
public class MDTestConfigAfterDisconnect : Attribute {}
