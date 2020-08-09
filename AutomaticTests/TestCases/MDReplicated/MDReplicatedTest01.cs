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

    protected void SetPrivateIntField(int Value)
    {
        PrivateIntField = 3;
    }

    public int GetPrivateIntField()
    {
        return PrivateIntField;
    }
}

[MDTestClass]
[MDAutoRegister]
public class MDReplicatedTest01 : MDReplicatedTest01Base
{
    [MDTest]
    protected void TestSetPublicIntField()
    {
        PublicIntField = 1;
    }

    protected void TestSetPublicIntFieldValidate()
    {
        if (PublicIntField != 1)
        {
            LogError($"PublicIntField is {PublicIntField} but expected 1");
        }
    }
    
    [MDTest]
    protected void TestSetProtectedIntProperty()
    {
       ProtectedIntProperty = 2;
    }

    protected void TestSetProtectedIntPropertyValidate()
    {
        if (ProtectedIntProperty != 2)
        {
            LogError($"ProtectedIntProperty is {ProtectedIntProperty} but expected 2");
        }
    }
    
    [MDTest]
    protected void TestCallProtectedMethod()
    {
        SetPrivateIntField(3);
    }

    protected void TestCallProtectedMethodValidate()
    {
        if (GetPrivateIntField() != 3)
        {
            LogError($"PrivateIntField is {GetPrivateIntField()} but expected 3");
        }
    }
}