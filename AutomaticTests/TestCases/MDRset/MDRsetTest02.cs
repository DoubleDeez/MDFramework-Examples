using Godot;
using MD;

[MDAutoRegister]
public class MDRsetTest02Base : AutomaticTestBase
{
    [Remote]
    public int PublicIntField;

    [Remote]
    protected int ProtectedIntProperty { get; set; }

    [Remote]
    private int PrivateIntField;

    public void SetPrivateIntField(int Value)
    {
        PrivateIntField = 3;
    }

    public int GetPrivateIntField()
    {
        return PrivateIntField;
    }
}

/// <summary>
/// Tests calling RPC methods that are on the base class
/// </summary>
[MDAutoRegister]
public class MDRsetTest02 : MDRsetTest02Base
{
    protected void Test1()
    {
        this.MDRset(nameof(PublicIntField), 1);
    }

    protected void ValidateTest1()
    {
        if (PublicIntField != 1)
        {
            LogError($"PublicIntField is {PublicIntField} but expected 1");
        }
    }
    
    protected void Test2()
    {
        this.MDRset(nameof(ProtectedIntProperty), 2);
    }

    protected void ValidateTest2()
    {
        if (ProtectedIntProperty != 2)
        {
            LogError($"ProtectedIntProperty is {ProtectedIntProperty} but expected 2");
        }
    }
    
    protected void Test3()
    {
        this.MDRset("PrivateIntField", 3);
    }

    protected void ValidateTest3()
    {
        if (GetPrivateIntField() != 3)
        {
            LogError($"PrivateIntField is {GetPrivateIntField()} but expected 3");
        }
    }
}