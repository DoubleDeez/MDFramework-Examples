using Godot;
using MD;

/// <summary>
/// Tests MDReplicated members that are on the base class
/// </summary>
[MDAutoRegister]
public class MDReplicatedTest01Base : AutomaticTestBase
{
    [MDReplicated]
    public int PublicIntField;

    [MDReplicated]
    protected int ProtectedIntProperty { get; set; }

    [MDReplicated]
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

[MDAutoRegister]
public class MDReplicatedTest01 : MDReplicatedTest01Base
{
    protected void Test1()
    {
        PublicIntField = 1;
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
       ProtectedIntProperty = 2;
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
        SetPrivateIntField(3);
    }

    protected void ValidateTest3()
    {
        if (GetPrivateIntField() != 3)
        {
            LogError($"PrivateIntField is {GetPrivateIntField()} but expected 3");
        }
    }
}